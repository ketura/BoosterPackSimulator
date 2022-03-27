

using static System.Net.WebRequestMethods;

namespace BoosterPackSimulator
{
    public class GameDefinition
    {
        public string GameName { get; set; } = "";
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;
        public string BaseURL { get; set; } = "https://wiki.lotrtcgpc.net/images/";

        public Dictionary<int, SetDefinition> Sets { get; set; } = new Dictionary<int, SetDefinition>();
        public Dictionary<int, DisplayCaseDefinition> CaseDefinitions { get; set; } = new Dictionary<int, DisplayCaseDefinition>();

        public void InsertDefinition(GameDefinition other)
        {
            if(other.LastUpdated > LastUpdated)
            {
                LastUpdated = other.LastUpdated;
            }

            foreach(var pair in other.Sets)
            {
                this.Sets.Add(pair.Key, pair.Value);
            }

            foreach(var def in other.CaseDefinitions)
            {
                CaseDefinitions.Add(def.Key, def.Value);
            }
        }

        public IEnumerable<IProduct> GetAllNonCardProductForSet(int setNum)
        {
            if (!Sets.ContainsKey(setNum))
                return new List<IProduct>();

            var set = Sets[setNum];

            var c = CaseDefinitions[set.SetNum];

            return new List<IProduct>()
            {
                c,
                c.DisplayBox,
                c.DisplayBox.BoosterDefinition,
            };
        }

        public IEnumerable<IProduct> GetAllNonCardProduct()
        {
            List<IProduct> products = new List<IProduct>();
            foreach(var Case in CaseDefinitions.Values)
            {
                products.Add(Case);
                products.Add(Case.DisplayBox);
                products.Add(Case.DisplayBox.BoosterDefinition);
            }

            return products;
        }

        public IProduct FindProduct(string name)
        {
            foreach(var def in CaseDefinitions.Values)
            {
                if (def.Name == name)
                    return def;

                if (def.DisplayBox.Name == name)
                    return def.DisplayBox;

                if (def.DisplayBox.BoosterDefinition.Name == name)
                    return def.DisplayBox.BoosterDefinition;
            }

            return null;
        }

    }

    public enum BoosterType
    {
        Movie,
        Shadows,
        PostShadows,
        Reflections,
        Fixed,
        ExpandedMiddleEarth
    }

    public class DisplayCaseDefinition : IProduct
    {
        public int SetNum { get; set; }
        
        public int Count { get; set; }
        public DisplayBoxDefinition DisplayBox { get; set; } = new DisplayBoxDefinition();
        
        public string SetName { get; set; }
        public string Name => $"{SetName} Booster Case ({Count}x Booster Display Boxes)";
        public ProductType ProductType => ProductType.Case;
        public string Filename { get; set; } = "";
        public string Description { get; set; } = "";
        public float Price { get; set; }
        public List<IProduct> Products { get; set; } = new List<IProduct>();
    }

    public class RandomDistribution 
    {
        public string PoolName { get; set; } = "";
        public int Period { get; set; }
        public string PoolReplaceName { get; set; } = "";
        public int PoolReplaceIndex { get; set; }
    }

    public class DisplayBoxDefinition : IProduct
    {
        public int Count { get; set; }
        public BoosterDefinition BoosterDefinition { get; set; } = new BoosterDefinition();

        public int SetNum { get; set; }
        public string SetName { get; set; }
        public string Name => $"{SetName} Booster Display Box ({Count}x Booster Packs)";
        public ProductType ProductType => ProductType.Box;
        public string Filename { get; set; } = "";
        public string Description { get; set; } = "";
        public float Price { get; set; }
        public List<IProduct> Products { get; set; } = new List<IProduct>();
    }

    public class BoosterDefinition : IProduct
    {
        public List<string> RaritySlots { get; set; } = new List<string>();

        public int SetNum { get; set; }
        public string SetName { get; set; }
        public string Name => $"{SetName} Loose Booster Pack ({RaritySlots.Count}x Cards)";
        public ProductType ProductType => ProductType.Booster;
        public BoosterType BoosterType { get; set; }
        public string Filename { get; set; } = "";
        public string Description { get; set; } = "";
        public float Price { get; set; }
        public List<IProduct> Products { get; set; } = new List<IProduct>();
        public List<RandomDistribution> RandomInsertions { get; set; } = new List<RandomDistribution>();

    }

    public class CardDefinition : IProduct
    {
        public string Name { get; set; } = "";
        public ProductType ProductType => ProductType.Card;
        public string CollInfo { get; set; } = "";
        public string Variant { get; set; } = "";
        public string Filename { get; set; } = "";

        public string Description => $"{Name} ({CollInfo}) [{GetFullVariant(Variant)}]";
        public float Price { get; set; }
        public List<IProduct> Products { get; set; } = new List<IProduct>();

        public static string GetFullVariant(string abbr)
        {
            switch (abbr)
            {
                case "S":
                    return "Standard";
                case "F":
                    return "Foil";
                default:
                    return abbr;
            }
        }
    }

    public class SetDefinition
    {
        public string Name { get; set; } = "";
        public int SetNum { get; set; }
        public Dictionary<string, List<CardDefinition>> CardPools { get; set; } = new Dictionary<string, List<CardDefinition>>();
    }


}
