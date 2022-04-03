using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Formula1;

public partial class CircuitPickerWindow : Window
{
    public CircuitPickerWindow(int gpk)
        {
            InitializeComponent();
            _grandprixkey = gpk;
        }

        private readonly int _grandprixkey;
        private int _selectedKey;
        private bool _loaded = false;

        internal int SelectedKey { get => _selectedKey; }

        private void InitialFillList()
        {
            ItemListBox.Items.Clear();
            if (_grandprixkey > 0)
            {
                GPHostedTextBlock.Text = $"Has hosted {Core.Instance.RaceTitles[_grandprixkey].Caption}";
            }
            else
            {
                GPHostedTextBlock.Text = string.Empty;
            }       
            List<int> tracks = Core.Instance.CircuitKeysForGrandPrix(_grandprixkey);
            List<Tuple<string, int>> CircuitNameList = new List<Tuple<string, int>>();
            foreach (int trak in tracks)
            {
                Circuit it = Core.Instance.RaceTracks[trak];
                    int w = it.CircuitTitleCount;
                    for (int v = 0; v < w; v++)
                    {
                        CircuitNameList.Add(new Tuple<string, int>(it.CircuitSingleTitle(v), it.Key));
                    }
            }
            CircuitNameList.Sort();
            foreach (Tuple<string, int> nm in CircuitNameList)
            {
                TextBlock tb = new TextBlock() { Text = nm.Item1 };
                ListBoxItem bi = new ListBoxItem() { Content = tb, Tag = nm.Item2 };
                ItemListBox.Items.Add(bi);
            }
        }

        private void ReFillList(int CountryKey)
        {
            GPHostedTextBlock.Text = string.Empty;
            ItemListBox.Items.Clear();
            List<Circuit> list = Core.Instance.RaceTracks.Values.ToList();
            List<Tuple<string, int>> CircuitNameList = new List<Tuple<string, int>>();
            foreach (Circuit it in list)
            {
                if ((CountryKey == 0) || (CountryKey == it.CountryKey))
                {
                    int w = it.CircuitTitleCount;
                    for (int v = 0; v < w; v++)
                    {
                        CircuitNameList.Add(new Tuple<string, int>(it.CircuitSingleTitle(v), it.Key));
                    }
                }
            }
            CircuitNameList.Sort();
            foreach (Tuple<string, int> nm in CircuitNameList)
            {
                TextBlock tb = new TextBlock() { Text = nm.Item1 };
                ListBoxItem bi = new ListBoxItem() { Content = tb, Tag = nm.Item2 };
                ItemListBox.Items.Add(bi);
            }
        }

        private void FillCountryCombo()
        {
            PaysComboBox.Items.Clear();
            PaysComboBox.Items.Add(new ComboBoxItem() { Tag = 0, Content = new TextBlock() { Text = "Not selected" } });
            List<NamedItem> pays = Core.Instance.Countries.Values.ToList();
            pays.Sort();
            foreach(NamedItem ni in pays)
            {
                PaysComboBox.Items.Add(new ComboBoxItem() { Tag = ni.Key, Content = new TextBlock() { Text =ni.Caption } });
            }
            PaysComboBox.SelectedIndex = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillCountryCombo();
            _loaded = true;
            InitialFillList();
        }

        private void PaysComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loaded) { return; }
            ComboBoxItem cbi = PaysComboBox.SelectedItem as ComboBoxItem;
            int i = (int)cbi.Tag;
            ReFillList(i);
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            CircuitEditorWindow cew = new CircuitEditorWindow() { Owner = this };
            bool? ans = cew.ShowDialog();
            if (ans.HasValue && ans.Value)
            {
                int clef = 1;
                while (Core.Instance.RaceTracks.ContainsKey(clef)) { clef++; }
                Circuit c = new Circuit() { Specification = cew.CircuitSpecification };
                c.Key = clef;
                Core.Instance.RaceTracks.Add(clef, c);
                _selectedKey = clef;
                DialogResult = true;
            }
        }

        private void ItemListBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ItemListBox.SelectedIndex < 0) { return; }
            ListBoxItem itm = ItemListBox.SelectedItem as ListBoxItem;
            _selectedKey = (int)itm.Tag;
            CircuitEditorWindow cew = new CircuitEditorWindow(Core.Instance.RaceTracks[_selectedKey].Specification) { Owner = this };
            bool? ans = cew.ShowDialog();
            if (ans.HasValue && ans.Value)
            {
                Circuit c = new Circuit() { Specification = cew.CircuitSpecification };
                Core.Instance.RaceTracks[c.Key].Specification = cew.CircuitSpecification;
                ReFillList(0);
            }
        }

        private void ItemListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ItemListBox.SelectedIndex >= 0)
            {
                ListBoxItem itm = ItemListBox.SelectedItem as ListBoxItem;
                _selectedKey = (int)itm.Tag;
                DialogResult = true;
            }
        }
}