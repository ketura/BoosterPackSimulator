namespace BoosterPackSimulator
{
    public class PackHandler
    {
        public Dictionary<string, PackRandomizer> Randomizers = new Dictionary<string, PackRandomizer>();
    }

    public class PackRandomizer
    {
        private static readonly Random RNG = new Random(unchecked((int)DateTime.Now.Ticks));

        public BoosterType RandomMethod { get; }
        public GameDefinition GameDef { get; }
        public BoosterDefinition BoosterDef { get; }
        public Dictionary<string, List<CardDefinition>> Sheets { get; }

        public Dictionary<string, int> LastPositions { get; set; } = new Dictionary<string, int>();
        private int ResetCounter;

        public List<List<CardDefinition>> PrebuiltBoosters { get; set; }

        
        public PackRandomizer(GameDefinition gamedef, BoosterDefinition boosterDef, Dictionary<string, List<CardDefinition>>? sheets=null)
        {
            RandomMethod = boosterDef.BoosterType;
            GameDef = gamedef;
            BoosterDef = boosterDef;

            if (sheets == null)
            {
                Sheets = SeedRandomSheet();
            }
            else
            {
                Sheets = sheets;
            }

            ResetPools();
        }

        private void ResetPools()
        {
            LastPositions.Clear();
            foreach (string pool in Sheets.Keys)
            {
                LastPositions[pool] = RNG.Next(0, Sheets[pool].Count);
            }

            ResetCounter = RNG.Next(0, 30);
        }

        private Dictionary<string, List<CardDefinition>> SeedRandomSheet()
        {
            var result = new Dictionary<string, List<CardDefinition>>();
            switch (RandomMethod)
            {
                case BoosterType.Shadows:
                case BoosterType.PostShadows:
                case BoosterType.Movie:
                case BoosterType.Reflections:

                    var set = GameDef.Sets[BoosterDef.SetNum];
                    if (set == default)
                        return result;

                    foreach(var rarity in set.CardPools.Keys)
                    {
                        result[rarity] = new List<CardDefinition>();
                        foreach(var card in set.CardPools[rarity].OrderBy(a => RNG.Next()).ToList())
                        {
                            result[rarity].Add(card);

                            if (rarity.Contains("Foil"))
                            {
                                var randomDef = BoosterDef.RandomInsertions.Where(x => x.PoolName == rarity).FirstOrDefault();
                                if (randomDef == null)
                                    continue;

                                for (int i = 0; i < randomDef.Period; i++)
                                {
                                    result[rarity].Add(null);
                                }
                            }
                        }
                        
                    }
                    break;
                case BoosterType.Fixed:
                case BoosterType.ExpandedMiddleEarth:
                    result = GameDef.Sets[BoosterDef.SetNum].CardPools;
                    break;
                default:
                    break;
            }

            return result;
        }

        public List<CardDefinition> GetNextBooster()
        {
            var result = new List<CardDefinition>();

            //First, determine the base booster
            switch (RandomMethod)
            {
                case BoosterType.Movie:
                case BoosterType.Shadows:
                case BoosterType.PostShadows:
                case BoosterType.Reflections:
                    var set = GameDef.Sets[BoosterDef.SetNum];
                    foreach(string slot in BoosterDef.RaritySlots)
                    {
                        result.Add(Sheets[slot][LastPositions[slot]]);
                        LastPositions[slot] += 1;
                        if(LastPositions[slot] >= set.CardPools[slot].Count)
                        {
                            LastPositions[slot] = 0;
                        }
                    }
                    break;
                case BoosterType.Fixed:
                case BoosterType.ExpandedMiddleEarth:
                    return Sheets.First().Value;
            }

            //Next, figure out what foils should be inserted
            switch (RandomMethod)
            {
                case BoosterType.Shadows:
                case BoosterType.PostShadows:
                case BoosterType.Reflections:

                case BoosterType.Movie:
                    var set = GameDef.Sets[BoosterDef.SetNum];
                    foreach (var rand in BoosterDef.RandomInsertions)
                    {
                        var card = Sheets[rand.PoolName][LastPositions[rand.PoolName]];
                        if (card != null)
                        {
                            int attempt = BoosterDef.RaritySlots.IndexOf(BoosterDef.RaritySlots.First(x => x == rand.PoolReplaceName));
                            if (result[attempt].Variant == "Foil")
                            {
                                attempt++;
                            }
                            if (result[attempt].Variant == "Foil")
                            {
                                attempt++;
                            }
                            result.Insert(attempt, card);
                            result.RemoveAt(attempt + 1);
                        }
                        LastPositions[rand.PoolName] += 1;
                        if (LastPositions[rand.PoolName] >= set.CardPools[rand.PoolName].Count)
                        {
                            LastPositions[rand.PoolName] = 0;
                        }
                    }
                    break;

            }

            ResetCounter--;
            if (ResetCounter == 0)
            {
                ResetPools();
            }

            return result;
        }

       
    }
}
