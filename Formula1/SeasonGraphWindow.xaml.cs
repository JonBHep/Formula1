using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Formula1;

public partial class SeasonGraphWindow
{
    public SeasonGraphWindow(int annee)
        {
            InitializeComponent();
            _raceSeason = Core.Instance.Seasons[annee];
            _raceSeason.RefreshStatistics();
        }
        private readonly Season _raceSeason;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double scrX = SystemParameters.PrimaryScreenWidth;
            double scrY = SystemParameters.PrimaryScreenHeight;
            double winX = scrX * .98;
            double winY = scrY * .94;
            double xm = (scrX - winX) / 2;
            double ym = (scrY - winY) / 4;
            Width = winX;
            Height = winY;
            Left = xm;
            Top = ym;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Title = $"Season's scores {_raceSeason.Anno}";
            SeasonTextBlock.Text = $"{_raceSeason.Anno} CHAMPIONSHIP";
            DisplayResultsList();
        }

        private void DisplayResultsList()
        {
            DriversCanvas.Children.Clear();
            ConstructorsCanvas.Children.Clear();
            TextBlock titleDriversBlock = new TextBlock() { Text = "DRIVERS' CHAMPIONSHIP", FontWeight = FontWeights.Black };
            Canvas.SetLeft(titleDriversBlock, 20);
            Canvas.SetTop(titleDriversBlock, 20);
            DriversCanvas.Children.Add(titleDriversBlock);

            if (_raceSeason.PointsAllocationSchemeForConstructors.Applies)
            {
                TextBlock titleConstructorsBlock = new TextBlock() { Text = "CONSTRUCTORS' CHAMPIONSHIP", FontWeight = FontWeights.Black };
                Canvas.SetLeft(titleConstructorsBlock, 20);
                Canvas.SetTop(titleConstructorsBlock, 20);
                ConstructorsCanvas.Children.Add(titleConstructorsBlock);
            }
            List<int> driverKeys = _raceSeason.SeasonScoringDrivers();

            double canvasheight = DriversCanvas.ActualHeight;
            double toppoints = _raceSeason.TopDriverSeasonPoints();
            int racecount = _raceSeason.RaceKeyList.Count;
            double xincrement = (DriversCanvas.ActualWidth - 212) / racecount;
            double yincrement = (canvasheight - 20) / toppoints;
            double finalscore = 0;

            foreach (int dr in driverKeys) // for each driver
            {
                double xpos = 12;
                double ypos = canvasheight;
                Brush driverbrush = Brushes.Black;
                if (_raceSeason.DriverSeasonRanked(1) == dr) { driverbrush = Brushes.Red; }
                if (_raceSeason.DriverSeasonRanked(2) == dr) { driverbrush = Brushes.Blue; }
                if (_raceSeason.DriverSeasonRanked(3) == dr) { driverbrush = Brushes.Green; }
                if (_raceSeason.DriverSeasonRanked(4) == dr) { driverbrush = Brushes.Magenta; }
                if (_raceSeason.DriverSeasonRanked(5) == dr) { driverbrush = Brushes.DarkCyan; }
                if (_raceSeason.DriverSeasonRanked(6) == dr) { driverbrush = Brushes.SaddleBrown; }
                string drivername = Core.Instance.Drivers[dr].FullName;
                foreach (int m in _raceSeason.RaceKeyList) // for each race
                {
                    foreach (IndividualScore driv in _raceSeason.RaceDriverResults(m)) // for each car result in that race
                    {
                        if (driv.IndividualKey == dr)
                        {
                            finalscore = driv.CumulativeScoreCounted;
                            Line lin = new Line() { X1 = xpos, X2 = xpos + xincrement, Y1 = ypos, Y2 = canvasheight - yincrement * finalscore, Stroke = driverbrush, StrokeThickness = 1 };
                            DriversCanvas.Children.Add(lin);
                            xpos = lin.X2;
                            ypos = lin.Y2;
                        }
                    }
                }
                TextBlock nameBlock = new TextBlock() { Text = $"{drivername} ({finalscore})", FontSize = 10, Foreground = driverbrush };
                Canvas.SetLeft(nameBlock, xpos + 6);
                Canvas.SetTop(nameBlock, ypos - 8);
                DriversCanvas.Children.Add(nameBlock);
            }

            List<int> constructorKeys = _raceSeason.SeasonScoringConstructors();
            if (constructorKeys.Count > 0)
            {
                canvasheight = ConstructorsCanvas.ActualHeight;
                toppoints = _raceSeason.TopConstructorSeasonPoints();
                xincrement = (ConstructorsCanvas.ActualWidth - 212) / racecount;
                yincrement = (canvasheight - 20) / toppoints;
                finalscore = 0;
                foreach (int cr in constructorKeys) // for each constructor
                {
                    double xpos = 12;
                    double ypos = canvasheight;
                    Brush constructorbrush = Brushes.Black;
                    if (_raceSeason.ConstructorSeasonRanked(1) == cr) { constructorbrush = Brushes.Red; }
                    if (_raceSeason.ConstructorSeasonRanked(2) == cr) { constructorbrush = Brushes.Blue; }
                    if (_raceSeason.ConstructorSeasonRanked(3) == cr) { constructorbrush = Brushes.Green; }
                    if (_raceSeason.ConstructorSeasonRanked(4) == cr) { constructorbrush = Brushes.Magenta; }
                    if (_raceSeason.ConstructorSeasonRanked(5) == cr) { constructorbrush = Brushes.DarkCyan; }
                    if (_raceSeason.ConstructorSeasonRanked(6) == cr) { constructorbrush = Brushes.SaddleBrown; }
                    string constructorname = Core.Instance.Constructors[cr].Caption;
                    foreach (int m in _raceSeason.RaceKeyList) // for each race
                    {
                        foreach (IndividualScore driv in _raceSeason.RaceConstructorResults(m)) // for each car result in that race
                        {
                            if (driv.IndividualKey == cr)
                            {
                                finalscore = driv.CumulativeScoreCounted;
                                Line lin = new Line() { X1 = xpos, X2 = xpos + xincrement, Y1 = ypos, Y2 = canvasheight - yincrement * finalscore, Stroke = constructorbrush, StrokeThickness = 1 };
                                ConstructorsCanvas.Children.Add(lin);
                                xpos = lin.X2;
                                ypos = lin.Y2;
                            }
                        }
                    }
                    TextBlock nameBlock = new TextBlock() { Text = $"{constructorname} ({finalscore})", FontSize = 10, Foreground = constructorbrush };
                    Canvas.SetLeft(nameBlock, xpos + 6);
                    Canvas.SetTop(nameBlock, ypos - 8);
                    ConstructorsCanvas.Children.Add(nameBlock);
                }
            }
        }
}