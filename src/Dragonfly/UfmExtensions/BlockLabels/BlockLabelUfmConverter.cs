namespace Dragonfly.UfmExtensions;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Converts AngularJS block labels (Umbraco 13 and earlier) into Umbraco Flavored Markdown.
/// Pure string handling, so it can be unit tested and reused outside this site.
/// </summary>
public static class BlockLabelUfmConverter
{
	private static readonly Regex Expression = new(@"\{\{(.*?)\}\}", RegexOptions.Compiled | RegexOptions.Singleline);
	private static readonly Regex SettingsEqualsOne = new(@"(\$settings\.\w+)\s*==\s*1\b", RegexOptions.Compiled);
	private static readonly Regex RichTextFilter = new(@"\|\s*ncRichText", RegexOptions.Compiled);
	private static readonly Regex TruncateFilter = new(@"\|\s*truncate\s*:\s*(?:true|false)\s*:\s*(\d+)", RegexOptions.Compiled);
	private static readonly Regex ArrayIndex = new(@"\[\s*\d+\s*\]", RegexOptions.Compiled);
	private static readonly Regex QuotedLiteral = new(@"^'[^']*'$", RegexOptions.Compiled);
	private static readonly Regex DoubleNegation = new(@"!!", RegexOptions.Compiled);

	//AngularJS expressions that a UFM component covers on its own. Group 1, where present, is the component's argument.
	private static readonly Regex StandaloneContentTypeName = new(@"^\$contentTypeName$", RegexOptions.Compiled);
	private static readonly Regex LinkName = new(@"^(\w+)\[0\]\[""name""\]$", RegexOptions.Compiled);
	private static readonly Regex LinkUrl = new(@"^(\w+)\[0\]\[""url""\]$", RegexOptions.Compiled);
	private static readonly Regex LinkDisplay = new(
		@"^(\w+)\[0\]\[""nodeName""\]\s*\?\s*\1\[0\]\[""nodeName""\]\s*:\s*\(?\s*\1\[0\]\[""name""\]\s*\?\s*\1\[0\]\[""name""\]\s*:\s*\1\[0\]\[""url""\]\s*\)?$",
		RegexOptions.Compiled);
	private static readonly Regex PickerName = new(@"^(\w+)\s*\|\s*nc(?:Node|Media)Name$", RegexOptions.Compiled);

	/// <summary>
	/// The UFM component each AngularJS expression becomes: one alias for when the Dragonfly UFM components are
	/// available, and one using only Umbraco's built-in components. A null built-in alias means there is no
	/// built-in equivalent, so the expression is converted as a plain ${ } expression instead.
	/// </summary>
	private static readonly ComponentSubstitution[] ComponentSubstitutions =
	[
		new(StandaloneContentTypeName, DragonflyAlias: "dufmBlockContentTypeName", BuiltInAlias: null),
		new(LinkDisplay, DragonflyAlias: "dufmLinkDisplay", BuiltInAlias: "umbLink"),
		new(LinkUrl, DragonflyAlias: "dufmLinkUrl", BuiltInAlias: null),
		new(LinkName, DragonflyAlias: "umbLink", BuiltInAlias: "umbLink"),
		new(PickerName, DragonflyAlias: "umbContentName", BuiltInAlias: "umbContentName"),
	];

	/// <summary>
	/// Converts a single label.
	/// </summary>
	/// <param name="Label">The stored label.</param>
	/// <param name="ContentTypeName">Name of the block's element type, used to replace $contentTypeName. Optional.</param>
	/// <param name="UseDragonflyUfmComponents">
	/// Uses this package's UFM components where they cover an expression, which requires the package to stay installed.
	/// When false, only Umbraco's built-in components are used, so the converted labels work without the package.
	/// </param>
	public static BlockLabelConversion Convert(string? Label, string? ContentTypeName, bool UseDragonflyUfmComponents = true)
	{
		var conversion = new BlockLabelConversion { OriginalLabel = Label ?? string.Empty };

		if (string.IsNullOrWhiteSpace(Label) || !Label.Contains("{{"))
		{
			conversion.Status = BlockLabelConversionStatus.NoChangeNeeded;
			conversion.ConvertedLabel = Label ?? string.Empty;
			return conversion;
		}

		var converted = new StringBuilder();
		var position = 0;

		foreach (Match match in Expression.Matches(Label))
		{
			converted.Append(Label, position, match.Index - position);
			converted.Append(ConvertExpression(match.Groups[1].Value, ContentTypeName, UseDragonflyUfmComponents, conversion));
			position = match.Index + match.Length;
		}

		converted.Append(Label, position, Label.Length - position);

		conversion.ConvertedLabel = converted.ToString();
		conversion.Status = conversion.Warnings.Count > 0
			? BlockLabelConversionStatus.NeedsManualReview
			: BlockLabelConversionStatus.Converted;

		return conversion;
	}

	private static string ConvertExpression(string Expression, string? ContentTypeName, bool UseDragonflyUfmComponents, BlockLabelConversion Conversion)
	{
		var expression = Expression.Trim();

		//A UFM component cannot live inside a ${ } expression, so only a standalone expression can use one.
		var component = ComponentFor(expression, UseDragonflyUfmComponents);
		if (component is not null)
		{
			return component;
		}

		//UFM components cannot be nested inside a ${ } expression, so these labels need rewriting by hand.
		if (expression.Contains("ncNodeName") || expression.Contains("ncMediaName"))
		{
			Conversion.Warnings.Add($"'{expression}' resolves a picked item's name: use {{umbContentName: alias}}.");
		}

		if (ArrayIndex.IsMatch(expression))
		{
			Conversion.Warnings.Add(UseDragonflyUfmComponents
				? $"'{expression}' reads into a picker value: use {{dufmLinkDisplay: alias}}, {{dufmLinkUrl: alias}}, {{dufmLinkUrlWithAnchor: alias}} or {{umbLink: alias}}."
				: $"'{expression}' reads into a picker value: {{umbLink: alias}} shows the link's name; no built-in UFM component shows its URL.");
		}

		if (expression.Contains("$contentTypeName"))
		{
			if (!string.IsNullOrWhiteSpace(ContentTypeName))
			{
				expression = expression.Replace("$contentTypeName", $"'{ContentTypeName.Replace("'", "\\'")}'");
			}
			else
			{
				Conversion.Warnings.Add("$contentTypeName has no UFM equivalent and the element type name could not be resolved.");
			}
		}

		//A settings toggle is a real boolean in v14+, so '== 1' would never be true.
		expression = SettingsEqualsOne.Replace(expression, "$1");

		expression = RichTextFilter.Replace(expression, "| stripHtml");
		expression = TruncateFilter.Replace(expression, "| truncate:$1");

		//The expression parser rejects '!!'; an empty value is falsy on its own, so the test still holds.
		expression = DoubleNegation.Replace(expression, string.Empty);

		//A bare string literal does not need an expression wrapper.
		return QuotedLiteral.IsMatch(expression)
			? expression.Substring(1, expression.Length - 2)
			: $"${{ {expression} }}";
	}

	/// <summary>
	/// Maps an expression that a UFM component covers on its own. Anything more involved stays an
	/// expression, because a component cannot be nested inside one.
	/// </summary>
	private static string? ComponentFor(string Expression, bool UseDragonflyUfmComponents)
	{
		foreach (var substitution in ComponentSubstitutions)
		{
			var match = substitution.Pattern.Match(Expression);
			if (!match.Success)
			{
				continue;
			}

			var alias = UseDragonflyUfmComponents ? substitution.DragonflyAlias : substitution.BuiltInAlias;

			return alias is null ? null : UfmComponent(alias, match.Groups[1].Value);
		}

		return null;
	}

	//A component with no argument still needs its colon: {alias:}
	private static string UfmComponent(string Alias, string Argument)
	{
		return Argument.Length == 0
			? $"{{{Alias}:}}"
			: $"{{{Alias}: {Argument}}}";
	}

	private sealed record ComponentSubstitution(Regex Pattern, string DragonflyAlias, string? BuiltInAlias);
}

public enum BlockLabelConversionStatus
{
	NoChangeNeeded,
	Converted,
	NeedsManualReview,
}

public class BlockLabelConversion
{
	public string OriginalLabel { get; set; } = string.Empty;

	public string ConvertedLabel { get; set; } = string.Empty;

	public BlockLabelConversionStatus Status { get; set; }

	public List<string> Warnings { get; set; } = [];
}
