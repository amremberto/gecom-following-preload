using Microsoft.AspNetCore.Mvc;

namespace GeCom.Following.Preload.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class CountriesController : ControllerBase
{
    private static readonly string[] Countries = new[]
    {
        "Canada", "United States", "Mexico", "Brazil", "Argentina", "United Kingdom", "France", "Germany", "Italy", "Spain"
    };

    private readonly ILogger<CountriesController> _logger;

    public CountriesController(ILogger<CountriesController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetCountries")]
    public IEnumerable<string> Get()
    {
        _logger.LogInformation("Fetching list of countries.");
        return Countries;
    }
}
