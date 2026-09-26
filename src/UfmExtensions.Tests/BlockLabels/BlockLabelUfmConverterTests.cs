namespace Dragonfly.UfmExtensions.Tests;

public class BlockLabelUfmConverterTests
{
	private const string ContentTypeName = "Rich Text";

	private const string LinkDisplayLabel = """{{Link[0]["nodeName"] ? Link[0]["nodeName"] : (Link[0]["name"] ? Link[0]["name"] : Link[0]["url"])}}""";

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("Plain label")]
	public void Label_without_expressions_needs_no_change(string? Label)
	{
		var result = BlockLabelUfmConverter.Convert(Label, ContentTypeName);

		Assert.Equal(BlockLabelConversionStatus.NoChangeNeeded, result.Status);
		Assert.Equal(Label ?? string.Empty, result.ConvertedLabel);
	}

	[Theory]
	[InlineData("{{Title}}", "${ Title }")]
	[InlineData("{{$index}}", "${ $index+1 }")]
	[InlineData("Item {{$index}}: {{Title}}", "Item ${ $index+1 }: ${ Title }")]
	[InlineData("{{'Item ' + $index}}", "${ 'Item ' + ($index+1) }")]
	[InlineData("{{$settings.umbracoNaviHide == 1 ? 'a' : 'b'}}", "${ $settings.umbracoNaviHide ? 'a' : 'b' }")]
	[InlineData("{{!!Name ? Name + '!' : Other}}", "${ Name ? Name + '!' : Other }")]
	[InlineData("{{Body | ncRichText | truncate:true:150}}", "${ truncate(Body | stripHtml, 150) }")]
	[InlineData("{{Title | truncate:true:20}}", "${ truncate(Title, 20) }")]
	[InlineData("{{'(' + Title | truncate:true:20}}", "${ truncate('(' + Title, 20) }")]
	[InlineData("{{'Literal text'}}", "Literal text")]
	public void Expressions_convert_to_ufm_expressions(string Label, string Expected)
	{
		var result = BlockLabelUfmConverter.Convert(Label, ContentTypeName);

		Assert.Equal(BlockLabelConversionStatus.Converted, result.Status);
		Assert.Equal(Expected, result.ConvertedLabel);
	}

	[Theory]
	[InlineData("{{$contentTypeName}}", "{dufmBlockContentTypeName:}")]
	[InlineData(LinkDisplayLabel, "{dufmLinkDisplay: Link}")]
	[InlineData("""{{Link[0]["url"]}}""", "{dufmLinkUrl: Link}")]
	[InlineData("""{{Link[0]["name"]}}""", "{umbLink: Link}")]
	[InlineData("{{Layout | ncNodeName}}", "{umbContentName: Layout}")]
	[InlineData("{{Image | ncMediaName}}", "{umbContentName: Image}")]
	public void Dragonfly_components_are_used_by_default(string Label, string Expected)
	{
		var result = BlockLabelUfmConverter.Convert(Label, ContentTypeName);

		Assert.Equal(BlockLabelConversionStatus.Converted, result.Status);
		Assert.Equal(Expected, result.ConvertedLabel);
	}

	[Theory]
	[InlineData("{{$contentTypeName}}", ContentTypeName)]
	[InlineData(LinkDisplayLabel, "{umbLink: Link}")]
	[InlineData("""{{Link[0]["name"]}}""", "{umbLink: Link}")]
	[InlineData("{{Layout | ncNodeName}}", "{umbContentName: Layout}")]
	public void Built_in_components_are_used_without_Dragonfly_components(string Label, string Expected)
	{
		var result = BlockLabelUfmConverter.Convert(Label, ContentTypeName, UseDragonflyUfmComponents: false);

		Assert.Equal(BlockLabelConversionStatus.Converted, result.Status);
		Assert.Equal(Expected, result.ConvertedLabel);
		Assert.DoesNotContain("dufm", result.ConvertedLabel);
	}

	[Theory]
	[InlineData("{{$index}}", "${ $index }")]
	[InlineData("{{'Item ' + $index}}", "${ 'Item ' + $index }")]
	public void Index_stays_zero_based_when_not_kept_one_based(string Label, string Expected)
	{
		var result = BlockLabelUfmConverter.Convert(Label, ContentTypeName, KeepIndexOneBased: false);

		Assert.Equal(BlockLabelConversionStatus.Converted, result.Status);
		Assert.Equal(Expected, result.ConvertedLabel);
	}

	[Fact]
	public void Link_url_needs_review_without_Dragonfly_components()
	{
		var result = BlockLabelUfmConverter.Convert("""{{Link[0]["url"]}}""", ContentTypeName, UseDragonflyUfmComponents: false);

		Assert.Equal(BlockLabelConversionStatus.NeedsManualReview, result.Status);
		Assert.Contains(result.Warnings, x => x.Contains("no built-in UFM component shows its URL"));
	}

	[Fact]
	public void Content_type_name_inside_an_expression_is_written_in_as_text()
	{
		var result = BlockLabelUfmConverter.Convert("{{$contentTypeName + ': ' + Title}}", "Editor's Pick");

		Assert.Equal(BlockLabelConversionStatus.Converted, result.Status);
		Assert.Equal(@"${ 'Editor\'s Pick' + ': ' + Title }", result.ConvertedLabel);
	}

	[Fact]
	public void Content_type_name_without_a_resolved_name_needs_review()
	{
		var result = BlockLabelUfmConverter.Convert("{{$contentTypeName + ': ' + Title}}", ContentTypeName: null);

		Assert.Equal(BlockLabelConversionStatus.NeedsManualReview, result.Status);
		Assert.Single(result.Warnings);
	}

	[Theory]
	[InlineData("{{BlockName ? BlockName : (ResourceNode | ncNodeName)}}", "{dufmFirstValue: BlockName, ResourceNode}")]
	[InlineData("{{!!BlockName? BlockName : (Image ? (Image | ncMediaName):'')}}", "{dufmFirstValue: BlockName, Image}")]
	[InlineData(
		"{{!!BlockName ? BlockName : (!!ContentTitle  ? (ContentTitle | ncRichText | truncate:true:150) : (Image| ncMediaName)) }}",
		"{dufmFirstValue: BlockName, ContentTitle:150, Image}")]
	[InlineData(
		"{{!!BlockName ? BlockName :(!!Header ? Header : (Eyebrow ? Eyebrow :(!!HtmlText ? (HtmlText | ncRichText | truncate:true:100) : (Image | ncMediaName))))}}",
		"{dufmFirstValue: BlockName, Header, Eyebrow, HtmlText:100, Image}")]
	[InlineData(
		"{{!!BlockName ? BlockName :(!!Header ? Header : (Eyebrow ? Eyebrow :(!!HtmlText ? (HtmlText | ncRichText | truncate:true:100) : '')))}}",
		"{dufmFirstValue: BlockName, Header, Eyebrow, HtmlText:100}")]
	[InlineData("{{!!Name ? Name : Other}}", "{dufmFirstValue: Name, Other}")]
	[InlineData("""{{Layout? Layout : "[No Layout Selected]"}}""", """{dufmFirstValue: Layout, "[No Layout Selected]"}""")]
	[InlineData("{{Title ? Title : 'Untitled'}}", """{dufmFirstValue: Title, "Untitled"}""")]
	public void Fallback_chain_becomes_first_value(string Label, string Expected)
	{
		var result = BlockLabelUfmConverter.Convert(Label, ContentTypeName);

		Assert.Equal(BlockLabelConversionStatus.Converted, result.Status);
		Assert.Equal(Expected, result.ConvertedLabel);
	}

	[Theory]
	[InlineData("{{Header ? Header : (Body | ncRichText | truncate:true:100)}}", "${ Header ? Header : (truncate(Body | stripHtml, 100)) }")]
	[InlineData("""{{Layout? Layout : "[No Layout Selected]"}}""", """${ Layout? Layout : "[No Layout Selected]" }""")]
	public void Fallback_chain_stays_an_expression_without_Dragonfly_components(string Label, string Expected)
	{
		var result = BlockLabelUfmConverter.Convert(Label, ContentTypeName, UseDragonflyUfmComponents: false);

		Assert.Equal(BlockLabelConversionStatus.Converted, result.Status);
		Assert.Equal(Expected, result.ConvertedLabel);
	}

	[Theory]
	[InlineData("{{$settings.umbracoNaviHide == 1 ? 'a' : 'b'}}", "${ $settings.umbracoNaviHide ? 'a' : 'b' }")]
	[InlineData("""{{$settings.BlockAnchorId? "(#" + $settings.BlockAnchorId + ")" : ''}}""", """${ $settings.BlockAnchorId? "(#" + $settings.BlockAnchorId + ")" : '' }""")]
	[InlineData("{{Title ? Title : 'Say {hi}'}}", "${ Title ? Title : 'Say {hi}' }")]
	public void Ternary_that_is_not_a_fallback_chain_stays_an_expression(string Label, string Expected)
	{
		var result = BlockLabelUfmConverter.Convert(Label, ContentTypeName);

		Assert.Equal(Expected, result.ConvertedLabel);
	}

	[Theory]
	[InlineData("{{BlockName ? Title : (ResourceNode | ncNodeName)}}")]
	[InlineData("{{BlockName ? BlockName : 'Untitled' + (ResourceNode | ncNodeName)}}")]
	public void Picked_name_inside_another_expression_needs_review(string Label)
	{
		var result = BlockLabelUfmConverter.Convert(Label, ContentTypeName);

		Assert.Equal(BlockLabelConversionStatus.NeedsManualReview, result.Status);
		Assert.Contains(result.Warnings, x => x.Contains("umbContentName"));
	}

	[Fact]
	public void Picked_name_inside_a_ternary_needs_review_without_Dragonfly_components()
	{
		var result = BlockLabelUfmConverter.Convert("{{BlockName ? BlockName : (ResourceNode | ncNodeName)}}", ContentTypeName, UseDragonflyUfmComponents: false);

		Assert.Equal(BlockLabelConversionStatus.NeedsManualReview, result.Status);
		Assert.Contains(result.Warnings, x => x.Contains("umbContentName"));
		Assert.DoesNotContain(result.Warnings, x => x.Contains("dufm"));
	}

	[Fact]
	public void Components_and_expressions_combine_in_one_label()
	{
		var result = BlockLabelUfmConverter.Convert("{{Title}} - {{$contentTypeName}}", ContentTypeName);

		Assert.Equal("${ Title } - {dufmBlockContentTypeName:}", result.ConvertedLabel);
	}

	[Fact]
	public void Original_label_is_kept_on_the_result()
	{
		const string label = "{{Title}}";

		var result = BlockLabelUfmConverter.Convert(label, ContentTypeName);

		Assert.Equal(label, result.OriginalLabel);
	}
}
