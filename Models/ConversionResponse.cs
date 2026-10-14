namespace UnitConversionApi.Models;

public sealed record ConversionResponse(
    double InputValue,
    string FromUnit,
    double Result,
    string ToUnit);
