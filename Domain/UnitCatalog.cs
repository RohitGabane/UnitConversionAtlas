namespace UnitConversionApi.Domain;

/// <summary>
/// Central registry; add a unit here without changing conversion logic.
/// </summary>
public sealed class UnitCatalog : IUnitCatalog
{
    private readonly Dictionary<string, UnitDefinition> _units =
        new(StringComparer.OrdinalIgnoreCase);

    public UnitCatalog()
    {
        // Length
        Add(MeasurementCategory.Length, "m", "meter", 1);
        Add(MeasurementCategory.Length, "km", "kilometer", 1_000);
        Add(MeasurementCategory.Length, "cm", "centimeter", 0.01);
        Add(MeasurementCategory.Length, "mm", "millimeter", 0.001);
        Add(MeasurementCategory.Length, "mi", "mile", 1_609.344);
        Add(MeasurementCategory.Length, "yd", "yard", 0.9144);
        Add(MeasurementCategory.Length, "ft", "foot", 0.3048);
        Add(MeasurementCategory.Length, "in", "inch", 0.0254);

        // Temperature
        Add(MeasurementCategory.Temperature, "C", "Celsius", 1, 273.15);
        Add(
            MeasurementCategory.Temperature,
            "F",
            "Fahrenheit",
            5d / 9d,
            255.3722222222222);
        Add(MeasurementCategory.Temperature, "K", "kelvin", 1, 0);

        // Mass
        Add(MeasurementCategory.Mass, "kg", "kilogram", 1);
        Add(MeasurementCategory.Mass, "g", "gram", 0.001);
        Add(MeasurementCategory.Mass, "mg", "milligram", 0.000001);
        Add(MeasurementCategory.Mass, "lb", "pound", 0.45359237);
        Add(MeasurementCategory.Mass, "oz", "ounce", 0.028349523125);
        Add(MeasurementCategory.Mass, "t", "metric tonne", 1_000);
    }

    public IReadOnlyCollection<UnitDefinition> All => _units.Values;

    public bool TryGet(string symbol, out UnitDefinition unit)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            unit = null!;
            return false;
        }

        return _units.TryGetValue(symbol.Trim(), out unit!);
    }

    private void Add(
        MeasurementCategory category,
        string symbol,
        string name,
        double scale,
        double offset = 0)
    {
        _units.Add(
            symbol,
            new UnitDefinition(
                symbol,
                name,
                category,
                scale,
                offset));
    }
}
