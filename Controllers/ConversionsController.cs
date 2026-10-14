using Microsoft.AspNetCore.Mvc;
using UnitConversionApi.Application;
using UnitConversionApi.Domain;
using UnitConversionApi.Models;

namespace UnitConversionApi.Controllers;

[ApiController]
[Route("api/conversions")]
public sealed class ConversionsController(IConversionService converter) : ControllerBase
{
    /// <summary>Convert a value between units in the same measurement category.</summary>
    [HttpPost]
    [ProducesResponseType<ConversionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ConversionResponse> Convert([FromBody] ConversionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FromUnit) || string.IsNullOrWhiteSpace(request.ToUnit))
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["units"] = ["fromUnit and toUnit are required."]
            }) { Status = StatusCodes.Status400BadRequest });

        if (!double.IsFinite(request.Value))
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["value"] = ["Value must be a finite number."]
            }) { Status = StatusCodes.Status400BadRequest });

        try
        {
            var result = converter.Convert(request.Value, request.FromUnit, request.ToUnit);
            return Ok(new ConversionResponse(request.Value, request.FromUnit, result, request.ToUnit));
        }
        catch (UnknownUnitException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (IncompatibleUnitsException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }
}
