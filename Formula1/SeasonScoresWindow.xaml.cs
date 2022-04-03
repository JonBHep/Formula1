using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Formula1;

public partial class SeasonScoresWindow : Window
{
    private readonly Season _raceSeason;

    public SeasonScoresWindow(int Annee)
    {
        InitializeComponent();
        _raceSeason = Core.Instance.Seasons[Annee];
        _raceSeason.RefreshStatistics();
    }

    private void DisplayResultsList()
    {
        GraphButton.IsEnabled = false; // Only enable this button when there are 3 or more races in the season
        GraphWarningTextBlock.Visibility = Visibility.Visible;
        StatedRaceCountTextBox.Text
            = _raceSeason.StatedNumberOfRaces.ToString(System.Globalization.CultureInfo.CurrentCulture);
        StatedRaceCountButton.Foreground = Brushes.Gray;
        StatedRaceCountButton.IsEnabled = false; // will be re-enabled if user enters text
        DriversSchemeButton.ToolTip = _raceSeason.PointsAllocationSchemeForDrivers.Explanation();
        ConstructorsSchemeButton.ToolTip = _raceSeason.PointsAllocationSchemeForConstructors.Explanation();

        SeasonListBox.Items.Clear();

        TextBlock t = new TextBlock() {Margin = new Thickness(0, 12, 0, 0)};
        Run r = new Run()
            {Text = "Drivers' championship: ", FontWeight = FontWeights.Bold, Foreground = Brushes.RoyalBlue};
        t.Inlines.Add(r);
        r = new Run()
        {
            Text = _raceSeason.PointsAllocationSchemeForDrivers.Explanation(), FontWeight = FontWeights.Normal
            , Foreground = Brushes.Blue
        };
        t.Inlines.Add(r);
        ListBoxItem i = new ListBoxItem() {Content = t};
        SeasonListBox.Items.Add(i);

        t = new TextBlock() {Margin = new Thickness(0, 12, 0, 0)};
        r = new Run()
            {Text = "Constructors' championship: ", FontWeight = FontWeights.Bold, Foreground = Brushes.RoyalBlue};
        t.Inlines.Add(r);
        r = new Run()
        {
            Text = _raceSeason.PointsAllocationSchemeForConstructors.Explanation(), FontWeight = FontWeights.Normal
            , Foreground = Brushes.Blue
        };
        t.Inlines.Add(r);
        i = new ListBoxItem() {Content = t};
        SeasonListBox.Items.Add(i);

        t = new TextBlock() {Margin = new Thickness(0, 12, 0, 0)};
        r = new Run() {Text = "Season champion: ", FontWeight = FontWeights.Bold, Foreground = Brushes.SeaGreen};
        t.Inlines.Add(r);
        int x = _raceSeason.DriverSeasonRanked(1);
        if (x > 0)
        {
            r = new Run()
            {
                Text = Core.Instance.Drivers[x].FullName, FontWeight = FontWeights.Normal
                , Foreground = Brushes.DarkGreen
            };
            t.Inlines.Add(r);
        }

        i = new ListBoxItem() {Content = t};
        SeasonListBox.Items.Add(i);

        List<Tuple<int, string>> champions = _raceSeason.NumberOfDifferentVictorsInSeason();
        t = new TextBlock() {Margin = new Thickness(0, 12, 0, 0)};
        r = new Run()
        {
            Text = "Drivers winning a race this season: ", FontWeight = FontWeights.Bold, Foreground = Brushes.SeaGreen
        };
        t.Inlines.Add(r);
        r = new Run()
        {
            Text = $"{champions.Count} victors in the {_raceSeason.RaceKeyList.Count} races"
            , FontWeight = FontWeights.Normal, Foreground = Brushes.DarkGreen
        };
        t.Inlines.Add(r);
        i = new ListBoxItem() {Content = t};
        SeasonListBox.Items.Add(i);

        foreach (Tuple<int, string> gagnant in champions)
        {
            t = new TextBlock() {Margin = new Thickness(12, 6, 0, 0)};
            r = new Run() {Text = gagnant.Item2, FontWeight = FontWeights.Bold, Foreground = Brushes.SeaGreen};
            t.Inlines.Add(r);
            string vix = (gagnant.Item1 > 1) ? "victories" : "victory";
            r = new Run()
                {Text = $" {gagnant.Item1} {vix}", FontWeight = FontWeights.Normal, Foreground = Brushes.DarkGreen};
            t.Inlines.Add(r);
            i = new ListBoxItem() {Content = t};
            SeasonListBox.Items.Add(i);
        }

        List<int> RacesSoFar = new List<int>();

        foreach (int m in _raceSeason.RaceKeyList)
        {
            RacesSoFar.Add(m);
            t = new TextBlock() {Margin = new Thickness(0, 36, 0, 0)};
            r = new Run()
            {
                Text = Core.Instance.RaceTitles[Core.Instance.Races[m].RaceTitleKey].Caption
                , FontWeight = FontWeights.Bold, Foreground = Brushes.RoyalBlue
            };
            t.Inlines.Add(r);
            r = new Run() {Text = $"  {Core.Instance.Races[m].RaceDate:MMMM dd}"};
            t.Inlines.Add(r);
            i = new ListBoxItem() {Content = t};
            SeasonListBox.Items.Add(i);

            // List of drivers' scores in this race
            i = new ListBoxItem() {Content = "DRIVERS"};
            SeasonListBox.Items.Add(i);

            List<IndividualScore> ListD = _raceSeason.RaceDriverResults(m);
            foreach (IndividualScore isc in ListD)
            {
                if (isc.Score > 0)
                {
                    TextBlock tbdrv = new TextBlock()
                        {Text = Core.Instance.Drivers[isc.IndividualKey].FullName, Foreground = Brushes.SaddleBrown};
                    TextBlock tbsco = new TextBlock()
                    {
                        Text = Core.MyFormat(isc.Score), Foreground = Brushes.Black, FontWeight = FontWeights.Bold
                        , Width = 44
                    };
                    StackPanel stp = new StackPanel() {Orientation = Orientation.Horizontal};
                    {
                        stp.Children.Add(tbsco);
                        stp.Children.Add(tbdrv);
                    }
                    ListBoxItem itm = new ListBoxItem() {Content = stp};
                    SeasonListBox.Items.Add(itm);
                }
            }

            if (_raceSeason.PointsAllocationSchemeForConstructors.Applies)
            {
                // List of scores in this race and cumul score (only for those teams scoring in this race)
                i = new ListBoxItem() {Content = "CONSTRUCTORS"};
                SeasonListBox.Items.Add(i);
                List<IndividualScore> ListC = _raceSeason.RaceConstructorResults(m);
                foreach (IndividualScore isc in ListC)
                {
                    if (isc.Score > 0)
                    {
                        TextBlock tbdrv = new TextBlock()
                        {
                            Text = Core.Instance.Constructors[isc.IndividualKey].Caption
                            , Foreground = Brushes.SaddleBrown
                        };
                        //TextBlock tbcum = new TextBlock() { Text = Core.MyFormat(isc.CumulativeScore), Foreground = Brushes.Black, Width = 44 };
                        TextBlock tbsco = new TextBlock()
                        {
                            Text = Core.MyFormat(isc.Score), Foreground = Brushes.Black, FontWeight = FontWeights.Bold
                            , Width = 44
                        };
                        StackPanel stp = new StackPanel() {Orientation = Orientation.Horizontal};
                        {
                            stp.Children.Add(tbsco);
                            //  stp.Children.Add(tbcum);
                            stp.Children.Add(tbdrv);
                        }
                        ListBoxItem itm = new ListBoxItem() {Content = stp};
                        SeasonListBox.Items.Add(itm);
                    }
                }
            }

            // Post-race drivers standings

            TextBlock z = new TextBlock()
            {
                Text = "Drivers' Championship standings", FontWeight = FontWeights.Bold
                , Margin = new Thickness(0, 12, 0, 0)
            };
            ListBoxItem zi = new ListBoxItem() {Content = z};
            SeasonListBox.Items.Add(zi);

            double[] cols = {50, 60, 60, 140};

            // Headings
            TextBlock headingpoints = new TextBlock()
                {Text = "Points", FontWeight = FontWeights.Black, Width = cols[0]};
            TextBlock headingcounted = new TextBlock()
            {
                Text = "Counted ", TextAlignment = TextAlignment.Center, Foreground = Brushes.Black
                , FontWeight = FontWeights.Bold, Width = cols[1]
            };
            TextBlock headingtotal = new TextBlock()
            {
                Text = "(Total) ", TextAlignment = TextAlignment.Center, FontWeight = FontWeights.Normal
                , Width = cols[2]
            };
            TextBlock headingname = new TextBlock()
                {Text = "Driver", Foreground = Brushes.SaddleBrown, Width = cols[3]};
            TextBlock headingpotential = new TextBlock()
                {Text = "Max achievable", Foreground = Brushes.Violet, Width = cols[3]};

            StackPanel stk = new StackPanel()
                {Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 6)};
            {
                stk.Children.Add(headingpoints);
                stk.Children.Add(headingcounted);
                stk.Children.Add(headingtotal);
                stk.Children.Add(headingname);
                stk.Children.Add(headingpotential);
            }
            ListBoxItem heads = new ListBoxItem() {Content = stk};
            SeasonListBox.Items.Add(heads);

            // Values

            foreach (IndividualScore driv in _raceSeason.RaceDriverResults(m))
            {
                if (driv.CumulativeScore > 0)
                {
                    TextBlock tbscore = new TextBlock()
                    {
                        Margin = new Thickness(cols[0], 0, 0, 0), Text = Core.MyFormat(driv.CumulativeScoreCounted)
                        , TextAlignment = TextAlignment.Center, Foreground = Brushes.Black
                        , FontWeight = FontWeights.Bold, Width = cols[1]
                    };
                    string tot = (driv.CumulativeScoreCounted == driv.CumulativeScore)
                        ? string.Empty
                        : $"({Core.MyFormat(driv.CumulativeScore)})";
                    TextBlock tbtotal = new TextBlock()
                        {Text = tot, TextAlignment = TextAlignment.Center, Foreground = Brushes.Black, Width = cols[2]};
                    TextBlock tbdriver = new TextBlock()
                    {
                        Text = Core.Instance.Drivers[driv.IndividualKey].FullName, Foreground = Brushes.SaddleBrown
                        , Width = cols[3]
                    };
                    TextBlock tbChamp = new TextBlock()
                    {
                        Text = driv.MaxTheoreticalSeasonTotal.ToString(System.Globalization.CultureInfo.CurrentCulture)
                        , Foreground = Brushes.Violet
                    };

                    StackPanel stp = new StackPanel() {Orientation = Orientation.Horizontal};
                    {
                        stp.Children.Add(tbscore);
                        stp.Children.Add(tbtotal);
                        stp.Children.Add(tbdriver);
                        stp.Children.Add(tbChamp);
                    }
                    ListBoxItem itm = new ListBoxItem() {Content = stp};
                    SeasonListBox.Items.Add(itm);
                }
            }


            // Post-race team standings
            if (_raceSeason.PointsAllocationSchemeForConstructors.Applies)
            {


                z = new TextBlock()
                {
                    Text = "Constructors' Championship standings", FontWeight = FontWeights.Bold
                    , Margin = new Thickness(0, 12, 0, 0)
                };
                zi = new ListBoxItem() {Content = z};
                SeasonListBox.Items.Add(zi);

                // Headings

                headingpoints = new TextBlock() {Text = "Points", FontWeight = FontWeights.Black, Width = cols[0]};
                headingcounted = new TextBlock()
                {
                    Text = "Counted ", TextAlignment = TextAlignment.Center, Foreground = Brushes.Black
                    , FontWeight = FontWeights.Bold, Width = cols[1]
                };
                headingtotal = new TextBlock()
                {
                    Text = "(Total) ", TextAlignment = TextAlignment.Center, FontWeight = FontWeights.Normal
                    , Width = cols[2]
                };
                headingname = new TextBlock() {Text = "Constructor", Foreground = Brushes.SaddleBrown, Width = cols[3]};

                stk = new StackPanel() {Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 6)};
                {
                    stk.Children.Add(headingpoints);
                    stk.Children.Add(headingcounted);
                    stk.Children.Add(headingtotal);
                    stk.Children.Add(headingname);
                }

                heads = new ListBoxItem() {Content = stk};
                SeasonListBox.Items.Add(heads);

                // Values
                foreach (IndividualScore isc in _raceSeason.RaceConstructorResults(m))
                {
                    if (isc.CumulativeScore > 0)
                    {

                        TextBlock tbscore = new TextBlock()
                        {
                            Margin = new Thickness(cols[0], 0, 0, 0), Text = Core.MyFormat(isc.CumulativeScoreCounted)
                            , TextAlignment = TextAlignment.Center, Foreground = Brushes.Black
                            , FontWeight = FontWeights.Bold, Width = cols[1]
                        };
                        string tot = (isc.CumulativeScoreCounted == isc.CumulativeScore)
                            ? string.Empty
                            : $"({Core.MyFormat(isc.CumulativeScore)})";
                        TextBlock tbtotal = new TextBlock()
                        {
                            Text = tot, TextAlignment = TextAlignment.Center, Foreground = Brushes.Black
                            , Width = cols[2]
                        };
                        TextBlock tbconstructor = new TextBlock()
                        {
                            Text = Core.Instance.Constructors[isc.IndividualKey].Caption
                            , Foreground = Brushes.SaddleBrown
                        };

                        StackPanel stp = new StackPanel() {Orientation = Orientation.Horizontal};
                        {
                            stp.Children.Add(tbscore);
                            stp.Children.Add(tbtotal);
                            stp.Children.Add(tbconstructor);
                        }
                        ListBoxItem itm = new ListBoxItem() {Content = stp};
                        SeasonListBox.Items.Add(itm);
                    }
                }
            }
        }

        DocumentedRacesCountTextBlock.Text = RacesSoFar.Count.ToString(System.Globalization.CultureInfo.CurrentCulture);
        if (RacesSoFar.Count > 2)
        {
            GraphButton.IsEnabled = true;
            GraphWarningTextBlock.Visibility = Visibility.Hidden;
        } // Only enable this button when there are 3 or more races in the season
    }

    private void Window_ContentRendered(object sender, EventArgs e)
    {
        Title = $"Season's scores {_raceSeason.Anno}";
        SeasonTextBlock.Text = $"{_raceSeason.Anno} CHAMPIONSHIP";

        DisplayResultsList();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void DriversSchemeButton_Click(object sender, RoutedEventArgs e)
    {
        ScoreSchemeWindow w
            = new ScoreSchemeWindow(_raceSeason.PointsAllocationSchemeForDrivers, $"{_raceSeason.Anno} Drivers'")
                {Owner = this};
        bool? Q = w.ShowDialog();
        if ((Q.HasValue) && (Q.Value))
        {
            _raceSeason.PointsAllocationSchemeForDrivers.Specification = w.Scheme.Specification;
            _raceSeason.RefreshStatistics();
            DisplayResultsList();
        }
    }

    private void ConstructorsSchemeButton_Click(object sender, RoutedEventArgs e)
    {
        ScoreSchemeWindow w = new ScoreSchemeWindow(_raceSeason.PointsAllocationSchemeForConstructors
            , $"{_raceSeason.Anno} Constructors'") {Owner = this};
        bool? Q = w.ShowDialog();
        if ((Q.HasValue) && (Q.Value))
        {
            _raceSeason.PointsAllocationSchemeForConstructors.Specification = w.Scheme.Specification;
            _raceSeason.RefreshStatistics();
            DisplayResultsList();
        }
    }

    private void ScoringButton_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Process.Start(Core.WikiLinkPoints);
    }

    private void GraphButton_Click(object sender, RoutedEventArgs e)
    {
        SeasonGraphWindow w = new SeasonGraphWindow(_raceSeason.Anno) {Owner = this};
        w.ShowDialog();
    }

    private void StatedRaceCountTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        StatedRaceCountButton.IsEnabled = true;
        StatedRaceCountButton.Foreground = Brushes.Red;
    }

    private void StatedRaceCountButton_Click(object sender, RoutedEventArgs e)
    {
        string qs = StatedRaceCountTextBox.Text.Trim();
        bool flag = false;
        if (string.IsNullOrEmpty(qs))
        {
            flag = true;
        }
        else
        {
            if (int.TryParse(qs, out int qi))
            {
                if (qi < _raceSeason.RaceKeyList.Count)
                {
                    flag = true;
                }
                else
                {
                    _raceSeason.StatedNumberOfRaces = qi;
                }
            }
            else
            {
                flag = true;
            }
        }

        if (flag)
        {
            MessageBox.Show("The stated number of races is invalid or less than the number of documented races"
                , Jbh.AppManager.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        StatedRaceCountButton.IsEnabled = false;
        StatedRaceCountButton.Foreground = Brushes.Gray;
    }
}