using UnitConversionApi.Domain;

namespace UnitConversionApi.Application;

public sealed class ConversionService : IConversionService
{
    private readonly IUnitCatalog _catalog;

    public ConversionService(IUnitCatalog catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    public double Convert(double value, string fromUnit, string toUnit)
    {
        if (!_catalog.TryGet(fromUnit, out var source))
            throw new UnknownUnitException(fromUnit);

        if (!_catalog.TryGet(toUnit, out var target))
            throw new UnknownUnitException(toUnit);

        if (source.Category != target.Category)
            throw new IncompatibleUnitsException(fromUnit, toUnit);

        var canonicalValue = value * source.ScaleToCanonical + source.OffsetToCanonical;

        var result = (canonicalValue - target.OffsetToCanonical) / target.ScaleToCanonical;

        if (!double.IsFinite(result))
            throw new ConversionOutOfRangeException(fromUnit, toUnit);

        return result;
    }
}
