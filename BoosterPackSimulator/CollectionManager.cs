namespace BoosterPackSimulator
{
    public class CollectionManager
    {
        public float TotalSpending { get; set; }
        public Dictionary<string, int> OwnedProducts { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> OwnedCards { get; set; } = new Dictionary<string, int>();

        public async Task PurchaseItem(IProduct product)
        {
            TotalSpending += product.Price;
            switch (product.ProductType)
            {
                case ProductType.Case:
                case ProductType.Box:
                case ProductType.Booster:
                    if (OwnedProducts.ContainsKey(product.Name))
                    {
                        OwnedProducts[product.Name] += 1;
                    }
                    else
                    {
                        OwnedProducts.Add(product.Name, 1);
                    }
                    break;
                case ProductType.Card:
                    if (OwnedCards.ContainsKey(product.Description))
                    {
                        OwnedCards[product.Description] += 1;
                    }
                    else
                    {
                        OwnedCards.Add(product.Description, 1);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
