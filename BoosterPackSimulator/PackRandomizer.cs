using System.Text.Json.Serialization;

namespace BoosterPackSimulator
{
    public class PackHandler
    {
        public Dictionary<string, PackRandomizer> Randomizers = new Dictionary<string, PackRandomizer>();

        public void Init(GameDefinition gamedef)
        {
            foreach(var randomizer in Randomizers.Values)
            {
                randomizer.Init(gamedef);
            }
        }
    }


    public class PackRandomizer
    {
        private static readonly Random RNG = new Random(unchecked((int)DateTime.Now.Ticks));

        
        [JsonIgnore]
        public GameDefinition GameDef { get; private set; }
        [JsonIgnore]
        public BoosterDefinition BoosterDef { get; private set; }
        
        private CardPool Pool { get; set; }


        public Dictionary<string, int> LastPositions { get; set; } = new Dictionary<string, int>();
        private int ResetCounter;

        public void ResetPositions()
        {
            LastPositions.Clear();
            foreach (string rarity in Pool.Sheets.Keys)
            {
                LastPositions[rarity] = RNG.Next(0, Pool.Sheets[rarity].Count);
            }

            ResetCounter = RNG.Next(0, 100);
        }

        public PackRandomizer(GameDefinition gamedef, BoosterDefinition boosterDef, Dictionary<string, List<CardDefinition>>? sheets=null)
        {
            BoosterDef = boosterDef;
            Init(gamedef);

            Pool = new CardPool(gamedef, boosterDef, sheets);

            ResetPositions();
        }

        public void Init(GameDefinition gamedef)
        {
            GameDef = gamedef;
        }


        public List<CardDefinition> GetNextBooster()
        {
            var result = new List<CardDefinition>();

            //First, determine the base booster
            switch (BoosterDef.BoosterType)
            {
                case BoosterType.Movie:
                case BoosterType.MovieUnsorted:
                case BoosterType.Shadows:
                case BoosterType.PostShadows:
                case BoosterType.Reflections:
                    var set = GameDef.Sets[BoosterDef.SetNum];
                    foreach(string slot in BoosterDef.RaritySlots)
                    {
                        result.Add(Pool.Sheets[slot][LastPositions[slot]]);
                        LastPositions[slot] += 1;
                        if(LastPositions[slot] >= set.CardPools[slot].Count)
                        {
                            LastPositions[slot] = 0;
                        }
                    }
                    break;
                case BoosterType.Fixed:
                case BoosterType.ExpandedMiddleEarth:
                    return Pool.Sheets.First().Value;
            }

            //Next, figure out what foils should be inserted
            switch (BoosterDef.BoosterType)
            {
                case BoosterType.Shadows:
                case BoosterType.PostShadows:
                case BoosterType.Reflections:
                case BoosterType.MovieUnsorted:
                case BoosterType.Movie:
                    var set = GameDef.Sets[BoosterDef.SetNum];
                    foreach (var rand in BoosterDef.RandomInsertions)
                    {
                        var card = Pool.Sheets[rand.PoolName][LastPositions[rand.PoolName]];
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
                        if (LastPositions[rand.PoolName] >= Pool.Sheets[rand.PoolName].Count)
                        {
                            LastPositions[rand.PoolName] = 0;
                        }
                    }
                    break;

            }

            ResetCounter--;
            if (ResetCounter == 0)
            {
                ResetPositions();
            }

            return result;
        }

        private class CardPool
        {
            public Dictionary<string, List<CardDefinition>> Sheets { get; private set; }


            public Dictionary<string, List<CardDefinition>> SeedRandomSheet(GameDefinition GameDef, BoosterDefinition BoosterDef)
            {
                var result = new Dictionary<string, List<CardDefinition>>();
                SetDefinition set = null;
                switch (BoosterDef.BoosterType)
                {
                    case BoosterType.Movie:
                        set = GameDef.Sets[BoosterDef.SetNum];
                        if (set == default)
                            return result;

                        foreach (var rarity in set.CardPools.Keys)
                        {
                            result[rarity] = new List<CardDefinition>();
                            if (set.Sheets.ContainsKey(rarity))
                            {
                                foreach (var collInfo in set.Sheets[rarity])
                                {
                                    var card = set.Cards.FirstOrDefault(x => x.CollInfo == collInfo);
                                    if (card == default)
                                        continue;
                                    result[rarity].Add(card);
                                }
                            }
                        }

                        foreach (var rarity in set.CardPools.Keys.Where(x => x.Contains('F')))
                        {
                            foreach (var card in set.CardPools[rarity])
                            {
                                var actualRarity = rarity;
                                if(rarity == "PF")
                                {
                                    actualRarity = "CF";
                                }

                                result[actualRarity].Add(card);

                                var randomDef = BoosterDef.RandomInsertions.Where(x => x.PoolName == actualRarity).FirstOrDefault();
                                if (randomDef == null)
                                    continue;

                                for (int i = 0; i < randomDef.Period; i++)
                                {
                                    result[actualRarity].Add(null);
                                }
                            }
                        }

                        break;

                    case BoosterType.Shadows:
                    case BoosterType.PostShadows:
                    case BoosterType.MovieUnsorted:
                    case BoosterType.Reflections:

                        set = GameDef.Sets[BoosterDef.SetNum];
                        if (set == default)
                            return result;

                        foreach (var rarity in set.CardPools.Keys)
                        {
                            result[rarity] = new List<CardDefinition>();
                            foreach (var card in set.CardPools[rarity].OrderBy(a => RNG.Next()).ToList())
                            {
                                result[rarity].Add(card);

                                if (rarity.Contains("F"))
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

            public CardPool(GameDefinition gamedef, BoosterDefinition boosterdef, Dictionary<string, List<CardDefinition>>? sheets = null)
            {
                if (sheets == null)
                {
                    Sheets = SeedRandomSheet(gamedef, boosterdef);
                }
                else
                {
                    Sheets = sheets;
                }
            }
        }
    }
}
