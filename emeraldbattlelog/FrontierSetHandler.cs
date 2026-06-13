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
        //This is all that's necessary for now, since we can't leak the enemy's set
        //but the full handler will be necessary for things like printing out the player's
        //current active battler or something, if I ever want to implement that.
        public PokemonSlot[] handleFrontierSlotSimple(String monName)
        {
            return handleFrontierSet("Battler 1 " + monName);
        }

        //We're not really using most of the information collected by this, but we might in the future.
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
                    //Currently we only want to bother for opponents.
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

                                //Change the EVs from numbers to a readable format.
                                s.setEVs(frontierSet[9]);

                                frontierSets[i] = s;
                                i++;
                            }
                        }
                        return frontierSets;
                    }
                }
            }
            //else, return an empty set.
            return new PokemonSlot[0];
        }
    }
}
