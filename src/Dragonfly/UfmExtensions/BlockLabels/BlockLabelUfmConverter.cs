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
	private static readonly Regex BlockIndex = new(@"\$index\b", RegexOptions.Compiled);

	//AngularJS expressions that a UFM component covers on its own. Group 1, where present, is the component's argument.
	private static readonly Regex StandaloneContentTypeName = new(@"^\$contentTypeName$", RegexOptions.Compiled);
	private static readonly Regex LinkName = new(@"^(\w+)\[0\]\[""name""\]$", RegexOptions.Compiled);
	private static readonly Regex LinkUrl = new(@"^(\w+)\[0\]\[""url""\]$", RegexOptions.Compiled);
	private static readonly Regex LinkDisplay = new(
		@"^(\w+)\[0\]\[""nodeName""\]\s*\?\s*\1\[0\]\[""nodeName""\]\s*:\s*\(?\s*\1\[0\]\[""name""\]\s*\?\s*\1\[0\]\[""name""\]\s*:\s*\1\[0\]\[""url""\]\s*\)?$",
		RegexOptions.Compiled);
	private static readonly Regex PickerName = new(@"^(\w+)\s*\|\s*nc(?:Node|Media)Name$", RegexOptions.Compiled);

	//One value in a chain of fallbacks. Group 1 is the property alias, group 2 a picker name filter, group 3 a truncate length.
	private static readonly Regex FallbackChainValue = new(
		@"^(\w+)(?:\s*\|\s*nc(?:RichText|(NodeName|MediaName)))?(?:\s*\|\s*truncate\s*:\s*(?:true|false)\s*:\s*(\d+))?$",
		RegexOptions.Compiled);
	private static readonly Regex FallbackChainCondition = new(@"^(?:!!)?(\w+)$", RegexOptions.Compiled);

	/// <summary>
	/// The UFM component each AngularJS expression becomes: one alias for when the Dragonfly UFM components are
	/// available, and one using only Umbraco's built-in components. A null built-in alias means there is no
	/// built-in equivalent, so the expression is converted as a plain ${ } expression instead.
	/// </summary>
	private static readonly ComponentSubstitution[] ComponentSubstitutions =
	[
		new(ArgumentFrom(StandaloneContentTypeName), DragonflyAlias: "dufmBlockContentTypeName", BuiltInAlias: null),
		new(ArgumentFrom(LinkDisplay), DragonflyAlias: "dufmLinkDisplay", BuiltInAlias: "umbLink"),
		new(ArgumentFrom(LinkUrl), DragonflyAlias: "dufmLinkUrl", BuiltInAlias: null),
		new(ArgumentFrom(LinkName), DragonflyAlias: "umbLink", BuiltInAlias: "umbLink"),
		new(ArgumentFrom(PickerName), DragonflyAlias: "umbContentName", BuiltInAlias: "umbContentName"),
		new(PickerFallbackChainArgument, DragonflyAlias: "dufmFirstValue", BuiltInAlias: null),
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
	/// <param name="KeepIndexOneBased">
	/// Adds 1 to $index, which counted from 1 in AngularJS labels but counts from 0 in UFM. When false, $index is
	/// left as it is, so the converted labels count from 0.
	/// </param>
	public static BlockLabelConversion Convert(string? Label, string? ContentTypeName, bool UseDragonflyUfmComponents = true, bool KeepIndexOneBased = true)
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
			converted.Append(ConvertExpression(match.Groups[1].Value, ContentTypeName, UseDragonflyUfmComponents, KeepIndexOneBased, conversion));
			position = match.Index + match.Length;
		}

		converted.Append(Label, position, Label.Length - position);

		conversion.ConvertedLabel = converted.ToString();
		conversion.Status = conversion.Warnings.Count > 0
			? BlockLabelConversionStatus.NeedsManualReview
			: BlockLabelConversionStatus.Converted;

		return conversion;
	}

	private static string ConvertExpression(string Expression, string? ContentTypeName, bool UseDragonflyUfmComponents, bool KeepIndexOneBased, BlockLabelConversion Conversion)
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
			Conversion.Warnings.Add(UseDragonflyUfmComponents
				? $"'{expression}' resolves a picked item's name: use {{umbContentName: alias}}, or {{dufmFirstValue: alias, alias}} for a chain of fallbacks."
				: $"'{expression}' resolves a picked item's name: use {{umbContentName: alias}}.");
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

		//$index counted from 1 in AngularJS labels and counts from 0 in UFM.
		if (KeepIndexOneBased)
		{
			expression = expression == "$index"
				? "$index+1"
				: BlockIndex.Replace(expression, "($$index+1)");
		}

		expression = RichTextFilter.Replace(expression, "| stripHtml");
		expression = WithTruncateCalls(expression);

		//The expression parser rejects '!!'; an empty value is falsy on its own, so the test still holds.
		expression = DoubleNegation.Replace(expression, string.Empty);

		//A bare string literal does not need an expression wrapper.
		return QuotedLiteral.IsMatch(expression)
			? expression.Substring(1, expression.Length - 2)
			: $"${{ {expression} }}";
	}

	/// <summary>
	/// Rewrites <c>value | truncate:true:N</c> as <c>truncate(value, N)</c>. The UFM expression parser cannot pass
	/// arguments to a piped filter: inside parentheses <c>| truncate:N</c> is a parse error, and elsewhere the length
	/// is ignored. As in AngularJS, the filter applies to everything before it within the same parentheses.
	/// </summary>
	private static string WithTruncateCalls(string Expression)
	{
		var expression = Expression;
		var match = TruncateFilter.Match(expression);

		while (match.Success)
		{
			var start = EnclosingGroupStartIndex(expression, match.Index);
			var value = expression.Substring(start, match.Index - start).Trim();
			var call = $"truncate({value}, {match.Groups[1].Value})";

			expression = expression.Substring(0, start) + call + expression.Substring(match.Index + match.Length);
			match = TruncateFilter.Match(expression);
		}

		return expression;
	}

	//Finds where the parentheses enclosing Index open, or the start of the expression when there are none.
	private static int EnclosingGroupStartIndex(string Expression, int Index)
	{
		var depth = 0;

		for (var i = Index - 1; i >= 0; i--)
		{
			if (Expression[i] == '\'')
			{
				i = i > 0 ? Expression.LastIndexOf('\'', i - 1) : -1;
				if (i < 0)
				{
					return 0;
				}
			}
			else if (Expression[i] == ')')
			{
				depth++;
			}
			else if (Expression[i] == '(' && depth-- == 0)
			{
				return i + 1;
			}
		}

		return 0;
	}

	/// <summary>
	/// Maps an expression that a UFM component covers on its own. Anything more involved stays an
	/// expression, because a component cannot be nested inside one.
	/// </summary>
	private static string? ComponentFor(string Expression, bool UseDragonflyUfmComponents)
	{
		foreach (var substitution in ComponentSubstitutions)
		{
			var argument = substitution.MatchArgument(Expression);
			if (argument is null)
			{
				continue;
			}

			var alias = UseDragonflyUfmComponents ? substitution.DragonflyAlias : substitution.BuiltInAlias;

			return alias is null ? null : UfmComponent(alias, argument);
		}

		return null;
	}

	//Matches an expression against a pattern, giving group 1 (or an empty argument) when it matches.
	private static Func<string, string?> ArgumentFrom(Regex Pattern)
	{
		return Expression =>
		{
			var match = Pattern.Match(Expression);
			return match.Success ? match.Groups[1].Value : null;
		};
	}

	/// <summary>
	/// Reads a chain of fallbacks that ends in a picked item's name, such as
	/// <c>a ? a : (b ? (b | ncRichText | truncate:true:150) : (c | ncMediaName))</c>, into the argument for
	/// dufmFirstValue: <c>a, b:150, c</c>. A trailing <c>''</c> fallback is dropped. Returns null for anything
	/// else, including chains without a picked item's name, which work as a plain expression.
	/// </summary>
	private static string? PickerFallbackChainArgument(string Expression)
	{
		var values = new List<FallbackChainEntry>();
		var remaining = WithoutEnclosingParentheses(Expression);

		var questionMark = TopLevelIndexOf(remaining, '?', 0);
		if (questionMark < 0)
		{
			return null;
		}

		while (questionMark >= 0)
		{
			var colon = TopLevelIndexOf(remaining, ':', questionMark + 1);
			if (colon < 0)
			{
				return null;
			}

			//Each step must test a property and then show that same property: a ? a : …
			var condition = FallbackChainCondition.Match(remaining.Substring(0, questionMark).Trim());
			var value = FallbackChainEntry.Parse(remaining.Substring(questionMark + 1, colon - questionMark - 1));
			if (!condition.Success || value is null || value.Alias != condition.Groups[1].Value)
			{
				return null;
			}

			values.Add(value);
			remaining = WithoutEnclosingParentheses(remaining.Substring(colon + 1));
			questionMark = TopLevelIndexOf(remaining, '?', 0);
		}

		if (remaining != "''")
		{
			var last = FallbackChainEntry.Parse(remaining);
			if (last is null)
			{
				return null;
			}

			values.Add(last);
		}

		return values.Exists(x => x.IsPickerName)
			? string.Join(", ", values)
			: null;
	}

	private static string WithoutEnclosingParentheses(string Expression)
	{
		var expression = Expression.Trim();

		//Searching from just inside the opening parenthesis finds its closing one.
		while (expression.StartsWith('(') && TopLevelIndexOf(expression, ')', 1) == expression.Length - 1)
		{
			expression = expression.Substring(1, expression.Length - 2).Trim();
		}

		return expression;
	}

	//Finds a character outside any parentheses or quoted string, relative to StartIndex.
	private static int TopLevelIndexOf(string Expression, char Character, int StartIndex)
	{
		var depth = 0;

		for (var i = StartIndex; i < Expression.Length; i++)
		{
			if (Expression[i] == '\'')
			{
				i = Expression.IndexOf('\'', i + 1);
				if (i < 0)
				{
					return -1;
				}
			}
			else if (Expression[i] == Character && depth == 0)
			{
				return i;
			}
			else if (Expression[i] == '(')
			{
				depth++;
			}
			else if (Expression[i] == ')')
			{
				depth--;
			}
		}

		return -1;
	}

	//A component with no argument still needs its colon: {alias:}
	private static string UfmComponent(string Alias, string Argument)
	{
		return Argument.Length == 0
			? $"{{{Alias}:}}"
			: $"{{{Alias}: {Argument}}}";
	}

	/// <param name="MatchArgument">Gives the component's argument when the expression matches, otherwise null.</param>
	private sealed record ComponentSubstitution(Func<string, string?> MatchArgument, string DragonflyAlias, string? BuiltInAlias);

	//One property in a chain of fallbacks, written the way dufmFirstValue reads it: alias, or alias:length.
	private sealed record FallbackChainEntry(string Alias, string? Length, bool IsPickerName)
	{
		public static FallbackChainEntry? Parse(string Expression)
		{
			var match = FallbackChainValue.Match(WithoutEnclosingParentheses(Expression));
			if (!match.Success)
			{
				return null;
			}

			return new FallbackChainEntry(
				match.Groups[1].Value,
				match.Groups[3].Success ? match.Groups[3].Value : null,
				match.Groups[2].Success);
		}

		public override string ToString()
		{
			return Length is null ? Alias : $"{Alias}:{Length}";
		}
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
