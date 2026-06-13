using emeraldbattlelog;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PokemonBattleLogger
{
    //This entire class is spaghetti
    public partial class FrontierSets : Window
    {
        private FileSystemWatcher? watcher;
        private PokemonSlot[] enemyTeam = new PokemonSlot[4];
        PokemonSlot[] sets;
        int enemyCount = 1;
        bool initializedTeams = false;
        StackPanel playerTeamPanel;
        StackPanel enemyTeamPanel;
        Grid teamsGrid;


        public FrontierSets()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("Starting watcher...");
            StartWatcher();
        }

        private void StartWatcher()
        {
            //Show the little pokeballs.
            if (!initializedTeams)
            {
                initializeTeams();
                initializedTeams = true;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public void setEnemyCount(int enemies)
        {
            enemyCount = enemies;
        }

        public void postSet(PokemonSlot[] possibleSets)
        {
            PossibleEnemies.Rows = enemyCount * 2;
            int addedSprite = 0;

            if (possibleSets == null || possibleSets.Length == 0)
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
                            continue;
                        }
                    }
                }

                if(addedSprite == 0)
                {
                    addSprite(set);
                }

                //Once we've finished one line, add a blank sprite to indent the next line.
                if (addedSprite == 4)
                {
                    var sprite = new Image
                    {
                        Width = 64,
                        Height = 64,
                        Stretch = Stretch.None,
                        Source = new BitmapImage(
                            new Uri($"pack://application:,,,/Images/pokemon_sprites/blank.png"))
                    };

                    RenderOptions.SetBitmapScalingMode(sprite, BitmapScalingMode.NearestNeighbor);

                    PossibleEnemies.Children.Add(sprite);
                }

                addedSprite++;

                var border = new Border
                {
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(5),
                    Margin = new Thickness(2)
                };

                var stack = new StackPanel();

                //Add top line: icon, pokemon name, index
                stack.Children.Add(addName(set));

                //Add next line: EVs
                stack.Children.Add(new TextBlock
                {
                    FontFamily = new FontFamily(
                        new Uri("pack://application:,,,/"),
                        "pack://application:,,,/Images/pokemon-emerald.ttf#Pokemon Emerald Regular"),
                    FontSize = 16,
                    Text = $"        {set.EVs}"
                });

                //Add next line: Item sprite, item name
                stack.Children.Add(addItem(set));

                //Add next lines: Moves
                stack.Children.Add(new TextBlock
                {
                    FontFamily = new FontFamily(
                        new Uri("pack://application:,,,/"),
                        "pack://application:,,,/Images/pokemon-emerald.ttf#Pokemon Emerald Regular"),
                    FontSize = 16,
                    Text = $"    • {set.move1}\n    • {set.move2}\n    • {set.move3}\n    • {set.move4}"
                });

                //Display it.
                border.Child = stack;
                PossibleEnemies.Children.Add(border);
            }   
        }

        public void displayTeam(string[] team, bool isPlayer)
        {
            Dispatcher.Invoke(() =>
            {
                StackPanel teamPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                Debug.WriteLine(team.ToString());

                int teammateCount = team.Count(x => !string.IsNullOrEmpty(x));
                int pokeballCount = Math.Max(3 - teammateCount, 0);

                Debug.WriteLine("Teammate count: " + teammateCount + ", " + "Pokeball count: " + pokeballCount);

                for (int i = 0; i < teammateCount; i++)
                {
                    teamPanel.Children.Add(initializeIcon(team[i]));
                }
                for (int i = 0; i < pokeballCount; i++)
                {
                    teamPanel = initializePokeballIcon(teamPanel);
                }

                if (isPlayer)
                {
                    playerTeamPanel = teamPanel;
                }
                else //if(!isPlayer)
                {
                    enemyTeamPanel = teamPanel;
                }

                //Make visible
                Grid.SetColumn(playerTeamPanel, 0);
                Grid.SetColumn(enemyTeamPanel, 2);

                teamsGrid.Children.Clear();
                teamIcons.Children.Clear();

                teamsGrid.Children.Add(playerTeamPanel);
                teamsGrid.Children.Add(enemyTeamPanel);
                teamIcons.Children.Add(teamsGrid);
            });
        }

        //Set up the little shaking pokeball sprites.
        public void initializeTeams()
        {
            //Set up the StackPanels and put them in the right spaces.
            teamsGrid = new Grid();
            teamsGrid.Children.Clear();
            teamIcons.Children.Clear();

            playerTeamPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            enemyTeamPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            teamsGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

            teamsGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = GridLength.Auto
            });
            teamsGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            });
            teamsGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = GridLength.Auto
            });

            //Add pokeball sprites, 3 per team.
            //TODO - make this dynamic based on team size
            //Not possible with current Lua script, though.
            for (int i = 0; i < 3; i++)
            {
                playerTeamPanel = initializePokeballIcon(playerTeamPanel);
                enemyTeamPanel = initializePokeballIcon(enemyTeamPanel);
            }

            //Make visible
            Grid.SetColumn(playerTeamPanel, 0);
            Grid.SetColumn(enemyTeamPanel, 2);
            teamsGrid.Children.Add(playerTeamPanel);
            teamsGrid.Children.Add(enemyTeamPanel);
            teamIcons.Children.Add(teamsGrid);
        }

        //This will only be called after postSet is called.
        //This function fills up any remaining spaces in the first
        //two lines with blank sprites, ensuring the second battler's
        //info starts on line 3.
        public void postSetDouble(PokemonSlot[] possibleSets)
        {
            while(PossibleEnemies.Children.Count < 10)
            {
                var sprite = new Image
                {
                    Width = 64,
                    Height = 64,
                    Stretch = Stretch.None,
                    Source = new BitmapImage(
                    new Uri($"pack://application:,,,/Images/pokemon_sprites/blank.png"))
                };

                RenderOptions.SetBitmapScalingMode(sprite, BitmapScalingMode.NearestNeighbor);

                PossibleEnemies.Children.Add(sprite);
            }

            postSet(possibleSets);
        }

        public void addSprite(PokemonSlot set)
        {
            var border = new Border
            {
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                Margin = new Thickness(2)
            };

            var stack = new StackPanel();

            //Load the correct sprite.
            var sprite = new Image
            {
                Width = 64,
                Height = 64,
                Margin = new Thickness(0),
                Stretch = Stretch.None,
                Source = new BitmapImage(
                    new Uri($"pack://application:,,,/Images/pokemon_sprites/{set.name.Replace(". ", "_").ToLower()}.png")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            RenderOptions.SetBitmapScalingMode(sprite, BitmapScalingMode.NearestNeighbor);

            //Add the name.
            stack.Children.Add(new TextBlock
            {
                Text = set.name,
                FontFamily = new FontFamily(
                    new Uri("pack://application:,,,/"),
                    "./Images/#Pokemon Emerald Regular"),
                FontSize = 32,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });

            //Add the loaded sprite underneath.
            stack.Children.Add(sprite);

            border.Child = stack;

            PossibleEnemies.Children.Add(border);
        }

        public Image initializeIcon(string name)
        {
            DispatcherTimer itemTimer;
            int frame = 0;

            var bitmap = new BitmapImage(
                            new Uri($"pack://application:,,,/Images/icons/{name.Replace(". ", "_").ToLower()}.png"));

            //The icons have both frames contained in the same spritesheet, only display the top half.
            var cropped = new CroppedBitmap(
                bitmap,
                new Int32Rect(0, 0, 32, 32));

            var itemImage = new Image
            {
                Width = 32,
                Height = 32,
                Source = cropped
            };

            //Make a timer for animating the icon and handle it.
            itemTimer = new DispatcherTimer();
            itemTimer.Interval = TimeSpan.FromMilliseconds(300);
            itemTimer.Tick += (s, e) =>
            {
                frame = 1 - frame;
                itemImage = UpdateItemFrame(itemImage, frame, "icons/" + name.ToLower(), 32);
            };

            itemTimer.Start();

            RenderOptions.SetBitmapScalingMode(itemImage, BitmapScalingMode.NearestNeighbor);

            return itemImage;
        }

        public StackPanel addName(PokemonSlot set)
        {
            var itemPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            //Display the icon and the set name + index.
            itemPanel.Children.Add(initializeIcon(set.name));
            itemPanel.Children.Add(new TextBlock
            {
                Text = "  " + set.name + set.index,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily(
                    new Uri("pack://application:,,,/"),
                    "./Images/#Pokemon Emerald Regular"),
                FontSize = 24,
                VerticalAlignment = VerticalAlignment.Center
            });

            return itemPanel;
        }
        public StackPanel addItem(PokemonSlot set)
        {
            var itemPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            string itemNameFixed = set.item.ToLower();
            itemNameFixed = itemNameFixed.Replace(" ", "_");
            itemNameFixed = itemNameFixed.Replace("'", "");
            itemNameFixed = itemNameFixed.Replace("brightpowder", "bright_powder"); //pkhex has this issue too
            itemNameFixed = itemNameFixed.Replace("nevermeltice", "never_melt_ice");
            itemNameFixed = itemNameFixed.Replace("twistedspoon", "twisted_spoon");
            itemNameFixed = itemNameFixed.Replace("white_herb", "in_battle_herb"); //the fuck?

            var itemImage = new Image
            {
                Width = 24,
                Height = 24,
                Source = new BitmapImage(
                            new Uri($"pack://application:,,,/Images/items/{itemNameFixed}.png")),
            };

            RenderOptions.SetBitmapScalingMode(itemImage, BitmapScalingMode.NearestNeighbor);

            itemPanel.Children.Add(itemImage);

            itemPanel.Children.Add(new TextBlock
            {
                Text = $" {set.item}",
                FontFamily = new FontFamily(
                    new Uri("pack://application:,,,/"),
                    "./Images/#Pokemon Emerald Regular"),
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center
            });


            return itemPanel;
        }

        public StackPanel initializePokeballIcon(StackPanel teamPanel)
        {
            var pokeball = new BitmapImage(
                            new Uri($"pack://application:,,,/Images/pokeball_selection.png"));

            var croppedPokeball = new CroppedBitmap(
                pokeball,
                new Int32Rect(0, 0, 32, 32));

            var pokeballImage = new Image
            {
                Width = 32,
                Height = 32,
                Margin = new Thickness(0, 5, 0, 0),
                Source = croppedPokeball
            };

            DispatcherTimer itemTimer;
            int frame = 0;

            Random rng = new Random();
            int frameIndex = rng.Next(0, 30); //0-29

            //Shaking pokeball pattern
            int[] frames =
                {
                    1, 1, 0, 0, 2, 2, 0, 0,
                    1, 1, 0, 0, 2, 2, 0, 0,
                    1, 0, 2, 0, 1, 0, 2, 0,
                    0, 0, 0, 0, 0, 0, 0, 0
                };

            //Make a timer for animating the icon and handle it.
            itemTimer = new DispatcherTimer();
            itemTimer.Interval = TimeSpan.FromMilliseconds(66.7);

            itemTimer.Tick += (s, e) =>
            {
                frame = frames[frameIndex];
                frameIndex = (frameIndex + 1) % frames.Length;
                pokeballImage = UpdateItemFrame(pokeballImage, frame, "pokeball_selection", 32);
            };

            itemTimer.Start();

            RenderOptions.SetBitmapScalingMode(pokeballImage, BitmapScalingMode.NearestNeighbor);


            //Display the icons
            teamPanel.Children.Add(pokeballImage);

            return teamPanel;
        }

        private Image UpdateItemFrame(Image itemImage, int frame, string name, int frameSize)
        {
            var bitmap = new BitmapImage(
                new Uri($"pack://application:,,,/Images/{name.Replace(". ", "_")}.png"));

            //I don't know how this code works but my friend Malamar said it would. and it does.
            int y = frame * frameSize;

            itemImage.Source = new CroppedBitmap(
                bitmap,
                new Int32Rect(0, y, frameSize, frameSize));

            itemImage.Source = new CroppedBitmap(
                bitmap,
                new Int32Rect(0, y, frameSize, frameSize));

            RenderOptions.SetBitmapScalingMode(
                itemImage,
                BitmapScalingMode.NearestNeighbor);

            return itemImage;
        }
    }

}
