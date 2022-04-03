using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Formula1;

public partial class CircuitEditorWindow : Window
{
    private Circuit _circuit;
        public CircuitEditorWindow()
        {
            InitializeComponent();
            _circuit = new Circuit();
        }
        public CircuitEditorWindow(string circuitSpec)
        {
            InitializeComponent();
            _circuit = new Circuit() { Specification = circuitSpec };
        }

        public string CircuitSpecification { get { return _circuit.Specification; } }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (_circuit.Key > 0) // editing existing Circuit
            {
                RefreshDisplay();
            }
        }

        private void RefreshDisplay()
        {
            NamesListBox.Items.Clear();
            List<string> titles = _circuit.CircuitName;
            foreach (string n in titles)
            {
                TextBlock t = new TextBlock() { Text = n };
                ListBoxItem l = new ListBoxItem() { Content = t ,Tag=n, Foreground=Brushes.DarkOrchid};
                NamesListBox.Items.Add(l);
            }
            CountryTextBlock.Text =(_circuit.CountryKey>0) ?  Core.Instance.Countries[_circuit.CountryKey].Caption: string.Empty;
            SaveButton.IsEnabled = ((titles.Count > 0)&&(_circuit.CountryKey>0));
            DeleteNameButton.IsEnabled = PromoteNameButton.IsEnabled = false;
        }

        private void AddNameButton_Click(object sender, RoutedEventArgs e)
        {
            string appellation = CircuitNameTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(appellation))
            {
                _circuit.CircuitName.Add(appellation);
                RefreshDisplay();
            }
        }

        private void NamesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           DeleteNameButton.IsEnabled = (NamesListBox.SelectedIndex >= 0);
           PromoteNameButton.IsEnabled = (NamesListBox.SelectedIndex > 0);
            if (NamesListBox.SelectedIndex >= 0)
            {
                ListBoxItem itm = (ListBoxItem)NamesListBox.SelectedItem;
                CircuitNameTextBox.Text = itm.Tag.ToString();
            }
        }

        private void DeleteNameButton_Click(object sender, RoutedEventArgs e)
        {
            int q = NamesListBox.SelectedIndex;
            _circuit.CircuitName.RemoveAt(q);
            RefreshDisplay();
        }

        private void CountriesButton_Click(object sender, RoutedEventArgs e)
        {
            GenericPickerWindow w = new GenericPickerWindow(Core.Instance.Countries, "Countries") { Owner = this };
            bool? q = w.ShowDialog();
            if (q.HasValue && q.Value)
            {
                int p = w.SelectedKey;
                _circuit.CountryKey = p;
                RefreshDisplay();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void PromoteNameButton_Click(object sender, RoutedEventArgs e)
        {
            int q = NamesListBox.SelectedIndex;
            string ww = _circuit.CircuitName[q];
            _circuit.CircuitName.RemoveAt(q);
            _circuit.CircuitName.Insert(q - 1, ww);
            RefreshDisplay();
        }
}