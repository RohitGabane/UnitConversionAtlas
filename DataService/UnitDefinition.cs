namespace UnitConversionApi.Domain;

/// <summary>Defines a unit using a scale and offset to its category's canonical unit.</summary>
public sealed record UnitDefinition(
    string Symbol,
    string Name,
    MeasurementCategory Category,
    double ScaleToCanonical,
    double OffsetToCanonical = 0);
