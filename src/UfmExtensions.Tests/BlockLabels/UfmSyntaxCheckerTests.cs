namespace Dragonfly.UfmExtensions.Tests;

public class UfmSyntaxCheckerTests
{
	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("Plain label")]
	[InlineData("${ $settings.umbracoNaviHide ? '🚫' : '🟢' } ${ $index+1 } {dufmBlockContentTypeName:}: {dufmFirstValue: BlockName, ContentTitle:150, RichTextContent:150}")]
	[InlineData("${ BlockName ? BlockName : (ContentTitle ? (truncate(ContentTitle | stripHtml, 150)) : '') } ${ Layout ? Layout : \"[No Layout Selected]\" }")]
	[InlineData("${ $settings.BlockAnchorId ? \"(#\" + $settings.BlockAnchorId + \")\" : '' }")]
	[InlineData("${ Body | stripHtml }")]
	public void Valid_label_has_no_issues(string? Label)
	{
		var result = UfmSyntaxChecker.Check(Label);

		Assert.Empty(result.Issues);
	}

	[Theory]
	[InlineData("${ dufmFirstValue: BlockName, ContentTitle:150 }", "remove the $")]
	[InlineData("${ BlockName ? BlockName : {umbContentName: Image} }", "nested inside an expression")]
	[InlineData("${ BlockName ? BlockName : (ContentTitle | stripHtml | truncate:150) }", "truncate(value, 150)")]
	[InlineData("${ Body | truncate:150 }", "piped filter")]
	[InlineData("${ !!Name ? Name : Other }", "'!!'")]
	[InlineData("${ BlockName ? BlockName : (ResourceNode | ncNodeName) }", "umbContentName")]
	[InlineData("${ Body | ncRichText }", "stripHtml")]
	[InlineData("${ $settings.umbracoNaviHide == 1 ? 'a' : 'b' }", "$settings.umbracoNaviHide on its own")]
	[InlineData("${ $contentTypeName }", "{dufmBlockContentTypeName:}")]
	[InlineData("""${ Link[0]["name"] }""", "dufmLinkDisplay")]
	[InlineData("{{Title}}", "AngularJS syntax")]
	[InlineData("${ Title ", "not closed")]
	public void Common_mistake_is_reported(string Label, string ExpectedMessagePart)
	{
		var result = UfmSyntaxChecker.Check(Label);

		Assert.Contains(result.Issues, x => x.Message.Contains(ExpectedMessagePart));
	}

	[Fact]
	public void Issue_names_the_expression_it_was_found_in()
	{
		var result = UfmSyntaxChecker.Check("${ Title } ${ !!Name }");

		var issue = Assert.Single(result.Issues);
		Assert.Equal("${ !!Name }", issue.Text);
	}

	[Fact]
	public void Ternary_after_a_filter_is_not_taken_for_a_filter_argument()
	{
		var result = UfmSyntaxChecker.Check("${ Body ? Body | stripHtml : 'None' }");

		Assert.Empty(result.Issues);
	}
}
