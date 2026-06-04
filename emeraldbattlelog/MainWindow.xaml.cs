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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        private bool newTurn = true;
        private int turnCounter = 1;


        public MainWindow()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("Starting watcher...");
            //BattleLog.Text = "Pokemon Battle Logger v0.3.1 Starting...\n";
            StartWatcher();
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

            //Fix "Got away safely"
            if(line.Equals("  Got away safely! "))
            {
                line = "Got away safely!";
            }

            //Remove uppercase spam.
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            foreach (string pokemonName in Tables.pokemon)
            {
                line = line.Replace(pokemonName, textInfo.ToTitleCase(pokemonName), StringComparison.OrdinalIgnoreCase);
            }

            foreach (string moveName in Tables.battleMoves)
            {
                line = line.Replace(moveName, textInfo.ToTitleCase(moveName), StringComparison.OrdinalIgnoreCase);

                //While we're here, don't display random move names alone.
                if (line.ToLower().Equals(moveName.ToLower()))
                {
                    line = "";
                }
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
            line = line.Replace("TraceD", "Traced");    ////Lazy fix - bug from "TRAED" having "TRACE" in it.


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
                || line.Contains("is paralyzed")                //paralzed
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
                || line.Contains("is paralyzed")                //paralzed
                || line.Contains("is frozen solid")             //frozen
                || line.Contains("is in love "))                //infatuated
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
            }

            //Handle battler info - player is battler 0, enemy is battler 1.
            //We can't leak battler 1's info unless it's visible, that would be cheating.
            if (line.StartsWith("Battler "))
            {
                //TODO - this needs to jump elsewhere to handle team displays.
                sets = new FrontierSetHandler().handleFrontierSet(line);
                line = "";
            }

            //Post battler info only once they actually send it out - we received it early.
            if (line.Contains("sent out")
                || line.Contains("was dragged out!"))
            {
                Dispatcher.Invoke(() =>
                {
                    PossibleEnemies.Children.Clear();
                    postSet(sets);
                });
            }
            
            //Clear sets when we win
            if (line.Contains("Player defeated ")
                || (line.Contains("Foe") && line.Contains("fainted!")))
            {
                Dispatcher.Invoke(() =>
                {
                    PossibleEnemies.Children.Clear();
                });
            }

            return line;
        }


        //                PossibleEnemies.Children.Clear();

        public void postSet(PokemonSlot[] possibleSets)
        {
            if (possibleSets.Length == 0)
            {
                return;
            }

            foreach (var set in possibleSets)
            {
                if (set == null)
                    continue;

                foreach (var enemy in enemyTeam)
                {
                    if (enemy != null)
                    {
                        //exit if it's a duplicate.
                        if (enemy.name.Equals(set.name))
                        {
                            return;
                        }
                    }
                }

                var border = new Border
                {
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(5),
                    Margin = new Thickness(2)
                };

                var stack = new StackPanel();

                stack.Children.Add(new TextBlock
                {
                    Text = set.name + set.index, // Species
                    FontWeight = FontWeights.Bold
                });

                stack.Children.Add(new TextBlock
                {
                    Text = $"Item: {set.item}"
                });

                stack.Children.Add(new TextBlock
                {
                    Text = $"- {set.move1}\n- {set.move2}\n- {set.move3}\n- {set.move4}"
                });

                border.Child = stack;

                PossibleEnemies.Children.Add(border);
            }
        }



        private void otherText(string line)
        {

        }
    }
}
