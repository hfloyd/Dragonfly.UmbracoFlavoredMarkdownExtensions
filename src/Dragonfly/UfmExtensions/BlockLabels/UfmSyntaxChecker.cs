namespace Dragonfly.UfmExtensions;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Checks a UFM label for common mistakes, such as those left behind by AngularJS labels or made while
/// rewriting them by hand. The checks are pattern based, so a label with no issues can still fail to render.
/// </summary>
public static class UfmSyntaxChecker
{
	//The same pattern UFM uses to find a ${ } expression, so this sees the expressions UFM will evaluate.
	private static readonly Regex UfmExpression = new(@"\$\{((?:[^{}]|\{[^{}]*\})*)\}", RegexOptions.Compiled);
	private static readonly Regex ExpressionStart = new(@"\$\{", RegexOptions.Compiled);
	private static readonly Regex AngularExpression = new(@"\{\{.*?\}\}", RegexOptions.Compiled | RegexOptions.Singleline);

	/// <summary>
	/// Mistakes found inside a ${ } expression, each with the message explaining it. To check for another
	/// mistake, add a row here.
	/// </summary>
	private static readonly ExpressionRule[] ExpressionRules =
	[
		new(new Regex(@"^\s*(\w+)\s*:", RegexOptions.Compiled),
			Match => $"'{Match.Groups[1].Value}' is a UFM component written as an expression: remove the $ so it reads {{{Match.Groups[1].Value}: …}}."),
		new(new Regex(@"\{\s*(\w+)\s*:", RegexOptions.Compiled),
			Match => $"The UFM component '{{{Match.Groups[1].Value}: …}}' is nested inside an expression, which UFM cannot render: move it outside the ${{ }}."),
		new(new Regex(@"\|\s*(\w+):([^|)]*)", RegexOptions.Compiled),
			Match => $"'| {Match.Groups[1].Value}:{Match.Groups[2].Value.Trim()}' passes an argument to a piped filter, which UFM expressions cannot do: inside parentheses it is a parse error, and elsewhere the argument is ignored. Call the filter instead, e.g. {Match.Groups[1].Value}(value, {Match.Groups[2].Value.Trim()})."),
		new(new Regex(@"!!", RegexOptions.Compiled),
			_ => "'!!' is a parse error in UFM expressions: remove it, since an empty value is already false."),
		new(new Regex(@"\|\s*nc(?:NodeName|MediaName)\b", RegexOptions.Compiled),
			_ => "ncNodeName and ncMediaName are AngularJS filters with no UFM equivalent: use {umbContentName: alias}, or {dufmFirstValue: alias, alias} for a chain of fallbacks."),
		new(new Regex(@"\|\s*ncRichText\b", RegexOptions.Compiled),
			_ => "ncRichText is an AngularJS filter with no UFM equivalent: use stripHtml."),
		new(new Regex(@"(\$settings\.\w+)\s*==\s*1\b", RegexOptions.Compiled),
			Match => $"'{Match.Value}' is never true, because settings toggles are real booleans in v14+: use {Match.Groups[1].Value} on its own."),
		new(new Regex(@"\$contentTypeName\b", RegexOptions.Compiled),
			_ => "$contentTypeName does not exist in UFM: use {dufmBlockContentTypeName:} outside the expression."),
		new(new Regex(@"\w\s*\[\s*\d+\s*\]", RegexOptions.Compiled),
			_ => "Reading into a picker value by index is AngularJS-era syntax: use {dufmLinkDisplay: alias}, {dufmLinkUrl: alias}, {dufmLinkUrlWithAnchor: alias} or {umbLink: alias}."),
	];

	/// <summary>
	/// Checks a single label.
	/// </summary>
	/// <param name="Label">The label to check, as it would be entered in a block's Label field.</param>
	public static UfmSyntaxCheck Check(string? Label)
	{
		var check = new UfmSyntaxCheck { Label = Label ?? string.Empty };

		foreach (Match angular in AngularExpression.Matches(check.Label))
		{
			check.Issues.Add(new UfmSyntaxIssue(angular.Value,
				$"'{angular.Value}' is AngularJS syntax, which UFM shows as plain text: use ${{ … }}, or convert the label with the block label converter."));
		}

		var expressions = UfmExpression.Matches(check.Label);

		if (ExpressionStart.Matches(check.Label).Count > expressions.Count)
		{
			check.Issues.Add(new UfmSyntaxIssue("${",
				"A ${ is not closed, or its expression holds braces nested more than one level deep, so UFM shows it as plain text."));
		}

		foreach (Match expression in expressions)
		{
			foreach (var rule in ExpressionRules)
			{
				var match = rule.Pattern.Match(expression.Groups[1].Value);
				if (match.Success)
				{
					check.Issues.Add(new UfmSyntaxIssue(expression.Value, rule.Message(match)));
				}
			}
		}

		return check;
	}

	private sealed record ExpressionRule(Regex Pattern, Func<Match, string> Message);
}

public class UfmSyntaxCheck
{
	public string Label { get; set; } = string.Empty;

	public int IssueCount => Issues.Count;

	public List<UfmSyntaxIssue> Issues { get; set; } = [];
}

/// <param name="Text">The part of the label the issue was found in.</param>
/// <param name="Message">What is wrong, and how to fix it.</param>
public record UfmSyntaxIssue(string Text, string Message);
