using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Formula1;

public partial class DriverStatsWindow : Window
{
    private bool _returnDialogValue = false;
        private readonly List<Tuple<string, int>> driverNames = new List<Tuple<string, int>>();

        public DriverStatsWindow()
        {
            InitializeComponent();
        }

        private void FillDriversList()
        {
            GraphButton.IsEnabled = false;
            HistoryListBox.Items.Clear();
            DriversListBox.Items.Clear();
            List<Driver> list = Core.Instance.Drivers.Values.ToList();
            list.Sort();

            // one-time build list of names to use in filter search
            if (driverNames.Count == 0)
            {
                foreach (Driver it in list)
                {
                    driverNames.Add(new Tuple<string, int>( it.FullName, it.Key));
                }
            }

            foreach (Driver it in list)
            {
                StackPanel spl = new StackPanel() { Orientation = Orientation.Horizontal };
                spl.Children.Add(new TextBlock() { Text = it.Forenames });
                spl.Children.Add(new TextBlock() { Text = it.Surname, FontWeight = FontWeights.Medium, Margin = new Thickness(4, 0, 0, 0) });
                if (Core.Instance.Countries[it.CountryKey].Caption == "France") { spl.Children.Add(Core.FrenchFlag()); }
                TextBlock tb = new TextBlock();
                it.AppendFateGlyph(ref tb);
                spl.Children.Add(tb);
                ListBoxItem bi = new ListBoxItem() { Content = spl, Tag = it.Key };
                DriversListBox.Items.Add(bi);
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            FillDriversList();
            AccidentRacingTextBlock.Text = AccidentOtherTextBlock.Text = Core.GlyphFatalAccident;
            VivantTextBlock.Text = BlesséTextBlock.Text = Core.GlyphLiving;
            FilterTextBox.Focus();
        }

        private struct RaceAppearance : IComparable<RaceAppearance>
        {
            internal readonly DateTime QDate;
            internal readonly int QRaceTitleKey;
            internal readonly int QPosition;
            internal readonly int QTeamKey;
            internal readonly float QPoints;
            internal readonly bool QF2;
            internal RaceAppearance(DateTime date, int titlK, int posn, int teamK, float points, bool F2)
            {
                QDate = date;
                QRaceTitleKey = titlK;
                QPosition = posn;
                QTeamKey = teamK;
                QPoints = points;
                QF2 = F2;
            }

            int IComparable<RaceAppearance>.CompareTo(RaceAppearance other)
            {
                return QDate.CompareTo(other.QDate);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = _returnDialogValue;
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button db)
            {
                if (db.Tag is int key)
                {
                    DriverPropertiesWindow w = new DriverPropertiesWindow(key) { Owner = this, ShowInTaskbar = false };
                    bool? q = w.ShowDialog();
                    if (q.HasValue && q.Value)
                    {
                        FillDriversList();
                        _returnDialogValue = true;
                    }
                }
            }
        }

        private void ItemListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GraphButton.IsEnabled = false;
            DisplayButton.IsEnabled = false;
            DetailsButton.IsEnabled = false;

            HistoryListBox.Items.Clear();

            if (DriversListBox.SelectedItem is ListBoxItem item)
            {
                int key = (int)item.Tag;
                GraphButton.Tag = key;
                GraphButton.IsEnabled = true;
                HistoryHeadingTextBlock.Text = Core.Instance.Drivers[key].FullName;
                DisplayButton.IsEnabled = true;
                DisplayButton.Tag = key;
                DetailsButton.IsEnabled = true;
                DetailsButton.Tag = key;
            }
        }

        private void DisplayDriverHistory(int conducteur)
        {
            Jbh.UiServices.SetBusyState();
            Driver man = Core.Instance.Drivers[conducteur];
            HistoryListBox.Items.Clear();

            ListBoxItem item = new ListBoxItem() { IsHitTestVisible = false };
            TextBlock b = new TextBlock();
            Run w = new Run() { Text = man.FullName, FontWeight = FontWeights.Bold };
            b.Inlines.Add(w);
            string pays = Core.Instance.Countries[man.CountryKey].Caption;
            w = new Run() { Text = $" ({pays})" };
            b.Inlines.Add(w);
            StackPanel s = new StackPanel() { Orientation = Orientation.Horizontal };
            s.Children.Add(b);
            if ((pays == "France")) { s.Children.Add(Core.FrenchFlag()); }
            item.Content = s; 
            HistoryListBox.Items.Add(item);

            TextBlock tblk = new TextBlock();
            tblk.Inlines.Add(new Run() { Text ="Born:" });
            tblk.Inlines.Add(new Run() { Text = man.BirthDate.ToLongDateString(), FontWeight = FontWeights.Bold });
            item = new ListBoxItem() { Content = tblk, IsHitTestVisible = false };
            HistoryListBox.Items.Add(item);

            if (man.DeathDate < Core.DateBase)
            {
                tblk = new TextBlock() { IsHitTestVisible = false };
                tblk.Inlines.Add(new Run() { Text ="StillLiving", Foreground = Brushes.DarkGreen });
            }
            else
            {
                tblk = new TextBlock() { IsHitTestVisible = false };
                tblk.Inlines.Add(new Run() { Text ="Died:" });
                tblk.Inlines.Add(new Run() { Text = man.DeathDate.ToLongDateString(), FontWeight = FontWeights.Bold });
            }
            item = new ListBoxItem() { Content = tblk, IsHitTestVisible = false };
            HistoryListBox.Items.Add(item);

            if (man.CeiDate > Core.DateBase)
            {
                tblk = new TextBlock() { IsHitTestVisible = false };
                tblk.Inlines.Add(new Run() { Text ="Career-ending injury" });
                tblk.Inlines.Add(new Run() { Text = man.CeiDate.ToLongDateString(), FontWeight = FontWeights.Bold });
                item = new ListBoxItem() { Content = tblk, IsHitTestVisible = false };
                HistoryListBox.Items.Add(item);
            }

            tblk = new TextBlock();
            switch (man.HowDied)
            {
                case Core.CauseOfDeath.Natural:
                    {
                        Run r = new Run() { Text = $"Died aged {man.LatestAge()}", Foreground = Brushes.Black };
                        tblk.Inlines.Add(r);
                        break;
                    }
                case Core.CauseOfDeath.OtherAccident:
                    {
                        Run r = new Run() { FontFamily = new FontFamily("Webdings"), Text = '\x0085'.ToString(System.Globalization.CultureInfo.CurrentCulture), FontSize = 16, Foreground = Brushes.Black };  // death mask symbol
                        tblk.Inlines.Add(r);
                        r = new Run() { Text = $"Died in accident, not racing related, aged {man.LatestAge()}", Foreground = Brushes.Black };
                        tblk.Inlines.Add(r);
                        break;
                    }
                case Core.CauseOfDeath.RacePracticeOrTestingAccident:
                    {
                        Run r = new Run() { FontFamily = new FontFamily("Webdings"), Text = '\x0085'.ToString(System.Globalization.CultureInfo.CurrentCulture), FontSize = 16, Foreground = Brushes.Red }; // death mask symbol
                        tblk.Inlines.Add(r);
                        r = new Run() { Text = $"Died in qualifying, testing or practice accident aged {man.LatestAge()}", Foreground = Brushes.Red };
                        tblk.Inlines.Add(r);
                        break;
                    }
                case Core.CauseOfDeath.RacingAccident:
                    {
                        Run r = new Run() { FontFamily = new FontFamily("Webdings"), Text = '\x0085'.ToString(System.Globalization.CultureInfo.CurrentCulture), FontSize = 16, Foreground = Brushes.Red }; // death mask symbol
                        tblk.Inlines.Add(r);
                        r = new Run() { Text = $"Died in racing accident aged {man.LatestAge()}", Foreground = Brushes.Red };
                        tblk.Inlines.Add(r);
                        break;
                    }
                default:
                    {
                        if ((man.BirthDate > Core.DateBase) && !(man.DeathDate > Core.DateBase))
                        {
                            Run r = new Run() { FontFamily = new FontFamily("Webdings"), Text = $"{'\x00F6'}", FontSize = 16, Foreground = Brushes.Blue }; // cat symbol (9 lives)
                            tblk.Inlines.Add(r);
                            r = new Run() { Text = $" ({man.LatestAge()})", Foreground = Brushes.Blue };
                            tblk.Inlines.Add(r);
                        }
                        break;
                    }
            }

            item = new ListBoxItem() { Content = tblk, IsHitTestVisible = false };
            HistoryListBox.Items.Add(item);

            if (!string.IsNullOrWhiteSpace(man.DeathMode))
            {
                tblk = new TextBlock() { Text = man.DeathMode, TextWrapping = TextWrapping.Wrap, MaxWidth = HistoryListBox.ActualWidth - 50, Foreground = Brushes.DarkCyan };
                item = new ListBoxItem() { Content = tblk, IsHitTestVisible = false };
                HistoryListBox.Items.Add(item);
            }

            ListBoxItem blk = new ListBoxItem() { Content = " ", IsHitTestVisible = false }; ;
            HistoryListBox.Items.Add(blk);

            List<RaceAppearance> races = new List<RaceAppearance>();
            List<string> teams = new List<string>();
            List<int> hisraces = new List<int>();

            foreach (Voiture rout in Core.Instance.Voitures.Values)
            {
                if (!hisraces.Contains(rout.RaceMeetingKey)) // so as to only record the driver's best performance in that race (in case s/he sat in 2 cars)
                {
                    if (rout.IncludesDriver(conducteur))
                    {
                        float pt = 0;
                        float? pts = Core.DriverRacePoints(rout.RaceMeetingKey, conducteur);
                        if (pts.HasValue) { pt = pts.Value; }
                        RaceAppearance app = new RaceAppearance(rout.RaceDate, Core.Instance.Races[rout.RaceMeetingKey].RaceTitleKey, rout.RacePosition, rout.ConstructorKey, pt, rout.Formula2);
                        races.Add(app);
                        hisraces.Add(rout.RaceMeetingKey);

                        string tm = Core.Instance.Constructors[rout.ConstructorKey].Caption;
                        if (!teams.Contains(tm)) { teams.Add(tm); }
                    }
                }
            }

            HistoryListBox.Items.Add(new ListBoxItem() { IsHitTestVisible = false, Content = new TextBlock() { Text =$"Competed: {man.RuntimeYears}" , Foreground=Brushes.DarkCyan, FontWeight=FontWeights.Medium} });

            teams.Sort();

            tblk = new TextBlock() { Text ="Constructors driven for", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 12, 0, 0) };
            item = new ListBoxItem() { Content = tblk, IsHitTestVisible = false };
            HistoryListBox.Items.Add(item);

            foreach (string t in teams)
            {
                tblk = new TextBlock() { Text = t };
                item = new ListBoxItem() { Content = tblk, IsHitTestVisible = false, Foreground = Brushes.DarkMagenta };
                HistoryListBox.Items.Add(item);
            }

            List<ListBoxItem> RaceDescriptions = new List<ListBoxItem>();
            races.Sort();
            foreach (RaceAppearance r in races)
            {
                Ellipse medal = new Ellipse() { Width = 18, Height = 18, Stroke = Brushes.Black, StrokeThickness = 1, Margin = new Thickness(2, 0, 8, 0), Visibility = Visibility.Hidden };
                FontWeight fwt = FontWeights.Medium;
                if (r.QPoints > 0) { fwt = FontWeights.Bold; medal.Visibility = Visibility.Visible; }
                if (r.QPosition > 999) { fwt = FontWeights.Normal; }

                switch (r.QPosition)
                {
                    case 1: { medal.Fill = Brushes.Gold; break; }
                    case 2: { medal.Fill = Brushes.Silver; break; }
                    case 3: { medal.Fill = Brushes.Peru; break; }
                    default: { medal.Fill = Brushes.Transparent; medal.Stroke = Brushes.Silver; break; }
                }
                Brush brosse = (r.QF2) ? Brushes.DarkGoldenrod : (r.QPosition > 999) ? Brushes.IndianRed : (r.QPosition == 1) ? Brushes.MediumSeaGreen : Brushes.Black;
                string q = $"{r.QDate.Year} {Core.Instance.RaceTitles[r.QRaceTitleKey].Caption} ~ {Core.Instance.Constructors[r.QTeamKey].Caption} ~ ({Core.RaceResultDescription(r.QPosition)})";

                if (r.QPoints > 0) { q += (r.QPoints == 1) ? $" 1 point" : $" {r.QPoints} points"; }
                if (r.QF2) { q += " Formula 2 car"; }
                StackPanel spnl = new StackPanel() { Orientation = Orientation.Horizontal };

                spnl.Children.Add(medal);
                tblk = new TextBlock() { Text = q, FontWeight = fwt, Foreground = brosse };
                spnl.Children.Add(tblk);
                item = new ListBoxItem() { Content = spnl, IsHitTestVisible = false, Tag=r.QDate.Year };
                RaceDescriptions.Add(item);
            }

            tblk = new TextBlock() { Text = $"Races: qualified for {man.RuntimeRacesQualified}, started {man.RuntimeRacesStarted}, finished {man.RuntimeRacesFinished}", FontWeight = FontWeights.Medium, Margin = new Thickness(0, 12, 0, 0) };
            item = new ListBoxItem() { Content = tblk, IsHitTestVisible = false };
            HistoryListBox.Items.Add(item);

            tblk = new TextBlock() { Text = $"Lifetime points total {Core.MyFormat(Core.Instance.Drivers[conducteur].LifetimePoints())}", FontWeight = FontWeights.Medium, Margin = new Thickness(0, 12, 0, 0) };
            item = new ListBoxItem() { Content = tblk, IsHitTestVisible = false };
            HistoryListBox.Items.Add(item);

            for (int y = 1949; y <= DateTime.Today.Year; y++)
            {
                float p = Core.Instance.Drivers[conducteur].SeasonPoints(y);
                if (p > 0)
                {
                    tblk = new TextBlock() { Text = $"Season points {y}: {Core.MyFormat(p)}", FontWeight = FontWeights.Medium, Margin = new Thickness(0, 4, 0, 0) };
                    item = new ListBoxItem() { Content = tblk, IsHitTestVisible = false };
                    HistoryListBox.Items.Add(item);
                }
            }

            List<Tuple<int, int>> winyears = new List<Tuple<int, int>>();
            int champ = 0;
            foreach (Season ssn in Core.Instance.Seasons.Values)
            {
                ssn.RefreshStatistics();
                if (ssn.DriverSeasonRanked(1) == conducteur) { winyears.Add(new Tuple<int, int>(ssn.Anno, 1)); champ++; }
                if (ssn.DriverSeasonRanked(2) == conducteur) { winyears.Add(new Tuple<int, int>(ssn.Anno, 2)); }
                if (ssn.DriverSeasonRanked(3) == conducteur) { winyears.Add(new Tuple<int, int>(ssn.Anno, 3)); }
            }

            if (winyears.Count > 0)
            {
                tblk = new TextBlock() { Text = $"DRIVERS CHAMPIONSHIP WINS ({champ})", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 12, 0, 0) };
                item = new ListBoxItem() { Content = tblk, IsHitTestVisible = false };
                HistoryListBox.Items.Add(item);

                foreach (Tuple<int, int> chmp in winyears)
                {
                    switch (chmp.Item2)
                    {
                        case 1: { tblk = new TextBlock() { Text = $"{chmp.Item1} Champion", FontWeight = FontWeights.Medium, Margin = new Thickness(8, 0, 0, 0) }; break; }
                        case 2: { tblk = new TextBlock() { Text = $"{chmp.Item1} Runner-up", FontWeight = FontWeights.Normal, Margin = new Thickness(8, 0, 0, 0) }; break; }
                        case 3: { tblk = new TextBlock() { Text = $"{chmp.Item1} 3rd place", FontWeight = FontWeights.Light, Margin = new Thickness(8, 0, 0, 0) }; break; }
                    }
                    item = new ListBoxItem() { Content = tblk, IsHitTestVisible = false };
                    HistoryListBox.Items.Add(item);
                }
            }

            tblk = new TextBlock() { Text ="Races", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 12, 0, 0) };
            item = new ListBoxItem() { Content = tblk, IsHitTestVisible = false };
            HistoryListBox.Items.Add(item);

            int prev = 0;
            foreach (ListBoxItem i in RaceDescriptions)
            {
                if (i.Tag is int y)
                {
                    if (y != prev) { HistoryListBox.Items.Add(new ListBoxItem() { IsHitTestVisible = false, Margin = new Thickness(0, 10, 0, 10), Content = new Border() { BorderBrush = Brushes.Black, BorderThickness =new Thickness(1,1,1,1), CornerRadius =new CornerRadius(4,4,4,4), Child = new TextBlock() { Text = $"{y}", FontWeight = FontWeights.Bold, Margin=new Thickness(4,2,4,2) } } }); prev = y; } // heading before new season
                }
                HistoryListBox.Items.Add(i);
            }

            List<Tuple<int, int, int, int>> Compadres = Core.Instance.CompadresOf(conducteur);

            HistoryListBox.Items.Add(new ListBoxItem() { IsHitTestVisible = false, Content = new TextBlock() { Text = $"COMPETED AGAINST {Compadres.Count} DRIVERS", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 12, 0, 0) } });

            // mec.Item1 = number of times competed
            // mec.Item2 = number of times I beat the other driver
            // mec.Item3 = number of times the other driver beat me
            // mec.Item4 = other driver ID

            int wins = 0;
            int losses = 0;
            foreach (Tuple<int, int, int, int> mec in Compadres)
            {
                wins += mec.Item2;
                losses += mec.Item3;
            }

            // Overall number of wins and losses against another driver, counting all drivers in all races in which 
            // subject AND the other driver started the race, and
            // subject OR the other driver finished the race
            HistoryListBox.Items.Add(new ListBoxItem() { IsHitTestVisible = false, Content = new TextBlock() { Text = $"Counting every driver in every race in which {man.Surname} competed (excluding occasions on which neither driver was ranked in the race (i.e. both retired)):", MaxWidth = HistoryListBox.ActualWidth - 40, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Medium, Margin = new Thickness(0, 6, 0, 6) } });

            HistoryListBox.Items.Add(new ListBoxItem() { IsHitTestVisible = false, Content =new  TextBlock() { Text = $"{man.Surname} outperformed another driver {wins} times", FontWeight = FontWeights.Medium, Foreground = Brushes.DarkGreen } });
            HistoryListBox.Items.Add(new ListBoxItem() { IsHitTestVisible = false, Content = new TextBlock() { IsHitTestVisible = false, Text = $"{man.Surname} was outperformed by another driver {losses} times", FontWeight = FontWeights.Medium, Foreground = Brushes.DarkRed } });

            if ((wins + losses) > 0)
            {
                double winproportion = wins / (float)(wins + losses);
                Brush propBrush = (winproportion > 0.5) ? Brushes.DarkGreen : (winproportion < 0.5) ? Brushes.DarkRed : Brushes.Black;
                winproportion = Math.Round(100 * winproportion);
                HistoryListBox.Items.Add(new ListBoxItem() { IsHitTestVisible = false, Content = new TextBlock() { IsHitTestVisible = false, Text = $"{man.Surname}'s success rate: {winproportion}%", FontWeight = FontWeights.Medium, Foreground = propBrush } });
            }

            HistoryListBox.Items.Add(new ListBoxItem() { IsHitTestVisible = false, Content = new TextBlock() { IsHitTestVisible = false, Text = $"The list below shows the number of times {man.Surname} competed against the listed drivers, and the percentage of wins against the other driver, excluding races in which neither driver finished (NF)", MaxWidth = HistoryListBox.ActualWidth - 40, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Medium, Margin = new Thickness(0, 6, 0, 6) } });

            List<int> tymes = new List<int>();
            foreach (Tuple<int, int, int, int> mec in Compadres)
            {
                if (!tymes.Contains(mec.Item1)) { tymes.Add(mec.Item1); }
            }

            foreach (int t in tymes)
            {
                HistoryListBox.Items.Add(new ListBoxItem() { IsHitTestVisible = false, Content = new TextBlock() { IsHitTestVisible = false, Text = (t == 1) ? "Competed once" : $"Competed {t} times", FontWeight = FontWeights.Medium, Foreground = Brushes.DarkGreen } });
                foreach (Tuple<int, int, int, int> mec in Compadres)
                {
                    if (mec.Item1 == t) // number of times competed against each other
                    {
                        StackPanel panel = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(20, 0, 0, 0) };
                        TextBlock tbName = new TextBlock() { Width = 300, Text ="Against", FontWeight = FontWeights.Light };
                        tbName.Inlines.Add(new Run() { Text = $" {Core.Instance.Drivers[mec.Item4].Forenames}", FontWeight = FontWeights.Medium });
                        tbName.Inlines.Add(new Run() { Text = $" {Core.Instance.Drivers[mec.Item4].Surname}", FontWeight = FontWeights.Bold });
                        panel.Children.Add(tbName);
                        int better = mec.Item2;
                        int worse = mec.Item3;
                        int neither = t - (better + worse);

                        if ((better + worse) > 0)
                        {
                            double pourcent = better / (float)(better + worse);
                            Brush successBrush = (pourcent > 0.5) ? Brushes.LightGreen : (pourcent < 0.5) ? Brushes.Salmon : Brushes.LightYellow;
                            pourcent = Math.Round(100 * pourcent);
                            string percent = $"Won {pourcent}%";
                            panel.Children.Add(new TextBlock() { Text = percent, Width = 72, TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 4, 0), Background = successBrush });
                            HistoryListBox.Items.Add(new ListBoxItem() { IsHitTestVisible = false, Content = panel });
                        }

                        string sbetter = (better == 0) ? string.Empty : (better == 1) ? "1 win" : $"{better} wins";
                        string sworse = (worse == 0) ? string.Empty : (worse == 1) ? "1 loss" : $"{worse} losses";
                        string sneither = (neither == 0) ? string.Empty : $"{neither} NF";
                        if ((!string.IsNullOrEmpty(sbetter)) && ((!string.IsNullOrEmpty(sneither)) || (!string.IsNullOrEmpty(sworse)))) { sbetter += "; "; };
                        if ((!string.IsNullOrEmpty(sworse)) && (!string.IsNullOrEmpty(sneither))) { sworse += "; "; };
                        panel.Children.Add(new TextBlock() { Text = $"{sbetter}{sworse}{sneither}" });

                    }
                }
            }
        }

        private void GraphButton_Click(object sender, RoutedEventArgs e)
        {
            int k = (int)GraphButton.Tag;
            CareerGraphWindow w = new CareerGraphWindow(k) { Owner = this };
            w.ShowDialog();
        }

        private void DisplayButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button db)
            {
                if (db.Tag is int q)
                {
                    DisplayDriverHistory(q);
                }
            }
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string quoi = FilterTextBox.Text.Trim().ToUpperInvariant();
            FilteredListBox.Items.Clear();
            if (!string.IsNullOrEmpty(quoi))
            {
                foreach (Tuple< string, int> nom in driverNames)
                {
                    if (nom.Item1.IndexOf(quoi, 0, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        FilteredListBox.Items.Add(new ListBoxItem() {Tag=nom.Item2, Content = new TextBlock() { Text = nom.Item1, FontWeight = FontWeights.Medium, Foreground = Brushes.SaddleBrown } });
                    }
                }
            }
        }

        private void FilteredListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem target = null;
            if (FilteredListBox.SelectedItem is ListBoxItem item)
            {
                if (item.Tag is int y)
                {
                    for(int x=0; x< DriversListBox.Items.Count; x++)
                    {
                        if (DriversListBox.Items[x] is ListBoxItem boxitem)
                        {
                            if (boxitem.Tag is int yy)
                            {
                                if (y == yy) { target = boxitem; }
                            }
                        }
                    }
                }
                if (target!= null)
                {
                    DriversListBox.SelectedItem = target;
                    DriversListBox.ScrollIntoView(target);
                }
            }
        }
}