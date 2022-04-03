using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Formula1;

public partial class CountriesWindow : Window
{
    private readonly string _mode;
        private bool _edited;
        public CountriesWindow(string mode)
        {
            InitializeComponent();
            _mode = mode;
            _edited = false;
        }

        internal bool MadeEdit { get { return _edited; } }
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
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            //EditCircuitButton.Visibility = Visibility.Hidden;
            switch (_mode)
            {
                case "countries": { PrepareCountries(); break; }
                case "circuits": { PrepareCircuits(); break; }
                default: { PrepareGrandsPrix(); break; }
            }
        }

        private void PrepareCountries()
        {
            List<NamedItem> lands = new List<NamedItem>();
            foreach (int k in Core.Instance.Countries.Keys)
            {
                lands.Add(Core.Instance.Countries[k]);
            }
            lands.Sort();
            foreach (NamedItem pays in lands)
            {
                TextBlock tb = new TextBlock() { Text = pays.Caption.ToUpper(Core.CultureUK), Foreground = Brushes.Black, FontWeight = FontWeights.Bold };
                ListBoxItem li = new ListBoxItem() { Content = tb, Tag = pays.Key };
                CountryListBox.Items.Add(li);
            }
        }

        private void PrepareGrandsPrix()
        {
            Title = "Grands Prix";
            List<NamedItem> gps = new List<NamedItem>();
            foreach (int k in Core.Instance.RaceTitles.Keys)
            {
                gps.Add(Core.Instance.RaceTitles[k]);
            }
            gps.Sort();
            foreach (NamedItem gp in gps)
            {
                TextBlock tb = new TextBlock() { Text = gp.Caption.ToUpper(Core.CultureUK), Foreground = Brushes.Black, FontWeight = FontWeights.Bold };
                ListBoxItem li = new ListBoxItem() { Content = tb, Tag = gp.Key };
                CountryListBox.Items.Add(li);
            }
        }

        private void PrepareCircuits()
        {
            Title = "Racing circuits";
            //EditCircuitButton.Visibility = Visibility.Visible;
            //EditCircuitButton.IsEnabled = false;
            List<NamedItem> circs = new List<NamedItem>();
            foreach (int k in Core.Instance.RaceTracks.Keys)
            {
                foreach (string s in Core.Instance.RaceTracks[k].CircuitName)
                {
                    NamedItem ni = new NamedItem(s, k);
                    circs.Add(ni);
                }
            }
            circs.Sort();
            foreach (NamedItem ci in circs)
            {
                TextBlock tb = new TextBlock() { Text = ci.Caption.ToUpper(Core.CultureUK), Foreground = Brushes.Black, FontWeight = FontWeights.Bold };
                ListBoxItem li = new ListBoxItem() { Content = tb, Tag = ci.Key };
                CountryListBox.Items.Add(li);
            }
        }

        private void CountryTables(int pays)
        {
            FirstListBox.Items.Clear();
            SecondListBox.Items.Clear();
            // CIRCUITS
            List<Tuple<string, int>> OutputList = new List<Tuple<string, int>>();
            List<Tuple<int, int>> pairs = Core.Instance.CircuitGrandPrixPairings();
            foreach (Circuit cirque in Core.Instance.RaceTracks.Values)
            {
                if (cirque.CountryKey == pays) { OutputList.Add(new Tuple<string, int>( cirque.CircuitSingleTitle(0), cirque.Key)); }
            }
            if (OutputList.Count > 0)
            {
                TextBlock tb = new TextBlock() { Text = $"RACE CIRCUITS ({OutputList.Count})", Foreground = Brushes.DarkCyan, FontWeight = FontWeights.Medium, Margin = new Thickness(12, 0, 0, 0) };
                ListBoxItem li = new ListBoxItem() { Content = tb, IsHitTestVisible = false };
                FirstListBox.Items.Add(li);

                OutputList.Sort();
                foreach (Tuple<string, int> s in OutputList)
                {
                    tb = new TextBlock() { Text = s.Item1, Foreground = Brushes.DarkSlateBlue, FontWeight = FontWeights.Normal, Margin = new Thickness(24, 0, 0, 0) };
                    li = new ListBoxItem() { Content = tb, IsHitTestVisible = false };
                    FirstListBox.Items.Add(li);
                    foreach (Tuple<int, int> pr in pairs)
                    {
                        if (pr.Item1 == s.Item2)
                        {
                            tb = new TextBlock() { Text = Core.Instance.RaceTitles[pr.Item2].Caption, Foreground = Brushes.Gray, FontWeight = FontWeights.Normal, Margin = new Thickness(48, 0, 0, 0) };
                            li = new ListBoxItem() { Content = tb, IsHitTestVisible = false };
                            FirstListBox.Items.Add(li);
                        }
                    }
                }
            }

            // RACES
            List<string> StringList = new List<string>();
            foreach (RaceMeeting piste in Core.Instance.Races.Values)
            {
                int c = piste.CircuitKey;
                int p = Core.Instance.RaceTracks[c].CountryKey;
                if (p == pays) { string t = Core.Instance.RaceTitles[piste.RaceTitleKey].Caption; if (!StringList.Contains(t)) {StringList.Add(t); } }
            }
            if (StringList.Count > 0)
            {
                TextBlock tb = new TextBlock() { Text = $"RACES HOSTED ({StringList.Count})", Foreground = Brushes.DarkCyan, FontWeight = FontWeights.Medium, Margin = new Thickness(12, 0, 0, 0) };
                ListBoxItem li = new ListBoxItem() { Content = tb, IsHitTestVisible = false };
                FirstListBox.Items.Add(li);

                StringList.Sort();
                foreach (string s in StringList)
                {
                    tb = new TextBlock() { Text = s, Foreground = Brushes.DarkSlateBlue, FontWeight = FontWeights.Normal, Margin = new Thickness(24, 0, 0, 0) };
                    li = new ListBoxItem() { Content = tb, IsHitTestVisible = false };
                    FirstListBox.Items.Add(li);
                }
            }

            // DRIVERS
            List<Tuple<string, string>> PilotList = new List<Tuple<string, string>>();
            foreach (Driver chauffeur in Core.Instance.Drivers.Values)
            {
                int p = chauffeur.CountryKey;
                if (p == pays) { PilotList.Add(new Tuple<string, string>(chauffeur.SortingName, chauffeur.FullName)); }
            }
            if (PilotList.Count > 0)
            {
                TextBlock tb = new TextBlock() { Text = $"DRIVERS ({PilotList.Count})", Foreground = Brushes.DarkCyan, FontWeight = FontWeights.Medium, Margin = new Thickness(12, 0, 0, 0) };
                ListBoxItem li = new ListBoxItem() { Content = tb, IsHitTestVisible = false };
                SecondListBox.Items.Add(li);

                PilotList.Sort();
                foreach (Tuple<string, string> s in PilotList)
                {
                    tb = new TextBlock() { Text = s.Item2, Foreground = Brushes.DarkSlateBlue, FontWeight = FontWeights.Normal, Margin = new Thickness(24, 0, 0, 0) };
                    li = new ListBoxItem() { Content = tb, IsHitTestVisible = false };
                    SecondListBox.Items.Add(li);
                }
            }
        }
        private void CircuitTables(int trak)
        {
            FirstListBox.Items.Clear();
            SecondListBox.Items.Clear();
            // LOCATION & NAMES
            Circuit cirque = Core.Instance.RaceTracks[trak];
            TextBlock tb = new TextBlock() { Text = $"LOCATION", Foreground = Brushes.DarkCyan, FontWeight = FontWeights.Medium, Margin = new Thickness(12, 0, 0, 0) };
            ListBoxItem li = new ListBoxItem() { Content = tb, IsHitTestVisible = false };
            FirstListBox.Items.Add(li);
            tb = new TextBlock() { Text = Core.Instance.Countries[cirque.CountryKey].Caption, Foreground = Brushes.DarkSlateBlue, FontWeight = FontWeights.Normal, Margin = new Thickness(24, 0, 0, 0) };
            li = new ListBoxItem() { Content = tb, IsHitTestVisible = false };
            FirstListBox.Items.Add(li);

            tb = new TextBlock() { Text = $"NAMES", Foreground = Brushes.DarkCyan, FontWeight = FontWeights.Medium, Margin = new Thickness(12, 0, 0, 0) };
            li = new ListBoxItem() { Content = tb, IsHitTestVisible = false };
            FirstListBox.Items.Add(li);
            foreach(string s in cirque.CircuitName)
            {
                tb = new TextBlock() { Text = s, Foreground = Brushes.DarkSlateBlue, FontWeight = FontWeights.Normal, Margin = new Thickness(24, 0, 0, 0) };
                li = new ListBoxItem() { Content = tb, IsHitTestVisible = false };
                FirstListBox.Items.Add(li);
            }

            List<int> GpKeys = new List<int>();

            List<Tuple<int, string, string, string>> courses = new List<Tuple<int, string, string, string>>();
            // previously used a dictionary with year as key but as of 2020 some circuits hosted more than one GP per year
            foreach (RaceMeeting mtg in Core.Instance.Races.Values)
            {
                if (mtg.CircuitKey == trak)
                {
                    if (!GpKeys.Contains(mtg.RaceTitleKey)) { GpKeys.Add(mtg.RaceTitleKey); }
                    Tuple<int, string, string, string> quoi = new Tuple<int, string, string, string>(mtg.RaceDate.Year, mtg.RaceDate.ToString("MMM dd", System.Globalization.CultureInfo.CurrentCulture), Core.Instance.RaceTitles[mtg.RaceTitleKey].Caption, Core.Instance.Drivers[mtg.PodiumOne.Item1].FullName);
                    courses.Add(quoi);
                }
            }

            int noo = DateTime.Today.Year;
            for (int y = 1950; y <= noo; y++)
            {
                foreach(Tuple<int, string, string, string> quelle in courses)
                if (quelle.Item1 == y)
                    {
                        StackPanel s = new StackPanel() { Orientation = Orientation.Horizontal };
                        TextBlock t1 = new TextBlock() { Text = y.ToString(System.Globalization.CultureInfo.CurrentCulture), Foreground = Brushes.DarkCyan, FontWeight = FontWeights.Bold };
                        TextBlock t2 = new TextBlock() { Text =quelle.Item2, Foreground = Brushes.DarkOliveGreen, Margin = new Thickness(4, 0, 0, 0), Width = 80 };
                        TextBlock t3 = new TextBlock() { Text =quelle.Item3, Foreground = Brushes.SeaGreen, Margin = new Thickness(4, 0, 0, 0), Width = 200 };
                        TextBlock t4 = new TextBlock() { Text = quelle.Item4, Foreground = Brushes.Green, Margin = new Thickness(4, 0, 0, 0) };
                        s.Children.Add(t1);
                        s.Children.Add(t2);
                        s.Children.Add(t3);
                        s.Children.Add(t4);
                        li = new ListBoxItem() { Content = s, IsHitTestVisible = false };
                        SecondListBox.Items.Add(li);
                    }
            }

            Button EditCircuitButton = new Button() { Content = "Edit", ToolTip = "Edit circuit names", Tag =trak, Padding=new Thickness(12,3,12,3) };
            EditCircuitButton.Click += EditCircuitButton_Click;
            FirstListBox.Items.Add(EditCircuitButton);

            // Venues
            TextBlock tbk = new TextBlock() { Text = "GRANDS PRIX HOSTED", Foreground = Brushes.DarkCyan, FontWeight = FontWeights.Medium, Margin = new Thickness(12, 0, 0, 0) };
            ListBoxItem lbi = new ListBoxItem() { Content = tbk, IsHitTestVisible = false };
            FirstListBox.Items.Add(lbi);
            foreach (int vk in GpKeys)
            {
                string p = Core.Instance.RaceTitles[vk].Caption;
                tbk = new TextBlock() { Text = p, Foreground = Brushes.DarkSlateBlue, FontWeight = FontWeights.Normal, Margin = new Thickness(24, 0, 0, 0) };
                lbi = new ListBoxItem() { Content = tbk, IsHitTestVisible = false };
                FirstListBox.Items.Add(lbi);
            }
        }

        private void GrandPrixTables(int prix)
        {
            FirstListBox.Items.Clear();
            SecondListBox.Items.Clear();

            int gapStart = 0;
            int gapEnd = 0;
            // Dates

            List<int> venueKeys = new List<int>();

            Dictionary<int, Tuple<string, string, string>> events = new Dictionary<int, Tuple<string, string, string>>();
            foreach (RaceMeeting mtg in Core.Instance.Races.Values)
            {
                if (mtg.RaceTitleKey == prix)
                {
                    if (!venueKeys.Contains(mtg.CircuitKey)) { venueKeys.Add(mtg.CircuitKey); }
                    Tuple<string, string, string> stuff = new Tuple<string, string, string>(mtg.RaceDate.ToString("MMM dd", System.Globalization.CultureInfo.CurrentCulture), Core.Instance.RaceTracks[mtg.CircuitKey].CircuitName[0], Core.Instance.Drivers[mtg.PodiumOne.Item1].FullName);
                    events.Add(mtg.RaceDate.Year, stuff);
                }
            }

            int noo = DateTime.Today.Year;
            for (int y = 1950; y <= noo; y++)
            {
                if (events.ContainsKey(y))
                {
                    AddGapYears(ref gapStart, gapEnd);

                    StackPanel s = new StackPanel() { Orientation = Orientation.Horizontal };
                    TextBlock t1 = new TextBlock() { Text = y.ToString(System.Globalization.CultureInfo.CurrentCulture), Foreground = Brushes.DarkCyan, FontWeight = FontWeights.Bold };
                    TextBlock t2 = new TextBlock() { Text = events[y].Item1, Foreground = Brushes.DarkOliveGreen, Margin = new Thickness(4, 0, 0, 0), Width = 80 };
                    TextBlock t3 = new TextBlock() { Text = events[y].Item2, Foreground = Brushes.SeaGreen, Margin = new Thickness(4, 0, 0, 0), Width = 200 };
                    TextBlock t4 = new TextBlock() { Text = events[y].Item3, Foreground = Brushes.Green, Margin = new Thickness(4, 0, 0, 0) };
                    s.Children.Add(t1);
                    s.Children.Add(t2);
                    s.Children.Add(t3);
                    s.Children.Add(t4);
                    ListBoxItem li = new ListBoxItem() { Content = s, IsHitTestVisible = false };
                    FirstListBox.Items.Add(li);
                }
                else
                {
                    if (gapStart == 0) { gapStart = y; }
                    gapEnd = y;
                }
            }
            AddGapYears(ref gapStart, gapEnd);

            // Venues
            TextBlock tbk = new TextBlock() { Text = "HOST CIRCUITS", Foreground = Brushes.DarkCyan, FontWeight = FontWeights.Medium, Margin = new Thickness(12, 0, 0, 0) };
            ListBoxItem lbi = new ListBoxItem() { Content = tbk, IsHitTestVisible = false };
            SecondListBox.Items.Add(lbi);
            foreach (int vk in venueKeys)
            {
                string p = Core.Instance.RaceTracks[vk].CircuitName[0];
                for(int w = 1; w < Core.Instance.RaceTracks[vk].CircuitTitleCount; w++)
                {
                    p += " = " + Core.Instance.RaceTracks[vk].CircuitName[w];
                }
                tbk = new TextBlock() { Text = p, Foreground = Brushes.DarkBlue, FontWeight = FontWeights.Medium, Margin = new Thickness(12, 0, 0, 0) };
                lbi = new ListBoxItem() { Content = tbk, IsHitTestVisible = false };
                SecondListBox.Items.Add(lbi);
            }
        }

        private void AddGapYears(ref int gapStart, int gapEnd)
        {
            if (gapStart == 0) { return; }
            string gapdescription = gapStart.ToString(System.Globalization.CultureInfo.CurrentCulture);
            string yrs = "year";
            if (gapEnd != gapStart) { gapdescription += $" - {gapEnd}";yrs = "years"; }
            int gap = 1 + gapEnd - gapStart;
            StackPanel sg = new StackPanel() { Orientation = Orientation.Horizontal };
            TextBlock g1 = new TextBlock() { Text = gapdescription, Foreground = Brushes.DarkCyan, FontWeight = FontWeights.Bold };
            TextBlock g2 = new TextBlock() { Text = $"not held for {gap} {yrs}", Foreground = Brushes.Gray, Margin = new Thickness(4, 0, 0, 0) };
            sg.Children.Add(g1);
            sg.Children.Add(g2);
            ListBoxItem lig = new ListBoxItem() { Content = sg, IsHitTestVisible = false, Margin=new Thickness(0,8,0,8) };
            FirstListBox.Items.Add(lig);
            gapStart = 0;
        }

        private void CountryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CountryListBox.SelectedIndex >= 0)
            {
                ListBoxItem itm = CountryListBox.SelectedItem as ListBoxItem;
                int etat = (int)itm.Tag;
                switch (_mode)
                {
                    case "countries": { CountryTables(etat); break; }
                    case "circuits": { CircuitTables(etat); break; }
                    default: { GrandPrixTables(etat); break; }
                }
            }
        }

        private void EditCircuitButton_Click(object sender, RoutedEventArgs e)
        {
            if (CountryListBox.SelectedIndex < 0) { return; }
            ListBoxItem itm = (ListBoxItem)CountryListBox.SelectedItem;
            int key = (int)itm.Tag;
            CircuitEditorWindow w = new CircuitEditorWindow(Core.Instance.RaceTracks[key].Specification) { Owner = this };
            bool? q = w.ShowDialog();
            if (q.HasValue && q.Value) { Core.Instance.RaceTracks[key].Specification = w.CircuitSpecification;CountryListBox.Items.Clear(); FirstListBox.Items.Clear();
                SecondListBox.Items.Clear(); PrepareCircuits(); _edited = true;
            }
        }
}