using Microsoft.AspNetCore.Mvc;
using TestTask;

namespace task_test.Controllers;

[ApiController]
[ApiVersion("1.0")]
public class SearchController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<PingController> _logger;
    private readonly SearchProviderOneService searchProviderOneService;
    private readonly SearchProviderTwoService searchProviderTwoService;

    public SearchController(ILogger<PingController> logger, SearchProviderOneService searchProviderOneService, SearchProviderTwoService searchProviderTwoService)
    {
        _logger = logger;
        this.searchProviderOneService = searchProviderOneService;
        this.searchProviderTwoService = searchProviderTwoService;
    }

    [MapToApiVersion("1.0")]
    [Route("provider-one/api/v{version:apiVersion}/[controller]")]
    [HttpPost]
    public async Task<IActionResult> ProviderOneGet(SearchRequest request, CancellationToken cancellationToken)
    {
        if (await searchProviderOneService.IsAvailableAsync(cancellationToken))
            return Ok(await searchProviderOneService.SearchAsync(request, cancellationToken));

        return StatusCode(500);
    }


    [MapToApiVersion("1.0")]
    [Route("provider-two/api/v{version:apiVersion}/[controller]")]
    [HttpPost]
    public async Task<IActionResult> ProviderTwoGet(SearchRequest request, CancellationToken cancellationToken)
    {
        if (await searchProviderTwoService.IsAvailableAsync(cancellationToken))
            return Ok(await searchProviderTwoService.SearchAsync(request, cancellationToken));

        return StatusCode(500);
    }


}

