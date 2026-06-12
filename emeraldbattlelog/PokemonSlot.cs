using System;
using System.Collections.Generic;
using System.Text;

namespace emeraldbattlelog
{
    public class PokemonSlot
    {
        public string name { get; set; }
        public string index { get; set; }
        public string nature { get; set; }
        public string move1 { get; set; }
        public string move2 { get; set; }
        public string move3 { get; set; }
        public string move4 { get; set; }
        public string item { get; set; }
        public string EVs { get; set; }
        public string abilities { get; set; }
        public string SpritePath { get; set; }

        public void setEVs(string evString)
        {
            EVs = formatEVs(evString);
        }

        public string formatEVs(string evString)
        {
            string[] stats = { "HP", "Atk", "Def", "SpA", "SpD", "Spe" };

            var values = evString.Split('/');

            List<string> investedStats = new List<string>();

            for (int i = 0; i < values.Length; i++)
            {
                if (int.Parse(values[i].Trim()) > 0)
                {
                    investedStats.Add(stats[i]);
                }
            }

            return string.Join("/", investedStats);
        }
    }
}
