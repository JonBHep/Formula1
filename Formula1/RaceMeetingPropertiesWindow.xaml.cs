using System;
using System.Windows;
using System.Windows.Controls;

namespace Formula1;

public partial class RaceMeetingPropertiesWindow : Window
{
    
    private readonly RaceMeeting _meeting;
        private bool _populate;
        internal RaceMeetingPropertiesWindow()
        {
            InitializeComponent();
            _meeting = new RaceMeeting();
            _populate = false;
        }
        internal RaceMeetingPropertiesWindow(string meetspec)
        {
            InitializeComponent();
            _meeting = new RaceMeeting();
            _meeting.Specification = meetspec;
            _populate = true;
        }

        internal string RaceMeetingSpecification { get => _meeting.Specification; }
        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            if ((_meeting.CircuitKey < 1) || (_meeting.RaceTitleKey < 1))
            {
                MessageBox.Show("Information is incomplete", "F1Stats", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (RaceDatePicker.SelectedDate.HasValue)
            {
                if (!_populate)
                {
                    DateTime Q = RaceDatePicker.SelectedDate.Value;
                    if (Core.Instance.AlreadyRaceOnDate(Q))
                    {
                        MessageBox.Show("There is already a race on that date", "F1Stats", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a date", "F1Stats", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _meeting.CountForConstructorsChampionship = ConstructorsCheckBox.IsChecked.Value;
            DialogResult = true;
        }

        private void TitleButton_Click(object sender, RoutedEventArgs e)
        {
            GenericPickerWindow w = new GenericPickerWindow(Core.Instance.RaceTitles, "Grands Prix") { Owner = this };
            bool? q = w.ShowDialog();
            if (q.HasValue && q.Value)
            {
                int k = w.SelectedKey;
                GrandPrixTextBlock.Text = Core.Instance.RaceTitles[k].Caption;
                _meeting.RaceTitleKey = k;
            }
        }

        private void VenueButton_Click(object sender, RoutedEventArgs e)
        {
            CircuitPickerWindow w = new CircuitPickerWindow(_meeting.RaceTitleKey) { Owner = this };
            bool? q = w.ShowDialog();
            if (q.HasValue && q.Value)
            {
                int k = w.SelectedKey;
                VenueTextBlock.Text = Core.Instance.RaceTracks[k].CircuitSingleTitle(0);
                _meeting.CircuitKey = k;
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RaceDatePicker.SelectedDate.HasValue)
            {
                _meeting.RaceDate = RaceDatePicker.SelectedDate.Value;
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if(_populate)
            {
                RaceDatePicker.SelectedDate = _meeting.RaceDate;
                GrandPrixTextBlock.Text= Core.Instance.RaceTitles[_meeting.RaceTitleKey].Caption;
                VenueTextBlock.Text= Core.Instance.RaceTracks[_meeting.CircuitKey].CircuitSingleTitle(0);
                ConstructorsCheckBox.IsChecked = _meeting.CountForConstructorsChampionship;
            }
            else
            {
                RaceDatePicker.SelectedDate = null;
                GrandPrixTextBlock.Text = string.Empty;
                VenueTextBlock.Text = string.Empty;
                ConstructorsCheckBox.IsChecked = true;
            }
            RaceDatePicker.Focus();
        }
}