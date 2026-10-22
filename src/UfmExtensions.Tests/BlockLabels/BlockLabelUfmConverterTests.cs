namespace Dragonfly.UfmExtensions.Tests;

public class BlockLabelUfmConverterTests
{
	private const string ContentTypeName = "Rich Text";

	private const string FirstBlockLinkDisplayLabel = """{{ value["contentData"][0]["Link"][0]["nodeName"] ? value["contentData"][0]["Link"][0]["nodeName"]: value["contentData"][0]["Link"][0]["name"]}}""";

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
	[InlineData("{{Title | truncate:true:20:'...'}}", "${ truncate(Title, 20, '...') }")]
	[InlineData("{{Title | limitTo:20}}", "${ truncate(Title, 20, '') }")]
	[InlineData("{{Body | ncRichText | limitTo:50}}", "${ truncate(Body | stripHtml, 50, '') }")]
	[InlineData("{{'Intro: ' + (Body | limitTo:50)}}", "${ 'Intro: ' + (truncate(Body, 50, '')) }")]
	[InlineData("{{Title | uppercase}}", "${ Title | uppercase }")]
	[InlineData("{{Title || 'Untitled'}}", "${ Title || 'Untitled' }")]
	[InlineData("{{'a | b' + Title}}", "${ 'a | b' + Title }")]
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
	[InlineData("{{Layout | ncNodeName}}", "{dufmFirstValue: Layout}")]
	[InlineData("{{Image | ncMediaName}}", "{dufmFirstValue: Image}")]
	[InlineData("{{ value | ncNodeName }}", "{dufmFirstValue: value}")]
	[InlineData("{{ value | umbCmsJoinArray:', ' |  ncNodeName}}", "{dufmFirstValue: value}")]
	[InlineData(FirstBlockLinkDisplayLabel, "{dufmFirstBlockValue: value, Link}")]
	[InlineData("""{{value["contentData"][0]["Link"][0]["url"]}}""", "{dufmFirstBlockValue: value, Link, Url}")]
	[InlineData("""{{value["contentData"][0]["Title"]}}""", "{dufmFirstBlockValue: value, Title}")]
	[InlineData("{{Link[0].url}}", "{dufmLinkUrl: Link}")]
	[InlineData("{{Link[0]['url']}}", "{dufmLinkUrl: Link}")]
	[InlineData("{{ Link[0] . name }}", "{umbLink: Link}")]
	[InlineData("{{Link[0].nodeName ? Link[0].nodeName : (Link[0].name ? Link[0].name : Link[0].url)}}", "{dufmLinkDisplay: Link}")]
	[InlineData("{{ page[0].name.length > 0 ? page[0].name : page[0].nodeName }}", "{umbLink: page}")]
	[InlineData("""{{page[0]["name"] ? page[0]["name"] : page[0]["nodeName"]}}""", "{umbLink: page}")]
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
	[InlineData("{{Image | ncMediaName}}", "{umbContentName: Image}")]
	[InlineData("{{Link[0].name}}", "{umbLink: Link}")]
	[InlineData("{{ page[0].name.length > 0 ? page[0].name : page[0].nodeName }}", "{umbLink: page}")]
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
	[InlineData("{{Title ? Title : (Body | limitTo:50)}}", "{dufmFirstValue: Title, Body:50}")]
	[InlineData("{{title.length > 0 ? title : subtitle}}", "{dufmFirstValue: title, subtitle}")]
	[InlineData("{{title.length ? title : 'Untitled'}}", """{dufmFirstValue: title, "Untitled"}""")]
	[InlineData("{{ title.length > 0 ? title : primaryLink[0].name }}", "{dufmFirstValue: title, primaryLink.name}")]
	[InlineData("""{{Link[0]["name"] ? Link[0]["name"] : Title | limitTo:20}}""", "{dufmFirstValue: Link.name:20, Title:20}")]
	[InlineData("{{Link[0].name.length > 0 ? Link[0].name : 'No link'}}", """{dufmFirstValue: Link.name, "No link"}""")]
	[InlineData("{{!!caption ? caption : videoUrl | limitTo:100}}", "{dufmFirstValue: caption:100, videoUrl:100}")]
	[InlineData("{{(Title ? Title : Body) | truncate:true:100}}", "{dufmFirstValue: Title:100, Body:100}")]
	[InlineData("{{Title ? Title : (Body | ncRichText | truncate:true:20) | limitTo:100}}", "{dufmFirstValue: Title:100, Body:20}")]
	public void Fallback_chain_becomes_first_value(string Label, string Expected)
	{
		var result = BlockLabelUfmConverter.Convert(Label, ContentTypeName);

		Assert.Equal(BlockLabelConversionStatus.Converted, result.Status);
		Assert.Equal(Expected, result.ConvertedLabel);
	}

	[Theory]
	[InlineData("{{Header ? Header : (Body | ncRichText | truncate:true:100)}}", "${ Header ? Header : (truncate(Body | stripHtml, 100)) }")]
	[InlineData("""{{Layout? Layout : "[No Layout Selected]"}}""", """${ Layout? Layout : "[No Layout Selected]" }""")]
	[InlineData("{{!!caption ? caption : videoUrl | limitTo:100}}", "${ truncate(caption ? caption : videoUrl, 100, '') }")]
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
	[InlineData("{{ value | umbCmsJoinArray:' / ' | ncNodeName }}")]
	public void Picked_name_inside_another_expression_needs_review(string Label)
	{
		var result = BlockLabelUfmConverter.Convert(Label, ContentTypeName);

		Assert.Equal(BlockLabelConversionStatus.NeedsManualReview, result.Status);
		Assert.Contains(result.Warnings, x => x.Contains("{dufmFirstValue: alias}"));
	}

	[Theory]
	[InlineData("""{{value["contentData"][1]["Link"][0]["url"]}}""")]
	[InlineData("""{{value["contentData"][0]["Link"][0]["nodeName"] ? value["contentData"][0]["Other"][0]["nodeName"] : value["contentData"][0]["Link"][0]["name"]}}""")]
	public void First_block_value_variations_that_are_not_covered_need_review(string Label)
	{
		var result = BlockLabelUfmConverter.Convert(Label, ContentTypeName);

		Assert.Equal(BlockLabelConversionStatus.NeedsManualReview, result.Status);
	}

	[Theory]
	[InlineData("{{Date | date:'dd MMM yyyy'}}", "'| date'")]
	[InlineData("{{Price | currency}}", "'| currency'")]
	[InlineData("{{Count | number:2}}", "'| number'")]
	[InlineData("{{Title | umbCmsTitleCase}}", "'| umbCmsTitleCase'")]
	[InlineData("{{Title | limitTo:-20}}", "'| limitTo'")]
	[InlineData("{{Title | limitTo:20:5}}", "'| limitTo'")]
	[InlineData("{{'On ' + (Date | date:'d MMM')}}", "'| date'")]
	public void Filter_UFM_does_not_have_needs_review(string Label, string Filter)
	{
		var result = BlockLabelUfmConverter.Convert(Label, ContentTypeName);

		Assert.Equal(BlockLabelConversionStatus.NeedsManualReview, result.Status);
		Assert.Contains(result.Warnings, x => x.StartsWith(Filter) && x.Contains("not a UFM filter"));
	}

	[Fact]
	public void Piped_filter_with_an_argument_needs_review()
	{
		var result = BlockLabelUfmConverter.Convert("{{Title | truncate:20}}", ContentTypeName);

		Assert.Equal(BlockLabelConversionStatus.NeedsManualReview, result.Status);
		Assert.Contains(result.Warnings, x => x.Contains("passes an argument to a piped filter"));
	}

	[Fact]
	public void Joined_picker_names_warn_about_udi_lists_without_Dragonfly_components()
	{
		var result = BlockLabelUfmConverter.Convert("{{ value | umbCmsJoinArray:', ' | ncNodeName }}", ContentTypeName, UseDragonflyUfmComponents: false);

		Assert.Equal(BlockLabelConversionStatus.NeedsManualReview, result.Status);
		Assert.Equal("{umbContentName: value}", result.ConvertedLabel);
		Assert.Contains(result.Warnings, x => x.Contains("udi") && x.Contains("{dufmFirstValue: value}"));
	}

	[Fact]
	public void Link_title_in_a_fallback_chain_needs_review_without_Dragonfly_components()
	{
		var result = BlockLabelUfmConverter.Convert("{{ title.length > 0 ? title : primaryLink[0].name }}", ContentTypeName, UseDragonflyUfmComponents: false);

		Assert.Equal(BlockLabelConversionStatus.NeedsManualReview, result.Status);
	}

	[Fact]
	public void Fallback_chain_step_showing_another_link_property_than_it_tests_needs_review()
	{
		var result = BlockLabelUfmConverter.Convert("{{Link[0].name ? Link[0].url : Title}}", ContentTypeName);

		Assert.Equal(BlockLabelConversionStatus.NeedsManualReview, result.Status);
	}

	[Fact]
	public void First_block_value_needs_review_without_Dragonfly_components()
	{
		var result = BlockLabelUfmConverter.Convert(FirstBlockLinkDisplayLabel, ContentTypeName, UseDragonflyUfmComponents: false);

		Assert.Equal(BlockLabelConversionStatus.NeedsManualReview, result.Status);
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
