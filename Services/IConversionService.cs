namespace UnitConversionApi.Application;

public interface IConversionService
{
    double Convert(double value, string fromUnit, string toUnit);
}
