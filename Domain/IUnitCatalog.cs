namespace UnitConversionApi.Domain;

public interface IUnitCatalog
{
    IReadOnlyCollection<UnitDefinition> All { get; }
    bool TryGet(string symbol, out UnitDefinition unit);
}
