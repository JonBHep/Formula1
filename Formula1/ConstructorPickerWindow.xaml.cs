using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Formula1;

public partial class ConstructorPickerWindow : Window
{
    public ConstructorPickerWindow(DateTime CurrentDate, int Interesting)
        {
            InitializeComponent();
            _currentDate = CurrentDate;
            _interesting = Interesting;
        }

        private int _selectedKey;

        private readonly DateTime _currentDate; // used for selecting currently active teams
        private readonly int _interesting; // used to select a driver's most recent team

        public int SelectedKey { get => _selectedKey; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (_currentDate<Core.DateBase)
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
            NameTextBox.Clear();
            AddButton.IsEnabled = false;
            ItemListBox.Items.Clear();
            List<NamedItem> list = Core.Instance.Constructors.Values.ToList();
            Core.Instance.GetRecentConstructors(_currentDate, out List<int> c12, out List<int> c24);
            list.Sort();
            foreach (NamedItem it in list)
            {
                if (c24.Contains(it.Key))
                {
                    TextBlock tb = new TextBlock() { Text = it.Caption, FontWeight = FontWeights.Normal, Foreground=Brushes.DarkSlateGray, Margin=new Thickness(6,1,6,1) };
                    if (c12.Contains(it.Key)) { tb.FontWeight = FontWeights.Bold; tb.Foreground = Brushes.DarkSlateBlue; }
                    if (it.Key == _interesting) { tb.Background = Brushes.Yellow; }
                    ListBoxItem bi = new ListBoxItem() { Content = tb, Tag = it.Key };
                    ItemListBox.Items.Add(bi);
                }
            }
        }

        private void FillCompleteList()
        {
            NameTextBox.Clear();
            AddButton.IsEnabled = false;
            ItemListBox.Items.Clear();
            List<NamedItem> list = Core.Instance.Constructors.Values.ToList();
            list.Sort();
            foreach (NamedItem it in list)
            {
                TextBlock tb = new TextBlock() { Text = it.Caption, FontWeight=FontWeights.Medium};
                ListBoxItem bi = new ListBoxItem() { Content = tb, Tag = it.Key };
                ItemListBox.Items.Add(bi);
            }
        }

        private void ItemListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemListBox.SelectedIndex >= 0)
            {
                ListBoxItem itm = ItemListBox.SelectedItem as ListBoxItem;
                _selectedKey = (int)itm.Tag;
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
            if (string.IsNullOrWhiteSpace(NameTextBox.Text)) { fullinfo = false; }
            AddButton.IsEnabled = fullinfo;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string cname = NameTextBox.Text.Trim();
            // duplication check
            bool flag = false;
            foreach (NamedItem it in Core.Instance.Constructors.Values) { if (it.Caption.Equals(cname, StringComparison.OrdinalIgnoreCase)) { flag = true; } }
            if (flag)
            {
                MessageBox.Show("This name already exists", Jbh.AppManager.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            int newKey = 1;
            while (Core.Instance.Constructors.ContainsKey(newKey)) { newKey++; }
            NamedItem dr = new NamedItem(cname, newKey);
            Core.Instance.Constructors.Add(dr.Key, dr);
            _selectedKey = newKey;
            DialogResult = true;
        }

        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            NewEntryDock.Visibility = Visibility.Visible;
            CompleteButton.IsEnabled = false;
            FillCompleteList();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Height = Core.LowWindowHeight;
            Top = Core.LowWindowTop;
            Left = Owner.Left;
            if (_interesting > 0)
            {
                PickButton.IsEnabled = true;
                PickLabel.Text = Core.Instance.Constructors[_interesting].Caption;
            }
            else
            {
                PickButton.IsEnabled = false;
                PickLabel.Text = string.Empty;
            }
        }

        private void PickButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedKey = _interesting;
            DialogResult = true;
        }
}