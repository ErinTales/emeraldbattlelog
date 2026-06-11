using emeraldbattlelog;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace PokemonBattleLogger
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileSystemWatcher? watcher;
        private PokemonSlot[] enemyTeam = new PokemonSlot[4];
        PokemonSlot[] sets;
        PokemonSlot[] setsDouble;
        private bool newTurn = true;
        private int turnCounter = 1;
        private FrontierSets frontierSetsWindow = new FrontierSets();
        private int battleType = 1;
        bool battleTypeLockout = false;
        string lastFainted = "";

        public MainWindow()
        {
            frontierSetsWindow.Show();

            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("Starting watcher...");
            StartWatcher();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void StartWatcher()
        {
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run("Pokemon Emerald Battle Logger Initialized\n"));

            BattleLog.Document.Blocks.Add(paragraph);

            long lastPosition = 0;

            var path = @"C:\Users\Erin\source\repos\emeraldbattlelog\emeraldbattlelog\pokemon lua\battlelog.txt";

            watcher = new FileSystemWatcher(
            @"C:\Users\Erin\source\repos\emeraldbattlelog\emeraldbattlelog\pokemon lua",
            "battlelog.txt");

            if (File.Exists(path))
            {
                lastPosition = new FileInfo(path).Length;
            }

            watcher.Changed += (sender, e) =>
            {
                try
                {
                    using var stream = new FileStream(
                        e.FullPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite);

                    // Jump to where we left off last time
                    stream.Seek(lastPosition, SeekOrigin.Begin);

                    using var reader = new StreamReader(stream);

                    while (!reader.EndOfStream)
                    {
                        string? line = reader.ReadLine();

                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            //We need to clean up the string first.
                            line = lineCleanup(line);

                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                    //Print the line to the log.
                                Dispatcher.Invoke(() =>
                                {
                                    var paragraph = new Paragraph();
                                    paragraph.Margin = new Thickness(0);

                                    if(line.Contains("Turn")
                                    || line.Contains("would like to battle"))
                                    {
                                        paragraph.FontSize = 48;
                                    }

                                    paragraph.Inlines.Add(new Run(line));
                                    BattleLog.Document.Blocks.Add(paragraph);
                                    LogScroller.ScrollToEnd();
                                });
                            }
                        }
                    }

                    // Remember where we stopped reading
                    lastPosition = stream.Position;
                }
                catch (Exception ex)
                {
                   //TODO - handle
                }
            };

            watcher.EnableRaisingEvents = true;
        }

        private string lineCleanup(string line)
        {
            //Debug.WriteLine(line);

            //Remove jank from the text, like japanese characters,
            //full with characters, newline tags, etc.
            line = Regex.Replace(line, @"<[^>]*>", " ");
            line = line.Replace("　", " ");
            line = line.Replace("！", "!");
            line = line.Replace("？", "?");
            line = line.Replace("。", ".");
            line = line.Replace(@"\'", "'");
            line = line.Replace("たち", "");
            line = line.Replace("け", "");
            line = line.Replace("ラ", "");
            line = line.Replace("ウエ", "POKéMON");
            line = line.Replace("シ", "");
            line = line.Replace("ス", "");

            //Don't display random spam of battler names.
            if (line.Contains("いい"))
            {
                line = "";
            }

            //Don't display move hovering.
            if (line.StartsWith("TYPE/"))
            {
                line = "";
            }

            //Don't display random pp counts.
            if (Regex.IsMatch(line, @"^\d+/\d+$"))
            {
                line = "";
            }

            if (line.Equals("PP "))
            {
                line = "";
            }

            //Don't display XP gained
            if (line.EndsWith("EXP. Points! "))
            {
                line = "";
            }

            //Don't display "would you like to switch?" message
            if (line.Contains("is about to use"))
            {
                line = "";
            }

            //Don't display items message
            if(line.Contains("Items can't be used"))
            {
                line = "";
            }

            //Fix "Got away safely"
            if(line.Equals("  Got away safely! "))
            {
                line = "Got away safely!";
            }

            if (line.Contains("Would you like to forfeit the match and quit now?"))
            {
                line = "";
            }

            //remove the battle end text
            if (Regex.IsMatch(line, @"^[^a-z]*$"))
            {
                line = "";
            }

            //Remove uppercase spam.
            line = FixCaps(line);

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            foreach (string moveName in Tables.battleMoves)
            {
                line = line.Replace(moveName, textInfo.ToTitleCase(moveName), StringComparison.OrdinalIgnoreCase);

                //While we're here, don't display random move names alone.
                if (line.ToLower().Equals(moveName.ToLower()))
                {
                    line = "";
                }
            }

            line = line.Replace("PORYGON2", "Porygon2");
            line = line.Replace("ひ", "é");
            line = line.Replace("Hp", "HP");
            line = line.Replace("POKéMON", "Pokémon");
            line = line.Replace("POKéFAN", "Pokéfan");
            line = line.Replace("oneーhit Ko!", "oneーhit KO!");
            line = line.Replace("WILLーOーWISP", "WillーOーWisp");
            line = line.Replace("SANDーATTACK", "SandーAttack");

            /*
            foreach (string pokemonName in Tables.pokemon)
            {
                line = line.Replace(pokemonName, textInfo.ToTitleCase(pokemonName), StringComparison.OrdinalIgnoreCase);
            }


            foreach (string itemName in Tables.items)
            {
                line = line.Replace(itemName, textInfo.ToTitleCase(itemName.ToLower()), StringComparison.OrdinalIgnoreCase);
            }

            foreach (string abilityName in Tables.abilities)
            {
                line = line.Replace(abilityName, textInfo.ToTitleCase(abilityName.ToLower()), StringComparison.OrdinalIgnoreCase);
            }

            line = line.Replace("ATTACK", "Attack");
            line = line.Replace("DEFENSE", "Defense");
            line = line.Replace("SP.", "Sp.");
            line = line.Replace("DEF", "Def");
            line = line.Replace("ATK", "Atk");
            line = line.Replace("SPEED", "Speed");
            line = line.Replace("POKひMON", "Pokémon");
            line = line.Replace("ExeggCute", "Exeggcute");      //Black magic or smth, idk why this happens.
            line = line.Replace("RestORE", "Restore");  //Lazy fix - bug from "FULL RESTORE" having "REST" in it
            line = line.Replace("WrapPED", "wrapped");  //Lazy fix - bug from "WRAPPED" having "WRAP" in it.
            line = line.Replace("USing", "Using");      //Lazy fix - bug from "USING" having "SING" in it.
            line = line.Replace("uSing", "Using");      //Lazy fix - bug from "USING" having "SING" in it.
            line = line.Replace("ParasOL", "PARASOL");  //Lazy fix - bug from "PARASOL" having "PARAS" in it.
            line = line.Replace("TraceD", "Traced");    ////Lazy fix - bug from "TRAED" having "TRACE" in it.*/


            //Remove weird "What will [partial type name]" spam. (this is buggy, idk why it happens but who cares lol)
            if (line.StartsWith("What will ") && !line.EndsWith(" do?"))
            {
                line = "";
            }

            //Add newlines before select messages, for prettyness
            if (line.EndsWith("would like to battle! ")         //New trainer battle
                || line.EndsWith("appeared! ")                  //wild battle
                || line.EndsWith("Got away safely!")            //more wild battle
                || line.Contains("used")                        //use a move
                || line.Contains("sent out")                    //opponent sends out pokemon
                || line.Contains("Go!")                         //player sends out pokemon
                || line.Contains("is confused")                 //pokemon is confused
                || line.Contains("Player defeated")             //player wins
                || line.Contains("is fast asleep")              //sleeping
                || line.Contains("is paralyzed")                //paralyzed
                || line.Contains("is frozen solid")             //frozen
                || line.Contains("is in love ")                 //infatuated
                || line.Contains("is Disabled!")                //Disable
                || line.Contains("restored health!")            //HP restore berry
                || line.Contains("took in sunlight")            //charged solarbeam
                || line.Contains("sprang up")                   //charged bounce
                || line.Contains("whipped up a whirlwind")      //charged razor wind
                || line.Contains("hid underwater")              //charged dive
                || line.Contains("dug a hole")                  //charged dig
                || line.Contains("lowered its head")            //charged skull bash
                || line.Contains("hid underwater"))             //charged dive
            {
                line = Environment.NewLine + line;
            }

            //Use "What will [Pokemon] do?" to add new turn line.
            //BUG - Outrage, Thrash, etc. break this. 
            if (line.StartsWith("What will ") && line.EndsWith(" do?"))
            {
                if (newTurn)
                {
                    line = Environment.NewLine + "Turn: " + turnCounter;// + Environment.NewLine + line;
                    newTurn = false;
                }
                else //if(!newTurn)
                {
                    line = "";
                }
            }

            if (line.Contains("used Outrage!")
                || line.Contains("used Thrash!")
                || line.Contains("used Petal Dance!"))
            {
                //TODO - handle outrage, etc. as it breaks the turn counter.
            }

            //TODO - add other things that can happen during a turn.

            //Retrigger new turn if a turn starts.
            if (line.EndsWith("that's enough! Come back!")        //switch
                || line.Contains(" used ")                       //used a move
                || line.Contains("is fast asleep")              //sleeping
                || line.Contains("is paralyzed")                //paralyzed
                || line.Contains("is frozen solid")             //frozen
                || line.Contains("is in love ")                //infatuated
                || line.Contains("is Disabled!")                //Disable
                || line.Contains("restored health!")            //HP restore berry
                || line.Contains("forfeited the match!")        //Quit match
                || line.Contains("took in sunlight")            //charged solarbeam
                || line.Contains("sprang up")                   //charged bounce
                || line.Contains("whipped up a whirlwind")      //charged razor wind
                || line.Contains("hid underwater")              //charged dive
                || line.Contains("dug a hole")                  //charged dig
                || line.Contains("lowered its head")            //charged skull bash
                || line.Contains("hid underwater"))             //charged dive
            {
                if (!newTurn)
                {
                    newTurn = true;
                    turnCounter++;
                }
            }

            //Reset turn counter when a new battle starts
            if (line.EndsWith("would like to battle! ")         //New trainer battle
                || line.EndsWith("appeared! "))                 //Handle wild battles just because
            {
                turnCounter = 1;
                battleTypeLockout = false;

                //clear battle log
                Dispatcher.Invoke(() =>
                {
                    frontierSetsWindow.PossibleEnemies.Children.Clear();
                });
            }

            //Handle battler info - player is battler 0, enemy is battler 1.
            /*if (line.StartsWith("Battler 3"))
            {
                Debug.WriteLine("double " + line);
                setsDouble = new FrontierSetHandler().handleFrontierSet(line);
            }
            else if (line.StartsWith("Battler ") && (line.Contains("Battler 1") || line.Contains("Battler 3"))) 
            {
                Debug.WriteLine(line);
                sets = new FrontierSetHandler().handleFrontierSet(line);
            }*/

            //Check for battle type
            if (!battleTypeLockout)
            {
                if (line.Contains(" sent out "))
                {
                    setBattleType(line.Contains(" and ") ? 2 : 1);
                }
            }

            //handle single battles
            if (battleType == 1)
            {
                //set frontier sets window to single battle
                frontierSetsWindow.setEnemyCount(battleType);

                //Handle battler info - player is battler 0, enemy is battler 1.
                if (line.Contains("Battler 1"))
                {
                    //Debug.WriteLine(line);
                    sets = new FrontierSetHandler().handleFrontierSet(line);
                }

                //Post battler info only once they actually send it out - we received it early.
                /*if (line.Contains("sent out")
                    || Regex.IsMatch(line, @"^Foe .* was dragged out! $"))*/
                if (line.Contains("Turn"))
                {
                    Dispatcher.Invoke(() =>
                    {
                        frontierSetsWindow.PossibleEnemies.Children.Clear();
                        frontierSetsWindow.postSet(sets);
                    });
                }

                //Clear sets when we win or score a KO
                if (line.Contains("Player defeated ")
                    || (line.Contains("Foe") && line.Contains("fainted!")))
                {
                    Dispatcher.Invoke(() =>
                    {
                        frontierSetsWindow.PossibleEnemies.Children.Clear();
                    });
                }
            }

            //handle double battles - these have been handled completely differently (and probably better) because all hail the spaghetti code monster.
            if(battleType == 2)
            {
                //set frontier sets window to double battle
                frontierSetsWindow.setEnemyCount(battleType);

                //Handle battler info - player is battler 0/2, enemy is battler 1/3.
                if (line.Contains("Battler 1") || line.Contains("Battler 3"))
                {
                    Debug.WriteLine(line);
                    sets = new FrontierSetHandler().handleFrontierSet(line);
                }

                //start of double battle
                if (line.Contains(" sent out ") && (line.Contains(" and ")))
                {
                    string sentOut = line.Split(" sent out ")[1];
                    sentOut = sentOut.TrimEnd('!');

                    if (sentOut.Contains(" and "))
                    {
                        string[] pokemon = sentOut.Split(" and ");
                        sets = new FrontierSetHandler().handleFrontierSlotSimple(pokemon[0]);
                        setsDouble = new FrontierSetHandler().handleFrontierSlotSimple(pokemon[1]);

                        Dispatcher.Invoke(() =>
                        {
                            frontierSetsWindow.PossibleEnemies.Children.Clear();
                            frontierSetsWindow.postSet(sets);
                            frontierSetsWindow.postSetDouble(setsDouble);
                        });
                    }
                }
                else if (line.Contains(" sent out "))
                {
                    //Clear last fainted so we know it's been handled.
                    lastFainted = "";

                    Dispatcher.Invoke(() =>
                    {
                        frontierSetsWindow.PossibleEnemies.Children.Clear();
                        frontierSetsWindow.postSet(sets);
                        frontierSetsWindow.postSetDouble(setsDouble);
                    });
                }
                else if (line.Contains("Foe ") && (line.Contains(" fainted!")))
                {
                    lastFainted = line.Split("Foe ")[1];
                    lastFainted = lastFainted.Trim();
                    lastFainted = lastFainted.Split(" fainted!")[0];
                }

                //Clear sets when we win
                if (line.Contains("Player defeated "))
                {
                    Dispatcher.Invoke(() =>
                    {
                        frontierSetsWindow.PossibleEnemies.Children.Clear();
                    });
                }

                //If we fainted something and there's nothing to replace it, clear then reload
                if(line.Contains("Turn") && lastFainted != "")
                {
                    if (sets[0].name.Equals(lastFainted))
                    {
                        sets = new FrontierSetHandler().handleFrontierSet("");
                    }
                    else if (setsDouble[0].name.Equals(lastFainted))
                    {
                        setsDouble = new FrontierSetHandler().handleFrontierSet("");
                    }

                    Dispatcher.Invoke(() =>
                    {
                        frontierSetsWindow.PossibleEnemies.Children.Clear();
                        frontierSetsWindow.postSet(sets);
                        frontierSetsWindow.postSetDouble(setsDouble);
                    });

                    lastFainted = "";
                }
            }

            //We can't leak battler info, that would be cheating.
            if (line.StartsWith("Battler "))
            {
                line = "";
            }

            return line;
        }

        void setBattleType(int numOfOpponents)
        {
            battleType = numOfOpponents;
            battleTypeLockout = true;
        }

        string FixCaps(string input)
        {
            return Regex.Replace(input, @"\b[A-Z]{2,}\b", match =>
            {
                string word = match.Value.ToLower();
                return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word);
            });
        }
    }
}
