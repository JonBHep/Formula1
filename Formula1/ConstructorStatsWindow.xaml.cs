using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Formula1;

public partial class ConstructorStatsWindow : Window
{
    public ConstructorStatsWindow()
        {
            InitializeComponent();
        }

        private bool _loaded = false;
        private string _quest = "ALL";

        private void FillConstructorsList()
        {
            TeamListBox.Items.Clear();
            HistoryListBox.Items.Clear();
            List<NamedItem> list = Core.Instance.Constructors.Values.ToList();
            list.Sort();

            string choice = string.Empty;
            if (KindListBox.SelectedIndex >= 0)
            {
                ListBoxItem item = (ListBoxItem)KindListBox.SelectedItem;
                choice = item.Tag.ToString();
            }

                switch (_quest)
            {
                case "ALL":
                    {
                        foreach (NamedItem it in list)
                        {
                            StackPanel stack = new StackPanel() { Orientation = Orientation.Horizontal };
                            TextBlock tb = new TextBlock() { Text =Core.ConstructorName( it.Caption) };
                            stack.Children.Add(tb);
                            Tuple<int, int> dates = Core.Instance.ConstructorFirstLastYears(it.Key);
                            string ds = dates.Item1.ToString(Core.CultureUk);
                            if (dates.Item2 != dates.Item1) { ds += "-" + dates.Item2.ToString(Core.CultureUk); }
                            tb = new TextBlock() { Foreground = Brushes.CornflowerBlue, Text = ds, Margin = new Thickness(6, 0, 0, 0) };
                            stack.Children.Add(tb);
                            ListBoxItem bi = new ListBoxItem() { Content = stack, Tag = it.Key };

                            TeamListBox.Items.Add(bi);
                        }
                        break;
                    }
                case "CON":
                    {
                        if (!string.IsNullOrEmpty(choice))
                        {
                            foreach (NamedItem it in list)
                            {
                                string bodybuilder = GetBodyConstructor(it.Caption);
                                if (bodybuilder == choice)
                                {
                                    StackPanel stack = new StackPanel() { Orientation = Orientation.Horizontal };
                                    TextBlock tb = new TextBlock() { Text = Core.ConstructorName(it.Caption) };
                                    stack.Children.Add(tb);
                                    Tuple<int, int> dates = Core.Instance.ConstructorFirstLastYears(it.Key);
                                    string ds = dates.Item1.ToString(Core.CultureUk);
                                    if (dates.Item2 != dates.Item1) { ds += "-" + dates.Item2.ToString(Core.CultureUk); }
                                    tb = new TextBlock() { Foreground = Brushes.CornflowerBlue, Text = ds, Margin = new Thickness(6, 0, 0, 0) };
                                    stack.Children.Add(tb);
                                    ListBoxItem bi = new ListBoxItem() { Content = stack, Tag = it.Key };
                                    TeamListBox.Items.Add(bi);
                                }
                            }
                        }
                        break;
                    }
                case "ENG":
                    {
                        if (!string.IsNullOrEmpty(choice))
                        {
                            foreach (NamedItem it in list)
                            {
                                string engineer = GetEngineMaufacturer(it.Caption);
                                if (engineer == choice)
                                {
                                    StackPanel stack = new StackPanel() { Orientation = Orientation.Horizontal };
                                    TextBlock tb = new TextBlock() { Text = Core.ConstructorName(it.Caption) };
                                    stack.Children.Add(tb);
                                    Tuple<int, int> dates = Core.Instance.ConstructorFirstLastYears(it.Key);
                                    string ds = dates.Item1.ToString(Core.CultureUk);
                                    if (dates.Item2 != dates.Item1) { ds += "-" + dates.Item2.ToString(Core.CultureUk); }
                                    tb = new TextBlock() { Foreground = Brushes.CornflowerBlue, Text = ds, Margin = new Thickness(6, 0, 0, 0) };
                                    stack.Children.Add(tb);
                                    ListBoxItem bi = new ListBoxItem() { Content = stack, Tag = it.Key };
                                    TeamListBox.Items.Add(bi);
                                }
                            }
                        }
                        break;
                    }
            }
        }

        private void FillKindList()
        {
            KindListBox.Items.Clear();
            TeamListBox.Items.Clear();
            HistoryListBox.Items.Clear();
            switch (_quest)
            {
                case "ALL":
                    {
                        FillConstructorsList();
                        break;
                    }
                case "CON":
                    {
                        List<NamedItem> list = Core.Instance.Constructors.Values.ToList();
                        List<string> cons = new List<string>();
                        foreach (NamedItem it in list)
                        {
                            string tm = it.Caption;
                            string bodybuilder = GetBodyConstructor(tm);
                            if (!cons.Contains(bodybuilder)) { cons.Add(bodybuilder); }
                        }
                        cons.Sort();
                        foreach (string it in cons)
                        {
                            TextBlock tb = new TextBlock() { Foreground = Brushes.CornflowerBlue, Text = it, Margin = new Thickness(6, 0, 0, 0) };
                            ListBoxItem bi = new ListBoxItem() { Content = tb, Tag = it };
                            KindListBox.Items.Add(bi);
                        }
                        break;
                    }
                case "ENG":
                    {
                        List<NamedItem> list = Core.Instance.Constructors.Values.ToList();
                        List<string> engs = new List<string>();
                        foreach (NamedItem it in list)
                        {
                            string tm = it.Caption;
                            string engineer = GetEngineMaufacturer(tm);
                            if (!engs.Contains(engineer)) { engs.Add(engineer); }
                        }
                        engs.Sort();
                        foreach (string it in engs)
                        {
                            TextBlock tb = new TextBlock() { Foreground = Brushes.CornflowerBlue, Text = it, Margin = new Thickness(6, 0, 0, 0) };
                            ListBoxItem bi = new ListBoxItem() { Content = tb, Tag = it };
                            KindListBox.Items.Add(bi);
                        }
                        break;
                    }
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            FillConstructorsList();
            _loaded = true;
        }

        private void TeamListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TeamListBox.SelectedIndex < 0) { return; }
            ListBoxItem box = (ListBoxItem)TeamListBox.SelectedItem;
            int key = (int)box.Tag;

            NamedItem team = Core.Instance.Constructors[key];
            HistoryListBox.Items.Clear();
            
            TextBlock b = new TextBlock() { Text = Core.ConstructorName( team.Caption).ToUpper(Core.CultureUk), FontWeight = FontWeights.Bold };
            ListBoxItem item = new ListBoxItem() { IsHitTestVisible = false, Content=b };
            HistoryListBox.Items.Add(item);

            ListBoxItem blk = new ListBoxItem() { Content = " ", IsHitTestVisible = false }; ;
            HistoryListBox.Items.Add(blk);

            List<int> teamraces = new List<int>();
            Dictionary<int, List<int>> raceDrivers = new Dictionary<int, List<int>>();

            Dictionary<int, int> driverAppearanceCounts = new Dictionary<int, int>();

            foreach (Voiture rout in Core.Instance.Voitures.Values)
            {
                if (rout.ConstructorKey == key)
                {

                    if (!teamraces.Contains(rout.RaceMeetingKey)) { teamraces.Add(rout.RaceMeetingKey);raceDrivers.Add(rout.RaceMeetingKey, new List<int>()); }

                    int dk = rout.DriverKey(0);
                    if (driverAppearanceCounts.ContainsKey(dk)) { driverAppearanceCounts[dk]++; } else { driverAppearanceCounts.Add(dk, 1); }
                    if (!raceDrivers[rout.RaceMeetingKey].Contains(dk)) { raceDrivers[rout.RaceMeetingKey].Add(dk); }
                    dk = rout.DriverKey(1);
                    if (driverAppearanceCounts.ContainsKey(dk)) { driverAppearanceCounts[dk]++; } else { driverAppearanceCounts.Add(dk, 1); }
                    if (!raceDrivers[rout.RaceMeetingKey].Contains(dk)) { raceDrivers[rout.RaceMeetingKey].Add(dk); }
                    dk = rout.DriverKey(2);
                    if (driverAppearanceCounts.ContainsKey(dk)) { driverAppearanceCounts[dk]++; } else { driverAppearanceCounts.Add(dk, 1); }
                    if (!raceDrivers[rout.RaceMeetingKey].Contains(dk)) { raceDrivers[rout.RaceMeetingKey].Add(dk); }
                }
            }

            b = new TextBlock() { Text = "Drivers", FontWeight = FontWeights.Bold };
            item = new ListBoxItem() { IsHitTestVisible = false, Content = b };
            HistoryListBox.Items.Add(item);

            List<Driver> dlist = new List<Driver>();
            foreach (int k in driverAppearanceCounts.Keys)
            {
                if (k > 0) // zeros will have been added for second and third drivers per car
                {
                    dlist.Add(Core.Instance.Drivers[k]);
                }
            }
            dlist.Sort();
            foreach(Driver dvr in dlist)
            {
                b = new TextBlock() { Text = $"{dvr.FullName} (x {driverAppearanceCounts[dvr.Key]})" };
                item = new ListBoxItem() { IsHitTestVisible = false, Content = b };
                HistoryListBox.Items.Add(item);
            }

            b = new TextBlock() { Text = "Races", FontWeight = FontWeights.Bold };
            item = new ListBoxItem() { IsHitTestVisible = false, Content = b };
            HistoryListBox.Items.Add(item);

            List<RaceMeeting> rlist = new List<RaceMeeting>();
            foreach (int k in teamraces)
            {
                rlist.Add(Core.Instance.Races[k]);
            }
            rlist.Sort();
            foreach (RaceMeeting rm in rlist)
            {
                b = new TextBlock() { Text =$"{rm.RaceDate.Year} R{rm.YearSerialNumber} {Core.Instance.RaceTitles[rm.RaceTitleKey].Caption}" };
                string drivers = string.Empty;
                foreach (int i in raceDrivers[rm.Key]) { if (i > 0) { drivers += ", " + Core.Instance.DriverShortName(i); } }
                drivers ="  ("+ drivers.Substring(2)+")";
                Run r = new Run() { Text = drivers, Foreground = Brushes.CornflowerBlue };
                b.Inlines.Add(r);
                item = new ListBoxItem() { IsHitTestVisible = false, Content = b };
                HistoryListBox.Items.Add(item);
            }
        }

        private void KindListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillConstructorsList();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!_loaded) { return; }
            RadioButton button = (RadioButton)sender;
            _quest = button.Tag.ToString();
            FillKindList();
        }

        private static string GetBodyConstructor(string equipe)
        {
            int pt = equipe.IndexOf('=');
            if (pt < 1) { return equipe; }
            string con = equipe.Substring(0, pt);
            return con;
        }
        private static string GetEngineMaufacturer(string equipe)
        {
            int pt = equipe.IndexOf('=');
            if (pt < 1) { return equipe; }
            string eng = equipe.Substring(pt+1);
            return eng;
        }
}