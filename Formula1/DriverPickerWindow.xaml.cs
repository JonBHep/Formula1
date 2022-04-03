using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Formula1;

public partial class DriverPickerWindow : Window
{
    public DriverPickerWindow(DateTime raceDate, List<int> already, int potentialRacePosition)
        {
            _raceDate = raceDate;
            InitializeComponent();
            _already = already;
            _potentialRaceResult = potentialRacePosition;
        }

        private int _selectedDriverKey;
        private int _selectedTeamKey;
        private readonly int _potentialRaceResult;
        private readonly List<int> _already;
        private readonly DateTime _raceDate; // used for selecting currently frequent drivers

        internal int SelectedDriverKey { get => _selectedDriverKey; }
        internal int SelectedTeamKey { get => _selectedTeamKey; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (_potentialRaceResult==0)
            {
                PotentialRacePositionStackPanel.Visibility = Visibility.Hidden;
            }
            else
            {
                PotentialRacePositionTextBox.Text =  $"{_potentialRaceResult}";
            }
            
            if (_raceDate<Core.DateBase)
            {
                FillCompleteList();
                CompleteButton.IsEnabled = false;
            }
            else
            {
                FillSelectedList();
            }
        }

        private void FillSelectedList()
        {
            NewEntryDock.Visibility = Visibility.Hidden;
            SurnameTextBox.Clear();
            ForenamesTextBox.Clear();
            CountryTextBlock.Text = string.Empty;
            AddButton.IsEnabled = false;
            DriverListBoxOne.Items.Clear();
            DriverListBoxTwo.Items.Clear();
            List<Driver> list = Core.Instance.Drivers.Values.ToList();
            list.Sort();
            Core.Instance.GetRecentDrivers(_raceDate, out List<int> d12, out List<int> d24);
            float halfway = (d24.Count - _already.Count) / 2f;
            int px = 0;
            foreach (Driver it in list)
            {
                if (!_already.Contains(it.Key))
                {
                    if (d24.Contains(it.Key))
                    {
                        ListBox boxAvec = DriverListBoxOne;
                        if (px >= halfway) { boxAvec = DriverListBoxTwo; }

                        px++;

                        StackPanel splAvec = new StackPanel() { Orientation = Orientation.Horizontal };

                        TextBlock tbAvec = new TextBlock() { Text = it.Forenames, FontWeight = FontWeights.Light, Foreground = Brushes.DarkSlateGray };
                        if (d12.Contains(it.Key)) { tbAvec.FontWeight = FontWeights.Normal; tbAvec.Foreground = Brushes.DarkSlateBlue; }
                        splAvec.Children.Add(tbAvec);

                        tbAvec = new TextBlock() { Text = it.Surname, FontWeight = FontWeights.Normal, Foreground = Brushes.DarkSlateGray, Margin = new Thickness(4, 0, 0, 0) };
                        if (d12.Contains(it.Key)) { tbAvec.FontWeight = FontWeights.Bold; tbAvec.Foreground = Brushes.DarkSlateBlue; }
                        splAvec.Children.Add(tbAvec);

                        int LikelyTeam = Core.Instance.MostRecentConstructorForDriver(it.Key, _raceDate);

                        if (LikelyTeam > 0)
                        {
                            tbAvec = new TextBlock() { Text = Core.ConstructorName(Core.Instance.Constructors[LikelyTeam].Caption), FontWeight = FontWeights.Normal, Margin = new Thickness(4, 0, 0, 0), Foreground = Brushes.SaddleBrown };
                            splAvec.Children.Add(tbAvec);
                        }

                        Tuple<int, int> Tagger = new Tuple<int, int>(it.Key, LikelyTeam);
                        ListBoxItem bi = new ListBoxItem() { Content = splAvec, Tag = Tagger };
                        boxAvec.Items.Add(bi);
                    }
                }
            }
        }

        private void FillCompleteList()
        {
            SurnameTextBox.Clear();
            ForenamesTextBox.Clear();
            CountryTextBlock.Text = string.Empty;
            AddButton.IsEnabled = false;
            DriverListBoxOne.Items.Clear();
            DriverListBoxTwo.Items.Clear();

            List<Driver> list = Core.Instance.Drivers.Values.ToList();
            list.Sort();
            List<int> viveurs = Core.Instance.LivingDrivers(_raceDate);
            float halfway = viveurs.Count / 2f;
            int px = 0;
            foreach (Driver it in list)
            {
                if (viveurs.Contains(it.Key))
                {
                    ListBox box = DriverListBoxOne;
                    if (px >= halfway) { box = DriverListBoxTwo; }
                    px++;
                    StackPanel spl = new StackPanel() { Orientation = Orientation.Horizontal };
                    TextBlock tb = new TextBlock() { Text = it.Forenames };
                    spl.Children.Add(tb);
                    tb = new TextBlock() { Text = it.Surname, FontWeight = FontWeights.Medium, Margin = new Thickness(4, 0, 0, 0) };
                    spl.Children.Add(tb);
                    string memento = it.TimeLeft(_raceDate);
                    Brush pinceau = (memento.Contains("M")) ? Brushes.OrangeRed : Brushes.HotPink;
                    tb = new TextBlock() { Text = memento, FontWeight = FontWeights.Normal, Margin = new Thickness(4, 0, 0, 0), Foreground=pinceau };
                    spl.Children.Add(tb);

                    Tuple<int, int>  Tagger = new Tuple<int, int>(it.Key, 0);
                    ListBoxItem bi = new ListBoxItem() { Content = spl, Tag = Tagger };
                    box.Items.Add(bi);
                }
            }
        }

        private void ItemListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox box = sender as ListBox;
            if (box.SelectedIndex >= 0)
            {
                ListBoxItem itm = box.SelectedItem as ListBoxItem;
                Tuple<int, int> Tags = (Tuple<int, int>)itm.Tag;
                _selectedDriverKey = Tags.Item1;
                _selectedTeamKey = Tags.Item2;
                DialogResult = true;
            }
        }

        private void TextBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckCompleteness();
        }

        private void CheckCompleteness()
        {
            bool fullinfo = true;
            if (string.IsNullOrWhiteSpace(ForenamesTextBox.Text)) { fullinfo = false; }
            if (string.IsNullOrWhiteSpace(SurnameTextBox.Text)) { fullinfo = false; }
            if (CountryTextBlock.Tag ==null) { fullinfo = false; }
            AddButton.IsEnabled = fullinfo;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string sname = SurnameTextBox.Text.Trim();
            string fname = ForenamesTextBox.Text.Trim();
            string fullname = $"{fname} {sname}";
            // duplication check
            bool flag = false;
            foreach (Driver it in Core.Instance.Drivers.Values) { if (it.FullName.Equals(fullname, StringComparison.CurrentCultureIgnoreCase)) { flag = true; } }
            if (flag)
            {
                MessageBox.Show("This name already exists", Jbh.AppManager.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            int ckey = (int)CountryTextBlock.Tag;
            int newKey = 1;
            while (Core.Instance.Drivers.ContainsKey(newKey)) { newKey++; }
            Driver dr = new Driver() { Surname = sname, Forenames = fname, CountryKey = ckey, Key = newKey, DeathMode=string.Empty };
            Core.Instance.Drivers.Add(dr.Key, dr);
            _selectedDriverKey = newKey;
            _selectedTeamKey = 0;
            DialogResult = true;
        }

        private void CountriesButton_Click(object sender, RoutedEventArgs e)
        {
            GenericPickerWindow w = new GenericPickerWindow(Core.Instance.Countries, "Countries") { Owner = this };
         bool? q=   w.ShowDialog();
            if (q.HasValue && q.Value)
            {
                int p = w.SelectedKey;
                CountryTextBlock.Text = Core.Instance.Countries[p].Caption;
                CountryTextBlock.Tag = p;
                CheckCompleteness();
            }
        }

        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            NewEntryDock.Visibility = Visibility.Visible;
            CompleteButton.IsEnabled = false;
            FillCompleteList();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Left = 32;
            Height = Core.LowWindowHeight;
            Top = Core.LowWindowTop;
        }
}