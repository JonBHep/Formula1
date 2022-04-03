using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Formula1;

public partial class RipWindow : Window
{
    private readonly Dictionary<int, AnnualDeaths> _almanac = new Dictionary<int, AnnualDeaths>();
        public RipWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double scrX = System.Windows.SystemParameters.PrimaryScreenWidth;
            double scrY = System.Windows.SystemParameters.PrimaryScreenHeight;
            double winX = scrX * .98;
            double winY = scrY * .94;
            double Xm = (scrX - winX) / 2;
            double Ym = (scrY - winY) / 4;
            this.Width = winX;
            this.Height = winY;
            this.Left = Xm;
            this.Top = Ym;
        }

        class AnnualDeaths
        {
            private readonly List<int> _driverRacingAccidentKeys;
            private readonly List<int> _driverOtherAccidentKeys;
            private readonly List<int> _driverDiedKeys;
            internal AnnualDeaths()
            {
                _driverDiedKeys = new List<int>();
                _driverOtherAccidentKeys = new List<int>();
                _driverRacingAccidentKeys = new List<int>();
            }

            internal List<int> DriverRacingAccidentKeys => _driverRacingAccidentKeys;

            internal List<int> DriverOtherAccidentKeys => _driverOtherAccidentKeys;

            internal List<int> DriverDiedKeys => _driverDiedKeys;

            internal int RaceKills { get { return _driverRacingAccidentKeys.Count; } }
            internal int ElseKills { get { return _driverOtherAccidentKeys.Count; } }
            internal int BedDeaths { get { return _driverDiedKeys.Count; } }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            BuildList();
            DisplayList();
            DrawGraph();
        }

        private void BuildList()
        {
            foreach(int k in Core.Instance.Drivers.Keys)
            {
                Driver d = Core.Instance.Drivers[k];
                if (d.DeathDate > Core.DateBase)
                {
                    int y = d.DeathDate.Year;
                    if (!_almanac.ContainsKey(y))
                    { 
                        AnnualDeaths ad = new AnnualDeaths();
                        _almanac.Add(y, ad);
                    }
                    AnnualDeaths Tally = _almanac[y];
                    switch (Core.Instance.Drivers[k].HowDied)
                    {
                        case Core.CauseOfDeath.OtherAccident: { Tally.DriverOtherAccidentKeys.Add(k); break; }
                        case Core.CauseOfDeath.RacePracticeOrTestingAccident: { Tally.DriverRacingAccidentKeys.Add(k); break; }
                        case Core.CauseOfDeath.RacingAccident: { Tally.DriverRacingAccidentKeys.Add(k); break; }
                        default: { Tally.DriverDiedKeys.Add(k); break; }
                    }
                }
            }
        }

        private void DisplayList()
        {
            RipListBox.Items.Clear();
            for (int yr = 1950; yr <= DateTime.Today.Year; yr++)
            {
                if (_almanac.ContainsKey(yr))
                {
                    
                    TextBlock bloc = new TextBlock() { Text = yr.ToString(System.Globalization.CultureInfo.CurrentCulture), FontWeight=FontWeights.Bold, Margin=new Thickness(6,2,6,2) };
                    Border bord = new Border() { Child = bloc, BorderThickness=new Thickness(1,1,1,1), BorderBrush=Brushes.Black, CornerRadius=new CornerRadius(4,4,4,4) };
                    AnnualDeaths YearTally = _almanac[yr];
                    ListBoxItem item = new ListBoxItem() { Content=bord, IsHitTestVisible=false};
                    RipListBox.Items.Add(item);

                    List<Mort> deces = new List<Mort>();
                    foreach (int k in YearTally.DriverRacingAccidentKeys)
                    {
                        Driver d = Core.Instance.Drivers[k];
                        Mort Q = new Mort(quand: d.DeathDate, nom: d.FullName, how: 1, old: d.LatestAge());
                        deces.Add(Q);
                    }
                    foreach (int k in YearTally.DriverOtherAccidentKeys)
                    {
                        Driver d = Core.Instance.Drivers[k];
                        Mort Q = new Mort(quand: d.DeathDate, nom: d.FullName, how: 2, old: d.LatestAge());
                        deces.Add(Q);
                    }
                    foreach (int k in YearTally.DriverDiedKeys)
                    {
                        Driver d = Core.Instance.Drivers[k];
                        Mort Q = new Mort(quand: d.DeathDate, nom: d.FullName, how: 3, old: d.LatestAge());
                        deces.Add(Q);
                    }
                    deces.Sort();
                    foreach (Mort m in deces)
                    {
                        Brush pinceau = Brushes.SeaGreen;
                        if (m.Mode == 1) { pinceau = Brushes.Red; }
                        if (m.Mode == 2) { pinceau = Brushes.DarkRed; }
                        StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };
                        sp.Children.Add(new TextBlock() { Text = m.When.ToString("dd MMM" , System.Globalization.CultureInfo.CurrentCulture), MinWidth=48 });
                        sp.Children.Add(new TextBlock() { Text = m.Driv, FontWeight = FontWeights.Medium, Foreground = pinceau });
                        sp.Children.Add(new TextBlock() { Text = $" ({m.Age})" , Foreground=pinceau});
                        item = new ListBoxItem() { Content = sp, IsHitTestVisible = false };
                        RipListBox.Items.Add(item);
                    }
                }
            }
        }

        private void DrawGraph()
        {
            RipCanvas.Children.Clear();
            List<int> yrs = _almanac.Keys.ToList();
            yrs.Sort();
            int yearA = yrs[0];
            int yearZ = yrs.Last();
            double wid = RipCanvas.ActualWidth;
            double hgt = RipCanvas.ActualHeight;
            double yearwidth = wid / (1+yearZ-yearA);
            // get vertical scale
            int mx = 0;
            foreach(int y in yrs)
            {
                AnnualDeaths ad = _almanac[y];
                int q = ad.RaceKills + ad.ElseKills + ad.BedDeaths;
                mx = Math.Max(q, mx);
            }
            double scaleheight = (hgt-80) / mx;
            int before = 0;
            for (int y=yearA; y<=yearZ; y++)
            {
                if (_almanac.ContainsKey(y))
                {
                    AnnualDeaths ad = _almanac[y];

                    Rectangle racerect = new Rectangle() { Width = yearwidth / 2, Height = ad.RaceKills * scaleheight, Fill = Brushes.Red };
                    Canvas.SetLeft(racerect, yearwidth * before);
                    Canvas.SetBottom(racerect, 0);
                    RipCanvas.Children.Add(racerect);

                    Rectangle elserect = new Rectangle() { Width = yearwidth / 2, Height = ad.ElseKills * scaleheight, Fill = Brushes.DarkRed };
                    Canvas.SetLeft(elserect, yearwidth * before);
                    Canvas.SetBottom(elserect, racerect.Height);
                    RipCanvas.Children.Add(elserect);

                    Rectangle naturect = new Rectangle() { Width = yearwidth / 2, Height = ad.BedDeaths * scaleheight, Fill = Brushes.SeaGreen };
                    Canvas.SetLeft(naturect, yearwidth * before);
                    Canvas.SetBottom(naturect, racerect.Height + elserect.Height);
                    RipCanvas.Children.Add(naturect);

                    TextBlock bloc = new TextBlock() { Text = y.ToString(System.Globalization.CultureInfo.CurrentCulture) };
                    RotateTransform rotateTransform1 = new RotateTransform(270);
                    bloc.RenderTransform = rotateTransform1;
                    Canvas.SetLeft(bloc, (yearwidth * before) - 3);
                    Canvas.SetBottom(bloc, (racerect.Height + elserect.Height + naturect.Height) - 8);
                    RipCanvas.Children.Add(bloc);
                }
                else
                {
                    TextBlock bloc = new TextBlock() { Text = y.ToString(System.Globalization.CultureInfo.CurrentCulture) };
                    RotateTransform rotateTransform1 = new RotateTransform(270);
                    bloc.RenderTransform = rotateTransform1;
                    Canvas.SetLeft(bloc, (yearwidth * before) - 3);
                    Canvas.SetBottom(bloc, -8);
                    RipCanvas.Children.Add(bloc);
                }
                before++;
            }
        }

        private struct Mort : IComparable<Mort>
        {
            internal int Mode;
            internal string Driv;
            internal DateTime When;
            internal int Age;

            internal Mort(string nom, DateTime quand, int how, int old)
            {
                Mode = how;
                Driv = nom;
                When = quand;
                Age = old;
            }

            int IComparable<Mort>.CompareTo(Mort other)
            {
                return this.When.CompareTo(other.When);
            }
        }
}