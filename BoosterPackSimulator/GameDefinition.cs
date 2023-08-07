

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
                var set = pair.Value;

                if(set.FullFoils)
                {
                    foreach (var card in set.Cards)
                    {
                        var rarity = card.Rarity;
                        var foilRarity = card.Rarity + "F";
                        if (!set.CardPools.ContainsKey(rarity))
                        {
                            set.CardPools[rarity] = new List<CardDefinition>();
                        }
                        if (!set.CardPools.ContainsKey(foilRarity))
                        {
                            set.CardPools[foilRarity] = new List<CardDefinition>();
                        }

                        set.CardPools[rarity].Add(card);
                        set.CardPools[foilRarity].Add(card.GetFoilVariant());
                    }
                }
                
                this.Sets.Add(pair.Key, set);
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
        MovieUnsorted,
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
        
        public string? SetName { get; set; }
        public string Name => $"{SetName} Booster Case ({Count}x Booster Display Boxes)";
        [JsonConverter(typeof(StringEnumConverter))] 
        public ProductType ProductType => ProductType.Case;
        public string Filename { get; set; } = "";
        public string Description { get; set; } = "";
        public float Price { get; set; }
        public List<IProduct> Products { get; set; } = new List<IProduct>();
        public string Variant => "";
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
        [JsonConverter(typeof(StringEnumConverter))] 
        public ProductType ProductType => ProductType.Box;
        public string Filename { get; set; } = "";
        public string Description { get; set; } = "";
        public float Price { get; set; }
        public List<IProduct> Products { get; set; } = new List<IProduct>();
        public string Variant => "";
    }

    public class BoosterDefinition : IProduct
    {
        public List<string> RaritySlots { get; set; } = new List<string>();

        public int SetNum { get; set; }
        public string SetName { get; set; }
        public string Name => $"{SetName} Loose Booster Pack ({RaritySlots.Count}x Cards)";
        [JsonConverter(typeof(StringEnumConverter))]
        public ProductType ProductType => ProductType.Booster;
        [JsonConverter(typeof(StringEnumConverter))]
        public BoosterType BoosterType { get; set; }
        public string Filename { get; set; } = "";
        public string Description { get; set; } = "";
        public float Price { get; set; }
        public List<IProduct> Products { get; set; } = new List<IProduct>();
        public List<RandomDistribution> RandomInsertions { get; set; } = new List<RandomDistribution>();

        public string Variant => "";
    }

    public class CardDefinition : IProduct
    {
        public string Name { get; set; } = "";
        [JsonConverter(typeof(StringEnumConverter))]
        public ProductType ProductType => ProductType.Card;
        public string CollInfo => $"{Set}{Rarity}{Num}";
        public string Set { get; set; } = "";
        public string Rarity { get; set; } = "";
        public string Num { get; set; } = "";
        public bool Horiz { get; set; } = false;

        [JsonProperty("EN_Image")]
        public string ImageURL { get; set; } = "";

        [JsonProperty("EN_Foil")]
        public string FoilURL { get; set; } = "";

        public string Variant { get; set; } = "S";

        public string Filename
        {
            get
            {
                switch (Variant.ToUpper())
                {
                    case "S":
                        return ImageURL;
                    case "F":
                        return FoilURL;
                    default:
                        return "";
                }
            }
        }

        public string Description => $"{Name} ({CollInfo}) [{GetFullVariant(Variant)}]";
        public float Price { get; set; }
        public List<IProduct> Products { get; set; } = new List<IProduct>();

        public CardDefinition GetFoilVariant()
        {
            return new CardDefinition()
            {
                Name = this.Name,
                Set = this.Set,
                Rarity = this.Rarity,
                Num = this.Num,
                Horiz = this.Horiz,
                ImageURL = this.ImageURL,
                FoilURL = this.FoilURL,
                Price = this.Price,
                Products = this.Products,
                Variant = "F"
            };
        }

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
        public bool FullFoils { get; set; } = true;
        public List<CardDefinition> Cards { get; set; } = new List<CardDefinition>();
        public Dictionary<string, List<string>> Sheets { get; set; } = new Dictionary<string, List<string>>();
        public Dictionary<string, List<CardDefinition>> CardPools { get; set; } = new Dictionary<string, List<CardDefinition>>();
    }


}
