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
    }
}
