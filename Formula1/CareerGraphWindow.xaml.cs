using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Formula1;

public partial class CareerGraphWindow : Window
{
    private readonly int _driverKey;
        public CareerGraphWindow(int Key)
        {
            InitializeComponent();
            _driverKey = Key;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double timeStretchRatio = 1;
            NomTextBlock.Text = Core.Instance.Drivers[_driverKey].FullName;
            int max = Core.Instance.HighestPosition();
            DateTime minDate = Core.Instance.Drivers[_driverKey].RuntimeFirstRaceStartDate;
            minDate = new DateTime(minDate.Year, 1, 1); // start graph from the start of the driver's first season
            DateTime maxDate = Core.Instance.Drivers[_driverKey].RuntimeLastRaceStartDate;
            DateTime demise = Core.Instance.Drivers[_driverKey].DeathDate;

            if (demise > Core.DateBase) // include death date if less than a year after last race
            {
                TimeSpan after = demise - maxDate;
                if (after.TotalDays < 366)
                {
                    maxDate = demise;
                }
                else
                {
                    demise = DateTime.MinValue;
                }
            }

            double vspan = ChartCanvas.ActualHeight - 20;
            double careerDays = (maxDate - minDate).TotalDays;
            double graphTimeStretch =20+( careerDays * timeStretchRatio);
            if (ChartCanvas.ActualWidth < graphTimeStretch) { ChartCanvas.Width = graphTimeStretch; }

            // position lines
            for (int p = 1; p <= max; p++)
            {
                double chequeredflag = 10 + vspan * p / (double)max;
                double breadth = (p < 4) ? 2 : 0.5;
                Line gagnant = new Line() { X1 = 0, X2 = graphTimeStretch, Y1 = chequeredflag, Y2 = chequeredflag, Stroke = Brushes.ForestGreen, StrokeThickness = breadth };
                ChartCanvas.Children.Add(gagnant);
            }
            // year labels
            for (int y=minDate.Year; y<=maxDate.Year; y++)
            {
                DateTime Nyd = new DateTime(y, 1, 1);
                                    TimeSpan elapse = Nyd - minDate;
                    double daze = elapse.TotalDays;
                    double hpixel = 10 + timeStretchRatio * daze;
                TextBlock bk = new TextBlock() { Text = y.ToString(System.Globalization.CultureInfo.CurrentCulture) };
                Canvas.SetLeft(bk, hpixel);
                Canvas.SetTop(bk, 10);
                ChartCanvas.Children.Add(bk);
                Line yline = new Line() { X1 = hpixel, X2 = hpixel, Y1 = 0, Y2 = vspan+20, Stroke = Brushes.Tan, StrokeThickness = 1, StrokeDashArray = { 3,4} };
                ChartCanvas.Children.Add(yline);
            }
            // birthday labels
            DateTime bday = Core.Instance.Drivers[_driverKey].BirthDate;
            DateTime born = bday;
            while (bday < maxDate)
            {
                if (bday >= minDate)
                {
                    TimeSpan elapse = bday - minDate;
                    double daze = elapse.TotalDays;
                    double hpixel = 10 + timeStretchRatio * daze;
                    TextBlock bk = new TextBlock() { Text =$"Age {bday.Year-born.Year}", Foreground=Brushes.Magenta };
                    Canvas.SetLeft(bk, hpixel);
                    Canvas.SetTop(bk, vspan-80);
                    ChartCanvas.Children.Add(bk);
                    Line bline = new Line() { X1 = hpixel, X2 = hpixel, Y1 = 0, Y2 = vspan+20, Stroke = Brushes.Magenta, StrokeThickness = 0.5, StrokeDashArray = { 3, 4 } };
                    ChartCanvas.Children.Add(bline);
                }
                bday = bday.AddYears(1);
            }
            for (int y = minDate.Year; y <= maxDate.Year; y++)
            {
                DateTime Nyd = new DateTime(y, 1, 1);
                TimeSpan elapse = Nyd - minDate;
                double daze = elapse.TotalDays;
                double hpixel = 10 + timeStretchRatio * daze;
                TextBlock bk = new TextBlock() { Text = y.ToString(System.Globalization.CultureInfo.CurrentCulture) };
                Canvas.SetLeft(bk, hpixel);
                Canvas.SetTop(bk, 10);
                ChartCanvas.Children.Add(bk);
                Line yline = new Line() { X1 = hpixel, X2 = hpixel, Y1 = 0, Y2 = vspan, Stroke = Brushes.Tan, StrokeThickness = 1, StrokeDashArray = { 3, 4 } };
                ChartCanvas.Children.Add(yline);
            }
            // death line

            if (demise > Core.DateBase)
            {
                TimeSpan elapse = demise - minDate;
                double daze = elapse.TotalDays;
                double hpixel = 10 + timeStretchRatio * daze;
                Line dline = new Line() { X1 = hpixel, X2 = hpixel, Y1 = 0, Y2 = vspan, Stroke = Brushes.Red, StrokeThickness = 2 };
                ChartCanvas.Children.Add(dline);
            }

            foreach (Voiture v in Core.Instance.Voitures.Values)
            {
                if (v.IncludesDriver(_driverKey))
                {
                    if (v.RacePosition != (int)Core.RaceResultConstants.DidNotStart)
                    {


                        TimeSpan elapse = v.RaceDate - minDate;
                        double daze = elapse.TotalDays;
                        double hpixel = 10 + timeStretchRatio * daze;
                        double vRacePixel = 10 + vspan * v.RacePosition / (double)max;
                        double vGridPixel = 10 + vspan * v.GridPosition / (double)max;

                        if (InRange(max, v.RacePosition))
                        {
                            double radius = 4;
                            Ellipse elips = new Ellipse() { Width = radius * 2, Height = radius * 2, Fill = Brushes.RoyalBlue };
                            Canvas.SetLeft(elips, hpixel - radius);
                            Canvas.SetTop(elips, vRacePixel - radius);
                            ChartCanvas.Children.Add(elips);
                        }
                        else
                        {
                            if (InRange(max, v.GridPosition))
                            {
                                if (v.RacePosition == (int)Core.RaceResultConstants.RetFatalDriver)
                                {
                                    double rad = 4;
                                    Ellipse wreath = new Ellipse() { Width = rad * 2, Height = rad * 2, Fill = Brushes.Red };
                                    Canvas.SetLeft(wreath, hpixel - rad);
                                    Canvas.SetTop(wreath, vGridPixel - rad);
                                    ChartCanvas.Children.Add(wreath);
                                }
                                double radius = 3;
                                Ellipse elips = new Ellipse() { Width = radius * 2, Height = radius * 2, Fill = Brushes.Peru };
                                Canvas.SetLeft(elips, hpixel - radius);
                                Canvas.SetTop(elips, vGridPixel - radius);
                                ChartCanvas.Children.Add(elips);

                            }
                        }
                    }
                }
            }
        }

        private bool InRange(int max, int value)
        {
            return ((value > 0) && (value <= max));
        }

        /*
         get all-time highest raceposn/ gridposn for any driver in any race and use that as baseline with first place at top of graph
         symbol for qual, started, finished, maybe gridposn
         spread / space dates horizontally
         */
}