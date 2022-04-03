using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Formula1;

public partial class TablesWindow : Window
{
    private string _mode;
    private Brush equalledRecordBrush = Brushes.IndianRed;
    private Brush newRecordBrush = Brushes.Red;

    public TablesWindow()
    {
        InitializeComponent();
    }

    // TODO In champions list exclude leader of the current season still in progress

    private void Window_ContentRendered(object sender, EventArgs e)
    {
        MatchedBorder.Background = equalledRecordBrush;
        MatchedRecordText.Foreground = equalledRecordBrush;
        NewRecordBorder.Background = newRecordBrush;
        NewRecordText.Foreground = newRecordBrush;

        Jbh.UiServices.SetBusyState();
        RecordsRubricPanel.Visibility = Visibility.Hidden;
        DisplayButton.IsEnabled = false;
        CountTextBlock.Text = $"{Core.Instance.Drivers.Count} drivers";
    }

    private void DisplayDriverStartsAndFinishes()
    {
        List<Tuple<int, int>> driverStarts = new List<Tuple<int, int>>();
        List<Tuple<int, int>> driverFinishes = new List<Tuple<int, int>>();
        foreach (Driver d in Core.Instance.Drivers.Values)
        {
            Tuple<int, int> tup = new Tuple<int, int>(d.RuntimeRacesStarted, d.Key);
            driverStarts.Add(tup);
            tup = new Tuple<int, int>(d.RuntimeRacesFinished, d.Key);
            driverFinishes.Add(tup);
        }

        driverStarts.Sort();
        driverStarts.Reverse();
        driverFinishes.Sort();
        driverFinishes.Reverse();

        MakeHeading("DRIVERS' RACE STARTS (TOP 40)", 1);
        int rank = 0;
        int prev = -1;
        int count = 0;
        int prevrank = 0;
        foreach (Tuple<int, int> tp in driverStarts)
        {
            count++;
            if (tp.Item1 != prev)
            {
                rank = count;
                if (rank > 40)
                {
                    break;
                }

                prev = tp.Item1;
            }

            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            spl.Children.Add(RankingTextBlock(rank, ref prevrank));
            TextBlock tb = new TextBlock()
            {
                Text = tp.Item1.ToString(Core.CultureUk), FontWeight = FontWeights.Bold, Foreground = Brushes.SeaGreen
                , MinWidth = 40
            };
            spl.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = Core.Instance.Drivers[tp.Item2].FullName, FontWeight = FontWeights.Bold
                , Foreground = Brushes.SaddleBrown
            };
            spl.Children.Add(tb);
            ListBoxItem i = new ListBoxItem() {Content = spl};
            FirstTablesListBox.Items.Add(i);
        }

        MakeHeading("DRIVERS' RACE FINISHES (TOP 40)", 2);
        rank = 0;
        prev = -1;
        count = 0;
        prevrank = 0;
        foreach (Tuple<int, int> tp in driverFinishes)
        {
            count++;
            if (tp.Item1 != prev)
            {
                rank = count;
                if (rank > 40)
                {
                    break;
                }

                prev = tp.Item1;
            }

            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            spl.Children.Add(RankingTextBlock(rank, ref prevrank));
            TextBlock tb = new TextBlock()
            {
                Text = tp.Item1.ToString(Core.CultureUk), FontWeight = FontWeights.Bold, Foreground = Brushes.SeaGreen
                , MinWidth = 40
            };
            spl.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = Core.Instance.Drivers[tp.Item2].FullName, FontWeight = FontWeights.Bold
                , Foreground = Brushes.SaddleBrown
            };
            spl.Children.Add(tb);
            ListBoxItem i = new ListBoxItem() {Content = spl};
            SecondTablesListBox.Items.Add(i);
        }
    }

    private void DisplayDriverUnsuccess()
    {
        List<Tuple<int, int>> driverStarts = new List<Tuple<int, int>>();
        foreach (Driver d in Core.Instance.Drivers.Values)
        {
            Tuple<int, int> tup = new Tuple<int, int>(d.RuntimeRacesStarted, d.Key);
            driverStarts.Add(tup);
        }

        driverStarts.Sort();
        driverStarts.Reverse();
        List<int> Winner = new List<int>();
        List<int> Podium = new List<int>();
        List<int> Points = new List<int>();
        List<int> Finish = new List<int>();
        foreach (Voiture car in Core.Instance.Voitures.Values)
        {
            for (int a = 0; a < 3; a++)
            {
                int dri = car.DriverKey(a);
                if (dri > 0)
                {
                    if (car.RacePosition == 1)
                    {
                        if (!Winner.Contains(dri))
                        {
                            Winner.Add(dri);
                        }
                    }

                    if (car.RacePosition < 4)
                    {
                        if (!Podium.Contains(dri))
                        {
                            Podium.Add(dri);
                        }
                    }

                    if (car.DriverPoints(dri).Value > 0)
                    {
                        if (!Points.Contains(dri))
                        {
                            Points.Add(dri);
                        }
                    }

                    if (car.RacePosition < 999)
                    {
                        if (!Finish.Contains(dri))
                        {
                            Finish.Add(dri);
                        }
                    }
                }
            }
        }

        MakeHeading("MOST RACE STARTS BY DRIVERS WHO NEVER WON A RACE (TOP 20)", 1);
        int rank = 0;
        int prev = -1;
        int count = 0;
        int prevrank = 0;
        foreach (Tuple<int, int> f in driverStarts)
        {
            if (!Winner.Contains(f.Item2))
            {
                count++;
                if (f.Item1 != prev)
                {
                    rank = count;
                    if (rank > 20)
                    {
                        break;
                    }

                    prev = f.Item1;
                }

                StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
                spl.Children.Add(RankingTextBlock(rank, ref prevrank));
                TextBlock tb = new TextBlock()
                {
                    Text = f.Item1.ToString(Core.CultureUk), FontWeight = FontWeights.Bold
                    , Foreground = Brushes.SeaGreen, MinWidth = 40
                };
                spl.Children.Add(tb);
                tb = new TextBlock()
                {
                    Text = Core.Instance.Drivers[f.Item2].FullName, FontWeight = FontWeights.Bold
                    , Foreground = Brushes.SaddleBrown
                };
                spl.Children.Add(tb);
                ListBoxItem i = new ListBoxItem() {Content = spl};
                FirstTablesListBox.Items.Add(i);
            }
        }

        MakeHeading("MOST RACE STARTS BY DRIVERS WHO NEVER ACHIEVED A PODIUM PLACE (TOP 20)", 1);
        rank = 0;
        prev = -1;
        count = 0;
        prevrank = 0;
        foreach (Tuple<int, int> f in driverStarts)
        {
            if (!Podium.Contains(f.Item2))
            {
                count++;
                if (f.Item1 != prev)
                {
                    rank = count;
                    if (rank > 20)
                    {
                        break;
                    }

                    prev = f.Item1;
                }

                StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
                spl.Children.Add(RankingTextBlock(rank, ref prevrank));
                TextBlock tb = new TextBlock()
                {
                    Text = f.Item1.ToString(Core.CultureUk), FontWeight = FontWeights.Bold
                    , Foreground = Brushes.SeaGreen, MinWidth = 40
                };
                spl.Children.Add(tb);
                tb = new TextBlock()
                {
                    Text = Core.Instance.Drivers[f.Item2].FullName, FontWeight = FontWeights.Bold
                    , Foreground = Brushes.SaddleBrown
                };
                spl.Children.Add(tb);
                ListBoxItem i = new ListBoxItem() {Content = spl};
                FirstTablesListBox.Items.Add(i);
            }
        }

        MakeHeading("MOST RACE STARTS BY DRIVERS WHO NEVER SCORED ANY CHAMPIONSHIP POINTS (TOP 20)", 2);
        rank = 0;
        prev = -1;
        count = 0;
        prevrank = 0;
        foreach (Tuple<int, int> f in driverStarts)
        {
            if (!Points.Contains(f.Item2))
            {
                count++;
                if (f.Item1 != prev)
                {
                    rank = count;
                    if (rank > 20)
                    {
                        break;
                    }

                    prev = f.Item1;
                }

                StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
                spl.Children.Add(RankingTextBlock(rank, ref prevrank));
                TextBlock tb = new TextBlock()
                {
                    Text = f.Item1.ToString(Core.CultureUk), FontWeight = FontWeights.Bold
                    , Foreground = Brushes.SeaGreen, MinWidth = 40
                };
                spl.Children.Add(tb);
                tb = new TextBlock()
                {
                    Text = Core.Instance.Drivers[f.Item2].FullName, FontWeight = FontWeights.Bold
                    , Foreground = Brushes.SaddleBrown
                };
                spl.Children.Add(tb);
                ListBoxItem i = new ListBoxItem() {Content = spl};
                SecondTablesListBox.Items.Add(i);
            }
        }

        MakeHeading("MOST RACE STARTS BY DRIVERS WHO NEVER FINISHED A RACE (TOP 10)", 2);
        rank = 0;
        prev = -1;
        count = 0;
        prevrank = 0;
        foreach (Tuple<int, int> f in driverStarts)
        {
            if (!Finish.Contains(f.Item2))
            {
                count++;
                if (f.Item1 != prev)
                {
                    rank = count;
                    if (rank > 10)
                    {
                        break;
                    }

                    prev = f.Item1;
                }

                StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
                spl.Children.Add(RankingTextBlock(rank, ref prevrank));
                TextBlock tb = new TextBlock()
                {
                    Text = f.Item1.ToString(Core.CultureUk), FontWeight = FontWeights.Bold
                    , Foreground = Brushes.SeaGreen, MinWidth = 40
                };
                spl.Children.Add(tb);
                tb = new TextBlock()
                {
                    Text = Core.Instance.Drivers[f.Item2].FullName, FontWeight = FontWeights.Bold
                    , Foreground = Brushes.SaddleBrown
                };
                spl.Children.Add(tb);
                ListBoxItem i = new ListBoxItem() {Content = spl};
                SecondTablesListBox.Items.Add(i);
            }
        }
    }

    private void DisplayDriverPoles()
    {
        TextBlock tb;

        MakeHeading("RECORD HOLDER FOR MOST POLE POSITIONS", 1);

        List<Tuple<DateTime, int, int>> champs = Core.Instance.MostPolesByDate();
        int previousbest = 0;
        foreach (Tuple<DateTime, int, int> champ in champs)
        {
            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            tb = new TextBlock()
            {
                Text = $"From {champ.Item1.ToShortDateString()}", FontWeight = FontWeights.Bold
                , Foreground = Brushes.SeaGreen, MinWidth = 20, Margin = new Thickness(0, 0, 4, 0)
            };
            spl.Children.Add(tb);
            Brush qualb = equalledRecordBrush;
            if (previousbest < champ.Item3)
            {
                previousbest = champ.Item3;
                qualb = newRecordBrush;
            }

            tb = new TextBlock()
            {
                Text = Core.Instance.Drivers[champ.Item2].FullName, FontWeight = FontWeights.Bold, Foreground = qualb
                , MinWidth = 160, Margin = new Thickness(0, 0, 4, 0)
            };
            spl.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = champ.Item3.ToString(Core.CultureUk), FontWeight = FontWeights.Bold, Foreground = qualb
                , MinWidth = 16
            };
            spl.Children.Add(tb);
            FirstTablesListBox.Items.Add(spl);
        }

        MakeHeading("DRIVERS' LIFETIME POLE POSITIONS", 2);

        List<Tuple<int, int>> driverPoles = new List<Tuple<int, int>>();
        foreach (Driver d in Core.Instance.Drivers.Values)
        {
            Tuple<int, int> tup = new Tuple<int, int>(d.RuntimePoles, d.Key);
            driverPoles.Add(tup);
        }

        driverPoles.Sort();
        driverPoles.Reverse();

        int rank = 0;
        int prev = -1;
        int count = 0;
        int prevrank = 0;
        foreach (Tuple<int, int> tp in driverPoles)
        {
            count++;
            if (tp.Item1 != prev)
            {
                rank = count;
                if (rank > 40)
                {
                    break;
                }

                prev = tp.Item1;
            }

            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            spl.Children.Add(RankingTextBlock(rank, ref prevrank));
            tb = new TextBlock()
            {
                Text = tp.Item1.ToString(Core.CultureUk), FontWeight = FontWeights.Bold, Foreground = Brushes.SeaGreen
                , MinWidth = 40
            };
            spl.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = Core.Instance.Drivers[tp.Item2].FullName, FontWeight = FontWeights.Bold
                , Foreground = Brushes.SaddleBrown
            };
            spl.Children.Add(tb);
            ListBoxItem i = new ListBoxItem() {Content = spl};
            SecondTablesListBox.Items.Add(i);
        }
    }

    private void FatalityTables()
    {
        MakeHeading("F1 GRAND PRIX RACE FATALITIES (racing accidents)", 1);
        MakeHeading("ALL RACE-RELATED FATALITIES OF F1 DRIVERS (TPQ = Testing, practice or qualifying)", 2);
        // date / race title / driver name / description
        List<Tuple<DateTime, string, string, string>> F1race = new List<Tuple<DateTime, string, string, string>>();
        List<Tuple<DateTime, string, string>> every = new List<Tuple<DateTime, string, string>>();
        foreach (RaceMeeting mtg in Core.Instance.Races.Values)
        {
            foreach (Voiture v in Core.Instance.Voitures.Values)
            {
                if (v.RaceMeetingKey == mtg.Key)
                {
                    if (v.RacePosition > 999)
                    {
                        Core.RaceResultConstants tod = (Core.RaceResultConstants) v.RacePosition;
                        if ((tod == Core.RaceResultConstants.RetFatalDriver) ||
                            (tod == Core.RaceResultConstants.RetFatalOthers))
                        {
                            string hoo = (v.DriverCount > 1)
                                ? "multiple drivers"
                                : Core.Instance.Drivers[v.DriverKey(0)].FullName;
                            string wot;
                            if (tod == Core.RaceResultConstants.RetFatalDriver)
                            {
                                wot = "Fatal accident";
                            }
                            else
                            {
                                wot = "Bystanders killed";
                            }

                            Tuple<DateTime, string, string, string> buzz
                                = new Tuple<DateTime, string, string, string>(mtg.RaceDate
                                    , Core.Instance.RaceTitles[mtg.RaceTitleKey].Caption, hoo, wot);
                            F1race.Add(buzz);
                        }
                    }
                }
            }
        }

        foreach (Driver pilot in Core.Instance.Drivers.Values)
        {
            if (pilot.HowDied == Core.CauseOfDeath.RacePracticeOrTestingAccident)
            {
                Tuple<DateTime, string, string> buzz
                    = new Tuple<DateTime, string, string>(pilot.DeathDate, pilot.FullName, "TPQ accident");
                every.Add(buzz);
            }
            else if (pilot.HowDied == Core.CauseOfDeath.RacingAccident)
            {
                Tuple<DateTime, string, string> buzz
                    = new Tuple<DateTime, string, string>(pilot.DeathDate, pilot.FullName, "Racing accident");
                every.Add(buzz);
            }
        }

        every.Sort();
        F1race.Sort();
        foreach (Tuple<DateTime, string, string, string> buzz in F1race)
        {
            TextBlock aBlock = new TextBlock()
            {
                MinWidth = 140, Foreground = Brushes.Brown, FontWeight = FontWeights.Medium
                , Text = buzz.Item1.ToLongDateString()
            };
            TextBlock rBlock = new TextBlock()
                {MinWidth = 160, Foreground = Brushes.Blue, FontWeight = FontWeights.Medium, Text = buzz.Item2};
            TextBlock dBlock = new TextBlock()
                {MinWidth = 160, Foreground = Brushes.DarkBlue, FontWeight = FontWeights.Medium, Text = buzz.Item3};
            TextBlock wBlock = new TextBlock()
                {MinWidth = 160, Foreground = Brushes.DarkRed, FontWeight = FontWeights.Medium, Text = buzz.Item4};
            StackPanel panel = new StackPanel() {Orientation = Orientation.Horizontal};
            panel.Children.Add(aBlock);
            panel.Children.Add(rBlock);
            panel.Children.Add(dBlock);
            panel.Children.Add(wBlock);
            ListBoxItem li = new ListBoxItem() {Content = panel, IsHitTestVisible = false};
            FirstTablesListBox.Items.Add(li);
        }

        foreach (Tuple<DateTime, string, string> buzz in every)
        {
            TextBlock aBlock = new TextBlock()
            {
                MinWidth = 140, Foreground = Brushes.Brown, FontWeight = FontWeights.Medium
                , Text = buzz.Item1.ToLongDateString()
            };
            TextBlock dBlock = new TextBlock()
                {MinWidth = 160, Foreground = Brushes.DarkBlue, FontWeight = FontWeights.Medium, Text = buzz.Item2};
            TextBlock wBlock = new TextBlock()
                {MinWidth = 160, Foreground = Brushes.DarkRed, FontWeight = FontWeights.Medium, Text = buzz.Item3};
            StackPanel panel = new StackPanel() {Orientation = Orientation.Horizontal};
            panel.Children.Add(aBlock);
            panel.Children.Add(dBlock);
            panel.Children.Add(wBlock);
            ListBoxItem li = new ListBoxItem() {Content = panel, IsHitTestVisible = false};
            SecondTablesListBox.Items.Add(li);
        }
    }

    private void AddPercentLine(string rubric, int interesting, int total)
    {
        StackPanel stac = new StackPanel() {Orientation = Orientation.Horizontal};
        TextBlock blocrubric = new TextBlock() {Text = $"{rubric}:", MinWidth = 200, Foreground = Brushes.Blue};
        stac.Children.Add(blocrubric);

        TextBlock bloc;
        if (total == 0)
        {
            bloc = new TextBlock() {Text = "none"};
        }
        else if (interesting > 0)
        {
            float pc = (interesting / (float) total);
            string pcstring = (100 * pc).ToString("0.0", Core.CultureUk);
            bloc = new TextBlock() {Text = $"{interesting} = {pcstring}%"};
        }
        else
        {
            bloc = new TextBlock() {Text = $"0"};
        }

        stac.Children.Add(bloc);
        FirstTablesListBox.Items.Add(stac);
    }

    private void AddNumberLine(string rubric, int sum)
    {
        StackPanel stac = new StackPanel() {Orientation = Orientation.Horizontal};
        TextBlock blocrubric = new TextBlock() {Text = $"{rubric}:", MinWidth = 200, Foreground = Brushes.Blue};
        stac.Children.Add(blocrubric);
        TextBlock bloc;
        bloc = new TextBlock() {Text = sum.ToString(Core.CultureUk)};
        stac.Children.Add(bloc);
        FirstTablesListBox.Items.Add(stac);
    }

    private void MakeHeading(string H, int box)
    {
        ListBox lb = (box == 1) ? FirstTablesListBox : SecondTablesListBox;
        TextBlock tb = new TextBlock()
            {Text = H, FontWeight = FontWeights.Bold, Foreground = Brushes.Teal, Margin = new Thickness(0, 12, 0, 12)};
        lb.Items.Add(new ListBoxItem() {Content = tb, IsHitTestVisible = false});
    }

    private void DisplayMultipleDriverCars()
    {
        MakeHeading("CARS WITH MULTIPLE DRIVERS", 1);
        List<Tuple<DateTime, int, string>> results = new List<Tuple<DateTime, int, string>>();
        foreach (Voiture v in Core.Instance.Voitures.Values)
        {
            if (v.DriverCount > 1)
            {
                RaceMeeting meet = Core.Instance.Races[v.RaceMeetingKey];
                DateTime qd = meet.RaceDate;
                int qi = meet.RaceTitleKey;
                string qs
                    = $"{Core.Instance.Drivers[v.DriverKey(0)].FullName} / {Core.Instance.Drivers[v.DriverKey(1)].FullName}";
                if (v.DriverCount > 2)
                {
                    qs += $" / {Core.Instance.Drivers[v.DriverKey(2)].FullName}";
                }

                Tuple<DateTime, int, string> entry = new Tuple<DateTime, int, string>(qd, qi, qs);
                results.Add(entry);
            }
        }

        results.Sort();
        foreach (Tuple<DateTime, int, string> v in results)
        {
            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            TextBlock tb = new TextBlock()
            {
                Text = v.Item1.ToShortDateString(), FontWeight = FontWeights.Medium, Foreground = Brushes.SeaGreen
                , MinWidth = 80, Margin = new Thickness(0, 0, 4, 0)
            };
            spl.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = Core.Instance.RaceTitles[v.Item2].Caption, FontWeight = FontWeights.Bold
                , Foreground = Brushes.Black, MinWidth = 150, Margin = new Thickness(0, 0, 4, 0)
            };
            spl.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = v.Item3, FontWeight = FontWeights.Medium, Foreground = Brushes.DarkGreen
                , Margin = new Thickness(0, 0, 4, 0)
            };
            spl.Children.Add(tb);
            FirstTablesListBox.Items.Add(spl);
        }
    }

    private void DisplayFormula2Cars()
    {
        MakeHeading("F2 CARS IN F1 RACES", 1);
        List<Tuple<DateTime, int, string>> results = new List<Tuple<DateTime, int, string>>();
        foreach (Voiture v in Core.Instance.Voitures.Values)
        {
            if (v.Formula2)
            {
                RaceMeeting meet = Core.Instance.Races[v.RaceMeetingKey];
                DateTime qd = meet.RaceDate;
                int qi = meet.RaceTitleKey;
                string qs = Core.Instance.Drivers[v.DriverKey(0)].FullName;
                if (v.DriverCount > 1)
                {
                    qs += $" / {Core.Instance.Drivers[v.DriverKey(1)].FullName}";
                }

                if (v.DriverCount > 2)
                {
                    qs += $" / {Core.Instance.Drivers[v.DriverKey(2)].FullName}";
                }

                Tuple<DateTime, int, string> entry = new Tuple<DateTime, int, string>(qd, qi, qs);
                results.Add(entry);
            }
        }

        results.Sort();
        foreach (Tuple<DateTime, int, string> v in results)
        {
            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            TextBlock tb = new TextBlock()
            {
                Text = v.Item1.ToShortDateString(), FontWeight = FontWeights.Medium, Foreground = Brushes.SeaGreen
                , MinWidth = 80, Margin = new Thickness(0, 0, 4, 0)
            };
            spl.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = Core.Instance.RaceTitles[v.Item2].Caption, FontWeight = FontWeights.Bold
                , Foreground = Brushes.Black, MinWidth = 150, Margin = new Thickness(0, 0, 4, 0)
            };
            spl.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = v.Item3, FontWeight = FontWeights.Medium, Foreground = Brushes.DarkGreen
                , Margin = new Thickness(0, 0, 4, 0)
            };
            spl.Children.Add(tb);
            FirstTablesListBox.Items.Add(spl);
        }
    }

    private void DisplayDriverWins()
    {
        TextBlock tb;

        MakeHeading("RECORD HOLDER FOR MOST RACES WON", 1);

        List<Tuple<DateTime, int, int>> champs = Core.Instance.MostWinsByDate();
        int previousbest = 0;
        foreach (Tuple<DateTime, int, int> champ in champs)
        {
            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            tb = new TextBlock()
            {
                Text = $"From {champ.Item1.ToShortDateString()}", FontWeight = FontWeights.Bold
                , Foreground = Brushes.SeaGreen, MinWidth = 20, Margin = new Thickness(0, 0, 4, 0)
            };
            spl.Children.Add(tb);
            Brush qualb = equalledRecordBrush;
            if (previousbest < champ.Item3)
            {
                previousbest = champ.Item3;
                qualb = newRecordBrush;
            }

            tb = new TextBlock()
            {
                Text = Core.Instance.Drivers[champ.Item2].FullName, FontWeight = FontWeights.Bold, Foreground = qualb
                , MinWidth = 160, Margin = new Thickness(0, 0, 4, 0)
            };
            spl.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = champ.Item3.ToString(Core.CultureUk), FontWeight = FontWeights.Bold, Foreground = qualb
                , MinWidth = 16
            };
            spl.Children.Add(tb);
            FirstTablesListBox.Items.Add(spl);
        }

        MakeHeading("DRIVERS' LIFETIME RACE WINS (TOP 40)", 2);

        List<Tuple<int, int>> RaceWins = new List<Tuple<int, int>>();
        foreach (int dk in Core.Instance.Drivers.Keys)
        {
            int w = Core.Instance.DriverRaceWins(dk);
            RaceWins.Add(new Tuple<int, int>(w, dk));
        }

        RaceWins.Sort();
        RaceWins.Reverse();

        int rank = 0;
        int prev = -1;
        int count = 0;
        int prevrank = 0;
        foreach (Tuple<int, int> tp in RaceWins)
        {
            count++;
            if (tp.Item1 != prev)
            {
                rank = count;
                if (rank > 40)
                {
                    break;
                }

                prev = tp.Item1;
            }

            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            spl.Children.Add(RankingTextBlock(rank, ref prevrank));
            tb = new TextBlock()
            {
                Text = tp.Item1.ToString(Core.CultureUk), FontWeight = FontWeights.Bold, Foreground = Brushes.SeaGreen
                , MinWidth = 40
            };
            spl.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = Core.Instance.Drivers[tp.Item2].FullName, FontWeight = FontWeights.Bold
                , Foreground = Brushes.SaddleBrown
            };
            spl.Children.Add(tb);
            ListBoxItem i = new ListBoxItem() {Content = spl};
            SecondTablesListBox.Items.Add(i);
        }
    }

    private static TextBlock RankingTextBlock(int ranking, ref int previousranking)
    {
        string rankstring = (ranking == previousranking) ? string.Empty : Core.Ordinal(ranking);
        TextBlock tb = new TextBlock()
            {Text = rankstring, FontWeight = FontWeights.Bold, Foreground = Brushes.Gray, MinWidth = 40};
        previousranking = ranking;
        return tb;
    }

    private void DisplayDriverConsecutiveWins()
    {
        TextBlock tb;
        Core.Instance.UpdateRaceConsecutiveRaceRecords();

        MakeHeading("RECORD HOLDER FOR MOST CONSECUTIVE RACES WON", 1);

        List<Tuple<DateTime, int, int>> champs = Core.Instance.MostConsecutiveWinsByDate();
        int previousbest = 0;
        foreach (Tuple<DateTime, int, int> champ in champs)
        {
            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            tb = new TextBlock()
            {
                Text = $"From {champ.Item1.ToShortDateString()}", FontWeight = FontWeights.Bold
                , Foreground = Brushes.SeaGreen, MinWidth = 20, Margin = new Thickness(0, 0, 4, 0)
            };
            spl.Children.Add(tb);
            Brush qualb = equalledRecordBrush;
            if (previousbest < champ.Item3)
            {
                previousbest = champ.Item3;
                qualb = newRecordBrush;
            }

            tb = new TextBlock()
            {
                Text = Core.Instance.Drivers[champ.Item2].FullName, FontWeight = FontWeights.Bold, Foreground = qualb
                , MinWidth = 160, Margin = new Thickness(0, 0, 4, 0)
            };
            spl.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = champ.Item3.ToString(Core.CultureUk), FontWeight = FontWeights.Bold, Foreground = qualb
                , MinWidth = 16
            };
            spl.Children.Add(tb);
            FirstTablesListBox.Items.Add(spl);
        }

        MakeHeading("DRIVERS' WITH A SERIES OF CONSECUTIVE RACE WINS (TOP 40)", 2);

        List<Tuple<int, int>> RaceWins = new List<Tuple<int, int>>();
        foreach (int dk in Core.Instance.Drivers.Keys)
        {
            int w = Core.Instance.DriverMostConsecutiveRaceWins(dk);
            RaceWins.Add(new Tuple<int, int>(w, dk));
        }

        RaceWins.Sort();
        RaceWins.Reverse();

        int rank = 0;
        int prev = -1;
        int count = 0;
        int prevrank = 0;
        foreach (Tuple<int, int> tp in RaceWins)
        {
            count++;
            if (tp.Item1 != prev)
            {
                rank = count;
                if (rank > 40)
                {
                    break;
                }

                if (tp.Item1 < 2)
                {
                    break;
                } // don't list drivers with one consecutive win!

                prev = tp.Item1;
            }

            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            spl.Children.Add(RankingTextBlock(rank, ref prevrank));
            tb = new TextBlock()
            {
                Text = tp.Item1.ToString(Core.CultureUk), FontWeight = FontWeights.Bold, Foreground = Brushes.SeaGreen
                , MinWidth = 40
            };
            spl.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = Core.Instance.Drivers[tp.Item2].FullName, FontWeight = FontWeights.Bold
                , Foreground = Brushes.SaddleBrown
            };
            spl.Children.Add(tb);
            ListBoxItem i = new ListBoxItem() {Content = spl};
            SecondTablesListBox.Items.Add(i);
        }
    }

    private void DisplayDriverPodiums()
    {
        TextBlock tb;

        MakeHeading("RECORD HOLDER FOR MOST PODIUM PLACES", 1);

        List<Tuple<DateTime, int, int>> champs = Core.Instance.MostPodiumsByDate();
        int previousbest = 0;
        foreach (Tuple<DateTime, int, int> champ in champs)
        {
            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            tb = new TextBlock()
            {
                Text = $"From {champ.Item1.ToShortDateString()}", FontWeight = FontWeights.Bold
                , Foreground = Brushes.SeaGreen, MinWidth = 20, Margin = new Thickness(0, 0, 4, 0)
            };
            spl.Children.Add(tb);
            Brush qualb = equalledRecordBrush;
            if (previousbest < champ.Item3)
            {
                previousbest = champ.Item3;
                qualb = newRecordBrush;
            }

            tb = new TextBlock()
            {
                Text = Core.Instance.Drivers[champ.Item2].FullName, FontWeight = FontWeights.Bold, Foreground = qualb
                , MinWidth = 160, Margin = new Thickness(0, 0, 4, 0)
            };
            spl.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = champ.Item3.ToString(Core.CultureUk), FontWeight = FontWeights.Bold, Foreground = qualb
                , MinWidth = 16
            };
            spl.Children.Add(tb);
            FirstTablesListBox.Items.Add(spl);
        }

        MakeHeading("DRIVERS' LIFETIME PODIUM PLACES (TOP 40)", 2);

        List<Tuple<int, int>> RacePods = new List<Tuple<int, int>>();
        foreach (int dk in Core.Instance.Drivers.Keys)
        {
            int w = Core.Instance.DriverRacePodiums(dk);
            RacePods.Add(new Tuple<int, int>(w, dk));
        }

        RacePods.Sort();
        RacePods.Reverse();

        int rank = 0;
        int prev = -1;
        int count = 0;
        int prevrank = 0;
        foreach (Tuple<int, int> tp in RacePods)
        {
            count++;
            if (tp.Item1 != prev)
            {
                rank = count;
                if (rank > 40)
                {
                    break;
                }

                prev = tp.Item1;
            }

            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            spl.Children.Add(RankingTextBlock(rank, ref prevrank));
            tb = new TextBlock()
            {
                Text = tp.Item1.ToString(Core.CultureUk), FontWeight = FontWeights.Bold, Foreground = Brushes.SeaGreen
                , MinWidth = 40
            };
            spl.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = Core.Instance.Drivers[tp.Item2].FullName, FontWeight = FontWeights.Bold
                , Foreground = Brushes.SaddleBrown
            };
            spl.Children.Add(tb);
            ListBoxItem i = new ListBoxItem() {Content = spl};
            SecondTablesListBox.Items.Add(i);
        }
    }

    private void DisplayDriverPoints()
    {
        TextBlock tb;

        List<Tuple<float, int>> rankingActual = Core.Instance.AllTimeActualDriverScores();
        List<Tuple<float, int>> rankingStandard = Core.Instance.AllTimeStandardDriverScores();

        MakeHeading("DRIVERS' ACTUAL CAREER POINTS (TOP 40)", 1);
        int rank = 0;
        float prev = -1;
        int count = 0;
        int prevrank = 0;
        foreach (Tuple<float, int> tp in rankingActual)
        {
            count++;
            if (tp.Item1 != prev)
            {
                rank = count;
                if (rank > 40)
                {
                    break;
                }

                prev = tp.Item1;
            }

            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            spl.Children.Add(RankingTextBlock(rank, ref prevrank));
            tb = new TextBlock()
            {
                Text = tp.Item1.ToString("0.0", Core.CultureUk), TextAlignment = TextAlignment.Right
                , FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown, MinWidth = 80
            };
            spl.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = Core.Instance.Drivers[tp.Item2].FullName, FontWeight = FontWeights.Medium
                , Foreground = Brushes.Brown, Margin = new Thickness(12, 0, 0, 0)
            };
            spl.Children.Add(tb);
            FirstTablesListBox.Items.Add(spl);
        }

        MakeHeading("DRIVERS' STANDARDISED CAREER POINTS (TOP 40, USING LATEST SCORING SYSTEM)", 2);
        rank = 0;
        prev = -1;
        count = 0;
        prevrank = 0;
        foreach (Tuple<float, int> tp in rankingStandard)
        {
            count++;
            if (tp.Item1 != prev)
            {
                rank = count;
                if (rank > 40)
                {
                    break;
                }

                prev = tp.Item1;
            }

            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            spl.Children.Add(RankingTextBlock(rank, ref prevrank));
            tb = new TextBlock()
            {
                Text = tp.Item1.ToString("0.0", Core.CultureUk), TextAlignment = TextAlignment.Right
                , FontWeight = FontWeights.Bold, Foreground = Brushes.SaddleBrown, MinWidth = 80
            };
            spl.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = Core.Instance.Drivers[tp.Item2].FullName, FontWeight = FontWeights.Medium
                , Foreground = Brushes.Brown, Margin = new Thickness(12, 0, 0, 0)
            };
            spl.Children.Add(tb);
            SecondTablesListBox.Items.Add(spl);
        }
    }

    private void DisplayDriverFates()
    {
        MakeHeading("Drivers' fate by decade born", 1);

        for (int d = 1890; d < DateTime.Today.Year; d += 10)
        {
            int tot = Core.Instance.DriversBorn(d);
            if (tot > 0)
            {
                TextBlock t = new TextBlock()
                {
                    Text = $"Drivers born in the {d}s", Margin = new Thickness(0, 8, 0, 0)
                    , FontWeight = FontWeights.Medium
                };
                FirstTablesListBox.Items.Add(t);
                AddNumberLine("Born in the decade:", tot);
                AddPercentLine("Still living", Core.Instance.DriversAlive(d), tot);
                AddPercentLine("Died racing", Core.Instance.DriversKilledRacing(d), tot);
                AddPercentLine("Died in a race-related accident", Core.Instance.DriversKilledRaceRelated(d), tot);
                AddPercentLine("Died in any accident", Core.Instance.DriversKilledAnyAccident(d), tot);
            }
        }
    }

    private void DisplayAccidents()
    {
        List<Tuple<DateTime, int>> incidents = new List<Tuple<DateTime, int>>();

        MakeHeading("Fatal racing accidents", 1);
        foreach (int d in Core.Instance.Drivers.Keys)
        {
            if (Core.Instance.Drivers[d].HowDied == Core.CauseOfDeath.RacingAccident)
            {
                incidents.Add(new Tuple<DateTime, int>(Core.Instance.Drivers[d].DeathDate, d));
            }
        }

        incidents.Sort();
        foreach (Tuple<DateTime, int> occurence in incidents)
        {
            FirstTablesListBox.Items.Add(Block(occurence));
        }

        incidents.Clear();

        MakeHeading("Fatal practice, qualifying or testing accidents", 1);
        foreach (int d in Core.Instance.Drivers.Keys)
        {
            if (Core.Instance.Drivers[d].HowDied == Core.CauseOfDeath.RacePracticeOrTestingAccident)
            {
                incidents.Add(new Tuple<DateTime, int>(Core.Instance.Drivers[d].DeathDate, d));
            }
        }

        incidents.Sort();
        foreach (Tuple<DateTime, int> occurence in incidents)
        {
            FirstTablesListBox.Items.Add(Block(occurence));
        }

        incidents.Clear();

        MakeHeading("Non-race-related fatal accidents", 1);
        foreach (int d in Core.Instance.Drivers.Keys)
        {
            if (Core.Instance.Drivers[d].HowDied == Core.CauseOfDeath.OtherAccident)
            {
                incidents.Add(new Tuple<DateTime, int>(Core.Instance.Drivers[d].DeathDate, d));
            }
        }

        incidents.Sort();
        foreach (Tuple<DateTime, int> occurence in incidents)
        {
            FirstTablesListBox.Items.Add(Block(occurence));
        }

        incidents.Clear();

        MakeHeading("Career-altering injuries", 2);
        foreach (int d in Core.Instance.Drivers.Keys)
        {
            if (Core.Instance.Drivers[d].CeiDate > Core.DateBase)
            {
                incidents.Add(new Tuple<DateTime, int>(Core.Instance.Drivers[d].CeiDate, d));
            }
        }

        incidents.Sort();
        foreach (Tuple<DateTime, int> occurence in incidents)
        {
            SecondTablesListBox.Items.Add(Block(occurence));
        }
    }

    private static TextBlock Block(Tuple<DateTime, int> incident)
    {
        Driver dv = Core.Instance.Drivers[incident.Item2];
        TextBlock t = new TextBlock()
        {
            Text = $"{incident.Item1:yyyy MMM dd}: {dv.FullName} ", Margin = new Thickness(0, 8, 0, 0)
            , FontWeight = FontWeights.Medium
        };
        return t;
    }

    private void DisplayOldestLivingDrivers()
    {
        MakeHeading("Oldest living drivers", 1);

        List<Tuple<DateTime, int>> births = new List<Tuple<DateTime, int>>();
        foreach (Driver d in Core.Instance.Drivers.Values)
        {
            if (d.BirthDate > Core.DateBase)
            {
                births.Add(new Tuple<DateTime, int>(d.BirthDate, d.Key));
            }
        }

        births.Sort();

        List<Tuple<DateTime, DateTime, int, bool>> recordholders = new List<Tuple<DateTime, DateTime, int, bool>>();
        DateTime previousdeath = Core.DateBase;
        foreach (Tuple<DateTime, int> tpl in births)
        {
            if (Core.Instance.Drivers[tpl.Item2].DeathDate < Core.DateBase)
            {
                recordholders.Add(new Tuple<DateTime, DateTime, int, bool>(Core.Instance.Drivers[tpl.Item2].BirthDate
                    , DateTime.Today, tpl.Item2, true));
                break; // driver hasn't died so he must be the current oldest
            }

            if (Core.Instance.Drivers[tpl.Item2].DeathDate > previousdeath)
            {
                previousdeath = Core.Instance.Drivers[tpl.Item2].DeathDate;
                recordholders.Add(new Tuple<DateTime, DateTime, int, bool>(Core.Instance.Drivers[tpl.Item2].BirthDate
                    , Core.Instance.Drivers[tpl.Item2].DeathDate, tpl.Item2, false));
            }
        }

        foreach (Tuple<DateTime, DateTime, int, bool> candidate in recordholders)
        {
            if (candidate.Item4)
            {
                TextBlock nameTb = new TextBlock()
                {
                    Foreground = Brushes.Olive, FontWeight = FontWeights.Bold, FontSize = 14
                    , Text
                        = $"{Core.Instance.Drivers[candidate.Item3].Forenames} {Core.Instance.Drivers[candidate.Item3].Surname} still alive aged {Core.Instance.Drivers[candidate.Item3].LatestAge()}"
                };
                FirstTablesListBox.Items.Add(nameTb);
            }
            else
            {
                TextBlock nameTb = new TextBlock()
                {
                    Foreground = Brushes.Olive, FontWeight = FontWeights.Medium, FontSize = 14
                    , Text
                        = $"{Core.Instance.Drivers[candidate.Item3].Forenames} {Core.Instance.Drivers[candidate.Item3].Surname} until {candidate.Item2:d} when he died aged {Core.Instance.Drivers[candidate.Item3].LatestAge()}"
                };
                FirstTablesListBox.Items.Add(nameTb);
            }
        }
    }

    private void DisplayDriverAges()
    {
        MakeHeading("YOUNGEST DRIVERS ON THE STARTING GRID FOR A GRAND PRIX (TOP 40)", 1);

        List<Tuple<int, int>> OldAndYoung = Core.Instance.OldestAndYoungestDriversOnRaceDay();
        int rank = 0;
        Tuple<int, int> prev = new Tuple<int, int>(-1, -1);
        int count = 0;
        int prevrank = 0;
        for (int x = 0; x < 50; x++)
        {
            int drivr = OldAndYoung[x].Item2;
            int meetg = OldAndYoung[x].Item1;
            DateTime born = Core.Instance.Drivers[drivr].BirthDate;
            DateTime race = Core.Instance.Races[meetg].RaceDate;
            Tuple<int, int> age = Core.AgeYearsDays(born, race);

            count++;
            if ((age.Item1 != prev.Item1) || (age.Item2 != prev.Item2))
            {
                rank = count;
                if (rank > 40)
                {
                    break;
                }

                prev = age;
            }

            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            spl.Children.Add(RankingTextBlock(rank, ref prevrank));
            TextBlock t = new TextBlock()
                {Text = $" {age.Item1} yrs {age.Item2} days ", Foreground = Brushes.RoyalBlue, Width = 100};
            spl.Children.Add(t);
            t = new TextBlock()
                {Text = Core.Instance.Drivers[drivr].FullName, Foreground = Brushes.DarkBlue, Width = 140};
            spl.Children.Add(t);
            t = new TextBlock()
            {
                Text
                    = $" {Core.Instance.Races[meetg].RaceDate.Year} {Core.Instance.RaceTitles[Core.Instance.Races[meetg].RaceTitleKey].Caption}"
                , Foreground = Brushes.CornflowerBlue
            };
            spl.Children.Add(t);
            FirstTablesListBox.Items.Add(spl);
        }

        MakeHeading("OLDEST DRIVERS ON THE STARTING GRID FOR A GRAND PRIX (TOP 40)", 2);
        rank = 0;
        prev = new Tuple<int, int>(-1, -1);
        count = 0;
        prevrank = 0;
        for (int x = 50; x < 100; x++)
        {
            int drivr = OldAndYoung[x].Item2;
            int meetg = OldAndYoung[x].Item1;
            DateTime born = Core.Instance.Drivers[drivr].BirthDate;
            DateTime race = Core.Instance.Races[meetg].RaceDate;
            Tuple<int, int> age = Core.AgeYearsDays(born, race);
            count++;
            if ((age.Item1 != prev.Item1) || (age.Item2 != prev.Item2))
            {
                rank = count;
                if (rank > 40)
                {
                    break;
                }

                prev = age;
            }

            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            spl.Children.Add(RankingTextBlock(rank, ref prevrank));
            TextBlock t = new TextBlock()
                {Text = $" {age.Item1} yrs {age.Item2} days ", Foreground = Brushes.RoyalBlue, Width = 100};
            spl.Children.Add(t);
            t = new TextBlock()
                {Text = Core.Instance.Drivers[drivr].FullName, Foreground = Brushes.DarkBlue, Width = 140};
            spl.Children.Add(t);
            t = new TextBlock()
            {
                Text
                    = $" {Core.Instance.Races[meetg].RaceDate.Year} {Core.Instance.RaceTitles[Core.Instance.Races[meetg].RaceTitleKey].Caption}"
                , Foreground = Brushes.CornflowerBlue
            };
            spl.Children.Add(t);
            SecondTablesListBox.Items.Add(spl);
        }
    }

    private void DisplayDriverCareers()
    {
        MakeHeading("DRIVERS WHO QUALIFIED BUT DID NOT START ANY RACES", 1);
        List<Tuple<int, int>> conducteurs = Core.Instance.DriversWhoQualifiedButNeverStarted();
        foreach (Tuple<int, int> item in conducteurs)
        {
            Driver d = Core.Instance.Drivers[item.Item2];
            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            string raceString = (item.Item1 > 1) ? "races" : "race";
            TextBlock t = new TextBlock()
                {Text = $"{item.Item1} {raceString}", Foreground = Brushes.RoyalBlue, Width = 120};
            spl.Children.Add(t);
            t = new TextBlock() {Text = d.FullName, Foreground = Brushes.DarkBlue};
            spl.Children.Add(t);
            FirstTablesListBox.Items.Add(spl);
        }

        MakeHeading("DRIVERS WHO STARTED BUT DID NOT FINISH ANY RACES", 1);
        conducteurs = Core.Instance.DriversWhoStartedButNeverFinished();
        foreach (Tuple<int, int> item in conducteurs)
        {
            Driver d = Core.Instance.Drivers[item.Item2];
            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            string raceString = (item.Item1 > 1) ? "races" : "race";
            TextBlock t = new TextBlock()
                {Text = $"{item.Item1} {raceString}", Foreground = Brushes.RoyalBlue, Width = 120};
            spl.Children.Add(t);
            t = new TextBlock() {Text = d.FullName, Foreground = Brushes.DarkBlue};
            spl.Children.Add(t);
            FirstTablesListBox.Items.Add(spl);
        }

        List<Tuple<int, int>> ShortAndLong = Core.Instance.LongestDriverCareers();
        MakeHeading("LONGEST GRAND PRIX RACING CAREERS (TOP 40)", 2);
        int rank = 0;
        int prev = -1;
        int count = 0;
        int prevrank = 0;
        foreach (Tuple<int, int> chap in ShortAndLong)
        {
            Driver d = Core.Instance.Drivers[chap.Item2];
            Tuple<int, int> age = Core.AgeYearsDays(d.RuntimeFirstRaceStartDate, d.RuntimeLastRaceStartDate);
            count++;
            if (chap.Item1 != prev)
            {
                rank = count;
                if (rank > 40)
                {
                    break;
                }

                prev = chap.Item1;
            }

            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            spl.Children.Add(RankingTextBlock(rank, ref prevrank));
            int dd = chap.Item1;
            string dazeString = (dd > 0) ? $"{dd} days ({age.Item1} yrs {age.Item2} days) " : "Single race";
            TextBlock t = new TextBlock() {Text = dazeString, Foreground = Brushes.RoyalBlue, Width = 200};
            spl.Children.Add(t);
            t = new TextBlock() {Text = d.FullName, Foreground = Brushes.DarkBlue, Width = 140};
            spl.Children.Add(t);
            SecondTablesListBox.Items.Add(spl);
        }

        List<Tuple<int, int>> MostSeasoned = Core.Instance.DriversWhoRacedInMostSeasons();
        MakeHeading("DRIVERS WHO RACED IN MOST GRAND PRIX SEASONS (TOP 40)", 2);
        rank = 0;
        prev = -1;
        count = 0;
        prevrank = 0;
        foreach (Tuple<int, int> tup in MostSeasoned)
        {
            Driver d = Core.Instance.Drivers[tup.Item2];
            count++;
            if (tup.Item1 != prev)
            {
                rank = count;
                if (rank > 40)
                {
                    break;
                }

                prev = tup.Item1;
            }

            StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
            spl.Children.Add(RankingTextBlock(rank, ref prevrank));
            int dd = tup.Item1;
            string dazeString = (dd > 1) ? $"{dd} seasons" : "Single season";
            TextBlock t = new TextBlock() {Text = dazeString, Foreground = Brushes.RoyalBlue, Width = 200};
            spl.Children.Add(t);
            t = new TextBlock() {Text = d.FullName, Foreground = Brushes.DarkBlue, Width = 140};
            spl.Children.Add(t);
            SecondTablesListBox.Items.Add(spl);
        }
    }

    private void DisplayDriverSuccessRates(int activity)
    {
        // activity = the number of drivers this person has raced against - so as to display only drivers with significant careers
        MakeHeading("MY CALCULATION OF DRIVERS' SUCCESS RATES", 1);
        Dictionary<string, int> Performances = Core.Instance.DriverRacePerformance();
        List<Tuple<double, int, int, int>> successList = new List<Tuple<double, int, int, int>>();
        int top = Core.Instance.Drivers.Keys.Max();
        int[] wins = new int[top + 1];
        int[] lost = new int[top + 1];
        if (activity < 0)
        {
            FirstTablesListBox.Items.Add(new ListBoxItem()
            {
                IsHitTestVisible = false
                , Content = new TextBlock()
                {
                    TextWrapping = TextWrapping.Wrap, Width = FirstTablesListBox.ActualWidth / 2
                    , Text
                        = "Calculation based on aggregating the number of drivers who placed worse or better than this driver in the races in which s/he participated (excluding races in which neither driver finished)"
                    , FontStyle = FontStyles.Italic, Margin = new Thickness(6, 6, 6, 12)
                }
            });
        }
        else
        {
            FirstTablesListBox.Items.Add(new ListBoxItem()
            {
                IsHitTestVisible = false
                , Content = new TextBlock()
                {
                    TextWrapping = TextWrapping.Wrap, Width = FirstTablesListBox.ActualWidth / 2
                    , Text
                        = $"Calculation based on aggregating the number of drivers who placed worse or better than this driver in the races in which s/he participated (excluding races in which neither driver finished). This table includes only those drivers who faced at least {activity} competitors across all their races."
                    , FontStyle = FontStyles.Italic, Margin = new Thickness(6, 6, 6, 12)
                }
            });
        }

        foreach (int r in Core.Instance.Races.Keys)
        {
            foreach (int m in Core.Instance.Drivers.Keys)
            {
                string mstr = $"{r}-{m}";
                if (Performances.ContainsKey(mstr))
                {
                    foreach (int n in Core.Instance.Drivers.Keys)
                    {
                        if (m < n) // only compare their performances once
                        {
                            string nstr = $"{r}-{n}";
                            if (Performances.ContainsKey(nstr))
                            {
                                if (Performances[mstr] > Performances[nstr])
                                {
                                    wins[m]++;
                                    lost[n]++;
                                }
                                else if (Performances[mstr] < Performances[nstr])
                                {
                                    wins[n]++;
                                    lost[m]++;
                                }
                            }
                        }
                    }
                }
            }
        }

        foreach (int d in Core.Instance.Drivers.Keys)
        {
            int total = wins[d] + lost[d];
            if (total > 0)
            {
                if (total >= activity)
                {
                    double winproportion = wins[d] / (double) total;
                    successList.Add(new Tuple<double, int, int, int>(winproportion, d, wins[d], lost[d]));
                }
            }
        }

        successList.Sort();
        successList.Reverse();
        foreach (Tuple<double, int, int, int> quelle in successList)
        {
            string nom = Core.Instance.Drivers[quelle.Item2].FullName;
            double percent = Math.Round(quelle.Item1 * 1000);
            percent /= 10;
            string score = percent.ToString("#0.0", Core.CultureUk);
            StackPanel panel = new StackPanel() {Orientation = Orientation.Horizontal};
            panel.Children.Add(new TextBlock()
                {Text = $"{score}%", MinWidth = 50, TextAlignment = TextAlignment.Right});
            panel.Children.Add(new TextBlock()
                {Text = nom, MinWidth = 200, Margin = new Thickness(8, 0, 0, 0), FontWeight = FontWeights.Medium});
            panel.Children.Add(new TextBlock()
            {
                Text = $"outperformed {quelle.Item3} out of {quelle.Item3 + quelle.Item4} competitors"
                , Foreground = Brushes.DarkGreen
            });
            FirstTablesListBox.Items.Add(new ListBoxItem() {Content = panel});
        }
    }

    private void DisplayChamps()
    {
        MakeHeading("WORLD CHAMPION DRIVERS", 1);

        Dictionary<int, int> DriverChamps = new Dictionary<int, int>();
        TextBlock tb;
        foreach (int y in Core.Instance.Years)
        {
            Season s = Core.Instance.Seasons[y];
            s.RefreshStatistics();
            int champ = s.DriverSeasonRanked(1);

            // counting championships for each driver
            if (DriverChamps.ContainsKey(champ))
            {
                DriverChamps[champ]++;
            }
            else
            {
                DriverChamps.Add(champ, 1);
            }

            StackPanel sp = new StackPanel() {Orientation = Orientation.Horizontal};
            tb = new TextBlock()
                {Text = $"{y} ", FontWeight = FontWeights.Bold, Foreground = Brushes.DarkCyan, Width = 60};
            sp.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = Core.Instance.Drivers[champ].FullName, FontWeight = FontWeights.Bold
                , Foreground = Brushes.CornflowerBlue
            };
            sp.Children.Add(tb);
            FirstTablesListBox.Items.Add(new ListBoxItem() {Content = sp});
        }

        MakeHeading("CAREER CHAMPIONSHIP WINS", 2);

        int max = DriverChamps.Values.Max();
        for (int n = max; n > 0; n--)
        {
            foreach (int k in DriverChamps.Keys)
            {
                if (DriverChamps[k] == n)
                {
                    StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
                    tb = new TextBlock()
                    {
                        Text = n.ToString(Core.CultureUk), FontWeight = FontWeights.Bold, Foreground = Brushes.SeaGreen
                        , MinWidth = 20
                    };
                    spl.Children.Add(tb);
                    tb = new TextBlock()
                    {
                        Text = Core.Instance.Drivers[k].FullName, FontWeight = FontWeights.Bold
                        , Foreground = Brushes.SaddleBrown, MinWidth = 80
                    };
                    spl.Children.Add(tb);
                    SecondTablesListBox.Items.Add(spl);
                }
            }
        }

    }

    private void RecordNumberOfSeasonWins()
    {
        Dictionary<int, List<Tuple<int, int, int>>> BestOfYear = new Dictionary<int, List<Tuple<int, int, int>>>();
        foreach (int y in Core.Instance.Years)
        {
            Season s = Core.Instance.Seasons[y];
            s.RefreshStatistics();
            List<Tuple<int, int, int>> seasonsbest = s.MostWinsInInSeason();
            BestOfYear.Add(y, seasonsbest);
        }

        // year, driver, %wins, better than previous
        Dictionary<int, List<Tuple<int, int, double, bool>>> Collectioneur
            = new Dictionary<int, List<Tuple<int, int, double, bool>>>();

        List<Tuple<int, int, int, bool>>
            MieuxN = new List<Tuple<int, int, int, bool>>(); // year, driver, wins, better than previous
        int mostN = 0;
        foreach (int y in Core.Instance.Years)
        {
            List<Tuple<int, int, int>> BestInYear = BestOfYear[y];
            if (BestInYear[0].Item2 >= mostN) // we only need to consider item 0 as each item scored equally
            {
                bool newrecord = (BestInYear[0].Item2 > mostN); // bettered rather than equalled previous record
                mostN = BestInYear[0].Item2;
                foreach (Tuple<int, int, int> chap in BestInYear)
                {
                    MieuxN.Add(new Tuple<int, int, int, bool>(y, chap.Item1, chap.Item2, newrecord));
                }
            }
        }

        for (int dec = 1950; dec < Core.Instance.Years.Max(); dec += 10)
        {
            double mostP = 0;
            List<Tuple<int, int, double, bool>> FromDecadeProportionResults = new List<Tuple<int, int, double, bool>>();
            foreach (int y in Core.Instance.Years)
            {
                if (y >= dec)
                {
                    List<Tuple<int, int, int>> CreamOfYear = BestOfYear[y];
                    double proportion
                        = CreamOfYear[0].Item2 /
                          (double) CreamOfYear[0].Item3; // we only need to consider item 0 as each item scored equally
                    if (proportion >= mostP) // bettered or equalled previous record
                    {
                        bool newrecord = (proportion > mostP); // bettered rather than equalled previous record
                        mostP = proportion;
                        foreach (Tuple<int, int, int> chap in CreamOfYear)
                        {
                            FromDecadeProportionResults.Add(
                                new Tuple<int, int, double, bool>(y, chap.Item1, proportion, newrecord));
                        }
                    }
                }
            }

            Collectioneur.Add(dec, FromDecadeProportionResults);
        }

        MakeHeading("MOST WINS IN A SEASON", 1);
        foreach (Tuple<int, int, int, bool> best in MieuxN)
        {
            StackPanel sp = new StackPanel() {Orientation = Orientation.Horizontal};
            TextBlock tb = new TextBlock()
                {Text = $"{best.Item1} ", FontWeight = FontWeights.Bold, Foreground = Brushes.DarkCyan, Width = 60};
            Brush pinceau = best.Item4 ? newRecordBrush : equalledRecordBrush;
            sp.Children.Add(tb);
            tb = new TextBlock()
            {
                Text = Core.Instance.Drivers[best.Item2].FullName, FontWeight = FontWeights.Bold, MinWidth = 200
                , Foreground = pinceau
            };
            sp.Children.Add(tb);
            tb = new TextBlock() {Text = $"{best.Item3} wins", FontWeight = FontWeights.Bold, Foreground = pinceau};
            sp.Children.Add(tb);
            FirstTablesListBox.Items.Add(new ListBoxItem() {Content = sp});
        }

        foreach (int fromDecade in Collectioneur.Keys)
        {
            if (fromDecade == 1950)
            {
                MakeHeading($"ALL TIME GREATEST PROPORTION OF WINS IN A SEASON", 2);
            }
            else
            {
                MakeHeading($"GREATEST PROPORTION OF WINS IN A SEASON SINCE {fromDecade}", 2);
            }

            List<Tuple<int, int, double, bool>> MieuxP = Collectioneur[fromDecade];
            foreach (Tuple<int, int, double, bool> goodest in MieuxP)
            {
                StackPanel sp = new StackPanel() {Orientation = Orientation.Horizontal};
                TextBlock tb = new TextBlock()
                {
                    Text = $"{goodest.Item1} ", FontWeight = FontWeights.Bold, Foreground = Brushes.DarkCyan, Width = 60
                };
                Brush pinceau = goodest.Item4 ? newRecordBrush : equalledRecordBrush;
                sp.Children.Add(tb);
                tb = new TextBlock()
                {
                    Text = Core.Instance.Drivers[goodest.Item2].FullName, FontWeight = FontWeights.Bold, MinWidth = 200
                    , Foreground = pinceau
                };
                sp.Children.Add(tb);
                double percent = 100 * goodest.Item3;
                tb = new TextBlock()
                    {Text = $"{percent:#0.0}% wins", FontWeight = FontWeights.Bold, Foreground = pinceau};
                sp.Children.Add(tb);
                SecondTablesListBox.Items.Add(new ListBoxItem() {Content = sp});
            }
        }
    }

    private void DisplayBirthdays()
    {
        MakeHeading("DRIVERS' BIRTHDAYS", 1);

        Dictionary<DateTime, bool> useddates = new Dictionary<DateTime, bool>();
        DateTime pt = new DateTime(2020, 1, 1);
        while (pt.Year == 2020)
        {
            useddates.Add(pt, false);
            pt = pt.AddDays(1);
        }

        List<Tuple<DateTime, int>> anniversaries = new List<Tuple<DateTime, int>>();
        foreach (Driver dr in Core.Instance.Drivers.Values)
        {
            DateTime bday = dr.BirthDate;
            DateTime xday = new DateTime(2020, bday.Month, bday.Day); // 2020 chosen as a leap year
            useddates[xday] = true;
            Tuple<DateTime, int> drbday = new Tuple<DateTime, int>(xday, dr.Key);
            anniversaries.Add(drbday);
        }

        anniversaries.Sort();
        int lastM = 0;
        int lastD = 0;
        foreach (Tuple<DateTime, int> drbday in anniversaries)
        {
            int m = drbday.Item1.Month;
            int d = drbday.Item1.Day;
            Brush pinceau = Brushes.DarkCyan;
            string ms = drbday.Item1.ToString("MMMM", System.Globalization.CultureInfo.CurrentCulture)
                .ToUpperInvariant();
            if (m == lastM)
            {
                ms = drbday.Item1.ToString("MMM", System.Globalization.CultureInfo.CurrentCulture);
                pinceau = Brushes.Gray;
            }

            string ds = drbday.Item1.ToString("dd", System.Globalization.CultureInfo.CurrentCulture);
            if (d == lastD)
            {
                ds = string.Empty;
                ms = string.Empty;
            }

            lastD = d;
            lastM = m;
            StackPanel sp = new StackPanel() {Orientation = Orientation.Horizontal};
            TextBlock tbm = new TextBlock()
                {Text = ms, FontWeight = FontWeights.Bold, Foreground = pinceau, Width = 100};
            sp.Children.Add(tbm);
            TextBlock tbd = new TextBlock()
                {Text = ds, FontWeight = FontWeights.Bold, Foreground = Brushes.DarkKhaki, Width = 60};
            sp.Children.Add(tbd);
            TextBlock tbc = new TextBlock()
            {
                Text
                    = $"{Core.Instance.Drivers[drbday.Item2].FullName} ({Core.Instance.Drivers[drbday.Item2].BirthDate.Year})"
                , FontWeight = FontWeights.Bold, Foreground = Brushes.DarkOliveGreen
            };
            sp.Children.Add(tbc);
            FirstTablesListBox.Items.Add(new ListBoxItem() {Content = sp});
        }

        MakeHeading("BORN ON THE SAME DAY", 2);
        List<Driver> drlist = Core.Instance.Drivers.Values.ToList();
        List<Tuple<DateTime, string, string>> matchlist = new List<Tuple<DateTime, string, string>>();
        for (int a = 0; a < (drlist.Count - 1); a++)
        {
            for (int b = a + 1; b < drlist.Count; b++)
            {
                if (drlist[a].BirthDate.Equals(drlist[b].BirthDate))
                {
                    Tuple<DateTime, string, string> eg = new Tuple<DateTime, string, string>(drlist[b].BirthDate
                        , drlist[a].FullName, drlist[b].FullName);
                    matchlist.Add(eg);
                }
            }
        }

        matchlist.Sort();
        foreach (Tuple<DateTime, string, string> eg in matchlist)
        {
            TextBlock tbk4 = new TextBlock() {Text = eg.Item1.ToLongDateString(), Width = 120};
            TextBlock tbk1 = new TextBlock() {Text = eg.Item2, FontWeight = FontWeights.Bold};
            TextBlock tbk2 = new TextBlock() {Text = " and "};
            TextBlock tbk3 = new TextBlock() {Text = eg.Item3, FontWeight = FontWeights.Bold};
            StackPanel sp = new StackPanel() {Orientation = Orientation.Horizontal};
            sp.Children.Add(tbk4);
            sp.Children.Add(tbk1);
            sp.Children.Add(tbk2);
            sp.Children.Add(tbk3);
            SecondTablesListBox.Items.Add(new ListBoxItem() {Content = sp});
        }

        MakeHeading("NOBODY'S BIRTHDAY", 2);
        FontFamily ff = new FontFamily("Lucida Console");
        lastM = 0;
        int vacs = 0;
        string report = string.Empty;
        foreach (DateTime d in useddates.Keys)
        {
            if (useddates[d] == false)
            {
                vacs++;
                if (d.Month != lastM)
                {
                    if (lastM > 0)
                    {
                        TextBlock rpt = new TextBlock() {Text = report, FontFamily = ff};
                        SecondTablesListBox.Items.Add(new ListBoxItem() {Content = rpt});
                    }

                    report = d.ToString("MMMM", System.Globalization.CultureInfo.CurrentCulture);
                    report = report.PadRight(10);
                }

                string m = $"{d.Day}".PadLeft(4);
                report += m;
                lastM = d.Month;
            }
        }

        TextBlock finalrpt = new TextBlock() {Text = report, FontFamily = ff};
        SecondTablesListBox.Items.Add(new ListBoxItem() {Content = finalrpt});
        TextBlock vacrpt = new TextBlock() {Text = $"{vacs} vacant dates", FontFamily = ff, Margin = new Thickness(12)};
        SecondTablesListBox.Items.Add(new ListBoxItem() {Content = vacrpt});
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        double scrX = SystemParameters.PrimaryScreenWidth;
        double scrY = SystemParameters.PrimaryScreenHeight;
        double winX = scrX * .98;
        double winY = scrY * .94;
        double Xm = (scrX - winX) / 2;
        double Ym = (scrY - winY) / 4;
        Width = winX;
        Height = winY;
        Left = Xm;
        Top = Ym;
    }

    private void DisplayButton_Click(object sender, RoutedEventArgs e)
    {
        Jbh.UiServices.SetBusyState();
        DisplayButton.IsEnabled = false;
        switch (_mode)
        {
            case "champs":
            {
                DisplayChamps();
                break;
            }
            case "wins":
            {
                RecordsRubricPanel.Visibility = Visibility.Visible;
                DisplayDriverWins();
                break;
            }
            case "podia":
            {
                RecordsRubricPanel.Visibility = Visibility.Visible;
                DisplayDriverPodiums();
                break;
            }
            case "poles":
            {
                RecordsRubricPanel.Visibility = Visibility.Visible;
                DisplayDriverPoles();
                break;
            }
            case "consec":
            {
                RecordsRubricPanel.Visibility = Visibility.Visible;
                DisplayDriverConsecutiveWins();
                break;
            }
            case "ages":
            {
                DisplayDriverAges();
                break;
            }
            case "vieux":
            {
                DisplayOldestLivingDrivers();
                break;
            }
            case "fate":
            {
                DisplayDriverFates();
                break;
            }
            case "points":
            {
                DisplayDriverPoints();
                break;
            }
            case "starts":
            {
                DisplayDriverStartsAndFinishes();
                break;
            }
            case "lost":
            {
                FatalityTables();
                break;
            }
            case "birthdays":
            {
                DisplayBirthdays();
                break;
            }
            case "worst":
            {
                DisplayDriverUnsuccess();
                break;
            }
            case "multi":
            {
                DisplayMultipleDriverCars();
                break;
            }
            case "f2":
            {
                DisplayFormula2Cars();
                break;
            }
            case "career":
            {
                DisplayDriverCareers();
                break;
            }
            case "jbhsuccess":
            {
                DisplayDriverSuccessRates(-1);
                break;
            }
            case "jbhsuccesspartial":
            {
                DisplayDriverSuccessRates(500);
                break;
            }
            case "seasonwins":
            {
                RecordsRubricPanel.Visibility = Visibility.Visible;
                RecordNumberOfSeasonWins();
                break;
            }
            case "blessure":
            {
                DisplayAccidents();
                break;
            }
            default:
            {
                break;
            }
        }
    }

    private void RadioChoice_Checked(object sender, RoutedEventArgs e)
    {
        FirstTablesListBox.Items.Clear();
        SecondTablesListBox.Items.Clear();
        DisplayButton.IsEnabled = true;
        RecordsRubricPanel.Visibility = Visibility.Hidden;
        RadioButton rad = (RadioButton) sender;
        _mode = rad.Tag.ToString();
    }
}