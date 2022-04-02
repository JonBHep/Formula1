using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Formula1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double scrX = System.Windows.SystemParameters.PrimaryScreenWidth;
            double scrY = System.Windows.SystemParameters.PrimaryScreenHeight;
            double winX = scrX * .98;
            double winY = scrY * .94;
            double Xm = (scrX - winX) / 2;
            double Ym = (scrY - winY) / 4;
            this.Width = winX;
            this.Height = winY;
            this.Left = Xm;
            this.Top = Ym;
            SaveButton.IsEnabled = false;
            ButtonsPanel.Visibility = Visibility.Hidden;
            DriverEditButton.Visibility = Visibility.Hidden;
        }

        private void DetectBadDrivers()
        {
            DriverEditButton.Visibility = Visibility.Hidden;
            foreach (int dk in Core.Instance.Drivers.Keys)
            {
                if (!Core.Instance.Drivers[dk].Complete)
                {
                    DriverEditButton.Tag = dk;
                    DriverEditButton.Visibility = Visibility.Visible;
                    break;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Core.Instance.SaveData();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Jbh.UiServices.SetBusyState();
            ListEverything();
            ButtonsPanel.Visibility = Visibility.Visible;
        }

        private void ListEverything()
        {
            DetectBadDrivers();
            ListDecades();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Jbh.UiServices.SetBusyState();
            Close();
        }

        private void RacesButton_Click(object sender, RoutedEventArgs e)
        {
            RaceListWindow w = new RaceListWindow() {Owner = this};
            this.Hide();
            w.ShowDialog();
            this.Show();
            if (w.DoneSomething)
            {
                ListEverything();
                SaveButton.IsEnabled = true;
            }
        }

        private void ChronologyButton_Click(object sender, RoutedEventArgs e)
        {
            ChronologyWindow w = new ChronologyWindow() {Owner = this};
            w.ShowDialog();
        }

        private void DriversHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            DriverStatsWindow w = new DriverStatsWindow() {Owner = this};
            bool? q = w.ShowDialog();
            if (q.HasValue && q.Value)
            {
                DetectBadDrivers();
            }
        }

        private void RipButton_Click(object sender, RoutedEventArgs e)
        {
            RipWindow w = new RipWindow() {Owner = this};
            w.ShowDialog();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Jbh.UiServices.SetBusyState();
            Core.Instance.SaveData();
            SaveButton.IsEnabled = false;
        }

        private void CountryButton_Click(object sender, RoutedEventArgs e)
        {
            CountriesWindow w = new CountriesWindow("countries") {Owner = this};
            w.ShowDialog();
        }

        private void ConstructorsButton_Click(object sender, RoutedEventArgs e)
        {
            ConstructorStatsWindow w = new ConstructorStatsWindow() {Owner = this};
            w.ShowDialog();
        }

        private void TimeButton_Click(object sender, RoutedEventArgs e)
        {
            TimelineWindow w = new TimelineWindow() {Owner = this};
            w.ShowDialog();
        }

        private void GrandPrixButton_Click(object sender, RoutedEventArgs e)
        {
            CountriesWindow w = new CountriesWindow("grandsprix") {Owner = this};
            w.ShowDialog();
        }

        private void DriverTablesButton_Click(object sender, RoutedEventArgs e)
        {
            TablesWindow w = new TablesWindow() {Owner = this};
            w.ShowDialog();
        }

        private void CircuitButton_Click(object sender, RoutedEventArgs e)
        {
            CountriesWindow w = new CountriesWindow("circuits") {Owner = this};
            w.ShowDialog();
            if (w.MadeEdit)
            {
                SaveButton.IsEnabled = true;
            }
        }



        private void RefreshSeasonRaces(Season sn)
        {
            TextBlock tba = new TextBlock()
                {Text = $"{sn.Anno} CALENDAR", Foreground = Brushes.DarkGreen, FontWeight = FontWeights.Bold};
            ListBoxItem itm = new ListBoxItem() {Content = tba, IsHitTestVisible = false};
            GrandsPrixListBox.Items.Add(itm);
            List<int> racekeys = sn.RaceKeyList;
            int x = 0;
            foreach (int k in racekeys)
            {
                x++;
                RaceMeeting mtg = Core.Instance.Races[k];
                TextBlock tbb = new TextBlock() {Text = $"R{x}", MinWidth = 40, Foreground = Brushes.SeaGreen};
                TextBlock tbc = new TextBlock()
                {
                    Text = Core.Instance.RaceTitles[mtg.RaceTitleKey].Caption, Foreground = Brushes.DarkGreen
                    , FontWeight = FontWeights.Medium
                };
                StackPanel spl = new StackPanel() {Orientation = Orientation.Horizontal};
                spl.Children.Add(tbb);
                spl.Children.Add(tbc);
                ListBoxItem lbi = new ListBoxItem() {Content = spl, Tag = k};
                GrandsPrixListBox.Items.Add(lbi);
            }
        }

        private void RefreshSeasonDrivers(Season sn)
        {
            int champ = sn.DriverSeasonRanked(1);
            if (champ == 0)
            {
                return;
            }

            TextBlock tba = new TextBlock()
                {Text = $"{sn.Anno} CHAMPION", Foreground = Brushes.DarkGreen, FontWeight = FontWeights.Bold};
            ListBoxItem itm = new ListBoxItem() {Content = tba, IsHitTestVisible = false};
            WinnersListBox.Items.Add(itm);
            tba = new TextBlock()
            {
                Margin = new Thickness(12, 6, 0, 0), Text = Core.Instance.Drivers[champ].FullName.ToUpperInvariant()
                , Foreground = Brushes.SaddleBrown, FontWeight = FontWeights.Bold
            };
            itm = new ListBoxItem() {Content = tba, IsHitTestVisible = false};
            WinnersListBox.Items.Add(itm);

            List<Tuple<int, string>> champions = sn.NumberOfDifferentVictorsInSeason();
            TextBlock t = new TextBlock()
            {
                Margin = new Thickness(0, 12, 0, 0), Text = "Drivers winning a race this season"
                , FontWeight = FontWeights.Bold, Foreground = Brushes.SeaGreen
            };
            itm = new ListBoxItem() {Content = t, IsHitTestVisible = false};
            WinnersListBox.Items.Add(itm);

            t = new TextBlock()
            {
                Text = $"{champions.Count} victors in the {sn.RaceKeyList.Count} races", FontWeight = FontWeights.Normal
                , Foreground = Brushes.DarkGreen
            };
            itm = new ListBoxItem() {Content = t, IsHitTestVisible = false};
            WinnersListBox.Items.Add(itm);

            foreach (Tuple<int, string> gagnant in champions)
            {
                t = new TextBlock() {Margin = new Thickness(12, 6, 0, 0)};
                Run r = new Run() {Text = gagnant.Item2, FontWeight = FontWeights.Bold, Foreground = Brushes.SeaGreen};
                t.Inlines.Add(r);
                string vix = (gagnant.Item1 > 1) ? "wins" : "win";
                r = new Run()
                    {Text = $" {gagnant.Item1} {vix}", FontWeight = FontWeights.Normal, Foreground = Brushes.DarkGreen};
                t.Inlines.Add(r);
                itm = new ListBoxItem() {Content = t, IsHitTestVisible = false};
                WinnersListBox.Items.Add(itm);
            }

            TextBlock z = new TextBlock()
            {
                Text = $"{sn.Anno} Drivers' Championship", FontWeight = FontWeights.Bold, Foreground = Brushes.SeaGreen
            };
            ListBoxItem zi = new ListBoxItem() {Content = z, IsHitTestVisible = false};
            ScorersListBox.Items.Add(zi);

            double[] cols = {60, 60, 140};

            // Headings
            TextBlock headingcounted = new TextBlock()
            {
                Text = "Counted ", TextAlignment = TextAlignment.Center, Foreground = Brushes.Black
                , FontWeight = FontWeights.Bold, Width = cols[0]
            };
            TextBlock headingtotal = new TextBlock()
            {
                Text = "(Total) ", TextAlignment = TextAlignment.Center, FontWeight = FontWeights.Normal
                , Width = cols[1]
            };
            TextBlock headingname = new TextBlock()
                {Text = "Driver", Foreground = Brushes.SaddleBrown, Width = cols[2], FontWeight = FontWeights.Bold};

            StackPanel stk = new StackPanel()
                {Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 6)};
            {
                stk.Children.Add(headingcounted);
                stk.Children.Add(headingtotal);
                stk.Children.Add(headingname);
            }
            ListBoxItem heads = new ListBoxItem() {Content = stk, IsHitTestVisible = false};
            ScorersListBox.Items.Add(heads);

            // Values
            int lastRace = sn.RaceKeyList[sn.RaceKeyList.Count - 1];
            foreach (IndividualScore driv in sn.RaceDriverResults(lastRace))
            {
                if (driv.CumulativeScore > 0)
                {
                    TextBlock tbscore = new TextBlock()
                    {
                        Text = Core.MyFormat(driv.CumulativeScoreCounted), TextAlignment = TextAlignment.Center
                        , Foreground = Brushes.Black, FontWeight = FontWeights.Bold, Width = cols[0]
                    };
                    string tot = (driv.CumulativeScoreCounted == driv.CumulativeScore)
                        ? string.Empty
                        : $"({Core.MyFormat(driv.CumulativeScore)})";
                    TextBlock tbtotal = new TextBlock()
                        {Text = tot, TextAlignment = TextAlignment.Center, Foreground = Brushes.Black, Width = cols[1]};
                    TextBlock tbdriver = new TextBlock()
                    {
                        Text = Core.Instance.Drivers[driv.IndividualKey].FullName, Foreground = Brushes.SaddleBrown
                        , Width = cols[2]
                    };

                    StackPanel stp = new StackPanel() {Orientation = Orientation.Horizontal};
                    {
                        stp.Children.Add(tbscore);
                        stp.Children.Add(tbtotal);
                        stp.Children.Add(tbdriver);
                    }
                    itm = new ListBoxItem() {Content = stp, IsHitTestVisible = false};
                    ScorersListBox.Items.Add(itm);
                }
            }

            if (sn.PointsAllocationSchemeForConstructors.Applies)
            {
                z = new TextBlock()
                {
                    Text = $"{sn.Anno} Constructors' Championship", FontWeight = FontWeights.Bold
                    , Margin = new Thickness(0, 12, 0, 0), Foreground = Brushes.SeaGreen
                };
                zi = new ListBoxItem() {Content = z, IsHitTestVisible = false};
                ScorersListBox.Items.Add(zi);

                // Headings
                headingcounted = new TextBlock()
                {
                    Text = "Counted ", TextAlignment = TextAlignment.Center, Foreground = Brushes.Black
                    , FontWeight = FontWeights.Bold, Width = cols[0]
                };
                headingtotal = new TextBlock()
                {
                    Text = "(Total) ", TextAlignment = TextAlignment.Center, FontWeight = FontWeights.Normal
                    , Width = cols[1]
                };
                headingname = new TextBlock()
                {
                    Text = "Constructor", Foreground = Brushes.SaddleBrown, Width = cols[2]
                    , FontWeight = FontWeights.Bold
                };

                stk = new StackPanel() {Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 6)};
                {
                    stk.Children.Add(headingcounted);
                    stk.Children.Add(headingtotal);
                    stk.Children.Add(headingname);
                }
                heads = new ListBoxItem() {Content = stk, IsHitTestVisible = false};
                ScorersListBox.Items.Add(heads);

                // Values
                foreach (IndividualScore team in sn.RaceConstructorResults(lastRace))
                {
                    if (team.CumulativeScore > 0)
                    {
                        TextBlock tbscore = new TextBlock()
                        {
                            Text = Core.MyFormat(team.CumulativeScoreCounted), TextAlignment = TextAlignment.Center
                            , Foreground = Brushes.Black, FontWeight = FontWeights.Bold, Width = cols[0]
                        };
                        string tot = (team.CumulativeScoreCounted == team.CumulativeScore)
                            ? string.Empty
                            : $"({Core.MyFormat(team.CumulativeScore)})";
                        TextBlock tbtotal = new TextBlock()
                        {
                            Text = tot, TextAlignment = TextAlignment.Center, Foreground = Brushes.Black
                            , Width = cols[1]
                        };
                        TextBlock tbdriver = new TextBlock()
                        {
                            Text = Core.Instance.Constructors[team.IndividualKey].Caption
                            , Foreground = Brushes.SaddleBrown, Width = cols[2]
                        };

                        StackPanel stp = new StackPanel() {Orientation = Orientation.Horizontal};
                        {
                            stp.Children.Add(tbscore);
                            stp.Children.Add(tbtotal);
                            stp.Children.Add(tbdriver);
                        }
                        itm = new ListBoxItem() {Content = stp, IsHitTestVisible = false};
                        ScorersListBox.Items.Add(itm);
                    }
                }
            }

            z = new TextBlock()
            {
                Text = $"{sn.Anno} Non-scoring drivers", FontWeight = FontWeights.Bold
                , Margin = new Thickness(0, 12, 0, 0), Foreground = Brushes.SeaGreen
            };
            zi = new ListBoxItem() {Content = z, IsHitTestVisible = false};
            ScorersListBox.Items.Add(zi);

            List<int> Losers = sn.SeasonNonScoringDrivers();
            List<Driver> Losing = new List<Driver>();
            foreach (int k in Losers)
            {
                Losing.Add(Core.Instance.Drivers[k]);
            }

            Losing.Sort();
            foreach (Driver d in Losing)
            {
                TextBlock tbd = new TextBlock() {Text = d.FullName, Foreground = Brushes.SaddleBrown};
                ListBoxItem lbd = new ListBoxItem() {Content = tbd, IsHitTestVisible = false};
                ScorersListBox.Items.Add(lbd);
            }
        }

        private void GrandsPrixListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RaceResultsListBox.Items.Clear();
            TeamsListBox.Items.Clear();
            if (GrandsPrixListBox.SelectedIndex < 0)
            {
                return;
            }

            ListBoxItem i = (ListBoxItem) GrandsPrixListBox.SelectedItem;
            if (!i.IsHitTestVisible)
            {
                return;
            }

            int k = (int) i.Tag;
            RaceMeeting mtg = Core.Instance.Races[k];
            SummariseSelectedRace(mtg);
        }

        private void SummariseSelectedRace(RaceMeeting meet)
        {
            TextBlock tb = new TextBlock()
            {
                Text = Core.Instance.RaceTitles[meet.RaceTitleKey].Caption.ToUpperInvariant()
                , Foreground = Brushes.DarkGreen, FontWeight = FontWeights.Bold
            };
            ListBoxItem lbi = new ListBoxItem() {Content = tb, IsHitTestVisible = false};
            RaceResultsListBox.Items.Add(lbi);

            tb = new TextBlock() {Text = meet.RaceDate.ToLongDateString()};
            lbi = new ListBoxItem() {Content = tb, IsHitTestVisible = false};
            RaceResultsListBox.Items.Add(lbi);

            tb = new TextBlock() {Text = Core.Instance.RaceTracks[meet.CircuitKey].CircuitSingleTitle(0)};
            lbi = new ListBoxItem() {Content = tb, IsHitTestVisible = false};
            RaceResultsListBox.Items.Add(lbi);

            tb = new TextBlock() {Text = $"Qualifiers: {meet.Qualifiers}"};
            lbi = new ListBoxItem() {Content = tb, IsHitTestVisible = false};
            RaceResultsListBox.Items.Add(lbi);

            int f = meet.Finishers;
            tb = new TextBlock() {Text = $"Finishers: {f}"};
            lbi = new ListBoxItem() {Content = tb, IsHitTestVisible = false};
            RaceResultsListBox.Items.Add(lbi);

            int K11 = meet.PodiumOne.Item1;
            int K12 = meet.PodiumOne.Item2;
            int K13 = meet.PodiumOne.Item3;

            string P1 = (K11 > 0) ? Core.Instance.Drivers[K11].FullName : "";
            int u = (K11 > 0) ? Core.Instance.Drivers[K11].WinsUpTo(meet.RaceDate) : 0;
            string N1 = (u > 0) ? $"{Core.Ordinal(u)} win for " : string.Empty;

            string P2 = (K12 > 0) ? $" and {Core.Instance.Drivers[K12].FullName}" : "";
            u = (K12 > 0) ? Core.Instance.Drivers[K12].WinsUpTo(meet.RaceDate) : 0;
            string N2 = (u > 0) ? $"{Core.Ordinal(u)} win for " : string.Empty;

            string P3 = (K13 > 0) ? $" and {Core.Instance.Drivers[K12].FullName}" : "";
            u = (K13 > 0) ? Core.Instance.Drivers[K13].WinsUpTo(meet.RaceDate) : 0;
            string N3 = (u > 0) ? $"{Core.Ordinal(u)} win for " : string.Empty;

            string winr = N1 + P1 + N2 + P2 + N3 + P3;
            tb = new TextBlock()
            {
                Text = winr, FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen
                , Margin = new Thickness(0, 0, 0, 16)
            };
            lbi = new ListBoxItem() {Content = tb, IsHitTestVisible = false};
            RaceResultsListBox.Items.Add(lbi);

            List<Voiture> outcome = new List<Voiture>();
            foreach (Voiture r in Core.Instance.Voitures.Values)
            {
                if (r.RaceMeetingKey == meet.Key)
                {
                    outcome.Add(r);
                }
            }

            outcome.Sort();

            bool dunpodium = false;
            bool dunpoints = false;
            bool dunplacing = false;
            List<int> _raceDrivers = new List<int>();
            foreach (Voiture r in outcome)
            {
                if ((!dunpodium) && (r.RacePosition > 3))
                {
                    dunpodium = true;
                    Rectangle sq = new Rectangle()
                        {Width = RaceResultsListBox.ActualWidth * 0.9, Height = 2, Fill = Brushes.Gold};
                    ListBoxItem l = new ListBoxItem()
                        {Content = sq, IsHitTestVisible = false, Margin = new Thickness(0, 0, 0, 0)};
                    RaceResultsListBox.Items.Add(l);
                }

                if ((!dunpoints) && (r.AggregatedPositionPoints == 0))
                {
                    dunpoints = true;
                    Rectangle sq = new Rectangle()
                        {Width = RaceResultsListBox.ActualWidth * 0.9, Height = 2, Fill = Brushes.DarkKhaki};
                    ListBoxItem l = new ListBoxItem()
                        {Content = sq, IsHitTestVisible = false, Margin = new Thickness(0, 0, 0, 0)};
                    RaceResultsListBox.Items.Add(l);
                }

                if ((!dunplacing) && (!r.Finished))
                {
                    dunplacing = true;
                    Rectangle sq = new Rectangle()
                        {Width = RaceResultsListBox.ActualWidth * 0.9, Height = 2, Fill = Brushes.DarkKhaki};
                    ListBoxItem l = new ListBoxItem()
                        {Content = sq, IsHitTestVisible = false, Margin = new Thickness(0, 0, 0, 0)};
                    RaceResultsListBox.Items.Add(l);
                    TextBlock t = new TextBlock() {Text = "NOT PLACED"};
                    l = new ListBoxItem()
                    {
                        Content = t, IsHitTestVisible = false, Foreground = Brushes.SteelBlue
                        , FontWeight = FontWeights.Medium, Margin = new Thickness(0, 6, 0, 0)
                    };
                    RaceResultsListBox.Items.Add(l);
                }

                // set colours
                Brush pinceau = (r.Finished) ? Brushes.DarkGreen : Brushes.SteelBlue;
                if ((Core.RaceResultConstants) r.RacePosition == Core.RaceResultConstants.RetFatalDriver)
                {
                    pinceau = Brushes.Red;
                }

                if ((Core.RaceResultConstants) r.RacePosition == Core.RaceResultConstants.DidNotStart)
                {
                    pinceau = Brushes.Black;
                }

                if (r.Formula2)
                {
                    pinceau = Brushes.Salmon;
                }

                FontWeight weight = (r.AggregatedAllPoints > 0) ? FontWeights.Bold : FontWeights.Normal;
                if (!dunpodium)
                {
                    weight = FontWeights.Black;
                }

                StackPanel panel = new StackPanel() {Orientation = Orientation.Horizontal};

                TextBlock tbOutcome = new TextBlock()
                {
                    Foreground = pinceau, Margin = new Thickness(0, 0, 6, 0)
                }; // margin is to provide a gap in case the outcome will be followed by a 'fastest lap' note
                TextBlock tbPoints = new TextBlock() {Foreground = Brushes.Magenta};
                TextBlock tbOutcomeCode = new TextBlock() {Foreground = pinceau, FontWeight = FontWeights.Black};

                // Position TextBlock (placeholder if driver not placed)
                TextBlock tbposn = new TextBlock()
                    {MinWidth = 32, Margin = new Thickness(0, 0, 6, 0), TextAlignment = TextAlignment.Right};
                string rrd = Core.RaceResultDescription(r.RacePosition);
                if (r.Finished)
                {
                    tbposn.Text = rrd.PadLeft(3);
                    tbposn.Foreground = pinceau;
                    tbposn.FontWeight = weight;
                }
                else
                {
                    tbOutcome.Text = rrd.Substring(4);
                    tbOutcomeCode.Text = rrd.Substring(0, 3);
                    tbOutcomeCode.MinWidth = 32;
                }

                // Grid position TextBlock
                string gridpos = (r.GridPosition == Core.SpecialNumber)
                    ? "PL"
                    : r.GridPosition.ToString(Core.CultureUK);
                TextBlock tbgrid = new TextBlock()
                {
                    Text = $"Grid: {gridpos}", MinWidth = 56, Foreground = Brushes.DarkSlateBlue
                    , Margin = new Thickness(0, 0, 6, 0)
                };

                // Driver TextBlock
                TextBlock DriversTextBlock = new TextBlock() {MinWidth = 250, Margin = new Thickness(0, 0, 6, 0)};
                DateTime racedate = Core.Instance.Races[meet.Key].RaceDate;
                _raceDrivers.Add(r.DriverKey(0));
                DriversTextBlock.Inlines.Add(new Run(Core.Instance.Drivers[r.DriverKey(0)].FullName)
                    {Foreground = pinceau, FontWeight = weight});

                if (r.DriverKey(1) > 0)
                {
                    _raceDrivers.Add(r.DriverKey(1));
                    DriversTextBlock.Inlines.Add(new Run($" and {Core.Instance.Drivers[r.DriverKey(1)].FullName}")
                        {Foreground = pinceau, FontWeight = weight});

                }

                if (r.DriverKey(2) > 0)
                {
                    _raceDrivers.Add(r.DriverKey(2));
                    DriversTextBlock.Inlines.Add(new Run($" and {Core.Instance.Drivers[r.DriverKey(2)].FullName}")
                        {Foreground = pinceau, FontWeight = weight});

                }

                if (r.Formula2)
                {
                    DriversTextBlock.Inlines.Add(new Run(" (F2 car)") {Foreground = pinceau});
                }

                string teamtext = Core.ConstructorName(Core.Instance.Constructors[r.ConstructorKey].Caption);
                Run rc = new Run() {Text = $" ({teamtext})", Foreground = pinceau, FontWeight = weight};
                DriversTextBlock.Inlines.Add(rc);

                if (r.ConstructorPointsDisallowed)
                {
                    rc = new Run()
                        {Text = " (no points)", Foreground = Brushes.Salmon, FontWeight = FontWeights.Normal};
                    DriversTextBlock.Inlines.Add(rc);
                }

                if (r.ConstructorPointsPenalty > 0)
                {
                    rc = new Run()
                    {
                        Text = $" (fined {r.ConstructorPointsPenalty} points)", Foreground = Brushes.Salmon
                        , FontWeight = FontWeights.Normal
                    };
                    DriversTextBlock.Inlines.Add(rc);
                }

                if (r.Controversial)
                {
                    rc = new Run() {Text = " Controversial", Foreground = Brushes.Red, FontWeight = FontWeights.Medium};
                    DriversTextBlock.Inlines.Add(rc);
                }

                // Points TextBlock (shows outcome for non-placed cars)
                string pString = string.Empty;
                if (r.AggregatedPositionPoints > 0)
                {
                    if (r.DriverCount > 2)
                    {
                        pString
                            += $"Points: {r.PointsForPosition(0).FloatValue}-{r.PointsForPosition(1).FloatValue}-{r.PointsForPosition(2).FloatValue}";
                        if (r.PointsForSprintQualifying(0).FloatValue > 0)
                        {
                            pString += " + Sprint Qualifying (D1)";
                        }

                        if (r.PointsForSprintQualifying(1).FloatValue > 0)
                        {
                            pString += " + Sprint Qualifying (D2)";
                        }

                        if (r.PointsForSprintQualifying(2).FloatValue > 0)
                        {
                            pString += " + Sprint Qualifying (D3)";
                        }

                        if (r.PointsForFastestLap(0).FloatValue > 0)
                        {
                            pString += " + fastest lap (D1)";
                        }

                        if (r.PointsForFastestLap(1).FloatValue > 0)
                        {
                            pString += " + fastest lap (D2)";
                        }

                        if (r.PointsForFastestLap(2).FloatValue > 0)
                        {
                            pString += " + fastest lap (D3)";
                        }
                    }
                    else if (r.DriverCount > 1)
                    {
                        pString += $"Points: {r.PointsForPosition(0).FloatValue}-{r.PointsForPosition(1).FloatValue}";
                        if (r.PointsForSprintQualifying(0).FloatValue > 0)
                        {
                            pString += " + Sprint Qualifying (D1)";
                        }

                        if (r.PointsForSprintQualifying(1).FloatValue > 0)
                        {
                            pString += " + Sprint Qualifying (D2)";
                        }

                        if (r.PointsForFastestLap(0).FloatValue > 0)
                        {
                            pString += " + fastest lap (D1)";
                        }

                        if (r.PointsForFastestLap(1).FloatValue > 0)
                        {
                            pString += " + fastest lap (D2)";
                        }
                    }
                    else
                    {
                        pString = $"Points: {r.PointsForPosition(0).FloatValue}";
                        if (r.PointsForSprintQualifying(0).FloatValue > 0)
                        {
                            pString += " + Sprint Qualifying";
                        }

                        if (r.PointsForFastestLap(0).FloatValue > 0)
                        {
                            pString += " + fastest lap";
                        }
                    }
                }
                else
                {
                    if (r.DriverCount > 1)
                    {
                        if (r.PointsForSprintQualifying(0).FloatValue > 0)
                        {
                            pString += " + Sprint Qualifying (D1)";
                        }

                        if (r.PointsForSprintQualifying(1).FloatValue > 0)
                        {
                            pString += " + Sprint Qualifying (D2)";
                        }

                        if (r.PointsForSprintQualifying(2).FloatValue > 0)
                        {
                            pString += " + Sprint Qualifying (D3)";
                        }

                        if (r.PointsForFastestLap(0).FloatValue > 0)
                        {
                            pString += "Fastest lap (D1)";
                        }

                        if (r.PointsForFastestLap(1).FloatValue > 0)
                        {
                            pString += " Fastest lap (D2)";
                        }

                        if (r.PointsForFastestLap(2).FloatValue > 0)
                        {
                            pString += " Fastest lap (D3)";
                        }
                    }
                    else
                    {
                        if (r.PointsForSprintQualifying(0).FloatValue > 0)
                        {
                            pString += " + Sprint Qualifying";
                        }

                        if (r.PointsForFastestLap(0).FloatValue > 0)
                        {
                            pString += "Fastest lap";
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(pString))
                {
                    tbPoints.Text = pString;
                }

                if (dunplacing)
                {
                    panel.Children.Add(tbgrid);
                }
                else
                {
                    panel.Children.Add(tbposn);
                }

                panel.Children.Add(DriversTextBlock);
                panel.Children.Add(tbOutcome);
                panel.Children.Add(tbPoints);

                ListBoxItem it = new ListBoxItem() {Content = panel, IsHitTestVisible = false};
                RaceResultsListBox.Items.Add(it);

            }

            List<int> Debutants = new List<int>();
            foreach (Driver dr in Core.Instance.Drivers.Values)
            {
                if (dr.RuntimeFirstRaceKey == meet.Key)
                {
                    Debutants.Add(dr.Key);
                }
            }

            if (Debutants.Count > 0)
            {
                TextBlock tj = new TextBlock()
                {
                    Text = "Debut race for:", Foreground = Brushes.SeaGreen, FontWeight = FontWeights.Bold
                    , Margin = new Thickness(0, 12, 0, 0)
                };
                RaceResultsListBox.Items.Add(new ListBoxItem() {Content = tj, IsHitTestVisible = false});
                foreach (int j in Debutants)
                {
                    tj = new TextBlock() {Text = Core.Instance.Drivers[j].FullName, Foreground = Brushes.SaddleBrown};
                    RaceResultsListBox.Items.Add(new ListBoxItem() {Content = tj, IsHitTestVisible = false});
                }
            }

            TeamsListBox.Items.Clear();
            bool drapeau = false;
            List<int> seasonTeamKeys = Core.Instance.TeamsInSeason(meet.RaceDate.Year);
            List<int> trkeys = Core.Instance.TeamsInRace(meet.Key);
            List<NamedItem> teamlist = new List<NamedItem>();
            foreach (int i in seasonTeamKeys)
            {
                teamlist.Add(Core.Instance.Constructors[i]);
            }

            teamlist.Sort();

            foreach (NamedItem ni in teamlist)
            {
                Brush brosse = Brushes.SaddleBrown;
                TextDecorationCollection decor = new TextDecorationCollection();
                if (!trkeys.Contains(ni.Key))
                {
                    brosse = Brushes.Coral;
                    decor.Add(TextDecorations.Strikethrough);
                    drapeau = true;
                }

                TextBlock tt = new TextBlock()
                {
                    Text = Core.ConstructorName(ni.Caption), Foreground = brosse, FontWeight = FontWeights.Bold
                    , TextDecorations = decor
                };
                ListBoxItem tlbi = new ListBoxItem() {Content = tt};
                TeamsListBox.Items.Add(tlbi);

                List<int> tdkeys = Core.Instance.DriversinTeamInRace(meet.Key, ni.Key);
                List<int> seasonDriverKeys = Core.Instance.DriversinTeamInSeason(meet.RaceDate.Year, ni.Key);
                foreach (int dk in seasonDriverKeys)
                {
                    brosse = Brushes.SeaGreen;
                    decor = new TextDecorationCollection();
                    if (!tdkeys.Contains(dk))
                    {
                        brosse = Brushes.Coral;
                        decor.Add(TextDecorations.Strikethrough);
                        drapeau = true;
                    }

                    TextBlock dt = new TextBlock()
                    {
                        Text = Core.Instance.Drivers[dk].FullName, Foreground = brosse
                        , Margin = new Thickness(12, 0, 0, 0), TextDecorations = decor
                    };
                    ListBoxItem dlbi = new ListBoxItem() {Content = dt};
                    TeamsListBox.Items.Add(dlbi);
                }
            }

            if (drapeau)
            {
                TextBlock dt = new TextBlock()
                    {Text = "Teams or drivers in red feature in this", Foreground = Brushes.Coral};
                ListBoxItem rlbi = new ListBoxItem()
                    {Content = dt, IsHitTestVisible = false, Margin = new Thickness(0, 12, 0, 0)};
                TeamsListBox.Items.Add(rlbi);
                dt = new TextBlock() {Text = "season but not in this race", Foreground = Brushes.Coral};
                rlbi = new ListBoxItem() {Content = dt, IsHitTestVisible = false};
                TeamsListBox.Items.Add(rlbi);
            }
        }

        private void DriverEditButton_Click(object sender, RoutedEventArgs e)
        {
            int key = (int) DriverEditButton.Tag;
            DriverPropertiesWindow w = new DriverPropertiesWindow(key) {Owner = this};
            this.Hide();
            bool? q = w.ShowDialog();
            if (q.HasValue && q.Value)
            {
                DetectBadDrivers();
                SaveButton.IsEnabled = true;
            }

            this.Show();
        }

        private void ListDecades()
        {
            DecadesListBox.Items.Clear();
            YearsListBox.Items.Clear();
            List<int> lst = Core.Instance.Decades;
            foreach (int y in lst)
            {
                TextBlock tbk = new TextBlock()
                    {Text = $"{y}0s", FontWeight = FontWeights.Medium, Foreground = Brushes.SaddleBrown};
                ListBoxItem itm = new ListBoxItem() {Content = tbk, Tag = y};
                DecadesListBox.Items.Add(itm);
            }
        }

        private void YearsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GrandsPrixListBox.Items.Clear();
            WinnersListBox.Items.Clear();
            ScorersListBox.Items.Clear();
            if (YearsListBox.SelectedIndex < 0)
            {
                return;
            }

            ListBoxItem i = (ListBoxItem) YearsListBox.SelectedItem;
            int y = (int) i.Tag;
            Season ssn = Core.Instance.Seasons[y];
            ssn.RefreshStatistics();
            RefreshSeasonRaces(ssn);
            RefreshSeasonDrivers(ssn);
        }

        private void YearsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (YearsListBox.SelectedIndex < 0)
            {
                return;
            }

            ListBoxItem i = (ListBoxItem) YearsListBox.SelectedItem;
            int y = (int) i.Tag;
            SeasonScoresWindow w = new SeasonScoresWindow(y) {Owner = this};
            w.ShowDialog();
            ListDecades();
        }

        private void DecadesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            YearsListBox.Items.Clear();
            if (DecadesListBox.SelectedIndex < 0)
            {
                return;
            }

            ListBoxItem i = (ListBoxItem) DecadesListBox.SelectedItem;
            int y = (int) i.Tag;
            y *= 10;
            List<int> yearlist = Core.Instance.Years;
            for (int annee = y; annee < y + 10; annee++)
            {
                if (yearlist.Contains(annee))
                {
                    TextBlock tbk = new TextBlock()
                        {Text = annee.ToString(Core.CultureUK), FontWeight = FontWeights.Bold};
                    ListBoxItem itm = new ListBoxItem() {Content = tbk, Tag = annee};
                    YearsListBox.Items.Add(itm);
                }
            }
        }

        private void LivingDriversButton_Click(object sender, RoutedEventArgs e)
        {
            DriversAliveWindow w = new DriversAliveWindow() {Owner = this};
            bool? q = w.ShowDialog();

            if (q.HasValue && q.Value)
            {
                DetectBadDrivers();
                SaveButton.IsEnabled = true;
            }
        }
    }
}