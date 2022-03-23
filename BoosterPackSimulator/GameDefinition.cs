

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

    public class DisplayCaseDefinition
    {
        public int SetNum { get; set; }
        public BoosterType BoosterType { get; set; }
        public int Count { get; set; }
        public DisplayBoxDefinition DisplayBox { get; set; } = new DisplayBoxDefinition();
        public List<RandomDistribution> RandomInsertions { get; set; } = new List<RandomDistribution>();

    }

    public class RandomDistribution
    {
        public string PoolName { get; set; } = "";
        public int Period { get; set; }
        public string PoolReplaceName { get; set; } = "";
        public int PoolReplaceIndex { get; set; }
    }

    public class DisplayBoxDefinition
    {
        public int Count { get; set; }
        public BoosterDefinition BoosterDefinition { get; set; } = new BoosterDefinition();
    }

    public class BoosterDefinition
    {
        public List<string> RaritySlots { get; set; } = new List<string>();
    }

    public class CardDefinition
    { 
        public string Name { get; set; } = "";
        public string CollInfo { get; set; } = "";
        public string Variant { get; set; } = "";
        public string Filename { get; set; } = "";
    }

    public class SetDefinition
    {
        public string Name { get; set; } = "";
        public int SetNum { get; set; }
        public Dictionary<string, List<CardDefinition>> CardPools { get; set; } = new Dictionary<string, List<CardDefinition>>();
    }


}
