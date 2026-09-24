using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Authorization;

namespace Dragonfly.UfmExtensions;

[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "UmbracoFlavoredMarkdownExtensions")]
public class UfmExtensionsApiController : UfmExtensionsApiControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly BlockLabelUfmMigrator _blockLabelUfmMigrator;

    public UfmExtensionsApiController(IBackOfficeSecurityAccessor backOfficeSecurityAccessor, BlockLabelUfmMigrator blockLabelUfmMigrator)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _blockLabelUfmMigrator = blockLabelUfmMigrator;
    }

    [HttpGet("ping")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    public string Ping() => "Pong";

    /// <summary>
    /// Reports how every Block List and Block Grid label would convert to UFM, without saving anything.
    /// </summary>
    [HttpGet("convertBlockLabelsToUfm")]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
    [ProducesResponseType<BlockLabelUfmReport>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<IActionResult> PreviewBlockLabelConversion(bool UseContentTypeNameComponent = false)
        => RunBlockLabelMigrator(DryRun: true, UseContentTypeNameComponent);

    /// <summary>
    /// Converts every Block List and Block Grid label to UFM and saves the changed datatypes.
    /// </summary>
    [HttpPost("convertBlockLabelsToUfm")]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
    [ProducesResponseType<BlockLabelUfmReport>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<IActionResult> ConvertBlockLabels(bool UseContentTypeNameComponent = false)
        => RunBlockLabelMigrator(DryRun: false, UseContentTypeNameComponent);

    private async Task<IActionResult> RunBlockLabelMigrator(bool DryRun, bool UseContentTypeNameComponent)
    {
        var currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var report = await _blockLabelUfmMigrator.RunAsync(DryRun, currentUser.Key, UseContentTypeNameComponent);

        return Ok(report);
    }

    //[HttpGet("whatsTheTimeMrWolf")]
    //[ProducesResponseType(typeof(DateTime), 200)]
    //public DateTime WhatsTheTimeMrWolf() => DateTime.Now;

    //[HttpGet("whatsMyName")]
    //[ProducesResponseType<string>(StatusCodes.Status200OK)]
    //public string WhatsMyName()
    //{
    //    // So we can see a long request in the dashboard with a spinning progress wheel
    //    Thread.Sleep(2000);

    //    var currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
    //    return currentUser?.Name ?? "I have no idea who you are";
    //}

    //[HttpGet("whoAmI")]
    //[ProducesResponseType<IUser>(StatusCodes.Status200OK)]
    //public IUser? WhoAmI() => _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
}

