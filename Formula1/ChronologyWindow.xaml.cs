using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Formula1;

public partial class ChronologyWindow : Window
{
    public ChronologyWindow()
    {
        InitializeComponent();
    }
    private struct ChronoEvent : IComparable<ChronoEvent>
        {
            internal DateTime When { get; private set; }
            internal string What { get; private set; }
            internal string Symbol { get; private set; }
            internal Brush PinceauText { get; private set; }
            internal Brush PinceauSymbol { get; private set; }
            internal ChronoEvent(DateTime eventDate, string eventText, string symbol, Brush textBrush, Brush symbolBrush)
            {
                When = eventDate;
                What = eventText;
                PinceauText = textBrush;
                PinceauSymbol = symbolBrush;
                Symbol = symbol;
            }

            int IComparable<ChronoEvent>.CompareTo(ChronoEvent other)
            {
                return this.When.CompareTo(other.When);
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            List<ChronoEvent> diary = new List<ChronoEvent>();
            foreach (RaceMeeting rm in Core.Instance.Races.Values)
            {
                ChronoEvent ce = new ChronoEvent(rm.RaceDate, $"{Core.Instance.RaceTitles[rm.RaceTitleKey].Caption} ({Core.Instance.RaceTracks[rm.CircuitKey].CircuitSingleTitle(0)})",string.Empty, Brushes.DarkGreen, Brushes.Black);
                diary.Add(ce);
            }
            foreach(Driver dr in Core.Instance.Drivers.Values)
            {
                if (dr.BirthDate > Core.DateBase)
                {
                    ChronoEvent ce = new ChronoEvent(dr.BirthDate, $"Birth of {dr.FullName}", string.Empty, Brushes.RoyalBlue, Brushes.Black);
                    diary.Add(ce);
                }
                if (dr.CeiDate > Core.DateBase)
                {
                    ChronoEvent ce = new ChronoEvent(dr.CeiDate, $"Career-ending injury to {dr.FullName}", string.Empty, Brushes.OrangeRed, Brushes.Black);
                    diary.Add(ce);
                }
                if (dr.DeathDate > Core.DateBase)
                {
                    string dethMark = string.Empty;
                    Brush bru =  Brushes.Black;
                    if (dr.HowDied == Core.CauseOfDeath.RacingAccident) { dethMark = "\x0085"; bru = Brushes.Red; }
                    if (dr.HowDied == Core.CauseOfDeath.RacePracticeOrTestingAccident) { dethMark = "\x0085"; bru = Brushes.Red; }
                    if (dr.HowDied == Core.CauseOfDeath.OtherAccident) { dethMark = "\x0085"; bru = Brushes.Black; }
                    ChronoEvent ce = new ChronoEvent(dr.DeathDate, $"Death of {dr.FullName}",dethMark, Brushes.Black, bru); ;
                    diary.Add(ce);
                }
            }
            diary.Sort();
            int cYear = 0;
            int cDayOfYear = 0;
            foreach (ChronoEvent ce in diary)
            {
                if (ce.When.Year != cYear)
                {
                    cYear = ce.When.Year;
                    cDayOfYear = 0;
                    ChronoListBox.Items.Add(MyLine(cYear));
                }
                if (cDayOfYear != ce.When.DayOfYear)
                {
                    ChronoListBox.Items.Add(MyLine(ce, false));
                    cDayOfYear = ce.When.DayOfYear;
                }
                else
                {
                    ChronoListBox.Items.Add(MyLine(ce, true));
                }
            }
            ChronoListBox.ScrollIntoView(ChronoListBox.Items[ChronoListBox.Items.Count - 1]);
        }

        private ListBoxItem MyLine(ChronoEvent ev, bool hideDate)
        {
            StackPanel pnl = new StackPanel() { Orientation = Orientation.Horizontal, Margin=new Thickness(54,0,0,0) };
            string datestring = (hideDate) ? "" : ev.When.ToString("MMM dd",Core.CultureUK);
            TextBlock bd = new TextBlock() { Text = datestring, Width = 84, Foreground = Brushes.DarkCyan, FontWeight=FontWeights.Bold };
            pnl.Children.Add(bd);
            TextBlock be = new TextBlock() { Text = ev.What, Foreground = ev.PinceauText };
            pnl.Children.Add(be);
            TextBlock bs = new TextBlock() { Margin=new Thickness(8,0,0,0), Text =ev.Symbol, FontFamily=new FontFamily("Webdings"),FontSize=16, Foreground = ev.PinceauSymbol };
            pnl.Children.Add(bs);
            ListBoxItem item = new ListBoxItem() { Content = pnl, IsHitTestVisible=false };
            return item;
        }
        private ListBoxItem MyLine(int yer)
        {
            TextBlock block = new TextBlock() { Text = yer.ToString(Core.CultureUK), Width = 54, Foreground = Brushes.DarkCyan, FontWeight= FontWeights.Black };
            ListBoxItem item = new ListBoxItem() { Content = block, IsHitTestVisible = false };
            return item;
        }

}