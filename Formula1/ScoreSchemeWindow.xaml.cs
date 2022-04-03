using System;
using System.Windows;

namespace Formula1;

public partial class ScoreSchemeWindow : Window
{
    private bool _loaded = false;

        private SeasonPointsAllocationScheme _scheme;

        internal SeasonPointsAllocationScheme Scheme { get => _scheme; }

        internal ScoreSchemeWindow(SeasonPointsAllocationScheme scheme, string cship)
        {
            InitializeComponent();
            _scheme = new SeasonPointsAllocationScheme(scheme.Specification);
            RubricTextBlock.Text = cship;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            _loaded = true;
            OneGroupBorder.Visibility = Visibility.Collapsed;
            TwoGroupBorder.Visibility = Visibility.Collapsed;
            if (_scheme.FirstBlockQuota ==Core.SpecialNumber)
            {
                RadioNone.IsChecked = true;
            }
            else if (_scheme.FirstBlockQuota == 0)
            {
                RadioAll.IsChecked = true;
            }
            else if (_scheme.LastBlockQuota == 0)
            {
                RadioOne.IsChecked = true;
                OneBlockQuotaTextBox.Text = _scheme.FirstBlockQuota.ToString(Core.CultureUk);
            }
            else
            {
                RadioTwo.IsChecked = true;
                TwoBlockFirstQuotaTextBox.Text=_scheme.FirstBlockQuota.ToString(Core.CultureUk);
                TwoBlockFirstGroupTextBox.Text=_scheme.FirstBlockSize.ToString(Core.CultureUk);
                TwoBlockLastQuotaTextBox.Text = _scheme.LastBlockQuota.ToString(Core.CultureUk);
            }
        }

        private void Radio_Checked(object sender, RoutedEventArgs e)
        {
            if (!_loaded) { return; }
            if (RadioNone.IsChecked.Value)
            {
                OneGroupBorder.Visibility = Visibility.Collapsed;
                TwoGroupBorder.Visibility = Visibility.Collapsed;
            }
            else if (RadioAll.IsChecked.Value)
            {
                OneGroupBorder.Visibility = Visibility.Collapsed;
                TwoGroupBorder.Visibility = Visibility.Collapsed;
            }
            else if (RadioOne.IsChecked.Value)
            {
                OneGroupBorder.Visibility = Visibility.Visible;
                TwoGroupBorder.Visibility = Visibility.Collapsed;
            }
            else if (RadioTwo.IsChecked.Value)
            {
                OneGroupBorder.Visibility = Visibility.Collapsed;
                TwoGroupBorder.Visibility = Visibility.Visible;
            }
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            
            bool mistake = false;
            if (RadioNone.IsChecked.Value)
            {
                _scheme = new SeasonPointsAllocationScheme(0, Core.SpecialNumber, 0);
            }
            else if (RadioAll.IsChecked.Value)
            {
                _scheme = new SeasonPointsAllocationScheme(0, 0, 0);
            }
            else if (RadioOne.IsChecked.Value)
            {
                string t = OneBlockQuotaTextBox.Text;
                if (int.TryParse(t, out int v))
                {
                    _scheme = new SeasonPointsAllocationScheme(0, v, 0);
                }
                else { mistake = true; }
            }
            else if (RadioTwo.IsChecked.Value)
            {
                string ss =TwoBlockFirstGroupTextBox.Text;
                string q1s = TwoBlockFirstQuotaTextBox.Text;
                string q2s = TwoBlockLastQuotaTextBox.Text;
                if (int.TryParse(ss, out int s))
                {
                    if (int.TryParse(q1s, out int q1))
                    {
                        if (int.TryParse(q2s, out int q2))
                        {
                            _scheme = new SeasonPointsAllocationScheme(s,q1, q2);
                        }
                        else { mistake = true; }
                    }
                    else { mistake = true; }
                }
                else { mistake = true; }
            }
            if (mistake) { MessageBox.Show("You need to enter all required integer values", Jbh.AppManager.AppName, MessageBoxButton.OK, MessageBoxImage.Asterisk); return; }
            DialogResult = true;
        }
}
