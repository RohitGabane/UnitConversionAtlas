using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UnitConversionApi.Models;

public sealed record ConversionRequest(
    [property: JsonRequired] double Value,
    [param: Required] string FromUnit,
    [param: Required] string ToUnit);
