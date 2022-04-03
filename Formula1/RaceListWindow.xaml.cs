using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Formula1;

public partial class RaceListWindow : Window
{

    private string _category;
        private int _itemkey;
        private bool _doneSomething = false;
        private bool _hasRacesExcludedFromConstructorsChampionship;
        public RaceListWindow()
        {
            InitializeComponent();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            SelectionComboBox.Items.Clear();
            _category = "year";
            RadioButton rb = RadioConstructor;
            if (rb.IsChecked.HasValue && rb.IsChecked.Value) { _category = "constructor"; }
            rb = RadioDriver;
            if (rb.IsChecked.HasValue && rb.IsChecked.Value) { _category = "driver"; }
            rb = RadioGrandPrix;
            if (rb.IsChecked.HasValue && rb.IsChecked.Value) { _category = "grandprix"; }
            rb = RadioVenue;
            if (rb.IsChecked.HasValue && rb.IsChecked.Value) { _category = "racetrack"; }
            List<NamedItem> selection = new List<NamedItem>();
            switch (_category)
            {
                case "year":
                    {
                        foreach (int yr in Core.Instance.Years)
                        {
                            NamedItem ni = new NamedItem(yr.ToString(System.Globalization.CultureInfo.CurrentCulture), yr);
                            selection.Add(ni);
                        }
                        break;
                    }
                case "constructor":
                    {
                        selection = Core.Instance.Constructors.Values.ToList();
                        selection.Sort();
                        break;
                    }
                case "driver":
                    {
                        List<Driver> a = Core.Instance.Drivers.Values.ToList();
                        a.Sort();
                        foreach (Driver dv in a)
                        {
                            NamedItem ni = new NamedItem(dv.FullName, dv.Key);
                            selection.Add(ni);
                        }
                        break;
                    }
                case "grandprix":
                    {
                        selection = Core.Instance.RaceTitles.Values.ToList();
                        selection.Sort();
                        break;
                    }
                case "racetrack":
                    {
                        foreach (Circuit tk in Core.Instance.RaceTracks.Values)
                        {
                            int w = tk.CircuitTitleCount;
                            for(int v = 0; v < w; v++)
                            {
                                NamedItem ni = new NamedItem(tk.CircuitSingleTitle(v), tk.Key);
                                selection.Add(ni);
                            }
                        }
                        selection.Sort();
                        break;
                    }
            }
            foreach (NamedItem it in selection)
            {
                ComboBoxItem cbi = new ComboBoxItem() { Content = it.Caption, Tag = it.Key };
                SelectionComboBox.Items.Add(cbi);
                if (selection.Count > 0) { SelectionComboBox.SelectedIndex = 0; }
            }
        }

        private void SelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionComboBox.SelectedIndex >= 0)
            {
                ComboBoxItem itm = SelectionComboBox.SelectedItem as ComboBoxItem;
                _itemkey = (int)itm.Tag;
                ListRaces();
            }
        }

        private void AddRaceButton_Click(object sender, RoutedEventArgs e)
        {
            RaceMeetingPropertiesWindow w = new RaceMeetingPropertiesWindow() { Owner = this };
            bool? q = w.ShowDialog();
            if (q.HasValue && q.Value)
            {
                RaceMeeting rm = new RaceMeeting() { Specification = w.RaceMeetingSpecification };
                int newKey = 1;
                while (Core.Instance.Races.ContainsKey(newKey)) { newKey++; }
                rm.Key = newKey;
                Core.Instance.Races.Add(newKey, rm);
                //RefreshDisplay();
                Core.Instance.HouseKeeping();
                _doneSomething = true;
                // select the race in the combo box
                int yr = Core.Instance.Races[newKey].RaceDate.Year;
                RadioYear.IsChecked = true;
                RefreshDisplay();
                int target = -1;
                SetYearComboToYear(yr);
                for (int n = 0; n < RaceMeetingListBox.Items.Count; n++)
                {
                    ListBoxItem itm = (ListBoxItem)RaceMeetingListBox.Items[n];
                    int u = (int)itm.Tag;
                    if (u == newKey) { target = n; break; }
                }
                RaceMeetingListBox.SelectedIndex = target;
                SaveDataButton.IsEnabled = true;
            }
        }

        private void SetYearComboToYear(int yer)
        {
            int target = -1;
            if (yer == 0)
            {
                target = SelectionComboBox.Items.Count - 1;
            }
            else
            {
                for (int n = 0; n < SelectionComboBox.Items.Count; n++)
                {
                    ComboBoxItem itm = (ComboBoxItem)SelectionComboBox.Items[n];
                    int u = (int)itm.Tag;
                    if (u == yer) { target = n; break; }
                }
            }
            SelectionComboBox.SelectedIndex = target;
        }

        private void ListRaces()
        {
            ClearRaceSummary();
            RaceMeetingListBox.Items.Clear();
            List<RaceMeeting> races = new List<RaceMeeting>();
            switch (_category)
            {
                case "year":
                    {
                        foreach (RaceMeeting mtg in Core.Instance.Races.Values)
                        {
                            if (mtg.RaceDate.Year.Equals(_itemkey))
                            {
                                if (!races.Contains(mtg)) { races.Add(mtg); }
                            }
                        }
                        SeasonRaceCountTextBlock.Text = $"{races.Count} of {Core.Instance.Seasons[_itemkey].StatedNumberOfRaces} races";
                        break;
                    }
                case "constructor":
                    {
                        foreach (RaceMeeting mtg in Core.Instance.Races.Values)
                        {
                            foreach (Voiture rslt in Core.Instance.Voitures.Values)
                            {
                                if (rslt.ConstructorKey == _itemkey)
                                {
                                    if (rslt.RaceMeetingKey == mtg.Key)
                                    {
                                        if (!races.Contains(mtg)) { races.Add(mtg); }
                                    }
                                }
                            }
                        }
                        SeasonRaceCountTextBlock.Text = $"{races.Count} races";
                        break;
                    }
                case "driver":
                    {
                        foreach (RaceMeeting mtg in Core.Instance.Races.Values)
                        {
                            foreach (Voiture rslt in Core.Instance.Voitures.Values)
                            {
                                if ((rslt.DriverKey(0) == _itemkey) || (rslt.DriverKey(1) == _itemkey) || (rslt.DriverKey(2)==_itemkey))
                                {
                                    if (rslt.RaceMeetingKey == mtg.Key)
                                    {
                                        if (!races.Contains(mtg)) { races.Add(mtg); }
                                    }
                                }
                            }
                        }
                        SeasonRaceCountTextBlock.Text = $"{races.Count} races";
                        break;
                    }
                case "grandprix":
                    {
                        foreach (RaceMeeting mtg in Core.Instance.Races.Values)
                        {
                            if (mtg.RaceTitleKey == _itemkey)
                            {
                                if (!races.Contains(mtg)) { races.Add(mtg); }
                            }
                        }
                        SeasonRaceCountTextBlock.Text = $"{races.Count} races";
                        break;
                    }
                case "racetrack":
                    {
                        foreach (RaceMeeting mtg in Core.Instance.Races.Values)
                        {
                            if (mtg.CircuitKey == _itemkey)
                            {
                                if (!races.Contains(mtg)) { races.Add(mtg); }
                            }
                        }
                        SeasonRaceCountTextBlock.Text = $"{races.Count} races";
                        break;
                    }
            }
            races.Sort();
            _hasRacesExcludedFromConstructorsChampionship = false;
            foreach (RaceMeeting mtg in races)
            {
                RaceMeetingListBox.Items.Add(Lister(mtg));
            }
            if (_hasRacesExcludedFromConstructorsChampionship)
            {
                TextBlock t = new TextBlock() { Text = $"XC signifies a race excluded from the Constructors' Championship", Foreground=Brushes.Orchid, Margin=new Thickness(0,10,0,0) };
                ListBoxItem i = new ListBoxItem() { Content = t, IsHitTestVisible = false, IsTabStop = false };
                RaceMeetingListBox.Items.Add(i);
            }
        }

        private ListBoxItem Lister(RaceMeeting r)
        {
            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };
            TextBlock t = new TextBlock() { Text = r.SerialNumber.ToString(Core.CultureUK), Foreground = Brushes.RosyBrown, Width = 30 };
            sp.Children.Add(t);
            TextBlock ty = new TextBlock() { Text =$"{r.RaceDate.Year} R{r.YearSerialNumber}", Foreground = Brushes.Brown, Width = 80 };
            sp.Children.Add(ty);
            TextBlock td = new TextBlock() { Text = r.RaceDate.ToShortDateString(), Foreground = Brushes.RosyBrown, Width = 80 };
            sp.Children.Add(td);
            string dow = r.RaceDate.ToString("ddd", Core.CultureUK);
            Brush br = Brushes.RosyBrown;
            if (dow != "Sun") { br = Brushes.Brown; }
            TextBlock tdow = new TextBlock() { Text = dow, Foreground = br, Width = 40 };
            sp.Children.Add(tdow);
            t = new TextBlock() { Text = Core.Instance.RaceTitles[r.RaceTitleKey].Caption };
            if (r.GridGaps != Core.RankingHealth.Healthy) { t.Foreground = Brushes.Red; t.Text += " (consistent grid not entered)"; }
            if (r.Qualifiers == 0) { t.Foreground = Brushes.Red; t.Text += " (no results)"; }
            if (!r.CountForConstructorsChampionship) { t.Text += " (XC)";_hasRacesExcludedFromConstructorsChampionship = true; }
            sp.Children.Add(t);
            ListBoxItem i = new ListBoxItem() { Content = sp, Tag = r.Key };
            return i;
        }

        private void RaceMeetingListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditRace();
        }

        private void EditRace()
        {
            if (RaceMeetingListBox.SelectedIndex < 0) { return; }
            ListBoxItem i = (ListBoxItem)RaceMeetingListBox.SelectedItem;
            int k = (int)i.Tag;
            RaceMeetingResultsWindow w = new RaceMeetingResultsWindow(k) { Owner = this };
            w.ShowDialog();
            Core.Instance.HouseKeeping();
            int truc = RaceMeetingListBox.SelectedIndex;
            ListRaces();
            RaceMeetingListBox.SelectedIndex = truc;
            SummariseSelectedRace();
            _doneSomething = true;
            SaveDataButton.IsEnabled = true;
        }

        internal bool DoneSomething { get { return _doneSomething; } }
        private void ClearRaceSummary()
        {
            RaceTB.Text = string.Empty;
            DateTB.Text = string.Empty;
            FinishersTB.Text = string.Empty;
            CorrelationTB.Text = string.Empty;
            CorrelationLine.Visibility = Visibility.Hidden;
            FirstTB.Text = string.Empty;
            QualifiersTB.Text = string.Empty;
            SecondTB.Text = string.Empty;
            ThirdTB.Text = string.Empty;
            VenueTB.Text = string.Empty;
            SeasonDriverPointsListBox.Items.Clear();
            SeasonTeamPointsListBox.Items.Clear();
            DebutantsListBox.Items.Clear();
            SwanSongsListBox.Items.Clear();
            LastObituaryListBox.Items.Clear();
            NextObituaryListBox.Items.Clear();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            RadioYear.IsChecked = true;
            SeasonRaceCountTextBlock.Text = string.Empty;
            int target = Core.Instance.FirstIncompleteSeason();
            SetYearComboToYear(target);
        }

        private void RaceMeetingListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SummariseSelectedRace();
        }

        private void SummariseSelectedRace()
        {
            WikiButton.IsEnabled = true;
            ClearRaceSummary();
            if (RaceMeetingListBox.SelectedIndex < 0) { return; }
            ListBoxItem i = (ListBoxItem)RaceMeetingListBox.SelectedItem;
            int k = (int)i.Tag;
            RaceMeeting mtg = Core.Instance.Races[k];
            RaceTB.Text = Core.Instance.RaceTitles[mtg.RaceTitleKey].Caption;
            DateTB.Text = mtg.RaceDate.ToLongDateString();
            DateTB.Inlines.Add(new Run() { Text = AgoString(mtg.RaceDate), Foreground = Brushes.Gray });
            
            VenueTB.Text = Core.Instance.RaceTracks[mtg.CircuitKey].CircuitSingleTitle(0);
            QualifiersTB.Text = mtg.Qualifiers.ToString(System.Globalization.CultureInfo.CurrentCulture);
            int f = mtg.Finishers;
            FinishersTB.Text = f.ToString(System.Globalization.CultureInfo.CurrentCulture);
            if (f > 4)
            {
                double kt = mtg.KendalTau();
                CorrelationTB.Text = kt.ToString("0.00", System.Globalization.CultureInfo.CurrentCulture);
                CorrelationLine.X1 = CorrelationLine.X2 = (100 + (100 * kt));
                CorrelationLine.Visibility = Visibility.Visible;
            }
            int K11 = mtg.PodiumOne.Item1;
            int K12 = mtg.PodiumOne.Item2;
            int K21 = mtg.PodiumTwo.Item1;
            int K22 = mtg.PodiumTwo.Item2;
            int K31 = mtg.PodiumThree.Item1;
            int K32 = mtg.PodiumThree.Item2;
            string P1 = (K11 > 0) ? $"1 { Core.Instance.Drivers[K11].FullName}" : "";
            string tmp = (K12 > 0) ? $" and { Core.Instance.Drivers[K12].FullName}" : "";
            P1 += tmp;
            string P2 = (K21 > 0) ? $"2 { Core.Instance.Drivers[K21].FullName}" : "";
            tmp = (K22 > 0) ? $" and { Core.Instance.Drivers[K22].FullName}" : "";
            P2 += tmp;
            string P3 = (K31 > 0) ? $"3 { Core.Instance.Drivers[K31].FullName}" : "";
            tmp = (K32 > 0) ? $" and { Core.Instance.Drivers[K32].FullName}" : "";
            P3 += tmp;
            int u = (K11 > 0) ? Core.Instance.Drivers[K11].WinsUpTo(mtg.RaceDate) : 0;
            string o = (u > 0) ? $" ({Core.Ordinal(u)} win)" : string.Empty;
            FirstTB.Text =P1;
            FirstTB.Inlines.Add(new Run() { Text = o, FontWeight = FontWeights.Normal });
            SecondTB.Text = P2;
            ThirdTB.Text = P3;

            if (Core.Instance.Seasons.ContainsKey(mtg.RaceDate.Year)) // avoid error in case no results have yet been entered for this year
            {
                Season ssn = Core.Instance.Seasons[mtg.RaceDate.Year];
                ssn.RefreshStatistics();
                List<Tuple<float, float,float, int>> ranking = ssn.RankedDrivers(mtg.Key);
                float scoretobeat =(ranking.Count>0) ? ranking[0].Item1:0;
                float prevscor = -1000;
                int rankIndex = 0;
                bool newrank;
                float topscore = (ranking.Count > 0) ? ranking[0].Item1 : 0;
                for (int r = 0; r < ranking.Count; r++)
                {
                    newrank = false;
                    if (prevscor != ranking[r].Item2)
                    {
                        newrank = true;
                        rankIndex = r + 1;
                        prevscor = ranking[r].Item2;
                    }
                    if (rankIndex < 6)
                    {
                        string d = (newrank) ? Core.Ordinal(rankIndex) : string.Empty;
                        StackPanel panel = new StackPanel() { Orientation = Orientation.Horizontal };
                        TextBlock RankTb = new TextBlock() { Text = d, MinWidth = 40 };
                        panel.Children.Add(RankTb);
                        TextBlock RacerTb = new TextBlock() { Text = Core.Instance.Drivers[ranking[r].Item4].FullName, MinWidth = 180 };
                        if (ranking[r].Item3 >= scoretobeat)
                        {
                            Run ChampionTb = new Run() { Text = "«", FontFamily = new FontFamily("Wingdings"), FontSize=16, Foreground = Brushes.Red };
                            RacerTb.Inlines.Add(ChampionTb);
                        }
                        panel.Children.Add(RacerTb);
                        TextBlock GoodPointsTb = new TextBlock() { Text = Core.MyFormat(ranking[r].Item1), MinWidth = 40, TextAlignment = TextAlignment.Right };
                        panel.Children.Add(GoodPointsTb);

                        // show total points if not all points have been counted towards championship
                        TextBlock TotlPointsTb = new TextBlock() { MinWidth = 80, TextAlignment = TextAlignment.Right };
                        panel.Children.Add(TotlPointsTb);
                        if (ranking[r].Item1 != ranking[r].Item2)
                        {
                            TotlPointsTb.Text = $"(total {Core.MyFormat(ranking[r].Item2)})";
                        }

                        // show how far this score is behind the leader
                        TextBlock BehindTb = new TextBlock() { MinWidth = 40, TextAlignment = TextAlignment.Right, Foreground = Brushes.Gray };
                        panel.Children.Add(BehindTb);
                        float behind = topscore - ranking[r].Item1;
                        if (behind > 0)
                        {
                            BehindTb.Text = $"-{behind}";
                        }

                        ListBoxItem item = new ListBoxItem() { Content = panel, IsHitTestVisible = false };
                        SeasonDriverPointsListBox.Items.Add(item);
                    }
                }
                
                TeamsListBox.Items.Clear();
                List<int> tkeys = Core.Instance.TeamsInRace(k);
                foreach (int tk in tkeys)
                {
                    TextBlock tt = new TextBlock() { Text =Core.ConstructorName( Core.Instance.Constructors[tk].Caption), Foreground = Brushes.SaddleBrown, FontWeight = FontWeights.Bold };
                    ListBoxItem tlbi = new ListBoxItem() { Content = tt };
                    TeamsListBox.Items.Add(tlbi);
                    List<int> dkeys = Core.Instance.DriversinTeamInRace(k, tk);
                    foreach (int dk in dkeys)
                    {
                        TextBlock dt = new TextBlock() { Text = Core.Instance.Drivers[dk].FullName, Foreground = Brushes.SeaGreen, Margin = new Thickness(12, 0, 0, 0) };
                        ListBoxItem dlbi = new ListBoxItem() { Content = dt };
                        TeamsListBox.Items.Add(dlbi);
                    }
                }
            }

            if (Core.Instance.Seasons.ContainsKey(mtg.RaceDate.Year)) // avoid error in case no results have yet been entered for this year
            {
                Season ssn = Core.Instance.Seasons[mtg.RaceDate.Year];
                ssn.RefreshStatistics();
                List<Tuple<float, float, int>> ranking = ssn.RankedConstructors(mtg.Key);
                float prevscor = -1000;
                int rankIndex = 0;
                bool newrank;
                float topscore = (ranking.Count > 0) ? ranking[0].Item1 : 0;
                for (int r = 0; r < ranking.Count; r++)
                {
                    newrank = false;
                    if (prevscor != ranking[r].Item2)
                    {
                        newrank = true;
                        rankIndex = r + 1;
                        prevscor = ranking[r].Item2;
                    }
                    if (rankIndex < 6)
                    {
                        string d = (newrank) ? Core.Ordinal(rankIndex) : string.Empty;
                        StackPanel panel = new StackPanel() { Orientation = Orientation.Horizontal };
                        TextBlock RankTb = new TextBlock() { Text = d, MinWidth = 40 };
                        panel.Children.Add(RankTb);
                        TextBlock RacerTb = new TextBlock() { Text = Core.ConstructorName(Core.Instance.Constructors[ranking[r].Item3].Caption), MinWidth = 180 };
                        panel.Children.Add(RacerTb);
                        TextBlock GoodPointsTb = new TextBlock() { Text = Core.MyFormat(ranking[r].Item1), MinWidth = 40, TextAlignment = TextAlignment.Right };
                        panel.Children.Add(GoodPointsTb);

                        // show total points if not all points have been counted towards championship
                        TextBlock TotlPointsTb = new TextBlock() { MinWidth = 80, TextAlignment = TextAlignment.Right };
                        panel.Children.Add(TotlPointsTb);
                        if (ranking[r].Item1 != ranking[r].Item2)
                        {
                            TotlPointsTb.Text = $"(total {Core.MyFormat(ranking[r].Item2)})";
                        }

                        // show how far this score is behind the leader
                        TextBlock BehindTb = new TextBlock() { MinWidth = 40, TextAlignment = TextAlignment.Right, Foreground = Brushes.Gray };
                        panel.Children.Add(BehindTb);
                        float behind = topscore - ranking[r].Item1;
                        if (behind > 0)
                        {
                            BehindTb.Text = $"-{behind}";
                        }

                        ListBoxItem item = new ListBoxItem() { Content = panel, IsHitTestVisible = false };
                        SeasonTeamPointsListBox.Items.Add(item);
                    }
                }
            }

            DebutantsListBox.Items.Clear();
            foreach (Driver dr in Core.Instance.Drivers.Values)
            {
                if (dr.RuntimeFirstRaceKey == mtg.Key)
                {
                    if (dr.RuntimeRacesStarted > 9)
                    {
                        TextBlock tblock = new TextBlock() { Text = dr.FullName, Foreground = Brushes.DarkSlateBlue };
                        ListBoxItem item = new ListBoxItem() { Content = tblock, IsHitTestVisible = false };
                        DebutantsListBox.Items.Add(item);
                    }
                }
            }
            foreach (Driver dr in Core.Instance.Drivers.Values)
            {
                if (dr.RuntimeFirstRaceKey == mtg.Key)
                {
                    if (dr.RuntimeRacesStarted < 10)
                    {
                        TextBlock tblock = new TextBlock() { Text = dr.FullName, Foreground = Brushes.Gray };
                        ListBoxItem item = new ListBoxItem() { Content = tblock, IsHitTestVisible = false };
                        DebutantsListBox.Items.Add(item);
                    }
                }
            }

            SwanSongsListBox.Items.Clear();
            foreach (Driver dr in Core.Instance.Drivers.Values)
            {
                if (dr.RuntimeLastRaceKey == mtg.Key)
                {
                    if (dr.RuntimeRacesStarted > 9)
                    {
                        TextBlock tblock = new TextBlock() { Text = dr.FullName, Foreground = Brushes.DarkSlateBlue };
                        ListBoxItem item = new ListBoxItem() { Content = tblock, IsHitTestVisible = false };
                        SwanSongsListBox.Items.Add(item);
                    }
                }
            }
            foreach (Driver dr in Core.Instance.Drivers.Values)
            {
                if (dr.RuntimeLastRaceKey == mtg.Key)
                {
                    if (dr.RuntimeRacesStarted < 10)
                    {
                        TextBlock tblock = new TextBlock() { Text = dr.FullName, Foreground = Brushes.Gray };
                        ListBoxItem item = new ListBoxItem() { Content = tblock, IsHitTestVisible = false };
                        SwanSongsListBox.Items.Add(item);
                    }
                }
            }

            LastObituaryListBox.Items.Clear();
            List<Tuple<DateTime, int, bool>> morts = Core.Instance.DriversDiedSinceLastRace(mtg.Key);
            foreach (Tuple<DateTime, int, bool>  m in morts)
            {
                TextBlock tblock = new TextBlock() { Text = $"{Core.Instance.Drivers[m.Item2].FullName} {m.Item1.ToString("d MMM yyyy", Core.CultureUK)}", Foreground = Brushes.SlateBlue };
                if (m.Item3) { tblock.Foreground = Brushes.Red; }
                ListBoxItem item = new ListBoxItem() { Content = tblock, IsHitTestVisible = false };
                LastObituaryListBox.Items.Add(item);
            }
            
            NextObituaryListBox.Items.Clear();
            List<Tuple<DateTime, int, bool>> deces = Core.Instance.DriversDiedBeforeNextRace(mtg.Key);
            foreach (Tuple<DateTime, int, bool> m in deces)
            {
                TextBlock tblock = new TextBlock() { Text = $"{Core.Instance.Drivers[m.Item2].FullName} {m.Item1.ToString("d MMM yyyy", Core.CultureUK)}", Foreground = Brushes.SlateBlue };
                if (m.Item3) { tblock.Foreground = Brushes.Red; }
                ListBoxItem item = new ListBoxItem() { Content = tblock, IsHitTestVisible = false };
                NextObituaryListBox.Items.Add(item);
            }

        }

        private void EditResultsButton_Click(object sender, RoutedEventArgs e)
        {
            EditRace();
        }

        private void WikiButton_Click(object sender, RoutedEventArgs e)
        {
            WikiButton.IsEnabled = false;
            if (RaceMeetingListBox.SelectedIndex < 0) { return; }
            ListBoxItem i = (ListBoxItem)RaceMeetingListBox.SelectedItem;
            int k = (int)i.Tag;
            System.Diagnostics.Process.Start(Core.Instance.Races[k].WikiLinkRace);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Height = Core.LowWindowHeight;
            this.Top = Core.LowWindowTop;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth - 360;
            this.Left = 20;
            CorrelationLine.Visibility = Visibility.Hidden;
        }

        private void SaveDataButton_Click(object sender, RoutedEventArgs e)
        {
            Core.Instance.SaveData();
            SaveDataButton.IsEnabled = false;
        }

        private void RaceDiagramButton_Click(object sender, RoutedEventArgs e)
        {
            if (RaceMeetingListBox.SelectedIndex < 0) { return; }
            ListBoxItem i = (ListBoxItem)RaceMeetingListBox.SelectedItem;
            int k = (int)i.Tag;
            RaceDiagramWindow w = new RaceDiagramWindow(k) { Owner = this };
            w.ShowDialog();
        }

        private static string AgoString(DateTime dat)
        {
            DateTime quand = DateTime.Today;
            int y = 0;
            int m = 0;
            while (quand > dat) { quand = quand.AddYears(-1); y++; }
            while (quand < dat) { quand = quand.AddMonths(1); m++; }
            y--;
            m = 12 - m;
            if (m == 12) { m = 0; y++; }
            return $" ({y} y {m} m)";
        }
}