using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Formula1;

public partial class RaceMeetingResultsWindow : Window
{
    private readonly int _raceMeetingKey;
        private readonly List<float> _likelyPointsScheme;
        private List<int> _raceDrivers;
        private readonly DateTime _raceDate;

        public RaceMeetingResultsWindow(int RaceMtgKey)
        {
            InitializeComponent();
            _raceMeetingKey = RaceMtgKey;
            _raceDate = Core.Instance.Races[_raceMeetingKey].RaceDate;
            _likelyPointsScheme =Core.Instance.ScoringSystemForLastRaceBefore(_raceDate);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            RaceMeeting m = Core.Instance.Races[_raceMeetingKey];
            TitleTextBlock.Text = Core.Instance.RaceTitles[m.RaceTitleKey].Caption;
            DateTextBlock.Text = m.RaceDate.ToLongDateString();
            CircuitTextBlock.Text = Core.Instance.RaceTracks[m.CircuitKey].CircuitName[0];
            ListResults();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            CarResultWindow w = new CarResultWindow(new Voiture(0) { RaceMeetingKey = _raceMeetingKey }, _likelyPointsScheme, _raceDrivers, _raceDate) { Owner = this };
            bool? q = w.ShowDialog();
            if (q.HasValue && q.Value)
            {
                int rk = 1;
                while (Core.Instance.Voitures.ContainsKey(rk)) { rk++; }
                Voiture r = new Voiture(rk)
                {
                    Specification = w.Result.Specification
                };
                Core.Instance.Voitures.Add(rk, r);
                ListResults();
            }
        }

        private void DelButton_Click(object sender, RoutedEventArgs e)
        {
            int ky = (int)DeleteButton.Tag;
            Core.Instance.Voitures.Remove(ky);
            ListResults();
        }
        private void SetConstructorsChampionshipNotice(bool f)
        {
            if (f)
            {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                ConstructorsTextBlock.Text = "Counts towards Constructors' Championship";
#pragma warning restore CA1303 // Do not pass literals as localized parameters
                ConstructorsTextBlock.Foreground = Brushes.Black;
            }
            else
            {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                ConstructorsTextBlock.Text = "Does not count towards Constructors' Championship";
#pragma warning restore CA1303 // Do not pass literals as localized parameters
                ConstructorsTextBlock.Foreground = Brushes.Red;
            }
        }

        private void ListResults()
        {
            Dictionary<int, string> Gpos = new Dictionary<int, string>();

            RaceMeeting rm = Core.Instance.Races[_raceMeetingKey];

            SetConstructorsChampionshipNotice(rm.CountForConstructorsChampionship);
            Core.RankingHealth GapsAndDupes = rm.GridGaps;

            if (rm.Finishers > 1)
            {
                double kt = rm.KendalTau();
                TauTextBlock.Text = $"{kt:0.00}";
            }
            else
            {
                TauTextBlock.Text = string.Empty;
            }
            
            switch (GapsAndDupes)
            {
                case Core.RankingHealth.HasDuplicates:
                    {
                        //GridStatusTextBlock.Background = Brushes.Red;
                        //GridStatusTextBlock.Text = "Bad grid";
                        //GridStatusTextBlock.ToolTip = "Duplicate(s) in starting grid";
                        GridPositionsBorder.BorderBrush = Brushes.Red;
                        GridPositionsTextBlock.Foreground = Brushes.Red;
                        GridPositionsTextBlock.ToolTip = "Duplicate(s) in starting grid";
                        break;
                    }
                case Core.RankingHealth.HasGaps:
                    {
                        //GridStatusTextBlock.Background = Brushes.Red;
                        //GridStatusTextBlock.Text = "Bad grid";
                        //GridStatusTextBlock.ToolTip = "Gap(s) in starting grid";
                        GridPositionsBorder.BorderBrush = Brushes.Red;
                        GridPositionsTextBlock.Foreground = Brushes.Red;
                        GridPositionsTextBlock.ToolTip = "Gap(s) in starting grid";
                        break;
                    }
                case Core.RankingHealth.HasGapsAndDupes:
                    {
                        //GridStatusTextBlock.Background = Brushes.Red;
                        //GridStatusTextBlock.Text = "Bad grid";
                        //GridStatusTextBlock.ToolTip = "Gap(s) and duplicates() in starting grid";
                        GridPositionsBorder.BorderBrush = Brushes.Red;
                        GridPositionsTextBlock.Foreground = Brushes.Red;
                        GridPositionsTextBlock.ToolTip = "Gap(s) and duplicates() in starting grid";
                        break;
                    }
                default:
                    {
                        //GridStatusTextBlock.Text = "Grid OK";
                        //GridStatusTextBlock.Background = Brushes.ForestGreen;
                        //GridStatusTextBlock.ToolTip = "No gaps or duplicates in starting grid";
                        GridPositionsBorder.BorderBrush = Brushes.ForestGreen;
                        GridPositionsTextBlock.Foreground = Brushes.Black;
                        GridPositionsTextBlock.ToolTip = "No gaps or duplicates in starting grid";
                        break;
                    }
            }

            DeleteButton.IsEnabled = false;

            CompetitorsListBox.Items.Clear();
            GridPositionListBox.Items.Clear();

            List<Voiture> outcome = new List<Voiture>();
            foreach (Voiture r in Core.Instance.Voitures.Values)
            {
                if (r.RaceMeetingKey == _raceMeetingKey)
                {
                    outcome.Add(r);
                }
            }
            outcome.Sort();
            bool dunpodium = false;
            bool dunpoints = false;
            bool dunplacing = false;
            _raceDrivers = new List<int>();
            foreach (Voiture r in outcome)
            {
                if ((!dunpodium) && (r.RacePosition > 3))
                {
                    dunpodium = true;
                    Rectangle sq = new Rectangle() { Width = CompetitorsListBox.ActualWidth * 0.9, Height = 2, Fill = Brushes.Gold };
                    ListBoxItem l = new ListBoxItem() { Content = sq, IsHitTestVisible = false, Margin = new Thickness(0, 0, 0, 0) };
                    CompetitorsListBox.Items.Add(l);
                }
                if ((!dunpoints) && (r.AggregatedPositionPoints==0))
                {
                    dunpoints = true;
                    Rectangle sq = new Rectangle() { Width = CompetitorsListBox.ActualWidth*0.9, Height = 2, Fill = Brushes.DarkKhaki };
                    ListBoxItem l = new ListBoxItem() { Content = sq, IsHitTestVisible = false, Margin = new Thickness(0, 0, 0, 0) };
                    CompetitorsListBox.Items.Add(l);
                }
                if ((!dunplacing) && (!r.Finished))
                {
                    dunplacing = true;
                    Rectangle sq = new Rectangle() { Width = CompetitorsListBox.ActualWidth * 0.9, Height = 2, Fill = Brushes.DarkKhaki };
                    ListBoxItem l = new ListBoxItem() { Content = sq, IsHitTestVisible = false, Margin = new Thickness(0, 0, 0, 0) };
                    CompetitorsListBox.Items.Add(l);
                    TextBlock t = new TextBlock() { Text = "NOT PLACED" };
                    l = new ListBoxItem() { Content = t, IsHitTestVisible = false, Foreground = Brushes.SteelBlue, FontWeight = FontWeights.Medium, Margin = new Thickness(0, 6, 0, 0) };
                    CompetitorsListBox.Items.Add(l);
                }

                // set colours
                Brush pinceau = (r.Finished) ? Brushes.DarkGreen : Brushes.SteelBlue;
                if ((Core.RaceResultConstants)r.RacePosition == Core.RaceResultConstants.RetFatalDriver) { pinceau = Brushes.Red; }
                if ((Core.RaceResultConstants)r.RacePosition== Core.RaceResultConstants.DidNotStart) { pinceau = Brushes.Black; }

                if (r.Formula2) { pinceau = Brushes.Salmon; }
                
                FontWeight weight = (r.AggregatedAllPoints > 0) ? FontWeights.Bold : FontWeights.Normal;
                if (!dunpodium) { weight = FontWeights.Black; }

                StackPanel panel = new StackPanel() { Orientation = Orientation.Horizontal };

                TextBlock tbOutcome = new TextBlock() { Foreground = pinceau, Margin = new Thickness(0, 0, 6, 0) };// margin is to provide a gap in case the outcome will be followed by a 'fastest lap' note
                TextBlock tbPoints = new TextBlock() { Foreground = Brushes.Magenta };
                TextBlock tbOutcomeCode = new TextBlock() { Foreground = pinceau, FontWeight = FontWeights.Black };

                // Position TextBlock (placeholder if driver not placed)
                TextBlock tbposn = new TextBlock() { MinWidth = 32, Margin = new Thickness(0, 0, 6, 0), TextAlignment = TextAlignment.Right };
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
                string gridpos = (r.GridPosition == Core.SpecialNumber) ? "PL" : r.GridPosition.ToString(Core.CultureUK);
                TextBlock tbgrid = new TextBlock() { Text = $"Grid: {gridpos}", MinWidth = 56, Foreground = Brushes.DarkSlateBlue, Margin = new Thickness(0, 0, 6, 0) };

                // Driver TextBlock
                TextBlock DriversTextBlock = new TextBlock() { MinWidth = 450, Margin = new Thickness(0, 0, 6, 0) };
                DateTime racedate = Core.Instance.Races[_raceMeetingKey].RaceDate;
                _raceDrivers.Add(r.DriverKey(0));
                DriversTextBlock.Inlines.Add(new Run(Core.Instance.Drivers[r.DriverKey(0)].FullName) { Foreground = pinceau, FontWeight = weight });
                DriversTextBlock.Inlines.Add(new Run($" {Core.Instance.Drivers[r.DriverKey(0)].AgeAsAt(racedate)}") {Foreground=Brushes.SlateGray });
                DriversTextBlock.Inlines.Add(Core.Instance.Drivers[r.DriverKey(0)].TimeLeftRun(racedate));
                if (r.DriverKey(1) > 0)
                {
                    _raceDrivers.Add(r.DriverKey(1));
                    DriversTextBlock.Inlines.Add(new Run($" and {Core.Instance.Drivers[r.DriverKey(1)].FullName}") { Foreground = pinceau, FontWeight = weight });
                    DriversTextBlock.Inlines.Add(Core.Instance.Drivers[r.DriverKey(1)].TimeLeftRun(racedate));
                }
                if (r.DriverKey(2) > 0)
                {
                    _raceDrivers.Add(r.DriverKey(2));
                    DriversTextBlock.Inlines.Add(new Run($" and {Core.Instance.Drivers[r.DriverKey(2)].FullName}") { Foreground = pinceau, FontWeight = weight });
                    DriversTextBlock.Inlines.Add(Core.Instance.Drivers[r.DriverKey(2)].TimeLeftRun(racedate));
                }
                if (r.Formula2) { DriversTextBlock.Inlines.Add(new Run(" (F2 car)")); }

                string countrytext = Core.Instance.Countries[Core.Instance.Drivers[r.DriverKey(0)].CountryKey].Caption;
                if (r.DriverKey(1) > 0) // there is a 2nd driver
                {
                    if (Core.Instance.Drivers[r.DriverKey(1)].CountryKey != Core.Instance.Drivers[r.DriverKey(0)].CountryKey) // the drivers' countries are different
                    {
                        countrytext += $" and {Core.Instance.Countries[Core.Instance.Drivers[r.DriverKey(1)].CountryKey].Caption}";
                    }
                    if (r.DriverKey(2) > 0) // there is a 3rd driver
                    {
                        if (Core.Instance.Drivers[r.DriverKey(2)].CountryKey != Core.Instance.Drivers[r.DriverKey(1)].CountryKey) // the 2/3 drivers' countries are different
                        {
                            countrytext += $" and {Core.Instance.Countries[Core.Instance.Drivers[r.DriverKey(2)].CountryKey].Caption}";
                        }
                    }
                }
                Run rc = new Run() { Text = $" ({countrytext})", Foreground = Brushes.SlateGray, FontWeight = FontWeights.Normal };
                DriversTextBlock.Inlines.Add(rc);

                // Constructor TextBlock
                TextBlock tbcons = new TextBlock() { Text =Core.ConstructorName( Core.Instance.Constructors[r.ConstructorKey].Caption), MinWidth = 200, Foreground = pinceau, FontWeight = weight, Margin = new Thickness(0, 0, 6, 0) };

                if (r.ConstructorPointsDisallowed)
                {
                    tbcons.Foreground = Brushes.Salmon;
                    rc = new Run() { Text = " (no points)", Foreground = Brushes.Salmon, FontWeight = FontWeights.Normal };
                    tbcons.Inlines.Add(rc);
                }
                if (r.ConstructorPointsPenalty > 0)
                {
                    tbcons.Foreground = Brushes.Salmon;
                    rc = new Run() { Text = $" (fined {r.ConstructorPointsPenalty} points)", Foreground = Brushes.Salmon, FontWeight = FontWeights.Normal };
                    tbcons.Inlines.Add(rc);
                }
                // Controversy TextBlock
                TextBlock tbcontroversy = new TextBlock() { Text =(r.Controversial) ? "Controversy":string.Empty, MinWidth = 72, Foreground =Brushes.Red, FontWeight = weight, Margin = new Thickness(0, 0, 6, 0) };
        

                // Points TextBlock (shows outcome for non-placed cars)
                string pString = string.Empty;
                if (r.AggregatedPositionPoints > 0)
                {
                    if (r.DriverCount > 2)
                    {
                        pString += $"Points: {r.PointsForPosition(0).FloatValue}-{r.PointsForPosition(1).FloatValue}-{r.PointsForPosition(2).FloatValue}";
                        if (r.PointsForSprintQualifying(0).FloatValue > 0) { pString += " + Sprint Qualifying (D1)"; }
                        if (r.PointsForSprintQualifying(1).FloatValue > 0) { pString += " + Sprint Qualifying (D2)"; }
                        if (r.PointsForSprintQualifying(2).FloatValue > 0) { pString += " + Sprint Qualifying (D3)"; }
                        if (r.PointsForFastestLap(0).FloatValue > 0) { pString += " + fastest lap (D1)"; }
                        if (r.PointsForFastestLap(1).FloatValue > 0) { pString += " + fastest lap (D2)"; }
                        if (r.PointsForFastestLap(2).FloatValue > 0) { pString += " + fastest lap (D3)"; }
                    }
                    else
                    if (r.DriverCount > 1)
                    {
                        pString += $"Points: {r.PointsForPosition(0).FloatValue}-{r.PointsForPosition(1).FloatValue}";
                        if (r.PointsForSprintQualifying(0).FloatValue > 0) { pString += " + Sprint Qualifying (D1)"; }
                        if (r.PointsForSprintQualifying(1).FloatValue > 0) { pString += " + Sprint Qualifying (D2)"; }
                        if (r.PointsForFastestLap(0).FloatValue > 0) { pString += " + fastest lap (D1)"; }
                        if (r.PointsForFastestLap(1).FloatValue > 0) { pString += " + fastest lap (D2)"; }
                    }
                    else
                    {
                        pString = $"Points: {r.PointsForPosition(0).FloatValue}";
                        if (r.PointsForSprintQualifying(0).FloatValue > 0) { pString += " + Sprint Qualifying"; }
                        if (r.PointsForFastestLap(0).FloatValue > 0) { pString += " + fastest lap"; }
                    }
                }
                else
                {
                    if (r.DriverCount > 1)
                    {
                        if (r.PointsForSprintQualifying(0).FloatValue > 0) { pString += " + Sprint Qualifying (D1)"; }
                        if (r.PointsForSprintQualifying(1).FloatValue > 0) { pString += " + Sprint Qualifying (D2)"; }
                        if (r.PointsForSprintQualifying(2).FloatValue > 0) { pString += " + Sprint Qualifying (D3)"; }
                        if (r.PointsForFastestLap(0).FloatValue > 0) { pString += "Fastest lap (D1)"; }
                        if (r.PointsForFastestLap(1).FloatValue > 0) { pString += " Fastest lap (D2)"; }
                        if (r.PointsForFastestLap(2).FloatValue > 0) { pString += " Fastest lap (D3)"; }
                    }
                    else
                    {
                        if (r.PointsForSprintQualifying(0).FloatValue > 0) { pString += " + Sprint Qualifying"; }
                        if (r.PointsForFastestLap(0).FloatValue > 0) { pString += "Fastest lap"; }
                    }
                }
                if (!string.IsNullOrWhiteSpace(pString)) { tbPoints.Text = pString; }

                panel.Children.Add(tbposn);
                panel.Children.Add(tbgrid);
                panel.Children.Add(DriversTextBlock);
                panel.Children.Add(tbcons);
                panel.Children.Add(tbcontroversy);
                panel.Children.Add(tbOutcomeCode);
                panel.Children.Add(tbOutcome);
                panel.Children.Add(tbPoints);

                ListBoxItem it = new ListBoxItem() { Content = panel, Tag = r.Key };
                CompetitorsListBox.Items.Add(it);


                if (Gpos.ContainsKey(r.GridPosition))
                {
                    Gpos[r.GridPosition] += $"/{Core.Instance.Drivers[r.DriverKey(0)].Surname}, {Core.Instance.Drivers[r.DriverKey(0)].Forenames[0]}";
                }
                else
                {
                    Gpos.Add(r.GridPosition, $"{Core.Instance.Drivers[r.DriverKey(0)].Surname}, {Core.Instance.Drivers[r.DriverKey(0)].Forenames[0]}");
                }

            }

            if (Gpos.Count > 0)
            {
                List<int> positions = Gpos.Keys.ToList();
                if (positions.Contains(Core.SpecialNumber)) { positions.Remove(Core.SpecialNumber); }
                int y =positions.Max(); // the highest grid position, ignoring the special value which signifies a pit-lane start
                for (int i = 1; i <= y; i++)
                {
                    TextBlock tbk = new TextBlock() { FontFamily = new FontFamily("Lucida Console"), FontSize = 12 };
                    if (Gpos.ContainsKey(i))
                    {
                        tbk.Text = $"{i} { Gpos[i]}";
                        tbk.Foreground = (Gpos[i].Contains("/")) ? Brushes.Red : Brushes.DarkSlateGray;
                    }
                    else
                    {
                        tbk.Text = $"{i} missing";
                        tbk.Foreground = Brushes.Red;
                    }
                    ListBoxItem li = new ListBoxItem() { Content = tbk, IsHitTestVisible = false };
                    GridPositionListBox.Items.Add(li);
                }

                // Add any 'pit-lane' starters to the end of the list
                if (Gpos.ContainsKey(Core.SpecialNumber))
                {
                    TextBlock tbk_pl = new TextBlock() { FontFamily = new FontFamily("Lucida Console"), FontSize = 12 };
                    tbk_pl.Text = $"PL { Gpos[Core.SpecialNumber]}";
                    tbk_pl.Foreground =  Brushes.Maroon;
                    ListBoxItem li_pl = new ListBoxItem() { Content = tbk_pl, IsHitTestVisible = false };
                    GridPositionListBox.Items.Add(li_pl);
                }
                
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CompetitorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CompetitorsListBox.SelectedIndex < 0) { return; }
            ListBoxItem itm = (ListBoxItem)CompetitorsListBox.SelectedItem;
            int ky = (int)itm.Tag;
            DeleteButton.IsEnabled = true;
            DeleteButton.Tag = ky;
        }

        private void WikiButton_Click(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Process.Start(Core.Instance.Races[_raceMeetingKey].WikiLinkRace);
            Core.LaunchWebPage(Core.Instance.Races[_raceMeetingKey].WikiLinkRace);
        }

        private void CompetitorsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CompetitorsListBox.SelectedIndex < 0) { return; }
            ListBoxItem itm = (ListBoxItem)CompetitorsListBox.SelectedItem;
            int ky = (int)itm.Tag;
            CarResultWindow crw = new CarResultWindow(Core.Instance.Voitures[ky], _likelyPointsScheme, _raceDrivers, _raceDate) { Owner = this };
            bool? k = crw.ShowDialog();
            if ((k.HasValue) && (k.Value))
            {
                Core.Instance.Voitures[ky].Specification = crw.Result.Specification;
                ListResults();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Top = Core.LowWindowTop;
            Left = 32;
            Height = Core.LowWindowHeight;
        }

        private void AmendButton_Click(object sender, RoutedEventArgs e)
        {
            RaceMeetingPropertiesWindow w = new RaceMeetingPropertiesWindow(Core.Instance.Races[_raceMeetingKey].Specification) { Owner = this };
            bool? q = w.ShowDialog();
            if (q.HasValue && q.Value)
            {
                RaceMeeting rm = Core.Instance.Races[_raceMeetingKey];
                rm.Specification = w.RaceMeetingSpecification;
                TitleTextBlock.Text = Core.Instance.RaceTitles[rm.RaceTitleKey].Caption;
                DateTextBlock.Text = rm.RaceDate.ToLongDateString();
                CircuitTextBlock.Text = Core.Instance.RaceTracks[rm.CircuitKey].CircuitName[0];
                SetConstructorsChampionshipNotice(rm.CountForConstructorsChampionship);
            }
        }

        private void RaceDiagramButton_Click(object sender, RoutedEventArgs e)
        {
            if (_raceMeetingKey < 0) { return; }
            RaceDiagramWindow w = new RaceDiagramWindow(_raceMeetingKey) { Owner = this };
            w.ShowDialog();
        }
}