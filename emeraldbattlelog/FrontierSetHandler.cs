using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace emeraldbattlelog
{
    //Format:
    //Battler 1 info: Spoink Lv50 HP 4/153 - 31 45 108 86 59 - Moves: 15 Psywave, 5 Bounce, 15 Magic Coat, 10 Confuse Ray

    //Battler 0 is player, 1 is opponent, 2 is player second, 3 is opponent second (2/3 only used in doubles)
    public class FrontierSetHandler()
    {
        public PokemonSlot[] handleFrontierSlotSimple(String monName)
        {
            return handleFrontierSet("Battler 1 " + monName);
        }

        public PokemonSlot[] handleFrontierSet(String monInfo)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            foreach (string pokemonName in Tables.pokemon)
            {
                //Match exact Pokemon name
                if (Regex.IsMatch(
                    monInfo,
                    $@"\b{Regex.Escape(pokemonName)}\b",
                    RegexOptions.IgnoreCase))
                {
                    //If is opponent
                    if (monInfo.StartsWith("Battler 1")
                        || monInfo.StartsWith("Battler 3"))
                    {
                        PokemonSlot[] frontierSets = new PokemonSlot[8];

                        int i = 0;

                        foreach (string[] frontierSet in Tables.frontierSets)
                        {
                            if (frontierSet[0].Equals(pokemonName))
                            {
                                PokemonSlot s = new PokemonSlot();
                                s.name = frontierSet[0];
                                s.index = frontierSet[1];
                                s.nature = frontierSet[2];
                                s.item = frontierSet[3];
                                s.move1 = frontierSet[4];
                                s.move2 = frontierSet[5];
                                s.move3 = frontierSet[6];
                                s.move4 = frontierSet[7];
                                s.abilities = frontierSet[8];
                                s.EVs = frontierSet[9];

                                s.setEVs(frontierSet[9]);

                                frontierSets[i] = s;
                                i++;
                            }
                        }

                        return frontierSets;

                    }
                }
            }
            return new PokemonSlot[0];
        }
    }
}
