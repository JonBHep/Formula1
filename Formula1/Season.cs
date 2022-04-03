using System;
using System.Collections.Generic;
using System.Linq;

namespace Formula1;

internal class Season : IComparable<Season>
{
    /// <summary>
    /// Dictionary of race keys, containing the scores of each driver in the race
    /// </summary>
    private Dictionary<int, Dictionary<int, float>> _raceDriverScores = new Dictionary<int, Dictionary<int, float>>();

    /// <summary>
    /// Dictionary of race keys, containing the scores of each team in the race
    /// </summary>
    private Dictionary<int, Dictionary<int, float>> _raceConstructorScores
        = new Dictionary<int, Dictionary<int, float>>();

    /// <summary>
    /// Dictionary of drivers, containing the driver's score in each race
    /// </summary>
    private Dictionary<int, Dictionary<int, float>> _driverScores = new Dictionary<int, Dictionary<int, float>>();

    /// <summary>
    /// Dictionary of constructors, containing the constructor's score in each race
    /// </summary>
    private Dictionary<int, Dictionary<int, float>> _constructorScores = new Dictionary<int, Dictionary<int, float>>();

    private List<int> _raceKeyList;

    internal SeasonPointsAllocationScheme PointsAllocationSchemeForDrivers { get; set; }
    internal SeasonPointsAllocationScheme PointsAllocationSchemeForConstructors { get; set; }

    internal int
        StatedNumberOfRaces
    {
        get;
        set;
    } // Only needed during a partially completed season so as to calculate which drivers have a chance to win the championship

    internal int Anno { get; set; }

    internal string Specification
    {
        get
        {
            return
                $"{Anno}~{StatedNumberOfRaces}~{PointsAllocationSchemeForDrivers.Specification}~{PointsAllocationSchemeForConstructors.Specification}";
        }
        set
        {
            string[] part = value.Split('~');
            Anno = int.Parse(part[0], System.Globalization.CultureInfo.InvariantCulture);
            StatedNumberOfRaces = int.Parse(part[1], System.Globalization.CultureInfo.InvariantCulture);
            PointsAllocationSchemeForDrivers = new SeasonPointsAllocationScheme(part[2]);
            PointsAllocationSchemeForConstructors = new SeasonPointsAllocationScheme(part[3]);
        }
    }

    int IComparable<Season>.CompareTo(Season other)
    {
        return Anno.CompareTo(other.Anno);
    }

    private void BuildConstructorsTable()
    {
        // Build table of team scores based on Table of race scores already populated
        _constructorScores = new Dictionary<int, Dictionary<int, float>>();
        // a dictionary of race scores for each team in a dictionary of teams
        foreach (int raceKey in _raceConstructorScores.Keys)
        {
            foreach (int teamkey in _raceConstructorScores[raceKey].Keys)
            {
                if (!_constructorScores.ContainsKey(teamkey))
                {
                    _constructorScores.Add(teamkey, new Dictionary<int, float>());
                }

                _constructorScores[teamkey].Add(raceKey, _raceConstructorScores[raceKey][teamkey]);
            }
        }
    }

    private void BuildResultsTable()
    {
        _raceDriverScores = new Dictionary<int, Dictionary<int, float>>();
        // a dictionary of driver scores for each race in a dictionary of races, including all drivers scoring in that season
        _raceConstructorScores = new Dictionary<int, Dictionary<int, float>>();
        // a dictionary of team scores for each race in a dictionary of races, including all teams scoring in that season

        foreach (int pisteKey in _raceKeyList)
        {
            Dictionary<int, float> DriverScoresToday = new Dictionary<int, float>();
            Dictionary<int, float> ConstructorScoresToday = new Dictionary<int, float>();

            // NB Results are sorted so that the higher results in a race are listed first

            // DRIVERS
            List<Voiture> CarPlaces = Core.Instance.Voitures.Values.ToList();
            CarPlaces.Sort();

            foreach (Voiture cr in CarPlaces)
            {
                if (cr.RaceMeetingKey == pisteKey)
                {
                    if (cr.AggregatedAllPoints > 0)
                    {
                        for (int dx = 0; dx < 3; dx++)
                        {
                            if (cr.DriverKey(dx) > 0)
                            {
                                if (DriverScoresToday.ContainsKey(cr.DriverKey(dx)))
                                {
                                    float pts = DriverScoresToday[cr.DriverKey(dx)];
                                    pts += cr.PointsForPosition(dx).FloatValue;
                                    pts += cr.PointsForSprintQualifying(dx).FloatValue;
                                    pts += cr.PointsForFastestLap(dx).FloatValue;
                                    DriverScoresToday[cr.DriverKey(dx)] = pts;
                                }
                                else
                                {
                                    float pts = cr.PointsForPosition(dx).FloatValue;
                                    pts += cr.PointsForSprintQualifying(dx).FloatValue;
                                    pts += cr.PointsForFastestLap(dx).FloatValue;
                                    DriverScoresToday.Add(cr.DriverKey(dx), pts);
                                }
                            }
                        }
                    }
                }
            }

            // ensure there is a record for each driver featuring in the season
            List<int> chaps = SeasonScoringDrivers();
            foreach (int k in chaps)
            {
                if (!DriverScoresToday.ContainsKey(k))
                {
                    DriverScoresToday.Add(k, 0);
                }
            }

            // CONSTRUCTORS
            List<int> equipes = SeasonScoringConstructors();
            foreach (int tm in equipes)
            {
                float pts = ConstructorRaceScore(pisteKey, tm, Anno);
                ConstructorScoresToday.Add(tm, pts);
            }

            _raceDriverScores.Add(pisteKey, DriverScoresToday);
            _raceConstructorScores.Add(pisteKey, ConstructorScoresToday);
        }
    }

    private void BuildDriversTable()
    {
        // Build table of driver scores based on the Table of race scores already populated
        _driverScores = new Dictionary<int, Dictionary<int, float>>();
        // a dictionary of race scores for each driver in a dictionary of drivers
        foreach (int raceKey in _raceDriverScores.Keys)
        {
            foreach (int driverkey in _raceDriverScores[raceKey].Keys)
            {
                if (!_driverScores.ContainsKey(driverkey))
                {
                    _driverScores.Add(driverkey, new Dictionary<int, float>());
                }

                _driverScores[driverkey].Add(raceKey, _raceDriverScores[raceKey][driverkey]);
            }
        }
    }

    private List<int> SeasonRaceKeys()
    {
        List<RaceMeeting> mtgs = new List<RaceMeeting>();
        foreach (RaceMeeting rm in Core.Instance.Races.Values)
        {
            if (rm.RaceDate.Year == Anno)
            {
                mtgs.Add(rm);
            }
        }

        mtgs.Sort(); // to date order
        List<int> RaceKeys = new List<int>();
        foreach (RaceMeeting rm in mtgs)
        {
            RaceKeys.Add(rm.Key);
        }

        return RaceKeys;
    }

    public void RefreshStatistics()
    {
        _raceKeyList = SeasonRaceKeys();
        if (StatedNumberOfRaces < _raceKeyList.Count)
        {
            StatedNumberOfRaces = _raceKeyList.Count;
        }

        BuildResultsTable();
        BuildDriversTable();
        BuildConstructorsTable();
    }

    public List<int> SeasonScoringDrivers()
    {
        List<int> ScoringDrivers = new List<int>();
        foreach (Voiture car in Core.Instance.Voitures.Values)
        {
            if (car.RaceDate.Year == Anno)
            {
                if (car.PointsForPosition(0).FloatValue > 0)
                {
                    ScoringDrivers.Add(car.DriverKey(0));
                }

                if (car.PointsForPosition(1).FloatValue > 0)
                {
                    ScoringDrivers.Add(car.DriverKey(1));
                }

                if (car.PointsForPosition(2).FloatValue > 0)
                {
                    ScoringDrivers.Add(car.DriverKey(2));
                }

                if (car.PointsForSprintQualifying(0).FloatValue > 0)
                {
                    ScoringDrivers.Add(car.DriverKey(0));
                }

                if (car.PointsForSprintQualifying(1).FloatValue > 0)
                {
                    ScoringDrivers.Add(car.DriverKey(1));
                }

                if (car.PointsForSprintQualifying(2).FloatValue > 0)
                {
                    ScoringDrivers.Add(car.DriverKey(2));
                }

                if (car.PointsForFastestLap(0).FloatValue > 0)
                {
                    ScoringDrivers.Add(car.DriverKey(0));
                }

                if (car.PointsForFastestLap(1).FloatValue > 0)
                {
                    ScoringDrivers.Add(car.DriverKey(1));
                }

                if (car.PointsForFastestLap(2).FloatValue > 0)
                {
                    ScoringDrivers.Add(car.DriverKey(2));
                }
            }
        }

        List<int> uniqueKeys = new List<int>();
        foreach (int y in ScoringDrivers)
        {
            if (y > 0)
            {
                if (!uniqueKeys.Contains(y))
                {
                    uniqueKeys.Add(y);
                }
            }
        }

        return uniqueKeys;
    }

    public List<int> SeasonNonScoringDrivers()
    {
        List<int> FeaturedDrivers = new List<int>();
        foreach (Voiture car in Core.Instance.Voitures.Values)
        {
            if (car.RaceDate.Year == Anno)
            {
                int a = car.DriverKey(0);
                if (a > 0)
                {
                    if (!FeaturedDrivers.Contains(a))
                    {
                        FeaturedDrivers.Add(a);
                    }
                }

                a = car.DriverKey(1);
                if (a > 0)
                {
                    if (!FeaturedDrivers.Contains(a))
                    {
                        FeaturedDrivers.Add(a);
                    }
                }

                a = car.DriverKey(2);
                if (a > 0)
                {
                    if (!FeaturedDrivers.Contains(a))
                    {
                        FeaturedDrivers.Add(a);
                    }
                }
            }
        }

        List<int> scorers = SeasonScoringDrivers();
        foreach (int b in scorers)
        {
            FeaturedDrivers.Remove(b);
        }

        return FeaturedDrivers;
    }

    public List<int> SeasonScoringConstructors()
    {
        List<int> ScoringTeams = new List<int>();
        foreach (Voiture cr in Core.Instance.Voitures.Values)
        {
            if (cr.RaceDate.Year == Anno)
            {
                if (Core.Instance.Races[cr.RaceMeetingKey].CountForConstructorsChampionship)
                {
                    if (cr.AggregatedPositionPoints > 0)
                    {
                        if (!ScoringTeams.Contains(cr.ConstructorKey))
                        {
                            ScoringTeams.Add(cr.ConstructorKey);
                        }
                    }
                }
            }
        }

        return ScoringTeams;
    }

    private static float ConstructorRaceScore(int RaceKey, int TeamKey, int Yr)
    {
        if (!Core.Instance.Races[RaceKey].CountForConstructorsChampionship)
        {
            return 0;
        }

        List<float> CarScores = new List<float>(); // list of all cars scoring in this race for this team
        foreach (Voiture cr in Core.Instance.Voitures.Values)
        {
            if (cr.RaceMeetingKey == RaceKey)
            {
                if (cr.ConstructorKey == TeamKey)
                {
                    float p;
                    if ((Yr == 1958) ||
                        (Yr == 1959)) // in 1958 and 1959 constructors were not entitled to fastest lap points
                    {
                        p = cr.AggregatedPositionPoints;
                    }
                    else
                    {
                        p = cr.AggregatedAllPoints;
                        // in 1961 constructors only got 8 points for a win (as opposed to the driver's 9 points)
                        if (Yr == 1961)
                        {
                            if (p == 9)
                            {
                                p = 8;
                            }
                        } // p of 9 for win reduced to 8 (there were no fastest lap points in 1961)
                    }

                    if (cr.ConstructorPointsDisallowed)
                    {
                        p = 0;
                    } // e.g. Brazil GP 1995 two teams points were disallowed for petrol type violation but drivers' points were awarded

                    if (cr.ConstructorPointsPenalty > 0)
                    {
                        p -= cr.ConstructorPointsPenalty;
                    }

                    CarScores.Add(p);
                }
            }
        }

        if (Yr < 1979)
        {
            // Return only the single best 'position points' score achieved for that team
            if (CarScores.Count > 0)
            {
                return CarScores.Max();
            }
        }
        else
        {
            // From 1979, return all of the 'position points' achieved for that team
            if (CarScores.Count > 0)
            {
                return CarScores.Sum();
            }
        }

        return 0;
    }

    internal List<int> RaceKeyList
    {
        get => _raceKeyList;
    }

    internal float DriverPoints(int RaceKey, int DriverKey)
    {
        return _raceDriverScores[RaceKey][DriverKey];
    }

    internal float ConstructorPoints(int RaceKey, int ConstructorKey)
    {
        return _raceConstructorScores[RaceKey][ConstructorKey];
    }

    internal List<IndividualScore> RaceDriverResults(int RaceKey)
    {
        float topscoreavailable = 0;
        int annus = Core.Instance.Races[RaceKey].RaceDate.Year;
        topscoreavailable += Core.Instance.ScoreForFirstPlace(annus);
        topscoreavailable += Core.Instance.ScoreForSprintQualifying(annus);
        topscoreavailable += Core.Instance.ScoreForFastestLap(annus);

        List<IndividualScore> returnList = new List<IndividualScore>();
        Dictionary<int, float> DriverScores = _raceDriverScores[RaceKey];
        foreach (int driverKey in DriverScores.Keys)
        {
            IndividualScore isc = new IndividualScore(driverKey) {Score = DriverScores[driverKey]};
            List<float> driverScoresToDate = new List<float>();
            float cumul = 0;
            foreach (int EventKey in _raceKeyList)
            {
                float pts = DriverPoints(EventKey, driverKey);
                driverScoresToDate.Add(pts);
                cumul += pts;
                if (EventKey == RaceKey)
                {
                    break;
                } // only count races up to the current one
            }

            isc.CumulativeScore = cumul;
            isc.CumulativeScoreCounted = PointsAllocationSchemeForDrivers.ModifiedPointsTotal(driverScoresToDate);
            returnList.Add(isc);
        }

        // based on scores to date, see who still has the potential to be season champion
        // create reduced list of race keys, just up to this one
        List<int> soFarRaces = new List<int>();
        foreach (int EventKey in _raceKeyList)
        {
            soFarRaces.Add(EventKey);
            if (EventKey == RaceKey)
            {
                break;
            } // only count races up to the current one
        }

        int statedraces = StatedNumberOfRaces;
        if (statedraces >= soFarRaces.Count)
        {
            // calc how many races remaining
            int remainingEvents = statedraces - soFarRaces.Count;
            // calc each driver's potential performance
            foreach (IndividualScore chap in returnList)
            {
                List<float> driverRealAndPotentialScores = new List<float>();
                foreach (int EventKey in soFarRaces)
                {
                    float pts = DriverPoints(EventKey, chap.IndividualKey);
                    driverRealAndPotentialScores.Add(pts);
                }

                // assume he scores max points in all remaining races
                for (int r = 0; r < remainingEvents; r++)
                {
                    driverRealAndPotentialScores.Add(topscoreavailable);
                }

                chap.MaxTheoreticalSeasonTotal
                    = PointsAllocationSchemeForDrivers.ModifiedPointsTotal(driverRealAndPotentialScores);
            }
        }

        returnList.Sort();
        return returnList;
    }

    internal List<Tuple<float, float, float, int>> RankedDrivers(int RaceKey)
    {
        List<IndividualScore> lisc = RaceDriverResults(RaceKey);
        List<Tuple<float, float, float, int>> rankable = new List<Tuple<float, float, float, int>>();
        foreach (IndividualScore i in lisc)
        {
            rankable.Add(new Tuple<float, float, float, int>(i.CumulativeScoreCounted, i.CumulativeScore
                , i.MaxTheoreticalSeasonTotal, i.IndividualKey));
        }

        rankable.Sort();
        rankable.Reverse();
        return rankable;
    }

    internal List<Tuple<float, float, int>> RankedConstructors(int RaceKey)
    {
        List<IndividualScore> lisc = RaceConstructorResults(RaceKey);
        List<Tuple<float, float, int>> rankable = new List<Tuple<float, float, int>>();
        foreach (IndividualScore i in lisc)
        {
            rankable.Add(new Tuple<float, float, int>(i.CumulativeScoreCounted, i.CumulativeScore, i.IndividualKey));
        }

        rankable.Sort();
        rankable.Reverse();
        return rankable;
    }

    internal int DriverSeasonRanked(int position)
    {
        int lastRaceKey = _raceKeyList.Last();
        List<Tuple<float, float, float, int>> lastRaceRanking = RankedDrivers(lastRaceKey);
        if (lastRaceRanking.Count > 0)
        {
            Core.Instance.SetChampionshipWinYear(lastRaceRanking[0].Item4, Anno); // take the opportunity to update the champions data
            return lastRaceRanking[position - 1].Item4;
        }

        return 0;
    }

    internal int ConstructorSeasonRanked(int position)
    {
        int lastRaceKey = _raceKeyList.Last();
        List<Tuple<float, float, int>> lastRaceRanking = RankedConstructors(lastRaceKey);
        return lastRaceRanking[position - 1].Item3;
    }

    internal double TopDriverSeasonPoints()
    {
        int lastRaceKey = _raceKeyList.Last();
        List<Tuple<float, float, float, int>> lastRaceRanking = RankedDrivers(lastRaceKey);
        return lastRaceRanking[0].Item1;
    }

    internal double TopConstructorSeasonPoints()
    {
        int lastRaceKey = _raceKeyList.Last();
        List<Tuple<float, float, int>> lastRaceRanking = RankedConstructors(lastRaceKey);
        return lastRaceRanking[0].Item1;
    }

    internal List<IndividualScore> RaceConstructorResults(int RaceKey)
    {
        List<IndividualScore> returnList = new List<IndividualScore>();
        Dictionary<int, float> ConstructorScores = _raceConstructorScores[RaceKey];
        foreach (int constructorKey in ConstructorScores.Keys)
        {
            IndividualScore isc = new IndividualScore(constructorKey) {Score = ConstructorScores[constructorKey]};
            List<float> constructorScoresToDate = new List<float>();
            float cumul = 0;
            foreach (int EventKey in _raceKeyList)
            {
                float pts = ConstructorPoints(EventKey, constructorKey);
                constructorScoresToDate.Add(pts);
                cumul += pts;
                if (EventKey == RaceKey)
                {
                    break;
                } // only count races up to the current one
            }

            isc.CumulativeScore = cumul;
            isc.CumulativeScoreCounted
                = PointsAllocationSchemeForConstructors.ModifiedPointsTotal(constructorScoresToDate);
            returnList.Add(isc);
        }

        returnList.Sort();
        return returnList;
    }

    internal List<Tuple<int, string>> NumberOfDifferentVictorsInSeason()
    {
        Dictionary<int, int> Gagnants = new Dictionary<int, int>();
        foreach (int racekey in _raceKeyList)
        {
            RaceMeeting mtg = Core.Instance.Races[racekey];
            Tuple<int, int, int> winners = mtg.PodiumOne;
            int drvr = winners.Item1;
            if (drvr > 0)
            {
                if (Gagnants.ContainsKey(drvr))
                {
                    Gagnants[drvr]++;
                }
                else
                {
                    Gagnants.Add(drvr, 1);
                }
            }

            drvr = winners.Item2;
            if (drvr > 0)
            {
                if (Gagnants.ContainsKey(drvr))
                {
                    Gagnants[drvr]++;
                }
                else
                {
                    Gagnants.Add(drvr, 1);
                }
            }

            drvr = winners.Item3;
            if (drvr > 0)
            {
                if (Gagnants.ContainsKey(drvr))
                {
                    Gagnants[drvr]++;
                }
                else
                {
                    Gagnants.Add(drvr, 1);
                }
            }
        }

        List<Tuple<int, string>> champs = new List<Tuple<int, string>>();
        foreach (int dd in Gagnants.Keys)
        {
            champs.Add(new Tuple<int, string>(Gagnants[dd], Core.Instance.Drivers[dd].FullName));
        }

        champs.Sort();
        champs.Reverse();
        return champs;
    }

    /// <summary>
    /// Returns list of {driver key, number of wins in the season, number of races in the season} for the drivers equalling the most wins in the season
    /// </summary>
    /// <returns></returns>
    internal List<Tuple<int, int, int>> MostWinsInInSeason()
    {
        Dictionary<int, int> Gagnants = new Dictionary<int, int>();
        foreach (int racekey in _raceKeyList)
        {
            RaceMeeting mtg = Core.Instance.Races[racekey];
            Tuple<int, int, int> winners = mtg.PodiumOne;
            int drvr = winners.Item1;
            if (drvr > 0)
            {
                if (Gagnants.ContainsKey(drvr))
                {
                    Gagnants[drvr]++;
                }
                else
                {
                    Gagnants.Add(drvr, 1);
                }
            }

            drvr = winners.Item2;
            if (drvr > 0)
            {
                if (Gagnants.ContainsKey(drvr))
                {
                    Gagnants[drvr]++;
                }
                else
                {
                    Gagnants.Add(drvr, 1);
                }
            }

            drvr = winners.Item3;
            if (drvr > 0)
            {
                if (Gagnants.ContainsKey(drvr))
                {
                    Gagnants[drvr]++;
                }
                else
                {
                    Gagnants.Add(drvr, 1);
                }
            }
        }

        int most = 0;
        foreach (int dd in Gagnants.Keys)
        {
            if (Gagnants[dd] > most)
            {
                most = Gagnants[dd];
            }
        }

        // most = max number of races won by a single driver
        List<Tuple<int, int, int>> champs = new List<Tuple<int, int, int>>();
        foreach (int dd in Gagnants.Keys)
        {
            if (Gagnants[dd] == most)
            {
                champs.Add(new Tuple<int, int, int>(dd, Gagnants[dd]
                    , _raceKeyList.Count)); // driver key, number of wins, races in season
            }
        }

        return champs;
    }
}