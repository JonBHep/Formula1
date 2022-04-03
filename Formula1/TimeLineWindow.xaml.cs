using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Formula1;

public partial class TimeLineWindow : Window
{
    private List<DriverTimeline> _lines;

        private readonly List<TextBlock> _yearLabels=new List<TextBlock>();
        
        public TimeLineWindow()
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
        private void ChartScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            foreach (TextBlock bloc in _yearLabels)
            {
                Canvas.SetTop(bloc,  e.VerticalOffset);
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawChart();
        }

        private void DrawChart()
        {
            double leftMargin = 160;
            BuildData();
            DateTime startDate = new DateTime(1950, 1, 1);
            DateTime endDate = DateTime.Today;
            
            long alpha = startDate.Ticks;
            long omega = endDate.Ticks;

            long tickSpan = omega - alpha;
            double canvasWidth = 7200-leftMargin;
            ChartCanvas.Width = canvasWidth+leftMargin;
            double screenRatio = canvasWidth / tickSpan;
            double verticalPosition = 0;
            double verticalInterval = 20;

            for (int yr=1950; yr<=DateTime.Today.Year; yr++)
            {
                DateTime d = new DateTime(yr, 1, 1);
                double xpos = leftMargin + ((d.Ticks - alpha) * screenRatio);
                Line jahr = new Line() { X1 = xpos, X2 = xpos, Y1 = 0, Y2 = verticalInterval*_lines.Count, StrokeDashArray = { 8, 8 }, Stroke=Brushes.Moccasin, StrokeThickness=1 };
                ChartCanvas.Children.Add(jahr);
                TextBlock t = new TextBlock() {Text=yr.ToString(System.Globalization.CultureInfo.CurrentCulture), Foreground=Brushes.SaddleBrown, Background=Brushes.PeachPuff };
                Canvas.SetLeft(t, xpos);
                Canvas.SetTop(t, 0);
                _yearLabels.Add(t);
                ChartCanvas.Children.Add(t);
            }

            foreach (DriverTimeline dtl in _lines)
            {
                verticalPosition += verticalInterval;
                double titleRight = 0;
                foreach (Resultat r in dtl.Races)
                {
                    Rectangle blob = new Rectangle() { Width = 2, Height = 4, Fill = Brushes.Black };
                    if (r.Outcome > 0) { blob.Fill = Brushes.Red; }
                    double horizontalPosition = leftMargin + ((r.When.Ticks - alpha) * screenRatio);
                    if (titleRight == 0) { titleRight = horizontalPosition; }
                    Canvas.SetLeft(blob, horizontalPosition - 1);
                    Canvas.SetTop(blob, verticalPosition);
                    ChartCanvas.Children.Add(blob);
                }
                Driver dd = Core.Instance.Drivers[dtl.DriverKey];
                TextBlock tb = new TextBlock() { FontSize = 10, Text = dd.FullName, Width = leftMargin, TextAlignment = TextAlignment.Right, Padding = new Thickness(0, 0, 4, 0) };
                Canvas.SetLeft(tb, titleRight - leftMargin);
                Canvas.SetTop(tb, verticalPosition - 6);
                ChartCanvas.Children.Add(tb);
                double lineleft = titleRight;
                double lineright = 0;
                // Death
                if (dd.DeathDate > Core.DateBase)
                {
                    Ellipse blob = new Ellipse() { Width = 8, Height = 8, Stroke = Brushes.Red, StrokeThickness = 1 };
                    if (dd.HowDied == Core.CauseOfDeath.Natural) { blob.Stroke = Brushes.Black; }
                    if (dd.HowDied == Core.CauseOfDeath.OtherAccident) { blob.Stroke = Brushes.IndianRed; }
                    double horizontalPosition = leftMargin + ((dd.DeathDate.Ticks - alpha) * screenRatio);
                    Canvas.SetLeft(blob, horizontalPosition - 4);
                    Canvas.SetTop(blob, verticalPosition - 2);
                    ChartCanvas.Children.Add(blob);

                    tb = new TextBlock() { FontSize = 10, Text = dd.FullName, Width = leftMargin };
                    Canvas.SetLeft(tb, horizontalPosition + 6);
                    Canvas.SetTop(tb, verticalPosition - 6);
                    ChartCanvas.Children.Add(tb);
                    lineright = horizontalPosition;
                }
                if (lineright > 0)
                {
                    Line lifeline = new Line() { X1 = lineleft, X2 = lineright, Y1 = verticalPosition+2, Y2 = verticalPosition+2, Stroke = Brushes.Gray, StrokeDashArray = { 3, 3 }, StrokeThickness = 0.5 };
                    ChartCanvas.Children.Add(lifeline);
                }
            }

            ChartCanvas.Height = verticalPosition + 20;
        }

        private void BuildData()
        {
            Dictionary<int, DriverTimeline> dic = new Dictionary<int, DriverTimeline>();
            List<Voiture> ballades = Core.Instance.Voitures.Values.ToList();
            foreach (Voiture v in ballades)
            {
                for (int driverNumber = 0; driverNumber < 3; driverNumber++)
                {
                    int a = v.DriverKey(driverNumber);
                    if (a > 0)
                    {
                        DriverTimeline dtl;
                        if (dic.ContainsKey(a))
                        {
                            dtl = dic[a];
                        }
                        else
                        {
                            dtl = new DriverTimeline(a);
                            dic.Add(a, dtl);
                        }
                        int posn = 0;
                        if (v.RacePosition == 1) { posn = 2; } else if (v.RacePosition < 4) { posn = 1; }
                        // 0=quailified for race; 1=podium place; 2=first place
                        Resultat rslt = new Resultat(v.RaceDate, posn);
                        dtl.Races.Add(rslt);
                    }
                }
            }
            _lines = dic.Values.ToList();
            foreach (DriverTimeline tl in _lines) { tl.Races.Sort(); } // necessary because Voitures are sorted by race position not by date
            _lines.Sort();
        }

        private class DriverTimeline : IComparable<DriverTimeline>
        {
          internal  int DriverKey { get; }
            internal List<Resultat> Races { get; }
            internal DriverTimeline(int conducteur)
            {
                DriverKey = conducteur;
                Races = new List<Resultat>();
            }

            int IComparable<DriverTimeline>.CompareTo(DriverTimeline other)
            {
                if (Races.Count < 1) { return 1; }
                if (other.Races.Count < 1) { return -1; }
                return this.Races[0].CompareTo(other.Races[0]);
            }
        }

        private struct Resultat : IComparable<Resultat>
        {
            internal int Outcome { get; }
            internal DateTime When { get; }
            internal Resultat(DateTime Quand, int Res)
            {
                Outcome = Res;
                When = Quand;
            }

            int IComparable<Resultat>.CompareTo(Resultat other)
            {
                return this.When.CompareTo(other.When);
            }

            internal int CompareTo(Resultat other)
            {
                return this.When.CompareTo(other.When);
            }
        }
}