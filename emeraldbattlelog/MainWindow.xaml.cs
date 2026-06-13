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

    // Interaction logic for MainWindow.xaml
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
        string[] enemyPokemon = new string[2];

        string[] playerTeamRevealed = new string[6];
        string[] enemyTeamRevealed = new string[6];

        string[] playerTeamStatus = new string[6];
        string[] enemyTeamStatus = new string[6];

        public MainWindow()
        {
            frontierSetsWindow.Show();

            InitializeComponent();
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

            //I don't know how to not hard code these, all attempts so far have failed.
            var path = @"C:\Users\Erin\source\repos\emeraldbattlelog\emeraldbattlelog\pokemon lua\battlelog.txt";

            watcher = new FileSystemWatcher(
            @"C:\Users\Erin\source\repos\emeraldbattlelog\emeraldbattlelog\pokemon lua",
            "battlelog.txt");

            if (File.Exists(path))
            {
                lastPosition = new FileInfo(path).Length;
            }

            //Whenever the file changes...
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

                    //While there's still lines left to read...
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

                                    //Large font for significant things.
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

                    //Remember where we stopped reading
                    lastPosition = stream.Position;
                }
                catch (Exception ex)
                {
                   //Don't crash thx
                }
            };

            watcher.EnableRaisingEvents = true;
        }

        private string lineCleanup(string line)
        {
            //Debug.WriteLine(line);

            //Remove jank from the text, like japanese characters,
            //full width characters, newline tags, etc.
            line = Regex.Replace(line, @"<[^>]*>", " ");
            line = line.Replace("　", " ");
            line = line.Replace("！", "!");
            line = line.Replace("？", "?");
            line = line.Replace("。", ".");
            line = line.Replace(@"\'", "'");
            line = line.Replace("たち", "");
            line = line.Replace("け", "");
            line = line.Replace("ラ", "");
            line = line.Replace("ウエ", "Pokémon");
            line = line.Replace("シ", "");
            line = line.Replace("ス", "");
            line = line.Replace("⋯", "...");
            line = line.Replace("♂", "");
            line = line.Replace("さ", "");
            line = line.Replace("♀", "");
            line = line.Replace("こ", "");
            line = line.Replace("あ", "");

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

            //Don't display Frontier items message
            if(line.Contains("Items can't be used"))
            {
                line = "";
            }

            //Don't display forfeit query
            if (line.Contains("Would you like to forfeit the match and quit now?"))
            {
                line = "";
            }

            //Remove the all caps easy chat battle end text.
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

            //Extra corrections not caught by the above regular expression
            line = line.Replace("PORYGON2", "Porygon2");
            line = line.Replace("ひ", "é");
            line = line.Replace("Hp", "HP");
            line = line.Replace("POKéMON", "Pokémon");
            line = line.Replace("POKéFAN", "Pokéfan");
            line = line.Replace("oneーhit Ko!", "oneーhit KO!");
            line = line.Replace("WILLーOーWISP", "WillーOーWisp");
            line = line.Replace("SANDーATTACK", "SandーAttack");
            line = line.Replace("MUDーSLAP", "MudーSlap");
            line = line.Replace("DOUBLEーEDGE", "DoubleーEdge");
            line = line.Replace("uSing", "using");

            //Remove weird/buggy "What will [partial type name]" spam. (the buffer seems to partially overwrite itself for some reason)
            if (line.StartsWith("What will ") && !line.EndsWith(" do?"))
            {
                line = "";
            }
            
            //Add newlines before select messages, for formatting reasons
            if (line.EndsWith("would like to battle! ")         //New trainer battle
                || line.EndsWith("appeared! ")                  //wild battle
                || line.Contains("Got away safely!")            //more wild battle
                || line.Contains("used")                        //use a move
                || line.Contains("sent out")                    //opponent sends out pokemon
                || line.Contains("Go!")                         //player sends out pokemon
                || line.Contains("Go for it,")                  //player sends out pokemon
                || line.Contains("Do it! ")                     //player sends out pokemon
                || line.Contains("dragged out")                 //sent out due to roar/ww
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
            //BUG - Outrage, Thrash, etc. can break this when used by the player.
            if (line.StartsWith("What will ") && line.EndsWith(" do?"))
            {
                if (newTurn)
                {
                    line = Environment.NewLine + "Turn: " + turnCounter;
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

            //Increment turn counter when we know the turn has definitely started.
            if (line.EndsWith("that's enough! Come back!")      //switch
                || line.Contains(" used ")                      //used a move
                || line.Contains("is fast asleep")              //sleeping
                || line.Contains("is paralyzed")                //paralyzed
                || line.Contains("is frozen solid")             //frozen
                || line.Contains("is in love ")                 //infatuated
                || line.Contains("is Disabled!")                //Disable
                || line.Contains("restored health!")            //HP restore berry
                || line.Contains("forfeited the match!")        //Quit match
                || line.Contains("took in sunlight")            //charged solarbeam
                || line.Contains("sprang up")                   //charged bounce
                || line.Contains("flew up high")                //charged fly
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
            if (line.Contains("would like to battle!")         //New trainer battle
                || line.Contains("appeared!"))                 //Handle wild battles just because
            {
                turnCounter = 1;
                battleTypeLockout = false;

                //clear frontier sets.
                Dispatcher.Invoke(() =>
                {
                    playerTeamRevealed = new string[6];
                    enemyTeamRevealed = new string[6];
                    playerTeamStatus = new string[6];
                    enemyTeamStatus = new string[6];
                    frontierSetsWindow.initializeTeams();
                    frontierSetsWindow.PossibleEnemies.Children.Clear();
                });
            }

            //Clear frontier sets window when we win.
            if (line.Contains("Player defeated "))
            {
                Dispatcher.Invoke(() =>
                {
                    playerTeamRevealed = new string[6];
                    enemyTeamRevealed = new string[6];
                    playerTeamStatus = new string[6];
                    enemyTeamStatus = new string[6];
                    //frontierSetsWindow.initializeTeams();
                    frontierSetsWindow.teamIcons.Children.Clear();
                    frontierSetsWindow.PossibleEnemies.Children.Clear();
                });
            }

            //Show the Battle Frontier sets.
            line = frontierSetsWindowHandler(line);

            /*
                We can't leak (enemy) battler info, that would be cheating.
                Any lines like this aren't actually from the buffer, 
                they're the Lua script dumping battler info.
            */
            if (line.StartsWith("Battler "))
            {
                line = "";
            }

            //Line is fixed, now we return it to be printed out.
            return line;
        }

        string frontierSetsWindowHandler(string line)
        {

            //Set battle type if not done so already (1 for single, 2 for double)
            if (!battleTypeLockout)
            {
                if (line.Contains(" sent out "))
                {
                    setBattleType(line.Contains(" and ") ? 2 : 1);
                }
            }

            //Wild battle
            if (line.Contains("Wild ") && line.Contains("appeared!"))
            {
                setBattleType(0);
            }

            //handle single battles
            if (battleType == 1)
            {
                //set frontier sets window to single battle
                frontierSetsWindow.setEnemyCount(battleType);

                //When enemy is sent out.
                if (line.Contains(" sent out ") || (line.Contains("Foe ") && line.Contains("dragged out")))
                {
                    string sentOut = "";
                    if (line.Contains(" sent out "))
                    {
                        sentOut = line.Split(" sent out ")[1];
                        sentOut = sentOut.TrimEnd('!');
                    }
                    else if (line.Contains("dragged out"))
                    {
                        sentOut = line.Split(" was dragged out")[0];
                        sentOut = line.Split("Foe ")[1];
                    }

                    //Prep the frontier sets based on enemy pokemon.
                    sets = new FrontierSetHandler().handleFrontierSlotSimple(line);
                }

                //Post battler info only once they actually send it out - we received it early.
                //Basically we just don't want to send it out as the line is first printing bc
                //it looks odd - it shows up before you can visually tell what you're fighting.
                if (line.Contains("Turn") || line.Contains("Go!") || line.Contains("Go for it,") || line.Contains("Do it! ") || line.Contains("used"))
                {
                    Dispatcher.Invoke(() =>
                    {
                        frontierSetsWindow.PossibleEnemies.Children.Clear();
                        frontierSetsWindow.postSet(sets);
                    });
                }

                //Clear sets when we score a KO
                if ((line.Contains("Foe") && line.Contains("fainted!"))
                    || (line.Contains("Wild") && line.Contains("fainted!")))
                {
                    Dispatcher.Invoke(() =>
                    {
                        frontierSetsWindow.PossibleEnemies.Children.Clear();
                    });
                }
            }

            //handle double battles
            if (battleType == 2)
            {
                //set frontier sets window to double battle
                frontierSetsWindow.setEnemyCount(battleType);

                //start of double battle
                if (line.Contains(" sent out ") && (line.Contains(" and ")))
                {
                    //Trim and split apart to get the enemy team.
                    string sentOut = line.Split(" sent out ")[1];
                    sentOut = sentOut.TrimEnd('!');
                    enemyPokemon = sentOut.Split(" and ");

                    //Prep the frontier sets based on enemy pokemon.
                    sets = new FrontierSetHandler().handleFrontierSlotSimple(enemyPokemon[0]);
                    setsDouble = new FrontierSetHandler().handleFrontierSlotSimple(enemyPokemon[1]);
                }
                //After a KO in double battles, figure out what's being sent out.
                //Roar/Whirlwind might still have issues in doubles. Baton Pass shouldn't but I haven't asked a "no" question yet.
                else if (line.Contains(" sent out ") || ((line.Contains("Turn") || line.Contains("used")) && lastFainted != "")) //Replacing an enemy || There are no enemies left (thx gen 3)
                {
                    string sentOut = "";

                    if (line.Contains(" sent out "))
                    {
                        sentOut = line.Split(" sent out ")[1];
                        sentOut = sentOut.TrimEnd('!');
                    }
                    else //if !line.Contains(" sent out ")) // there are no enemies left
                    {
                        sentOut = "";
                    }

                    //Replace the enemy pokemon with whatever was sent out.
                    int i = 0;
                    foreach (string pokemon in enemyPokemon)
                    {
                        if (pokemon.Equals(lastFainted))
                        {
                            enemyPokemon[i] = sentOut;
                        }
                        i++;
                    }

                    //Prep the frontier sets based on enemy pokemon.
                    sets = new FrontierSetHandler().handleFrontierSlotSimple(enemyPokemon[0]);
                    setsDouble = new FrontierSetHandler().handleFrontierSlotSimple(enemyPokemon[1]);

                    //Clear last fainted so we know it's been handled.
                    lastFainted = "";
                }
                //Keep track of what fainted so we know which set to replace.
                else if (line.Contains("Foe ") && (line.Contains(" fainted!")))
                {
                    lastFainted = line.Split("Foe ")[1];
                    lastFainted = lastFainted.Trim();
                    lastFainted = lastFainted.Split(" fainted!")[0];
                }

                //Post battler info only once they actually send it out - we received it early.
                //Basically we just don't want to send it out as the line is first printing bc
                //it looks odd - it shows up before you can visually tell what you're fighting.
                if (line.Contains("Turn") || line.Contains("Go!") || line.Contains("Go for it,") || line.Contains("Do it! ") || line.Contains("used"))
                {
                    Dispatcher.Invoke(() =>
                    {
                        frontierSetsWindow.PossibleEnemies.Children.Clear();
                        frontierSetsWindow.postSet(sets);
                        frontierSetsWindow.postSetDouble(setsDouble);
                    });
                }
            }

            //Send teammates off to be displayed at the bottom.
            handleTeamIcons(line);
            updateStatus(line);

            return line;
        }

        //Checks for lines where someone is sending out a pokemon and adds
        //them to the little team tracker near the bottom of the window
        void handleTeamIcons(string line)
        {
            if (line.Contains("Go!") || line.Contains("Go for it,") || line.Contains("Do it! ") || (!line.Contains("Foe ") && line.Contains("dragged out")))
            {
                string pokemonName = "";

                if (line.Contains("Go!"))
                {
                    if (line.Contains(" and "))
                    {
                        string sentOut = line.Split("Go! ")[1];
                        sentOut = sentOut.TrimEnd('!');

                        pokemonName = sentOut.Split(" and ")[1];
                        displayTeamIcons(line, playerTeamRevealed, sentOut.Split(" and ")[0], true);
                    }
                    else //if (!line.Contains(" and "))
                    {
                        pokemonName = line.Split("Go! ")[1].TrimEnd('!').Replace(". ", "_").ToLower().Trim();
                    }
                }
                else if (line.Contains("Go for it,"))
                {
                    pokemonName = line.Split("Go for it, ")[1].TrimEnd('!').Replace(". ", "_").ToLower().Trim();
                }
                else if (line.Contains("Do it! "))
                {
                    pokemonName = line.Split("Do it! ")[1].TrimEnd('!').Replace(". ", "_").ToLower().Trim();
                }
                else if (line.Contains("dragged out"))
                {
                    //Weird quirk here - the newline ("\r\n") we added earlier needs removed
                    pokemonName = line.Split(" was dragged out")[0].Replace(". ", "_").ToLower().Trim();
                }

                displayTeamIcons(line, playerTeamRevealed, pokemonName, true);
            }

            if (line.Contains(" sent out ") || (line.Contains("Foe ") && line.Contains("dragged out")))
            {
                string pokemonName = "";

                if (line.Contains("sent out "))
                {
                    if (line.Contains(" and "))
                    {
                        string sentOut = line.Split(" sent out ")[1];
                        sentOut = sentOut.TrimEnd('!');

                        pokemonName = sentOut.Split(" and ")[1];
                        displayTeamIcons(line, enemyTeamRevealed, sentOut.Split(" and ")[0], false);
                    }
                    else //if (!line.Contains(" and "))
                    {
                        pokemonName = line.Split("sent out ")[1].TrimEnd('!').Replace(". ", "_").ToLower().Trim();
                    }
                }
                else if (line.Contains("dragged out"))
                {
                    pokemonName = line.Split(" was dragged out")[0].Split("Foe ")[1].Replace(". ", "_").ToLower().Trim();
                }

                displayTeamIcons(line, enemyTeamRevealed, pokemonName, false);
            }
        }

        //For now this only displays fainting. It supports other status conditions, but
        //it's not always possible to tell when status is caused/cured due to Natural Cure,
        //Heal Bell, Aromatherapy, and some facilities allowing statusing outside/between battles.

        void updateStatus(string line)
        {
            foreach (string status in Tables.status)
            {
                //match exact status name
                if (Regex.IsMatch(
                    line,
                    $@"\b{Regex.Escape(status)}\b",
                    RegexOptions.IgnoreCase))
                {
                    //We only want to handle fainting for now.
                    if(status != "fainted")
                    {
                        return;
                    }

                    foreach (string pokemonName in Tables.pokemon)
                    {
                        //match exact pokemon name
                        if (Regex.IsMatch(
                            line,
                            $@"\b{Regex.Escape(pokemonName)}\b",
                            RegexOptions.IgnoreCase))
                        {
                            if (line.Contains("Foe "))
                            {
                                //Search the current enemy team for our target
                                for (int i = 0; i < enemyTeamRevealed.Length; i++)
                                {
                                    if (!String.IsNullOrEmpty(enemyTeamRevealed[i]))
                                    {
                                        if (enemyTeamRevealed[i].ToLower().Equals(pokemonName.ToLower()))
                                        {
                                            enemyTeamStatus[i] = status;

                                            //If they get cured by a berry (or heal naturally) delete the status.
                                            //This never gets called atm because only fainting is handled.
                                            if (line.Contains("cured") || line.Contains("defrosted") || line.Contains("woke up"))
                                            {
                                                enemyTeamStatus[i] = "";
                                            }

                                            frontierSetsWindow.displayTeam(enemyTeamRevealed, enemyTeamStatus, false);
                                        }
                                    }
                                }
                            }
                            else //if(!line.Contains("Foe ")
                            {
                                for (int i = 0; i < playerTeamRevealed.Length; i++)
                                {
                                    if (!String.IsNullOrEmpty(playerTeamRevealed[i]))
                                    {
                                        if (playerTeamRevealed[i].ToLower().Equals(pokemonName.ToLower()))
                                        {
                                            playerTeamStatus[i] = status;

                                            //If they get cured by a berry (or heal naturally) delete the status.
                                            //This never gets called atm because only fainting is handled.
                                            if (line.Contains("cured") || line.Contains("defrosted") || line.Contains("woke up"))
                                            {
                                                playerTeamStatus[i] = "";
                                            }

                                            frontierSetsWindow.displayTeam(playerTeamRevealed, playerTeamStatus, true);

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        void displayTeamIcons(string line, string[] team, string teammate, bool isPlayer)
        {
            //teammate.Remove(  

            if(team.Contains(teammate))
            {
                return;
            }
            else //if (!team.Contains(teammate))
            {
                for (int i = 0; i < team.Length; i++)
                {
                    if (team[i] == null)
                    {
                        team[i] = teammate;
                        if (isPlayer)
                        {
                            playerTeamRevealed = team;
                            frontierSetsWindow.displayTeam(playerTeamRevealed, playerTeamStatus, isPlayer);
                        }
                        else //if (!isPlayer)
                        {
                            enemyTeamRevealed = team;
                            frontierSetsWindow.displayTeam(enemyTeamRevealed, enemyTeamStatus, isPlayer);
                        }

                        break;
                    }
                }
            }
        }

        void setBattleType(int numOfOpponents)
        {
            battleType = numOfOpponents;
            battleTypeLockout = true;
        }

        //My friend Malamar is responsible for whatever this does. And also for pretty much every regex you see.
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
