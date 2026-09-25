namespace Dragonfly.UfmExtensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

/// <summary>
/// Rewrites the AngularJS block labels on every Block List and Block Grid datatype as Umbraco Flavored Markdown.
/// Run with DryRun to report what would change without saving anything.
/// </summary>
public sealed class BlockLabelUfmMigrator(
	IDataTypeService dataTypeService,
	IContentTypeService contentTypeService,
	IConfigurationEditorJsonSerializer configurationSerializer,
	ILogger<BlockLabelUfmMigrator> logger)
{
	private static readonly string[] BlockEditorAliases = ["Umbraco.BlockList", "Umbraco.BlockGrid"];

	public async Task<BlockLabelUfmReport> RunAsync(bool DryRun, Guid UserKey, bool UseDragonflyUfmComponents = true)
	{
		var report = new BlockLabelUfmReport { DryRun = DryRun, UseDragonflyUfmComponents = UseDragonflyUfmComponents };

		var dataTypes = (await dataTypeService.GetAllAsync())
			.Where(x => BlockEditorAliases.Contains(x.EditorAlias))
			.ToList();

		foreach (var dataType in dataTypes)
		{
			report.DataTypesScanned++;

			var configuration = JsonNode.Parse(configurationSerializer.Serialize(dataType.ConfigurationData))?.AsObject();
			if (configuration is null)
			{
				continue;
			}

			var changed = ConvertBlocks(dataType, configuration, report);

			if (!changed || DryRun)
			{
				continue;
			}

			var updated = configurationSerializer.Deserialize<Dictionary<string, object>>(configuration.ToJsonString());
			if (updated is null)
			{
				logger.LogWarning("BlockLabelUfmMigrator : Unable to rebuild the configuration for datatype '{DataTypeName}'", dataType.Name);
				continue;
			}

			dataType.ConfigurationData = updated;
			await dataTypeService.UpdateAsync(dataType, UserKey);
			report.DataTypesSaved++;

			logger.LogInformation("BlockLabelUfmMigrator : Updated block labels on datatype '{DataTypeName}'", dataType.Name);
		}

		return report;
	}

	private bool ConvertBlocks(IDataType DataType, JsonObject Configuration, BlockLabelUfmReport Report)
	{
		var changed = false;

		foreach (var block in Configuration["blocks"]?.AsArray().OfType<JsonObject>() ?? [])
		{
			var contentTypeName = ContentTypeName(block["contentElementTypeKey"]?.GetValue<string>());

			changed |= ConvertLabel(block, "label", DataType, contentTypeName, contentTypeName ?? "(unknown block)", Report);

			//Block Grid areas carry their own label for the create button.
			foreach (var area in block["areas"]?.AsArray().OfType<JsonObject>() ?? [])
			{
				var areaAlias = area["alias"]?.GetValue<string>() ?? "(unnamed area)";
				var description = $"{contentTypeName ?? "(unknown block)"} > area '{areaAlias}'";

				changed |= ConvertLabel(area, "createLabel", DataType, contentTypeName, description, Report);
			}
		}

		return changed;
	}

	private bool ConvertLabel(JsonObject Owner, string PropertyName, IDataType DataType, string? ContentTypeName, string Description, BlockLabelUfmReport Report)
	{
		var label = Owner[PropertyName]?.GetValue<string>();
		var conversion = BlockLabelUfmConverter.Convert(label, ContentTypeName, Report.UseDragonflyUfmComponents);

		switch (conversion.Status)
		{
			case BlockLabelConversionStatus.NoChangeNeeded:
				Report.LabelsUnchanged.Add(BlockLabelChange.From(DataType, Description, PropertyName, conversion, Applied: false));
				return false;

			//Half-converting a label that needs a UFM component would leave it broken, so leave it as it is.
			case BlockLabelConversionStatus.NeedsManualReview:
				Report.LabelsNeedingReview.Add(BlockLabelChange.From(DataType, Description, PropertyName, conversion, Applied: false));
				return false;

			default:
				Owner[PropertyName] = conversion.ConvertedLabel;
				Report.LabelsConverted.Add(BlockLabelChange.From(DataType, Description, PropertyName, conversion, Applied: !Report.DryRun));
				return true;
		}
	}

	private string? ContentTypeName(string? ContentElementTypeKey)
	{
		return Guid.TryParse(ContentElementTypeKey, out var key)
			? contentTypeService.Get(key)?.Name
			: null;
	}
}

public class BlockLabelUfmReport
{
	public bool DryRun { get; set; }

	public bool UseDragonflyUfmComponents { get; set; }

	public int DataTypesScanned { get; set; }

	public int DataTypesSaved { get; set; }

	public int LabelsNeedingReviewCount => LabelsNeedingReview.Count;

	public int LabelsConvertedCount => LabelsConverted.Count;

	public int LabelsUnchangedCount => LabelsUnchanged.Count;

	public List<BlockLabelChange> LabelsNeedingReview { get; set; } = [];

	public List<BlockLabelChange> LabelsConverted { get; set; } = [];

	public List<BlockLabelChange> LabelsUnchanged { get; set; } = [];
}

public class BlockLabelChange
{
	public string DataTypeName { get; set; } = string.Empty;

	public string Block { get; set; } = string.Empty;

	public string Property { get; set; } = string.Empty;

	public string OriginalLabel { get; set; } = string.Empty;

	public string ConvertedLabel { get; set; } = string.Empty;

	public string Status { get; set; } = string.Empty;

	public bool Applied { get; set; }

	public List<string> Warnings { get; set; } = [];

	internal static BlockLabelChange From(IDataType DataType, string Description, string PropertyName, BlockLabelConversion Conversion, bool Applied)
	{
		return new BlockLabelChange
		{
			DataTypeName = DataType.Name ?? string.Empty,
			Block = Description,
			Property = PropertyName,
			OriginalLabel = Conversion.OriginalLabel,
			ConvertedLabel = Conversion.ConvertedLabel,
			Status = Conversion.Status.ToString(),
			Applied = Applied,
			Warnings = Conversion.Warnings,
		};
	}
}

public class BlockLabelUfmComposer : IComposer
{
	public void Compose(IUmbracoBuilder builder)
	{
		builder.Services.AddTransient<BlockLabelUfmMigrator>();
	}
}
