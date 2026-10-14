using UnitConversionApi.Domain;

namespace UnitConversionApi.Application;

public sealed class ConversionService(IUnitCatalog catalog) : IConversionService
{
    public double Convert(double value, string fromUnit, string toUnit)
    {
        if (!catalog.TryGet(fromUnit, out var source)) throw new UnknownUnitException(fromUnit);
        if (!catalog.TryGet(toUnit, out var target)) throw new UnknownUnitException(toUnit);
        if (source.Category != target.Category) throw new IncompatibleUnitsException(fromUnit, toUnit);

        var canonicalValue = value * source.ScaleToCanonical + source.OffsetToCanonical;
        return (canonicalValue - target.OffsetToCanonical) / target.ScaleToCanonical;
    }
}
