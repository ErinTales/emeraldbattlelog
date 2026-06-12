using emeraldbattlelog;
using System.Collections;
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
using System.Windows.Threading;
using System.Windows.Input;

namespace PokemonBattleLogger
{
    public partial class FrontierSets : Window
    {
        private FileSystemWatcher? watcher;
        private PokemonSlot[] enemyTeam = new PokemonSlot[4];
        PokemonSlot[] sets;
        int enemyCount = 1;

        public FrontierSets()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("Starting watcher...");
            StartWatcher();
        }

        private void StartWatcher()
        {

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
                            //return;
                            continue;
                        }
                    }
                }

                if(addedSprite == 0)
                {
                    addSprite(set);
                }

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

                stack.Children.Add(addName(set));

                stack.Children.Add(new TextBlock
                {
                    FontFamily = new FontFamily(
                        new Uri("pack://application:,,,/"),
                        "pack://application:,,,/Images/pokemon-emerald.ttf#Pokemon Emerald Regular"),
                    FontSize = 16,
                    Text = $"        {set.EVs}"
                });

                stack.Children.Add(addItem(set));

                stack.Children.Add(new TextBlock
                {
                    FontFamily = new FontFamily(
                        new Uri("pack://application:,,,/"),
                        "pack://application:,,,/Images/pokemon-emerald.ttf#Pokemon Emerald Regular"),
                    FontSize = 16,
                    Text = $"    • {set.move1}\n    • {set.move2}\n    • {set.move3}\n    • {set.move4}"
                });

                border.Child = stack;

                PossibleEnemies.Children.Add(border);
            }
        }

        //This will only be called after postSet is called.
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

            if (set.name.Contains("."))
            {
                set.name = "mr_mime";
            }

            var sprite = new Image
            {
                Width = 64,
                Height = 64,
                Margin = new Thickness(0),
                Stretch = Stretch.None,
                Source = new BitmapImage(
                    new Uri($"pack://application:,,,/Images/pokemon_sprites/{set.name.ToLower()}.png")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (set.name.Contains("mr_mime"))
            {
                set.name = "Mr. Mime";
            }

            RenderOptions.SetBitmapScalingMode(sprite, BitmapScalingMode.NearestNeighbor);

            stack.Children.Add(new TextBlock
            {
                Text = set.name, // Species
                FontFamily = new FontFamily(
                    new Uri("pack://application:,,,/"),
                    "./Images/#Pokemon Emerald Regular"),
                FontSize = 32,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });

            stack.Children.Add(sprite);

            border.Child = stack;

            PossibleEnemies.Children.Add(border);
        }

        public StackPanel addName(PokemonSlot set)
        {
            var itemPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            DispatcherTimer itemTimer;
            bool frame = false;

            if (set.name.Contains("."))
            {
                set.name = "mr_mime";
            }

            var bitmap = new BitmapImage(
                            new Uri($"pack://application:,,,/Images/icons/{set.name.ToLower()}.png"));

            if (set.name.Contains("mr_mime"))
            {
                set.name = "Mr. Mime";
            }

            var cropped = new CroppedBitmap(
                bitmap,
                new Int32Rect(0, 0, 32, 32));

            var itemImage = new Image
            {
                Width = 32,
                Height = 32,
                Source = cropped
            };

            itemTimer = new DispatcherTimer();
            itemTimer.Interval = TimeSpan.FromMilliseconds(300);
            itemTimer.Tick += (s, e) =>
            {
                frame = !frame;
                itemImage = UpdateItemFrame(itemImage, frame, set.name.ToLower());
            };

            itemTimer.Start();

            RenderOptions.SetBitmapScalingMode(itemImage, BitmapScalingMode.NearestNeighbor);

            itemPanel.Children.Add(itemImage);
            itemPanel.Children.Add(new TextBlock
            {
                Text = "  " + set.name + set.index, // Species
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

        private Image UpdateItemFrame(Image itemImage, bool frame, string name)
        {
            if (name.Contains("."))
            {
                name = "mr_mime";
            }

            var bitmap = new BitmapImage(
                new Uri($"pack://application:,,,/Images/icons/{name}.png"));

            if (name.Contains("mr_mime"))
            {
                name = "Mr. Mime";
            }

            int y = frame ? 32 : 0;

            itemImage.Source = new CroppedBitmap(
                bitmap,
                new Int32Rect(0, y, 32, 32));

            RenderOptions.SetBitmapScalingMode(
                itemImage,
                BitmapScalingMode.NearestNeighbor);

            return itemImage;
        }
    }

}
