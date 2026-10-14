using Microsoft.AspNetCore.Mvc;
using UnitConversionApi.Domain;

namespace UnitConversionApi.Controllers;

[ApiController]
[Route("api/units")]
public sealed class UnitsController(IUnitCatalog catalog) : ControllerBase
{
    /// <summary>List supported units grouped by measurement category.</summary>
    [HttpGet]
    [ProducesResponseType<Dictionary<string, IEnumerable<object>>>(StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var unitsByCategory = catalog.All
            .GroupBy(unit => unit.Category)
            .ToDictionary(
                group => group.Key.ToString().ToLowerInvariant(),
                group => group.Select(unit => (object)new { unit.Symbol, unit.Name }));

        return Ok(unitsByCategory);
    }
}
