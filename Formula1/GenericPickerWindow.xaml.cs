using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Formula1;

public partial class GenericPickerWindow : Window
{
    
    /// <summary>
        /// Used to pick from list or create new entries for Constructors, Countries and RaceTitles
        /// </summary>
        private readonly Dictionary<int, NamedItem> _dic;
        private int _selectedKey;

        public int SelectedKey { get => _selectedKey; }

        internal GenericPickerWindow(Dictionary<int, NamedItem> dic, string windowTitle)
        {
            InitializeComponent();
            _dic = dic;
            Title = windowTitle;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            FillList();
        }

        private void FillList()
        {
            AddTextBox.Clear();
            AddButton.IsEnabled = false;
            ItemListBox.Items.Clear();
            List<NamedItem> list = _dic.Values.ToList();
            list.Sort();
            foreach (NamedItem it in list)
            {
                TextBlock tb = new TextBlock() { Text = it.Caption };
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

        private void AddTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string entry = AddTextBox.Text.Trim();
            if (string.IsNullOrEmpty(entry))
            {
                AddButton.IsEnabled = false;
            }
            else
            {
                AddButton.IsEnabled = true;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string entry = AddTextBox.Text.Trim();
            // duplication check
            bool flag = false;
            foreach(NamedItem it in _dic.Values) { if (it.Caption.Equals(entry, StringComparison.OrdinalIgnoreCase)) { flag = true; } }
            if (flag)
            {
                MessageBox.Show("This name already exists", Jbh.AppManager.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            int newKey = 1;
            while (_dic.ContainsKey(newKey)) { newKey++; }
            NamedItem ni = new NamedItem(entry, newKey);
            _dic.Add(newKey, ni);
            _selectedKey = newKey;
            DialogResult = true;
        }
}