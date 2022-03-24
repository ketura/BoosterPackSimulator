﻿

namespace BoosterPackSimulator
{
    public class GameDefinition
    {
        public string GameName { get; set; } = "";
        public DateTime LastUpdated { get; set; }

        public List<SetDefinition> Sets { get; set; } = new List<SetDefinition>();
        public Dictionary<int, DisplayCaseDefinition> CaseDefinitions { get; set; } = new Dictionary<int, DisplayCaseDefinition>();

        public void InsertDefinition(GameDefinition other)
        {
            if(other.LastUpdated > LastUpdated)
            {
                LastUpdated = other.LastUpdated;
            }

            Sets.AddRange(other.Sets);
            foreach(var def in other.CaseDefinitions)
            {
                CaseDefinitions.Add(def.Key, def.Value);
            }
        }

        public IEnumerable<IProduct> GetAllNonCardProductForSet(int setNum)
        {
            var set = Sets.FirstOrDefault(x => x.SetNum == setNum);
            if (set == null)
                return new List<IProduct>();

            var c = CaseDefinitions[set.SetNum];

            return new List<IProduct>() 
            {
                c,
                c.DisplayBox,
                c.DisplayBox.BoosterDefinition,
            };
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
        public BoosterType BoosterType { get; set; }
        public int Count { get; set; }
        public DisplayBoxDefinition DisplayBox { get; set; } = new DisplayBoxDefinition();
        public List<RandomDistribution> RandomInsertions { get; set; } = new List<RandomDistribution>();

        public string Name => $"$SETNAME Booster Case ({Count} ct.)";
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

        public string Name => $"$SETNAME Booster Display Box ({Count} ct.)";
        public ProductType ProductType => ProductType.Box;
        public string Filename { get; set; } = "";
        public string Description { get; set; } = "";
        public float Price { get; set; }
        public List<IProduct> Products { get; set; } = new List<IProduct>();
    }

    public class BoosterDefinition : IProduct
    {
        public List<string> RaritySlots { get; set; } = new List<string>();

        public string Name => $"$SETNAME Loose Booster Pack ({RaritySlots.Count} ct.)";
        public ProductType ProductType => ProductType.Booster;
        public string Filename { get; set; } = "";
        public string Description { get; set; } = "";
        public float Price { get; set; }
        public List<IProduct> Products { get; set; } = new List<IProduct>();
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
