namespace UnitConversionApi.Domain;

public sealed class UnknownUnitException(string unit)
    : Exception($"Unknown unit '{unit}'. See GET /api/units for supported units.");

public sealed class IncompatibleUnitsException(string from, string to)
    : Exception($"Units '{from}' and '{to}' belong to different measurement categories.");
