

namespace BoosterPackSimulator
{
    public enum ProductType
    {
        Case,
        Box,
        Booster,
        Card
    }
    public interface IProduct
    {
        string Name { get; }
        ProductType ProductType { get; }
        string Variant { get; }
        string Filename { get; }
        string Description { get; }
        float Price { get; }
        List<IProduct> Products { get; }
    }

}
