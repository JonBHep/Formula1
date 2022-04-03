using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Formula1;

public partial class DriversAliveWindow : Window
{
    private bool _returnDialogValue = false;
        public DriversAliveWindow()
        {
            InitializeComponent();
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(VivantListBox.SelectedItem is ListBoxItem item)) { return; }
            int i =(int) item.Tag;
            DriverPropertiesWindow w = new DriverPropertiesWindow(i) { Owner = this };
            bool? q = w.ShowDialog();
            if (q.HasValue && q.Value) { FillList();_returnDialogValue = true; }
        }

        private void WikiButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(VivantListBox.SelectedItem is ListBoxItem item)) { return; }
            int i = (int)item.Tag;
            System.Diagnostics.Process.Start(Core.Instance.Drivers[i].WikiLinkString);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            FillList();
        }

        private void FillList()
        {
            DetailsButton.IsEnabled = false;
            WikiButton.IsEnabled = false;
            VivantListBox.Items.Clear();
            List<int> viveurs = Core.Instance.LivingDrivers(DateTime.Today);
            // Create List of drivers with ages, oldest first
            List<Tuple<double, int>> conducteurs = new List<Tuple<double, int>>();
            foreach (int a in viveurs)
            {
                double dur = (DateTime.Today - Core.Instance.Drivers[a].BirthDate).TotalDays;
                conducteurs.Add(new Tuple<double, int>(dur, a));
            }
            conducteurs.Sort();
            conducteurs.Reverse();
            foreach (Tuple<double, int> it in conducteurs)
            {
                Driver q = Core.Instance.Drivers[it.Item2];
                StackPanel spl = new StackPanel() { Orientation = Orientation.Horizontal };
                TextBlock tb = new TextBlock() { Text = q.Forenames };
                spl.Children.Add(tb);
                tb = new TextBlock() { Text = q.Surname, FontWeight = FontWeights.Medium, Margin = new Thickness(4, 0, 0, 0) };
                spl.Children.Add(tb);
                tb = new TextBlock() { Text = q.BirthDate.ToShortDateString(), FontWeight = FontWeights.Normal, Margin = new Thickness(4, 0, 0, 0), Foreground = Brushes.SeaGreen };
                spl.Children.Add(tb);
                tb = new TextBlock() { Text = q.DisplayAge(), FontWeight = FontWeights.Medium, Margin = new Thickness(4, 0, 0, 0), Foreground = Brushes.DarkSeaGreen };
                spl.Children.Add(tb);
                ListBoxItem bi = new ListBoxItem() { Content = spl, Tag = q.Key };
                VivantListBox.Items.Add(bi);
            }
        }

        private void VivantListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int p = VivantListBox.SelectedIndex;
            if (p >= 0)
            {
                DetailsButton.IsEnabled = true;
                WikiButton.IsEnabled = true;
            }
            else
            {
                DetailsButton.IsEnabled = false;
                WikiButton.IsEnabled = false;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = _returnDialogValue;
        }
}