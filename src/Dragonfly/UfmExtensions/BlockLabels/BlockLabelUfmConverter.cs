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
	private static readonly Regex TruncateFilter = new(
		@"\|\s*truncate\s*:\s*(?:true|false)\s*:\s*(?<length>\d+)(?:\s*:\s*(?<tail>'[^']*'|""[^""]*""))?",
		RegexOptions.Compiled);
	//limitTo:N:begin and a negative limitTo are not matched, so they are reported as unknown filters instead.
	private static readonly Regex LimitToFilter = new(@"\|\s*limitTo\s*:\s*(?<length>\d+)(?!\d)(?!\s*:)", RegexOptions.Compiled);
	//A length filter applying to the whole of a fallback chain, e.g. a ? a : b | limitTo:100
	private static readonly Regex TrailingLengthFilter = new(
		@"\|\s*(?:limitTo\s*:\s*(?<length>\d+)|truncate\s*:\s*(?:true|false)\s*:\s*(?<length>\d+))\s*$",
		RegexOptions.Compiled);
	private static readonly Regex PipedFilter = new(@"(?<!\|)\|(?!\|)\s*(\w+)(\s*:)?", RegexOptions.Compiled);
	private static readonly Regex QuotedString = new(@"'(?:[^'\\]|\\.)*'|""(?:[^""\\]|\\.)*""", RegexOptions.Compiled);
	private static readonly Regex ArrayIndex = new(@"\[\s*\d+\s*\]", RegexOptions.Compiled);
	private static readonly Regex QuotedLiteral = new(@"^'[^']*'$", RegexOptions.Compiled);
	private static readonly Regex DoubleNegation = new(@"!!", RegexOptions.Compiled);
	private static readonly Regex BlockIndex = new(@"\$index\b", RegexOptions.Compiled);

	//AngularJS expressions that a UFM component covers on its own. Group 1, where present, is the component's argument.
	private static readonly Regex StandaloneContentTypeName = new(@"^\$contentTypeName$", RegexOptions.Compiled);
	private static readonly Regex LinkName = new(@"^(\w+)\[0\]\[""name""\]$", RegexOptions.Compiled);
	private static readonly Regex LinkUrl = new(@"^(\w+)\[0\]\[""url""\]$", RegexOptions.Compiled);
	//The link's title, else the picked item's name, which is what umbLink shows. The test may read the title's length.
	private static readonly Regex LinkTitleOrItemName = new(
		@"^(\w+)\[0\]\[""name""\](?:\[""length""\]\s*>\s*0)?\s*\?\s*\1\[0\]\[""name""\]\s*:\s*\1\[0\]\[""nodeName""\]$",
		RegexOptions.Compiled);
	private static readonly Regex LinkDisplay = new(
		@"^(\w+)\[0\]\[""nodeName""\]\s*\?\s*\1\[0\]\[""nodeName""\]\s*:\s*\(?\s*\1\[0\]\[""name""\]\s*\?\s*\1\[0\]\[""name""\]\s*:\s*\1\[0\]\[""url""\]\s*\)?$",
		RegexOptions.Compiled);
	private static readonly Regex PickerName = new(@"^(\w+)\s*\|\s*nc(?:Node|Media)Name$", RegexOptions.Compiled);
	private static readonly Regex JoinedPickerNames = new(
		@"^(\w+)\s*\|\s*umbCmsJoinArray\s*:\s*(?:', '|"", "")\s*\|\s*nc(?:Node|Media)Name$",
		RegexOptions.Compiled);

	//A property of the first block in a Block List or Block Grid value, e.g. value["contentData"][0]["Link"]. Group "blocks" is the
	//property holding the blocks and "alias" the block property; "prefix" repeats the whole path, so a backreference can check it is the same one.
	private const string FirstBlockValuePath = @"(?<prefix>(?<blocks>\w+)\[""contentData""\]\[0\]\[""(?<alias>\w+)""\])";
	private static readonly Regex FirstBlockValue = new($"^{FirstBlockValuePath}$", RegexOptions.Compiled);
	private static readonly Regex FirstBlockLinkDisplay = new(
		$@"^{FirstBlockValuePath}\[0\]\[""nodeName""\]\s*\?\s*\k<prefix>\[0\]\[""nodeName""\]\s*:\s*\k<prefix>\[0\]\[""name""\]$",
		RegexOptions.Compiled);
	private static readonly Regex FirstBlockLinkUrl = new($@"^{FirstBlockValuePath}\[0\]\[""url""\]$", RegexOptions.Compiled);

	//One value in a chain of fallbacks. Group 1 is the property alias, group "linkName" a read of the first link's title,
	//and group "length" a truncate or limitTo length.
	private static readonly Regex FallbackChainValue = new(
		@"^(\w+)(?<linkName>\[0\]\[""name""\])?(?:\s*\|\s*nc(?:RichText|NodeName|MediaName))?(?:\s*\|\s*(?:truncate\s*:\s*(?:true|false)\s*:\s*(?<length>\d+)|limitTo\s*:\s*(?<length>\d+)))?$",
		RegexOptions.Compiled);
	//Text a chain of fallbacks ends in. Group 2 is the text, which cannot hold quotes or braces, since it becomes a quoted component argument.
	private static readonly Regex FallbackChainText = new(@"^(['""])([^'""{}]*)\1$", RegexOptions.Compiled);
	//A step's test: a ? …, !!a ? …, or a.length > 0 ? …, which is the same test for text. The property may be a link's title,
	//a[0]["name"], whose length reads as a[0]["name"]["length"] once its property reads are in bracket form.
	private static readonly Regex FallbackChainCondition = new(
		@"^(?:!!)?(\w+)(?<linkName>\[0\]\[""name""\])?(?:(?:\.length|\[""length""\])(?:\s*>\s*0)?)?$",
		RegexOptions.Compiled);

	//Property reads that AngularJS accepted in several forms: x[0].name and x[0]['name'] both mean x[0]["name"].
	private static readonly Regex DotPropertyRead = new(@"\]\s*\.\s*(\w+)", RegexOptions.Compiled);
	private static readonly Regex SingleQuotedPropertyRead = new(@"\[\s*'(\w+)'\s*\]", RegexOptions.Compiled);

	/// <summary>
	/// The filters UFM expressions support. Any other piped filter, such as AngularJS's date, currency or number,
	/// would break the label, so it is left for manual review.
	/// </summary>
	private static readonly HashSet<string> UfmFilterAliases =
		["bytes", "fallback", "lowercase", "stripHtml", "titleCase", "truncate", "uppercase", "wordLimit"];

	/// <summary>
	/// The UFM component each AngularJS expression becomes: one alias for when the Dragonfly UFM components are
	/// available, and one using only Umbraco's built-in components. A null built-in alias means there is no
	/// built-in equivalent, so the expression is converted as a plain ${ } expression instead.
	/// </summary>
	private static readonly ComponentSubstitution[] ComponentSubstitutions =
	[
		new(ArgumentFrom(StandaloneContentTypeName), DragonflyAlias: "dufmBlockContentTypeName", BuiltInAlias: null),
		new(ArgumentFrom(LinkTitleOrItemName), DragonflyAlias: "umbLink", BuiltInAlias: "umbLink"),
		new(ArgumentFrom(LinkDisplay), DragonflyAlias: "dufmLinkDisplay", BuiltInAlias: "umbLink"),
		new(ArgumentFrom(LinkUrl), DragonflyAlias: "dufmLinkUrl", BuiltInAlias: null),
		new(ArgumentFrom(LinkName), DragonflyAlias: "umbLink", BuiltInAlias: "umbLink"),
		//umbContentName ignores udi strings and lists picked items in its own order, not the order they were picked as ncNodeName
		//did; dufmFirstValue reads udi strings and keeps the picked order.
		new(ArgumentFrom(PickerName), DragonflyAlias: "dufmFirstValue", BuiltInAlias: "umbContentName"),
		//A list of picked items, such as a Contentment Data List, holds udi strings that umbContentName ignores; dufmFirstValue reads them.
		new(ArgumentFrom(JoinedPickerNames), DragonflyAlias: "dufmFirstValue", BuiltInAlias: "umbContentName"),
		new(FallbackChainArgument, DragonflyAlias: "dufmFirstValue", BuiltInAlias: null),
		new(FirstBlockValueArgument, DragonflyAlias: "dufmFirstBlockValue", BuiltInAlias: null),
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
			//umbContentName ignores the udi strings a list of picked items, such as a Contentment Data List, is stored as.
			var joinedPickerNames = JoinedPickerNames.Match(expression);
			if (!UseDragonflyUfmComponents && joinedPickerNames.Success)
			{
				Conversion.Warnings.Add($"'{expression}' becomes {component}, which shows nothing when '{joinedPickerNames.Groups[1].Value}' holds a list of udi strings, such as a Contentment Data List: check the column shows names, or convert with UseDragonflyUfmComponents=true to use {{dufmFirstValue: {joinedPickerNames.Groups[1].Value}}}.");
			}

			return component;
		}

		//UFM components cannot be nested inside a ${ } expression, so these labels need rewriting by hand.
		if (expression.Contains("ncNodeName") || expression.Contains("ncMediaName"))
		{
			Conversion.Warnings.Add(UseDragonflyUfmComponents
				? $"'{expression}' resolves a picked item's name: use {{dufmFirstValue: alias}}, or {{dufmFirstValue: alias, alias}} for a chain of fallbacks."
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
		expression = WithFilterCalls(expression, TruncateFilter,
			Filter => Filter.Groups["tail"].Success ? $", {Filter.Groups["length"].Value}, {Filter.Groups["tail"].Value}" : $", {Filter.Groups["length"].Value}");
		//limitTo cut the text without an ellipsis, which truncate adds unless its tail is empty.
		expression = WithFilterCalls(expression, LimitToFilter, Filter => $", {Filter.Groups["length"].Value}, ''");

		//The expression parser rejects '!!'; an empty value is falsy on its own, so the test still holds.
		expression = DoubleNegation.Replace(expression, string.Empty);

		AddUnsupportedFilterWarnings(expression, Conversion);

		//A bare string literal does not need an expression wrapper.
		return QuotedLiteral.IsMatch(expression)
			? expression.Substring(1, expression.Length - 2)
			: $"${{ {expression} }}";
	}

	/// <summary>
	/// Rewrites a piped filter with arguments as a truncate call: <c>value | truncate:true:N</c> as <c>truncate(value, N)</c>,
	/// and <c>value | limitTo:N</c> as <c>truncate(value, N, '')</c>. The UFM expression parser cannot pass arguments to a
	/// piped filter: inside parentheses <c>| truncate:N</c> is a parse error, and elsewhere the length is ignored. As in
	/// AngularJS, the filter applies to everything before it within the same parentheses.
	/// </summary>
	/// <param name="Arguments">Gives the call's arguments after the value, including the leading comma.</param>
	private static string WithFilterCalls(string Expression, Regex Filter, Func<Match, string> Arguments)
	{
		var expression = Expression;
		var match = Filter.Match(expression);

		while (match.Success)
		{
			var start = EnclosingGroupStartIndex(expression, match.Index);
			var value = expression.Substring(start, match.Index - start).Trim();
			var call = $"truncate({value}{Arguments(match)})";

			expression = expression.Substring(0, start) + call + expression.Substring(match.Index + match.Length);
			match = Filter.Match(expression);
		}

		return expression;
	}

	/// <summary>
	/// Warns about any piped filter UFM does not have, such as AngularJS's date, currency or number, and any filter
	/// still passed an argument, which UFM cannot do. ncNodeName and ncMediaName already have their own warning.
	/// </summary>
	private static void AddUnsupportedFilterWarnings(string Expression, BlockLabelConversion Conversion)
	{
		//A '|' inside a string is text, not a filter.
		var expression = QuotedString.Replace(Expression, "''");

		foreach (Match filter in PipedFilter.Matches(expression))
		{
			var alias = filter.Groups[1].Value;

			if (alias is "ncNodeName" or "ncMediaName")
			{
				continue;
			}

			if (!UfmFilterAliases.Contains(alias))
			{
				Conversion.Warnings.Add($"'| {alias}' is not a UFM filter, so the label would not render: UFM's filters are {string.Join(", ", UfmFilterAliases)}.");
			}
			else if (filter.Groups[2].Success)
			{
				Conversion.Warnings.Add($"'| {alias}:' passes an argument to a piped filter, which UFM expressions cannot do: call it instead, e.g. {alias}(value, …).");
			}
		}
	}

	//Finds where the parentheses enclosing Index open, or the start of the expression when there are none.
	private static int EnclosingGroupStartIndex(string Expression, int Index)
	{
		var depth = 0;

		for (var i = Index - 1; i >= 0; i--)
		{
			if (Expression[i] is '\'' or '"')
			{
				i = i > 0 ? Expression.LastIndexOf(Expression[i], i - 1) : -1;
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
		var expression = WithBracketPropertyReads(Expression);

		foreach (var substitution in ComponentSubstitutions)
		{
			var argument = substitution.MatchArgument(expression);
			if (argument is null)
			{
				continue;
			}

			var alias = UseDragonflyUfmComponents ? substitution.DragonflyAlias : substitution.BuiltInAlias;

			return alias is null ? null : UfmComponent(alias, argument);
		}

		return null;
	}

	/// <summary>
	/// Rewrites the property reads that follow an index in the bracket form the component patterns expect:
	/// <c>Link[0].name</c> and <c>Link[0]['name']</c> become <c>Link[0]["name"]</c>. Used only to find a component,
	/// so an expression that stays an expression keeps its original form.
	/// </summary>
	private static string WithBracketPropertyReads(string Expression)
	{
		var expression = SingleQuotedPropertyRead.Replace(Expression, @"[""$1""]");

		//Each rewrite can expose the next read in a chain, e.g. Link[0].name.length.
		string previous;
		do
		{
			previous = expression;
			expression = DotPropertyRead.Replace(expression, @"][""$1""]");
		}
		while (expression != previous);

		return expression;
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
	/// Reads a property of the first block in a blocks value into the arguments for dufmFirstBlockValue:
	/// <c>value["contentData"][0]["Link"]</c> becomes <c>value, Link</c>, a link's name or node name
	/// <c>value["contentData"][0]["Link"][0]["nodeName"] ? … : …["name"]</c> the same, and a link's URL
	/// <c>…["Link"][0]["url"]</c> <c>value, Link, Url</c>. Returns null for anything else.
	/// </summary>
	private static string? FirstBlockValueArgument(string Expression)
	{
		foreach (var (pattern, outputType) in new[] { (FirstBlockValue, ""), (FirstBlockLinkDisplay, ""), (FirstBlockLinkUrl, ", Url") })
		{
			var match = pattern.Match(Expression);
			if (match.Success)
			{
				return $"{match.Groups["blocks"].Value}, {match.Groups["alias"].Value}{outputType}";
			}
		}

		return null;
	}

	/// <summary>
	/// Reads a chain of fallbacks, such as
	/// <c>a ? a : (b ? (b | ncRichText | truncate:true:150) : (c | ncMediaName))</c>, into the argument for
	/// dufmFirstValue: <c>a, b:150, c</c>. A chain ending in text passes it on as dufmFirstValue's fallback text,
	/// and a trailing <c>''</c> is dropped. A length filter after the whole chain, <c>a ? a : b | limitTo:100</c>,
	/// applies to every value, as AngularJS filters apply to everything before them: <c>a:100, b:100</c>.
	/// Returns null for anything else.
	/// </summary>
	private static string? FallbackChainArgument(string Expression)
	{
		var values = new List<string>();
		var remaining = WithoutEnclosingParentheses(Expression);

		int? chainLength = null;
		var trailingFilter = TrailingLengthFilter.Match(remaining);
		if (trailingFilter.Success && IsTopLevel(remaining, trailingFilter.Index))
		{
			chainLength = int.Parse(trailingFilter.Groups["length"].Value);
			remaining = WithoutEnclosingParentheses(remaining.Substring(0, trailingFilter.Index));
		}

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
			if (!condition.Success || value is null || value.Alias != condition.Groups[1].Value || value.LinkName != condition.Groups["linkName"].Success)
			{
				return null;
			}

			values.Add(value.WithMaxLength(chainLength).ToString());
			remaining = WithoutEnclosingParentheses(remaining.Substring(colon + 1));
			questionMark = TopLevelIndexOf(remaining, '?', 0);
		}

		var text = FallbackChainText.Match(remaining);
		if (text.Success)
		{
			if (text.Groups[2].Value.Length > 0)
			{
				values.Add($"\"{text.Groups[2].Value}\"");
			}
		}
		else
		{
			var last = FallbackChainEntry.Parse(remaining);
			if (last is null)
			{
				return null;
			}

			values.Add(last.WithMaxLength(chainLength).ToString());
		}

		return string.Join(", ", values);
	}

	//Whether Index sits outside any parentheses or quoted string.
	private static bool IsTopLevel(string Expression, int Index)
	{
		var depth = 0;

		for (var i = 0; i < Index; i++)
		{
			if (Expression[i] is '\'' or '"')
			{
				i = Expression.IndexOf(Expression[i], i + 1);
				if (i < 0 || i >= Index)
				{
					return false;
				}
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

		return depth == 0;
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
			if (Expression[i] is '\'' or '"')
			{
				i = Expression.IndexOf(Expression[i], i + 1);
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

	//One property in a chain of fallbacks, written the way dufmFirstValue reads it: alias, alias.name for a link's title,
	//and either followed by :length.
	private sealed record FallbackChainEntry(string Alias, bool LinkName, int? Length)
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
				match.Groups["linkName"].Success,
				match.Groups["length"].Success ? int.Parse(match.Groups["length"].Value) : null);
		}

		//A length applying to the whole chain shortens this value too, keeping the shorter of the two.
		public FallbackChainEntry WithMaxLength(int? MaxLength)
		{
			return MaxLength is null || Length <= MaxLength ? this : this with { Length = MaxLength };
		}

		public override string ToString()
		{
			var entry = LinkName ? $"{Alias}.name" : Alias;

			return Length is null ? entry : $"{entry}:{Length}";
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
