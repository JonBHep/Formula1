using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Formula1;

public partial class CarResultWindow : Window
{
    private readonly Voiture _result = new Voiture(0); // this is only a holding result so key 0
        private readonly List<float> _putativePointsScheme;
        private readonly List<int> _chosenDrivers;
        private readonly DateTime _raceDate;
        private int _putativeRacePosition;
        internal CarResultWindow(Voiture car, List<float> scheme, List<int> chosenDrivers, DateTime racedate)
        {
            InitializeComponent();
            _result.Specification = car.Specification;
            _putativePointsScheme = scheme;
            _chosenDrivers = chosenDrivers;
            _raceDate = racedate;
        }

        internal Voiture Result { get { return _result; } }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Window pop = Owner;
            Left = pop.Left + 4;
            Top = pop.Top + ((pop.ActualHeight - ActualHeight) / 2);
            RaceMeeting rm = Core.Instance.Races[_result.RaceMeetingKey];
            string tit = Core.Instance.RaceTitles[rm.RaceTitleKey].Caption;
            string dat = rm.RaceDate.ToLongDateString();
            string vnu = Core.Instance.RaceTracks[rm.CircuitKey].CircuitSingleTitle(0);
            RaceTitleTextBlock.Text = $"{tit} {dat} at {vnu}";
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            CloseMethod();
        }

        private void CloseMethod()
        {
            if (_result.ConstructorKey < 1) { _ = MessageBox.Show("Please select a constructor", "Race result", MessageBoxButton.OK, MessageBoxImage.Asterisk); return; }
            if (_result.DriverKey(0) < 1) { MessageBox.Show("Please select a driver", "Race result", MessageBoxButton.OK, MessageBoxImage.Asterisk); return; }
            
            if (GridPosnComboBox.SelectedIndex >= 0)
            {
                ComboBoxItem i = (ComboBoxItem)GridPosnComboBox.SelectedItem;
                _result.GridPosition = (int)i.Tag;
            }
            else
            {
                MessageBox.Show("Please enter a grid position", "Race result", MessageBoxButton.OK, MessageBoxImage.Asterisk); return;
            }

            if (RacePosnComboBox.SelectedIndex >= 0)
            {
                ComboBoxItem i = (ComboBoxItem)RacePosnComboBox.SelectedItem;
                _result.RacePosition = (int)i.Tag;
            }
            else
            {
                MessageBox.Show("Please enter a race position", "Race result", MessageBoxButton.OK, MessageBoxImage.Asterisk); return;
            }

            bool pointserror = false;

            string ptss = RacePoints0ComboBox.Text;
            Tuple<int, int> val = RacePoints.Interpret(ptss);
            if (val.Item2 == 0) { pointserror = true; } else { _result.SetPointsForPosition(0, new RacePoints(val)); }

            ptss = RacePoints1ComboBox.Text;
            val = RacePoints.Interpret(ptss);
            if (val.Item2 == 0) { pointserror = true; } else { _result.SetPointsForPosition(1, new RacePoints(val)); }

            ptss = RacePoints2ComboBox.Text;
            val = RacePoints.Interpret(ptss);
            if (val.Item2 == 0) { pointserror = true; } else { _result.SetPointsForPosition(2, new RacePoints(val)); }

            ptss=SprintPoints0ComboBox.Text;    
            val=RacePoints.Interpret(ptss);
            if (val.Item2 == 0) { pointserror=true; } else { _result.SetPointsForSprintQualifying(0,new RacePoints(val)); }

            ptss = SprintPoints1ComboBox.Text;
            val = RacePoints.Interpret(ptss);
            if (val.Item2 == 0) { pointserror = true; } else { _result.SetPointsForSprintQualifying(1, new RacePoints(val)); }

            ptss = SprintPoints2ComboBox.Text;
            val = RacePoints.Interpret(ptss);
            if (val.Item2 == 0) { pointserror = true; } else { _result.SetPointsForSprintQualifying(2, new RacePoints(val)); }

            ptss = FLapPoints0ComboBox.Text;
            val = RacePoints.Interpret(ptss);
            if (val.Item2 == 0) { pointserror = true; } else { _result.SetPointsForFastestLap(0, new RacePoints(val)); }

            ptss = FLapPoints1ComboBox.Text;
            val = RacePoints.Interpret(ptss);
            if (val.Item2 == 0) { pointserror = true; } else { _result.SetPointsForFastestLap(1, new RacePoints(val)); }

            ptss = FLapPoints2ComboBox.Text;
            val = RacePoints.Interpret(ptss);
            if (val.Item2 == 0) { pointserror = true; } else { _result.SetPointsForFastestLap(2, new RacePoints(val)); }

            if (pointserror) { MessageBox.Show("The points have not been entered correctly", Jbh.AppManager.AppName, MessageBoxButton.OK, MessageBoxImage.Asterisk); return; }

            _result.Formula2 = Formula2CheckBox.IsChecked.Value;
            _result.ConstructorPointsDisallowed = ConstructorDisallowedCheckBox.IsChecked.Value;
            _result.Controversial = ControversyCheckBox.IsChecked.Value;
            if (int.TryParse( ConstructorPointsFinedTextBox.Text, out int fine))
            {
                _result.ConstructorPointsPenalty = fine;
            }
            
            DialogResult = true;
        }

        private void Driver0Button_Click(object sender, RoutedEventArgs e)
        {
            PickFirstDriver();
        }

        private void PickFirstDriver()
        {
            DriverPickerWindow w = new DriverPickerWindow(_result.RaceDate, _chosenDrivers, _putativeRacePosition) { Owner = this };
            bool? q = w.ShowDialog();
            if (!q.HasValue) { return; }
            if (!q.Value) { return; }
            int kd = w.SelectedDriverKey;
            int kc = w.SelectedTeamKey;
            Driver d = Core.Instance.Drivers[kd];
            DriverNameTextBlock.Text = d.FullName;
            Driver1Label.Content = d.Surname;
            _result.SetDriverKey(0, kd);
            Driver2NamePanel.Visibility = Visibility.Visible;
            if (kc < 1) { PickConstructor(kd); } else
            {
                NamedItem c = Core.Instance.Constructors[kc];
                ConstructorNameTextBlock.Text =Core.ConstructorName( c.Caption);
                _result.ConstructorKey = kc;
            }
        }

        private void Driver1Button_Click(object sender, RoutedEventArgs e)
        {
            DriverPickerWindow w = new DriverPickerWindow(_result.RaceDate, _chosenDrivers, _putativeRacePosition) { Owner = this };
            bool? q = w.ShowDialog();
            if (!q.HasValue) { return; }
            if (!q.Value) { return; }
            int x = w.SelectedDriverKey;
            Driver d = Core.Instance.Drivers[x];
            Driver2NameTextBlock.Text = d.FullName;
            Driver2Label.Content = d.Surname;
            _result.SetDriverKey(1, x);
            Driver2PointsPanel.Visibility = Visibility.Visible;
            Driver3NamePanel.Visibility = Visibility.Visible;
        }

        private void Driver2Button_Click(object sender, RoutedEventArgs e)
        {
            DriverPickerWindow w = new DriverPickerWindow(_result.RaceDate, _chosenDrivers, _putativeRacePosition) { Owner = this };
            bool? q = w.ShowDialog();
            if (!q.HasValue) { return; }
            if (!q.Value) { return; }
            int x = w.SelectedDriverKey;
            Driver d = Core.Instance.Drivers[x];
            Driver3NameTextBlock.Text = d.FullName;
            Driver3Label.Content = d.Surname;
            _result.SetDriverKey(2, x);
            Driver3PointsPanel.Visibility = Visibility.Visible;
        }

        private void ConstructorButton_Click(object sender, RoutedEventArgs e)
        {
            PickConstructor(_result.DriverKey(0));
        }

        private void PickConstructor(int pilot)
        {
            int LikelyTeam = 0;
            if (pilot > 0) { LikelyTeam = Core.Instance.MostRecentConstructorForDriver(pilot, _raceDate); }
            ConstructorPickerWindow w = new ConstructorPickerWindow(_result.RaceDate, LikelyTeam) { Owner = this };
            bool? q = w.ShowDialog();
            if (!q.HasValue) { return; }
            if (!q.Value) { return; }
            int x = w.SelectedKey;
            NamedItem c = Core.Instance.Constructors[x];
            ConstructorNameTextBlock.Text =Core.ConstructorName( c.Caption);
            _result.ConstructorKey = x;
        }

        private void FillComboBoxes()
        {
            List<int> gridUsed = new List<int>();
            if (_result.DriverKey(0) <1)// not a pre-existing result being edited
            {
                gridUsed = Core.Instance.GridFilledPlaces(_result.RaceMeetingKey);
            }
            int top = (gridUsed.Count > 0) ? gridUsed.Max() : 0;
           
            GridPosnComboBox.Items.Clear();

            int buttoncount = 0;
            Brush b = Brushes.Maroon;
            FontWeight w= FontWeights.Medium;
            TextBlock t = new TextBlock() { Text ="Pit Lane", Foreground = b, FontWeight = w };
            ComboBoxItem c = new ComboBoxItem() { Content = t, Tag = Core.SpecialNumber };
            GridPosnComboBox.Items.Add(c);

            for (int n = 1; n <= Core.MaxEntrants; n++)
            {
                if (!gridUsed.Contains(n))
                {
                    b = (n < top) ? Brushes.Blue : Brushes.Black;
                    w = (n < top) ? FontWeights.Bold : FontWeights.Normal;
                    t = new TextBlock() { Text = n.ToString(Core.CultureUk), Foreground = b, FontWeight = w };
                    c = new ComboBoxItem() { Content = t, Tag = n };
                    GridPosnComboBox.Items.Add(c);

                    if (n <= Core.MaxExpectedGrid)
                    {
                        Button bouton = new Button() { VerticalAlignment = VerticalAlignment.Center, Content = n.ToString(Core.CultureUk), Tag = buttoncount + 1, Width = 40, Margin = new Thickness(4, 0, 0, 0), Foreground = b,Background=Brushes.Ivory, FontWeight = w };
                        bouton.Click += GridPositionButton_Click;
                        if (buttoncount > 11) { GridPosnStackPanel2.Children.Add(bouton); } else { GridPosnStackPanel.Children.Add(bouton); }
                        buttoncount++;
                    }
                }
            }

            List<int> raceVacancies = Core.Instance.RaceVacancies(_result.RaceMeetingKey);
            // allow pre-existing position if editing existing car
            if ((_result.RacePosition > 0)&& (_result.RacePosition < 1000))
            {
                if (!raceVacancies.Contains(_result.RacePosition)){ raceVacancies.Add(_result.RacePosition); raceVacancies.Sort(); }
            }
           
            RacePosnComboBox.Items.Clear();

            AddRaceResultEntry((int)Core.RaceResultConstants.DidNotStart);
            AddRaceResultEntry((int)Core.RaceResultConstants.RetMech);
            AddRaceResultEntry((int)Core.RaceResultConstants.DriverHealth);
            AddRaceResultEntry((int)Core.RaceResultConstants.RetFuel);
            AddRaceResultEntry((int)Core.RaceResultConstants.RetAccident);
            AddRaceResultEntry((int)Core.RaceResultConstants.RetFatalDriver);
            AddRaceResultEntry((int)Core.RaceResultConstants.RetFatalOthers);
            AddRaceResultEntry((int)Core.RaceResultConstants.RetUnexplained);
            AddRaceResultEntry((int)Core.RaceResultConstants.Unclassified);
            AddRaceResultEntry((int)Core.RaceResultConstants.Disqualified);

            _putativeRacePosition = raceVacancies[0];
            PositionButton.Content = _putativeRacePosition.ToString(Core.CultureUk);
            PositionGoButton.Content = $"{_putativeRacePosition} ->";
            if (raceVacancies[0] < _putativePointsScheme.Count) { PositionGoButton.Visibility= Visibility.Hidden; } // we don't want the user to click the 'Go' button without reviewing the points allocation (if the position is likely to merit points)
            foreach (int n in raceVacancies)
            {
                AddRaceResultEntry(n);
            }

            FillPointsBox(RacePoints0ComboBox);
            FillPointsBox(RacePoints1ComboBox);
            FillPointsBox(RacePoints2ComboBox);
            FillSprintQualifyingPointsBox(SprintPoints0ComboBox);
            FillSprintQualifyingPointsBox(SprintPoints1ComboBox);
            FillSprintQualifyingPointsBox(SprintPoints2ComboBox);
            FillLapPointsBox(FLapPoints0ComboBox);
            FillLapPointsBox(FLapPoints1ComboBox);
            FillLapPointsBox(FLapPoints2ComboBox);
        }

        private void AddRaceResultEntry(int n)
        {
            string qs = Core.RaceResultDescription(n);
            TextBlock t = new TextBlock() { Text = qs };
            ComboBoxItem c = new ComboBoxItem() { Content = t, Tag = n };
            RacePosnComboBox.Items.Add(c);
        }

        private static void FillPointsBox(ComboBox cbx)
        {
            List<string> range = Core.Instance.PointsValuesPosition();
            foreach (string z in range)
            {
                TextBlock t = new TextBlock() { Text = z };
                if (!z.Contains("/")) { t.FontWeight = FontWeights.Bold; }
                ComboBoxItem i = new ComboBoxItem() { Content = t };
                cbx.Items.Add(i);
            }
            cbx.SelectedIndex = 0;
        }

        private static void FillSprintQualifyingPointsBox(ComboBox cbx)
        {
            List<string> range = Core.Instance.PointsValuesSprintQualifying();
            foreach (string z in range)
            {
                TextBlock t = new TextBlock() { Text = z };
                if (!z.Contains("/")) { t.FontWeight = FontWeights.Bold; }
                ComboBoxItem i = new ComboBoxItem() { Content = t };
                cbx.Items.Add(i);
            }
            cbx.SelectedIndex = 0;
        }

        private static void FillLapPointsBox(ComboBox cbx)
        {
            List<string> range = Core.Instance.PointsValuesLap();
            foreach (string z in range)
            {
                TextBlock t = new TextBlock() { Text = z };
                if (!z.Contains("/")) { t.FontWeight = FontWeights.Bold; }
                ComboBoxItem i = new ComboBoxItem() { Content = t };
                cbx.Items.Add(i);
            }
            cbx.SelectedIndex = 0;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DriverNameTextBlock.Text = string.Empty;
            Driver2NameTextBlock.Text = string.Empty;
            Driver3NameTextBlock.Text = string.Empty;
            ConstructorNameTextBlock.Text = string.Empty;
            Driver2NamePanel.Visibility = Visibility.Hidden;
            Driver2PointsPanel.Visibility = Visibility.Hidden;
            Driver3NamePanel.Visibility = Visibility.Hidden;
            Driver3PointsPanel.Visibility = Visibility.Hidden;
            FillComboBoxes();
            if (_result.DriverKey(0) < 1)
            {
                PickFirstDriver();
            }
            else
            {
                Populate();
            }
            
        }

        private void RetireMechanicalButton_Click(object sender, RoutedEventArgs e)
        {
            RacePosnComboBox.SelectedIndex = 1;
            CloseMethod();
        }

        private void RetireAccidentButton_Click(object sender, RoutedEventArgs e)
        {
            RacePosnComboBox.SelectedIndex = 4;
            CloseMethod();
        }

        private void PositionGoButton_Click(object sender, RoutedEventArgs e)
        {
            RacePosnComboBox.SelectedIndex = 10;
            CloseMethod();
        }

        private void PositionButton_Click(object sender, RoutedEventArgs e)
        {
            RacePosnComboBox.SelectedIndex = 10;
            PositionButton.IsEnabled = false;
            PositionButton.Content = string.Empty;
        }

        private void GridPositionButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            int t = (int)(b.Tag);
            GridPosnComboBox.SelectedIndex = t;
            b.IsEnabled = false; b.Content = string.Empty;// just to give visible confirmation of click
        }

        private void Populate()
        {
            DriverNameTextBlock.Text = Core.Instance.Drivers[_result.DriverKey(0)].FullName;
            if (_result.DriverCount > 1)
            {
                Driver2NameTextBlock.Text = Core.Instance.Drivers[_result.DriverKey(1)].FullName;
                Driver2NamePanel.Visibility = Visibility.Visible;
                Driver2PointsPanel.Visibility = Visibility.Visible;
            }
            if (_result.DriverCount > 2)
            {
                Driver3NameTextBlock.Text = Core.Instance.Drivers[_result.DriverKey(2)].FullName;
                Driver3NamePanel.Visibility = Visibility.Visible;
                Driver3PointsPanel.Visibility = Visibility.Visible;
            }
            ConstructorNameTextBlock.Text =Core.ConstructorName( Core.Instance.Constructors[_result.ConstructorKey].Caption);

            Formula2CheckBox.IsChecked = _result.Formula2;
            ConstructorDisallowedCheckBox.IsChecked = _result.ConstructorPointsDisallowed;
            ControversyCheckBox.IsChecked = _result.Controversial;
            ConstructorPointsFinedTextBox.Text = _result.ConstructorPointsPenalty.ToString(System.Globalization.CultureInfo.CurrentCulture);
            // set grid posn in combobox
            int targetIndex = -1;
            for (int i = 0; i < GridPosnComboBox.Items.Count; i++)
            {
                ComboBoxItem cbi = GridPosnComboBox.Items[i] as ComboBoxItem;
                int pvalue = (int)cbi.Tag;
                if (pvalue == _result.GridPosition) { targetIndex = i; }
            }
            if (targetIndex >= 0) { GridPosnComboBox.SelectedIndex = targetIndex; }

            // set race posn in combobox
            targetIndex = -1;
            for (int i = 0; i < RacePosnComboBox.Items.Count; i++)
            {
                ComboBoxItem cbi = RacePosnComboBox.Items[i] as ComboBoxItem;
                int pvalue = (int)cbi.Tag;
                if (pvalue == _result.RacePosition) { targetIndex = i; }
            }
            if (targetIndex >= 0) { RacePosnComboBox.SelectedIndex = targetIndex; }

            // points

            RacePoints rp = _result.PointsForPosition(0);
            RacePoints0ComboBox.Text=rp.Representation;

            rp = _result.PointsForPosition(1);
            RacePoints1ComboBox.Text = rp.Representation;

            rp = _result.PointsForPosition(2);
            RacePoints2ComboBox.Text = rp.Representation;

            rp=_result.PointsForSprintQualifying(0);
            SprintPoints0ComboBox.Text=rp.Representation;

            rp = _result.PointsForSprintQualifying(1);
            SprintPoints1ComboBox.Text = rp.Representation;

            rp = _result.PointsForSprintQualifying(2);
            SprintPoints2ComboBox.Text = rp.Representation;

            rp = _result.PointsForFastestLap(0);
            FLapPoints0ComboBox.Text = rp.Representation;

            rp = _result.PointsForFastestLap(1);
            FLapPoints1ComboBox.Text = rp.Representation;

            rp = _result.PointsForFastestLap(2);
            FLapPoints2ComboBox.Text = rp.Representation;
           
        }

        private void RacePosnComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RacePosnComboBox.SelectedIndex >= 0)
            {
                ComboBoxItem i = (ComboBoxItem)RacePosnComboBox.SelectedItem;
                int position = (int)i.Tag;
                float pointsindividual = 0;
                if (position < _putativePointsScheme.Count)
                {
                    pointsindividual = _putativePointsScheme[position] / _result.DriverCount;
                }
                if (RacePoints0ComboBox.Text == "0") { RacePoints0ComboBox.Text = pointsindividual.ToString(Core.CultureUk); }
                if (_result.DriverCount > 1)
                {
                    if (RacePoints1ComboBox.Text == "0") { RacePoints1ComboBox.Text = pointsindividual.ToString(Core.CultureUk); }
                    if (_result.DriverCount > 2)
                    {
                        if (RacePoints2ComboBox.Text == "0") { RacePoints2ComboBox.Text = pointsindividual.ToString(Core.CultureUk); }
                    }
                }
            }
        }

        // INFO as the rules for awarding points have changed so often over the years with so may variable factors e.g. whether points are awarded for shared drives, whether a driver can score points for more than one car in a race, whether a driver retains his position while being disallowed his points for a technical violation etc. there are so many variations that it is impossible to try to automate them all, so we record the actual driver and constructor points awarded. However a suggestion is made based on the scoring system applied in the previous race.
}