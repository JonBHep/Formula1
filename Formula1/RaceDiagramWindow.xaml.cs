using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Formula1;

public partial class RaceDiagramWindow : Window
{
    private List<Tuple<int, int, int>> _ranking;

        public RaceDiagramWindow(int RaceKey)
        {
            InitializeComponent();
            _ranking = Core.Instance.CompetitorRankings(RaceKey);
            RaceMeeting meet = Core.Instance.Races[RaceKey];
            double kt = meet.KendalTau();
            Title =$"{Core.Instance.RaceTitles[meet.RaceTitleKey].Caption} {meet.RaceDate.Year} Kendal Tau: {kt.ToString("0.00", Core.CultureUk)}";
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            double wid = RaceCanvas.ActualWidth;
            double rightMargin = 140;
            double verticalSpacing = 18;
            List<Tuple<int, int, string>> nonfinishers = new List<Tuple<int, int, string>>();

            // Get drivers' names
            // add initial if there is duplication of surnames e.g. Schumacher brothers
            List<Tuple<int, int, string>> troop = new List<Tuple<int, int, string>>();
            foreach (Tuple<int, int, int> car1 in _ranking)
            {
                Driver pilot1 = Core.Instance.Drivers[car1.Item3];
                int surnamecount = 0;
                foreach (Tuple<int, int, int> car2 in _ranking)
                {
                    Driver pilot2 = Core.Instance.Drivers[car2.Item3];
                    if (pilot1.Surname == pilot2.Surname) { surnamecount++; }
                }
                string nom = (surnamecount == 1) ? pilot1.Surname : $"{pilot1.Surname}, {pilot1.Forenames[0]}";
                Tuple<int, int, string> chap = new Tuple<int, int, string>(car1.Item1, car1.Item2, nom);
                troop.Add(chap);
            }

            foreach (Tuple<int,int, string> car in troop)
            {
                if (car.Item2 < 1000) // finished the race
                {
                    double yq = car.Item1 * verticalSpacing;
                    double yf = car.Item2 * verticalSpacing;

                    Brush p = Brushes.DodgerBlue;
                    if (yf < yq) { p = Brushes.SeaGreen; }
                    if (yf > yq) { p = Brushes.Firebrick; }

                    Line trak = new Line() { X1 = 34, X2 = 4 + wid - rightMargin, Y1 = yq + 4, Y2 = yf + 4, Stroke = p, StrokeThickness = 2 };
                    RaceCanvas.Children.Add(trak);

                    TextBlock bloc = new TextBlock() { Text = car.Item3, Foreground = p };
                    Canvas.SetLeft(bloc, 20 + wid - rightMargin);
                    Canvas.SetTop(bloc, yf - 5);
                    RaceCanvas.Children.Add(bloc);

                    Ellipse blob = new Ellipse() { Width = 8, Height = 8, Fill = Brushes.SaddleBrown };
                    Canvas.SetLeft(blob, 30);
                    Canvas.SetTop(blob, yq);
                    RaceCanvas.Children.Add(blob);

                    Ellipse plop = new Ellipse() { Width = 8, Height = 8, Fill = Brushes.SaddleBrown };
                    Canvas.SetLeft(plop, wid - rightMargin);
                    Canvas.SetTop(plop, yf);
                    RaceCanvas.Children.Add(plop);
                }
                else
                {
                    nonfinishers.Add(car);
                }
            }

            foreach (Tuple<int, int, string> b in nonfinishers)
            {
                Ellipse blob = new Ellipse() { Width = 8, Height = 8, Fill = Brushes.Tan };
                Canvas.SetLeft(blob, 30);
                Canvas.SetTop(blob, b.Item1 * verticalSpacing);
                RaceCanvas.Children.Add(blob);
                TextBlock bloc = new TextBlock() { Text = b.Item3, Foreground = Brushes.Gray };
                Canvas.SetLeft(bloc, 40);
                Canvas.SetTop(bloc, b.Item1 * verticalSpacing-5);
                RaceCanvas.Children.Add(bloc);
            }
        }
}