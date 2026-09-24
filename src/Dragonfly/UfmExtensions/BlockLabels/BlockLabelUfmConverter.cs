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

	//Link picker patterns the AngularJS labels used, where a UFM component now covers the whole expression.
	private static readonly Regex LinkName = new(@"^(\w+)\[0\]\[""name""\]$", RegexOptions.Compiled);
	private static readonly Regex LinkUrl = new(@"^(\w+)\[0\]\[""url""\]$", RegexOptions.Compiled);
	private static readonly Regex LinkDisplay = new(
		@"^(\w+)\[0\]\[""nodeName""\]\s*\?\s*\1\[0\]\[""nodeName""\]\s*:\s*\(?\s*\1\[0\]\[""name""\]\s*\?\s*\1\[0\]\[""name""\]\s*:\s*\1\[0\]\[""url""\]\s*\)?$",
		RegexOptions.Compiled);
	private static readonly Regex PickerName = new(@"^(\w+)\s*\|\s*nc(?:Node|Media)Name$", RegexOptions.Compiled);

	/// <summary>
	/// Converts a single label.
	/// </summary>
	/// <param name="Label">The stored label.</param>
	/// <param name="ContentTypeName">Name of the block's element type, used to replace $contentTypeName. Optional.</param>
	/// <param name="UseContentTypeNameComponent">
	/// Replaces a standalone $contentTypeName with the {blockContentTypeName:} UFM component, which resolves
	/// the name at render time. Requires the Dragonfly UFM components. When false, or when the expression is
	/// not standalone, the element type name is written into the label instead.
	/// </param>
	public static BlockLabelConversion Convert(string? Label, string? ContentTypeName, bool UseContentTypeNameComponent = false)
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
			converted.Append(ConvertExpression(match.Groups[1].Value, ContentTypeName, UseContentTypeNameComponent, conversion));
			position = match.Index + match.Length;
		}

		converted.Append(Label, position, Label.Length - position);

		conversion.ConvertedLabel = converted.ToString();
		conversion.Status = conversion.Warnings.Count > 0
			? BlockLabelConversionStatus.NeedsManualReview
			: BlockLabelConversionStatus.Converted;

		return conversion;
	}

	private static string ConvertExpression(string Expression, string? ContentTypeName, bool UseContentTypeNameComponent, BlockLabelConversion Conversion)
	{
		var expression = Expression.Trim();

		//A UFM component cannot live inside a ${ } expression, so only a standalone expression can use one.
		if (UseContentTypeNameComponent && expression == "$contentTypeName")
		{
			return "{blockContentTypeName:}";
		}

		var component = ComponentFor(expression);
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
			Conversion.Warnings.Add($"'{expression}' reads into a picker value: use {{linkDisplay: alias}}, {{linkUrl: alias}} or {{umbLink: alias}}.");
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
	private static string? ComponentFor(string Expression)
	{
		var display = LinkDisplay.Match(Expression);
		if (display.Success)
		{
			return $"{{linkDisplay: {display.Groups[1].Value}}}";
		}

		var url = LinkUrl.Match(Expression);
		if (url.Success)
		{
			return $"{{linkUrl: {url.Groups[1].Value}}}";
		}

		var name = LinkName.Match(Expression);
		if (name.Success)
		{
			return $"{{umbLink: {name.Groups[1].Value}}}";
		}

		var picked = PickerName.Match(Expression);
		if (picked.Success)
		{
			return $"{{umbContentName: {picked.Groups[1].Value}}}";
		}

		return null;
	}
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
