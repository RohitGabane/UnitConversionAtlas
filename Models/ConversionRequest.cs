using System.ComponentModel.DataAnnotations;

namespace UnitConversionApi.Models;

public sealed record ConversionRequest(
    double Value,
    [property: Required] string FromUnit,
    [property: Required] string ToUnit);
