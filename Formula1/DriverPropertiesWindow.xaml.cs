using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Formula1;

public partial class DriverPropertiesWindow : Window
{
    public DriverPropertiesWindow(int driverKey)
        {
            InitializeComponent();
            _driverKey = driverKey;
        }

        private readonly int _driverKey;

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            GlyphBlockThree.Text= GlyphBlockTwo.Text= GlyphBlockOne.Text = Core.GlyphFatalAccident;

            FillCountryComboBox();
            KeyTextBlock.Text = _driverKey.ToString(Core.CultureUk);
            Driver d = Core.Instance.Drivers[_driverKey];
            SurnameBox.Text = d.Surname;
            ForenameBox.Text = d.Forenames;
            int target = -1;
            for (int a = 0; a < CountriesComboBox.Items.Count; a++)
            {
                ComboBoxItem b = (ComboBoxItem)CountriesComboBox.Items[a];
                int c = (int)b.Tag;
                if (c == d.CountryKey) { target = a; break; }
            }
            if (target >= 0) { CountriesComboBox.SelectedIndex = target; }
            if (d.BirthDate > Core.DateBase) { BirthDatePicker.SelectedDate = d.BirthDate; }
            if (d.CeiDate > Core.DateBase) { CeiDatePicker.SelectedDate = d.CeiDate; }
            if (d.DeathDate > Core.DateBase) { DeathDatePicker.SelectedDate = d.DeathDate; }
            DeathModeTextBox.Text = d.DeathMode;
            switch (d.HowDied)
            {
                case Core.CauseOfDeath.Natural: {RadioNatural.IsChecked = true; break; }
                case Core.CauseOfDeath.OtherAccident: {RadioAccident.IsChecked = true; break; }
                case Core.CauseOfDeath.RacePracticeOrTestingAccident: { RadioPractice.IsChecked = true; break; }
                case Core.CauseOfDeath.RacingAccident: { RadioRacing.IsChecked = true; break; }
                default: { RadioUnknown.IsChecked = true; break; }
            }
            BirthDatePicker.Focus();
        }

        private void FillCountryComboBox()
        {
            List<NamedItem> list = Core.Instance.Countries.Values.ToList();
            list.Sort();
            foreach (NamedItem it in list)
            {
                TextBlock tb = new TextBlock() { Text = it.Caption };
                ComboBoxItem bi = new ComboBoxItem() { Content = tb, Tag = it.Key };
                CountriesComboBox.Items.Add(bi);
            }
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            string sn = SurnameBox.Text.Trim();
            string fn = ForenameBox.Text.Trim();
            bool problem = false;
            if (string.IsNullOrEmpty(sn)) { problem = true; }
            if (string.IsNullOrEmpty(fn)) { problem = true; }
            if (CountriesComboBox.SelectedIndex < 0) { problem = true; }

            Core.CauseOfDeath how = Core.CauseOfDeath.Unknown;
            if (RadioAccident.IsChecked.Value)
            {
                how = Core.CauseOfDeath.OtherAccident;
            }
            else if (RadioNatural.IsChecked.Value)
            {
                how = Core.CauseOfDeath.Natural;
            }
            else if (RadioPractice.IsChecked.Value)
            {
                how = Core.CauseOfDeath.RacePracticeOrTestingAccident;
            }
            else if (RadioRacing.IsChecked.Value)
            {
                how = Core.CauseOfDeath.RacingAccident;
            }

            if (problem)
            {
                MessageBox.Show("Essential data is missing", Jbh.AppManager.AppName, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }

            DateTime bx;
            DateTime dx;
            DateTime ix;

            DateTime? bd = BirthDatePicker.SelectedDate;
            if (bd.HasValue)
            {
                bx = bd.Value;
            }
            else
            {
                bx = DateTime.MinValue;
            }
            DateTime? id = CeiDatePicker.SelectedDate;
            if (id.HasValue)
            {
                ix = id.Value;
            }
            else
            {
                ix = DateTime.MinValue;
            }
            DateTime? dd = DeathDatePicker.SelectedDate;
            if (dd.HasValue)
            {
                dx = dd.Value;
            }
            else
            {
                dx = DateTime.MinValue;
            }

            if ((how!= Core.CauseOfDeath.Unknown) && (dx < Core.DateBase)) { problem = true; }
            if ((how == Core.CauseOfDeath.Unknown) && (dx > Core.DateBase)) { problem = true; }
            if (problem)
            {
                MessageBox.Show("Death date and mode of death do not correspond", Jbh.AppManager.AppName, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }

            Driver d = Core.Instance.Drivers[_driverKey];

            ComboBoxItem cbi = (ComboBoxItem)CountriesComboBox.SelectedItem;
            int ck = (int)cbi.Tag;
            d.CountryKey = ck;

            d.Surname = sn;
            d.Forenames = fn;
            d.BirthDate = bx;
            d.CeiDate = ix;
            d.DeathDate = dx;

            string dm = DeathModeTextBox.Text.Trim();
            if (string.IsNullOrEmpty(dm))
            {
                if (d.DeathDate < Core.DateBase)
                {
                    dm = "Still alive as at " + DateTime.Today.ToShortDateString();
                }
                else
                {
                    dm = "Circumstances of death unknown";
                }
            }
            if (!string.IsNullOrEmpty( Driver.TextWorry(dm)))
            {
                MessageBox.Show("There are text faults", Jbh.AppManager.AppName, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            d.DeathMode = dm;
            d.HowDied = how;
            DialogResult = true;
        }

        private void VoidBirthButton_Click(object sender, RoutedEventArgs e)
        {
            BirthDatePicker.SelectedDate = null;
            DisplayAge();
        }

        private void VoidDeathButton_Click(object sender, RoutedEventArgs e)
        {
            DeathDatePicker.SelectedDate = null;
            DisplayAge();
        }

        private void VoidCeiButton_Click(object sender, RoutedEventArgs e)
        {
            CeiDatePicker.SelectedDate = null;
            DisplayAge();
        }
        private void DisplayAge()
        {
            DateTime? bd = BirthDatePicker.SelectedDate;
            DateTime? dd = DeathDatePicker.SelectedDate;
            DateTime? id = CeiDatePicker.SelectedDate;

            if (bd.HasValue)
            {
                if (id.HasValue)
                {
                    DateTime bv = bd.Value;
                    DateTime iv = id.Value;
                    int yrs = iv.Year - bv.Year;
                    if (iv.Month < bv.Month)
                    { yrs--; }
                    else if (iv.Month == bv.Month)
                    {
                        if (iv.Day < bv.Day) { yrs--; }
                    }
                    AgeCeiTextBlock.Text = $"Career ended by injury aged: {yrs}";
                }
                else
                {
                    AgeCeiTextBlock.Text = string.Empty;
                }
                if (dd.HasValue)
                {
                    AgeLiveTextBlock.Text = string.Empty;
                    DateTime bv = bd.Value;
                    DateTime dv = dd.Value;
                    int yrs = dv.Year - bv.Year;
                    if (dv.Month < bv.Month)
                    { yrs--; }
                    else if (dv.Month == bv.Month)
                    {
                        if (dv.Day < bv.Day) { yrs--; }
                    }
                    AgeDeadTextBlock.Text = $"Age at death: {yrs}";
                }
                else
                {
                    AgeDeadTextBlock.Text = string.Empty;
                    DateTime bv = bd.Value;
                    DateTime dv = DateTime.Today;
                    int yrs = dv.Year - bv.Year;
                    if (dv.Month < bv.Month)
                    { yrs--; }
                    else if (dv.Month == bv.Month)
                    {
                        if (dv.Day < bv.Day) { yrs--; }
                    }
                    AgeLiveTextBlock.Text = $"Age now: {yrs}";
                }
            }
            else
            {
                AgeLiveTextBlock.Text = string.Empty;
                AgeDeadTextBlock.Text = string.Empty;
            }
        }

        private void BirthDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DisplayAge();
        }
        private void CeiDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DisplayAge();
        }
        private void DeathDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DisplayAge();
        }

        private void WikiButton_Click(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Process.Start(Core.Instance.Drivers[_driverKey].WikiLinkString);
            Core.LaunchWebPage(Core.Instance.Drivers[_driverKey].WikiLinkString);
        }

        private void DeathModeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextQueriesTextBlock.Text =Driver.TextWorry(DeathModeTextBox.Text);
        }
}