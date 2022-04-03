using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Formula1;

internal sealed class Core
{
    /*
         NB There have been F1 races that didn’t count towards the drivers’ championship and races that did count which were not F1 races, esp. in the early years.
        */

        // Core is implemented as a singleton so that only a single instance can be created and this instance can be accessed globally, and because the class is sealed no other class can inherit from it

        // The internal scope could equally well be public

        // TODO Label a driver at a point in time as being a former World Champion (number of times) and former Race Winner (number of times) e.g. WCx3 RWx18
        
        internal static CultureInfo CultureUK = CultureInfo.CreateSpecificCulture("en-GB");

        internal const int SpecialNumber = 5617;

        private static readonly Core _instance = new Core();

        internal const int MaxEntrants = 50;
        internal const int MaxExpectedGrid = 24;

        internal static DateTime DateBase = new DateTime(1850, 1, 1);
        internal enum CauseOfDeath { Unknown, Natural, RacingAccident, RacePracticeOrTestingAccident, OtherAccident };

        internal enum RankingHealth { Healthy, HasGaps, HasDuplicates, HasGapsAndDupes };

        private Core()
        {
            // the constructor is private thus preventing instances other than the single private instance from being created
            _voitures = new Dictionary<int, Voiture>();
            _constructors = new Dictionary<int, NamedItem>();
            _countries = new Dictionary<int, NamedItem>();
            _drivers = new Dictionary<int, Driver>();
            _races = new Dictionary<int, RaceMeeting>();
            _raceTitles = new Dictionary<int, NamedItem>();
            _raceTracks = new Dictionary<int, Circuit>();
            _seasons = new Dictionary<int, Season>();
            LoadData();
        }

        internal static Core Instance // this static property allows global access to the single private instance of this class
        {
            get
            {
                return _instance;
            }
        }

        internal Dictionary<int, Driver> Drivers => _drivers;

        internal Dictionary<int, NamedItem> Countries => _countries;

        internal Dictionary<int, NamedItem> Constructors => _constructors;

        internal Dictionary<int, NamedItem> RaceTitles => _raceTitles;

        internal Dictionary<int, RaceMeeting> Races => _races;

        internal Dictionary<int, Circuit> RaceTracks => _raceTracks;

        internal Dictionary<int, Voiture> Voitures => _voitures;

        internal Dictionary<int, Season> Seasons => _seasons;

        private readonly Dictionary<int, Driver> _drivers;
        private readonly Dictionary<int, NamedItem> _countries;
        private readonly Dictionary<int, NamedItem> _constructors;
        private readonly Dictionary<int, NamedItem> _raceTitles;
        private readonly Dictionary<int, RaceMeeting> _races;
        private readonly Dictionary<int, Circuit> _raceTracks;
        private readonly Dictionary<int, Voiture> _voitures;
        private readonly Dictionary<int, Season> _seasons;

        internal const double LowWindowHeight = 800;

        internal static double LowWindowTop
        {
            get
            {
                double scrY = System.Windows.SystemParameters.PrimaryScreenHeight; // Locates the window towards bottom of screen to allow reading of webpage above
                return scrY - (LowWindowHeight + 30);
            }
        }

        internal static string RaceResultDescription(int _position)
        {
            if (_position <= 1000) { return Ordinal(_position); }
            RaceResultConstants rrc = (RaceResultConstants)_position;
            string r = "Undefined";
            switch (rrc)
            {
                case RaceResultConstants.DidNotStart:
                    { r = "DNS Did not start"; break; }
                case RaceResultConstants.RetMech:
                    { r = "Ret Retired (mechanical failure)"; break; }
                case RaceResultConstants.RetFuel:
                    { r = "Ret Retired (out of fuel)"; break; }
                case RaceResultConstants.RetAccident:
                    { r = "Ret Retired (accident / spun off)"; break; }
                case RaceResultConstants.RetFatalDriver:
                    { r = "FAT Fatal accident"; break; }
                case RaceResultConstants.RetFatalOthers:
                    { r = "Ret Retired (accident fatal to others)"; break; }
                case RaceResultConstants.Unclassified:
                    { r = "NC  Not classified"; break; }
                case RaceResultConstants.Disqualified:
                    { r = "DSQ Disqualified"; break; }
                case RaceResultConstants.DriverHealth:
                    { r = "Ret Retired (driver unfit)"; break; }
                case RaceResultConstants.RetUnexplained:
                    { r = "Ret Retired (other reason)"; break; }
                default:
                    { break; }
            }
            return r;
        }

        internal enum RaceResultConstants { DidNotStart = 1001, RetMech = 1002, RetFuel = 1003, RetAccident = 1004, RetFatalDriver = 1005, RetFatalOthers = 1006, Unclassified = 1007, Disqualified = 1008, DriverHealth = 1009, RetUnexplained = 1010 };

        internal int HighestPosition()
        {
            int mx = 0;
            foreach (Voiture v in _voitures.Values)
            {
                if (v.RacePosition < 999)
                {
                    if (v.RacePosition > mx) { mx = v.RacePosition; }
                }
                if (v.GridPosition < 999)
                {
                    if (v.GridPosition > mx) { mx = v.GridPosition; }
                }
            }
            return mx;
        }

        internal void GetRecentDrivers(DateTime TargetDate, out List<int> CurrentSeason, out List<int> Last2Seasons)
        {
            // returns a list of driver keys including drivers who have competed within the last two seasons AND who are still alive
            List<int> conducteurs = _drivers.Keys.ToList();
            int mx = conducteurs.Max();
            bool[] occursThis = new bool[mx + 1];
            bool[] occursLast = new bool[mx + 1];
            foreach (Voiture c in _voitures.Values)
            {
                //TimeSpan ago = (TargetDate - c.RaceDate);
                int lastyear = TargetDate.Year - 1;
                if ((c.RaceDate.Year == lastyear) || (c.RaceDate.Year == TargetDate.Year)) // driver has featured this season or last
                {
                    for (int w = 0; w < 3; w++)
                    {
                        if (c.DriverKey(w) > 0)
                        {
                            if (Core.Instance.Drivers[c.DriverKey(w)].NotDiedBefore(TargetDate)) { occursLast[c.DriverKey(w)] = true; }
                        }
                    }
                    if (c.RaceDate.Year == TargetDate.Year) // driver has featured this season
                    {
                        for (int w = 0; w < 3; w++)
                        {
                            if (c.DriverKey(w) > 0)
                            {
                                if (Core.Instance.Drivers[c.DriverKey(w)].NotDiedBefore(TargetDate)) { occursThis[c.DriverKey(w)] = true; }
                            }
                        }
                    }
                }
            }
            CurrentSeason = new List<int>();
            Last2Seasons = new List<int>();
            foreach (int k in conducteurs)
            {
                if (occursThis[k]) { CurrentSeason.Add(k); }
                if (occursLast[k]) { Last2Seasons.Add(k); }
            }
        }

        internal void GetRecentConstructors(DateTime TargetDate, out List<int> Last12Months, out List<int> Last24Months)
        {
            List<int> teamKeys = _constructors.Keys.ToList();
            int mx = teamKeys.Max();
            bool[] occurs12 = new bool[mx + 1];
            bool[] occurs24 = new bool[mx + 1];
            foreach (Voiture c in _voitures.Values)
            {
                DateTime _raceDate = c.RaceDate;
                TimeSpan ago = (TargetDate - _raceDate);
                if ((ago.TotalDays < 730) && (ago.TotalDays >= 0)) // later than 2 years ago but not in the future
                {
                    occurs24[c.ConstructorKey] = true;
                    if (ago.TotalDays < 365) // later than 1 years ago
                    {
                        occurs12[c.ConstructorKey] = true;
                    }
                }
            }
            Last12Months = new List<int>();
            Last24Months = new List<int>();
            foreach (int k in teamKeys)
            {
                if (occurs12[k]) { Last12Months.Add(k); }
                if (occurs24[k]) { Last24Months.Add(k); }
            }
        }

        internal List<int> LivingDrivers(DateTime TargetDate)
        {
            // returns a list of driver keys including drivers are still alive at the given date
            List<int> conducteurs = _drivers.Keys.ToList();
            List<int> vivants = new List<int>();
            foreach (int k in conducteurs)
            {
                if (Core.Instance.Drivers[k].NotDiedBefore(TargetDate)) { vivants.Add(k); }
            }
            return vivants;
        }

        internal List<string> PointsValuesPosition()
        {
            List<RacePoints> ptList = new List<RacePoints>();
            List<string> range = new List<string>();
            foreach (Voiture r in _voitures.Values)
            {
                ptList.Add(r.PointsForPosition(0));
                ptList.Add(r.PointsForPosition(1));
                ptList.Add(r.PointsForPosition(2));
            }
            ptList.Sort();
            foreach (RacePoints rp in ptList)
            {
                string rep = rp.Representation;
                if (!range.Contains(rep)) { range.Add(rep); }
            }
            return range;
        }

        internal List<string> PointsValuesLap()
        {
            List<RacePoints> ptList = new List<RacePoints>();
            List<string> range = new List<string>();
            foreach (Voiture r in _voitures.Values)
            {
                ptList.Add(r.PointsForFastestLap(0));
                ptList.Add(r.PointsForFastestLap(1));
                ptList.Add(r.PointsForFastestLap(2));
            }
            ptList.Sort();
            foreach (RacePoints rp in ptList)
            {
                string rep = rp.Representation;
                if (!range.Contains(rep)) { range.Add(rep); }
            }
            return range;
        }

        internal List<string> PointsValuesSprintQualifying()
        {
            List<RacePoints> ptList = new List<RacePoints>();
            List<string> range = new List<string>();
            foreach (Voiture r in _voitures.Values)
            {
                ptList.Add(r.PointsForSprintQualifying(0));
                ptList.Add(r.PointsForSprintQualifying(1));
                ptList.Add(r.PointsForSprintQualifying(2));
            }
            ptList.Sort();
            foreach (RacePoints rp in ptList)
            {
                string rep = rp.Representation;
                if (!range.Contains(rep)) { range.Add(rep); }
            }
            return range;
        }

        internal int Qualifiers(int raceKey)
        {
            int q = 0;
            foreach (Voiture r in _voitures.Values)
            {
                if (r.RaceMeetingKey == raceKey) { q++; }
            }
            return q;
        }

        internal RankingHealth GridHealth(int raceKey)
        {
            List<Voiture> entrants = new List<Voiture>();
            foreach (Voiture r in _voitures.Values)
            {
                if (r.RaceMeetingKey == raceKey) { entrants.Add(r); }
            }
            int gridmax = 0;
            int gaps = 0;
            int dupes = 0;
            foreach (Voiture cr in entrants) { if (cr.GridPosition != Core.SpecialNumber) { gridmax = Math.Max(gridmax, cr.GridPosition); } }
            int[] filled = new int[gridmax + 1];
            foreach (Voiture cr in entrants) { if (cr.GridPosition != Core.SpecialNumber) { filled[cr.GridPosition]++; } }

            for (int p = 1; p <= gridmax; p++)
            {
                if (filled[p] == 0) { gaps++; }
                if (filled[p] > 1) { dupes++; }
            }
            RankingHealth health = RankingHealth.Healthy;
            if (gaps > 0)
            {
                if (dupes > 0)
                {
                    health = RankingHealth.HasGapsAndDupes;
                }
                else
                {
                    health = RankingHealth.HasGaps;
                }
            }
            else
            {
                if (dupes > 0)
                {
                    health = RankingHealth.HasDuplicates;
                }
            }
            return health;
        }

        internal List<int> GridFilledPlaces(int raceKey)
        {
            List<int> filled = new List<int>();
            foreach (Voiture r in _voitures.Values)
            {
                if (r.RaceMeetingKey == raceKey)
                {
                    if (r.GridPosition != Core.SpecialNumber) { filled.Add(r.GridPosition); }
                }
            }
            filled.Sort();
            return filled;
        }

        internal List<int> RaceVacancies(int raceKey)
        {
            List<int> vacant = new List<int>();
            for (int p = 1; p <= MaxEntrants; p++) { vacant.Add(p); }
            foreach (Voiture r in _voitures.Values)
            {
                if (r.RaceMeetingKey == raceKey)
                {
                    if (r.RacePosition <= MaxEntrants)
                        vacant.Remove(r.RacePosition);
                }
            }
            vacant.Sort();
            return vacant;
        }

        internal List<int> TeamsInRace(int raceKey)
        {
            List<int> indices = new List<int>();
            foreach (Voiture r in _voitures.Values)
            {
                if (r.RaceMeetingKey == raceKey)
                {
                    int tm = r.ConstructorKey;
                    if (!indices.Contains(tm)) { indices.Add(tm); }
                }
            }
            List<NamedItem> sortable = new List<NamedItem>();
            foreach (int x in indices) { sortable.Add(new NamedItem(Constructors[x].Caption, x)); }
            sortable.Sort();
            indices.Clear();
            foreach (NamedItem ni in sortable) { indices.Add(ni.Key); }
            return indices;
        }
        internal List<int> TeamsInSeason(int annee)
        {
            List<int> indices = new List<int>();
            foreach (Voiture r in _voitures.Values)
            {
                if (r.RaceDate.Year == annee)
                {
                    int tm = r.ConstructorKey;
                    if (!indices.Contains(tm)) { indices.Add(tm); }
                }
            }
            List<NamedItem> sortable = new List<NamedItem>();
            foreach (int x in indices) { sortable.Add(new NamedItem(Constructors[x].Caption, x)); }
            sortable.Sort();
            indices.Clear();
            foreach (NamedItem ni in sortable) { indices.Add(ni.Key); }
            return indices;
        }
        internal List<int> DriversinTeamInRace(int raceKey, int teamKey)
        {
            List<int> indices = new List<int>();
            foreach (Voiture r in _voitures.Values)
            {
                if (r.RaceMeetingKey == raceKey)
                {
                    if (r.ConstructorKey == teamKey)
                    {
                        for (int x = 0; x < r.DriverCount; x++)
                        {
                            if (!indices.Contains(r.DriverKey(x))) { indices.Add(r.DriverKey(x)); }
                        }
                    }
                }
            }
            List<NamedItem> sortable = new List<NamedItem>();
            foreach (int x in indices) { sortable.Add(new NamedItem(Drivers[x].SortingName, x)); }
            sortable.Sort();
            indices.Clear();
            foreach (NamedItem ni in sortable) { indices.Add(ni.Key); }
            return indices;
        }
        internal List<int> DriversinTeamInSeason(int annee, int teamKey)
        {
            List<int> indices = new List<int>();
            foreach (Voiture r in _voitures.Values)
            {
                if (r.RaceDate.Year == annee)
                {
                    if (r.ConstructorKey == teamKey)
                    {
                        for (int x = 0; x < r.DriverCount; x++)
                        {
                            if (!indices.Contains(r.DriverKey(x))) { indices.Add(r.DriverKey(x)); }
                        }
                    }
                }
            }
            List<NamedItem> sortable = new List<NamedItem>();
            foreach (int x in indices) { sortable.Add(new NamedItem(Drivers[x].SortingName, x)); }
            sortable.Sort();
            indices.Clear();
            foreach (NamedItem ni in sortable) { indices.Add(ni.Key); }
            return indices;
        }
        internal int Finishers(int raceKey)
        {
            int q = 0;
            foreach (Voiture r in _voitures.Values)
            {
                if (r.RaceMeetingKey == raceKey)
                {
                    if (r.Finished) { q++; }
                }
            }
            return q;
        }

        internal List<Tuple<int, int, int>> CompetitorRankings(int raceKey)
        {
            List<Voiture> cars = new List<Voiture>();
            foreach (Voiture r in _voitures.Values)
            {
                if (r.RaceMeetingKey == raceKey)
                {
                    cars.Add(r);
                }
            }
            List<int> gridPositions = new List<int>();
            foreach (Voiture r in cars)
            {
                if (r.GridPosition != Core.SpecialNumber) { gridPositions.Add(r.GridPosition); }
            }

            List<Tuple<int, int, int>> enders = new List<Tuple<int, int, int>>();
            if (gridPositions.Count > 0)
            {
                int last = gridPositions.Max();
                foreach (Voiture r in cars)
                {
                    int p = r.GridPosition;
                    if (p == Core.SpecialNumber) { last++; p = last; } // any cars starting from the pit lane are added to the end of the grid
                    Tuple<int, int, int> d = new Tuple<int, int, int>(p, r.RacePosition, r.DriverKey(0));
                    enders.Add(d);
                }
            }
            return enders;
        }

        internal Tuple<int, int, int> PlacedDriverKeys(int race, int podium)
        {
            Tuple<int, int, int> q = new Tuple<int, int, int>(0, 0, 0);
            foreach (Voiture r in _voitures.Values)
            {
                if (r.RaceMeetingKey == race)
                {
                    if (r.RacePosition == podium)
                    { q = new Tuple<int, int, int>(r.DriverKey(0), r.DriverKey(1), r.DriverKey(2)); }
                }
            }
            return q;
        }

        internal static string DateToCode(DateTime dt)
        {
            return dt.ToString("yyyyMMdd", Core.CultureUK);
        }

        internal static DateTime DateFromCode(string cd)
        {
            string ys = cd.Substring(0, 4);
            string ms = cd.Substring(4, 2);
            string ds = cd.Substring(6, 2);
            int yi = int.Parse(ys, Core.CultureUK);
            int mi = int.Parse(ms, Core.CultureUK);
            int di = int.Parse(ds, Core.CultureUK);
            DateTime dt = new DateTime(yi, mi, di);
            return dt;
        }

        internal void SaveData()
        {
            string path = Jbh.AppManager.DataPath;
            path = System.IO.Path.Combine(path, "F1Data.txt");
            Jbh.AppManager.CreateBackupDataFile(path);
            Jbh.AppManager.PurgeOldBackups("txt", 40, 3);
            // NB all specifications use System.Globalization.CultureInfo.InvariantCulture to ensure consistent interpretation
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(path))
            {
                List<Season> ssn = _seasons.Values.ToList();
                ssn.Sort();
                foreach (Season i in ssn)
                {
                    sw.WriteLine("SEASO:" + i.Specification);
                }
                List<NamedItem> con = _constructors.Values.ToList();
                con.Sort();
                foreach (NamedItem i in con)
                {
                    sw.WriteLine("CONST:" + i.Specification);
                }
                con = _countries.Values.ToList();
                con.Sort();
                foreach (NamedItem i in con)
                {
                    sw.WriteLine("CNTRY:" + i.Specification);
                }
                List<Driver> drv = _drivers.Values.ToList();
                drv.Sort();
                foreach (Driver i in drv)
                {
                    sw.WriteLine("DRIVR:" + i.Specification);
                }
                List<RaceMeeting> mtg = _races.Values.ToList();
                mtg.Sort();
                foreach (RaceMeeting i in mtg)
                {
                    sw.WriteLine("MEETG:" + i.Specification);
                }
                con = _raceTitles.Values.ToList();
                con.Sort();
                foreach (NamedItem i in con)
                {
                    sw.WriteLine("GPTIT:" + i.Specification);
                }
                List<Circuit> trx = _raceTracks.Values.ToList();
                trx.Sort();
                foreach (Circuit i in trx)
                {
                    if (VenueAnyOccurrence(i.Key))
                    {
                        sw.WriteLine("CIRCT:" + i.Specification);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show($"Forgetting unreferenced circuit {i.CircuitSingleTitle(0)}", Jbh.AppManager.AppName, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                }
                List<Voiture> voi = _voitures.Values.ToList();
                voi.Sort();
                foreach (Voiture i in voi)
                {
                    sw.WriteLine("COMPT:" + i.Specification);
                }
            }
        }

        internal void LoadData()
        {
            _constructors.Clear();
            _countries.Clear();
            _drivers.Clear();
            _voitures.Clear();
            _raceTitles.Clear();
            _raceTracks.Clear();
            _races.Clear();
            _seasons.Clear();

            string path = Jbh.AppManager.DataPath;
            path = System.IO.Path.Combine(path, "F1Data.txt");
            if (System.IO.File.Exists(path))
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(path))
                {
                    while (!sr.EndOfStream)
                    {
                        string dat = sr.ReadLine();
                        string clue = dat.Substring(0, 6);
                        string content = dat.Substring(6);
                        switch (clue)
                        {
                            case "CONST:":
                                {
                                    NamedItem ting = new NamedItem(content);
                                    _constructors.Add(ting.Key, ting);
                                    break;
                                }
                            case "CNTRY:":
                                {
                                    NamedItem ting = new NamedItem(content);
                                    _countries.Add(ting.Key, ting);
                                    break;
                                }
                            case "DRIVR:":
                                {
                                    Driver driv = new Driver
                                    {
                                        Specification = content
                                    };
                                    _drivers.Add(driv.Key, driv);
                                    break;
                                }
                            case "MEETG:":
                                {
                                    RaceMeeting rmtg = new RaceMeeting
                                    {
                                        Specification = content
                                    };
                                    _races.Add(rmtg.Key, rmtg);
                                    break;
                                }
                            case "GPTIT:":
                                {
                                    NamedItem ting = new NamedItem(content);
                                    _raceTitles.Add(ting.Key, ting);
                                    break;
                                }
                            case "CIRCT:":
                                {
                                    Circuit trak = new Circuit()
                                    {
                                        Specification = content
                                    };
                                    _raceTracks.Add(trak.Key, trak);
                                    break;
                                }
                            case "COMPT:":
                                {
                                    int k = 1;
                                    while (_voitures.ContainsKey(k)) { k++; }
                                    Voiture raco = new Voiture(k) { Specification = content };
                                    _voitures.Add(k, raco);
                                    break;
                                }
                            case "SEASO:":
                                {
                                    Season seas = new Season()
                                    {
                                        Specification = content
                                    };
                                    _seasons.Add(seas.Anno, seas);
                                    break;
                                }
                        }
                    }
                }
            }
            HouseKeeping();
        }

        internal List<int> Years
        {
            get
            {
                List<int> lst = new List<int>();
                foreach (RaceMeeting mtg in _races.Values)
                {
                    int y = mtg.RaceDate.Year;
                    if (!lst.Contains(y)) { lst.Add(y); }
                }
                lst.Sort();
                return lst;
            }
        }

        internal List<int> Decades
        {
            get
            {
                List<int> lst = new List<int>();
                foreach (RaceMeeting mtg in _races.Values)
                {
                    string f = mtg.RaceDate.Year.ToString(CultureUK);
                    f = f.Substring(0, 3);
                    int d = int.Parse(f, CultureUK);
                    if (!lst.Contains(d)) { lst.Add(d); }
                }
                lst.Sort();
                return lst;
            }
        }

        internal void HouseKeeping()
        {
            CleanUpSeasons();
            UpdateDriverRuntimeProperties();
            NumberRaces();
        }
        private void CleanUpSeasons()
        {
            // remove any seasons to which none of our races corresponds ...
            List<int> annees = Years;
            List<int> condem = new List<int>();
            foreach (int y in _seasons.Keys)
            {
                if (!annees.Contains(y)) { condem.Add(y); }
            }
            foreach (int y in condem)
            {
                _seasons.Remove(y);
            }
            // create racing seasons which are missing ...
            foreach (int y in annees)
            {
                if (!_seasons.ContainsKey(y))
                {
                    Season s = new Season() { Anno = y, PointsAllocationSchemeForDrivers = new SeasonPointsAllocationScheme(0, 0, 0), PointsAllocationSchemeForConstructors = new SeasonPointsAllocationScheme(0, 0, 0) };
                    _seasons.Add(y, s);
                }
            }
        }

        internal static string MyFormat(float f)
        {
            string q = f.ToString("0.00", Core.CultureUK);
            while (q.EndsWith("0", StringComparison.OrdinalIgnoreCase)) { q = q.Substring(0, q.Length - 1); }
            if (q.EndsWith(".", StringComparison.OrdinalIgnoreCase)) { q = q.Substring(0, q.Length - 1); }
            return q;
        }

        internal bool AlreadyRaceOnDate(DateTime d)
        {
            bool deja = false;
            foreach (RaceMeeting m in _races.Values)
            {
                if (m.RaceDate.Date == d.Date) { deja = true; break; }
            }
            return deja;
        }

        internal static float DriverRacePoints(int RaceKey, int DriverKey)
        {
            float p = 0;
            foreach (Voiture v in Core.Instance.Voitures.Values)
            {
                if (v.RaceMeetingKey == RaceKey)
                {
                    float? f = v.DriverPoints(DriverKey);
                    if (f.HasValue) { p += f.Value; }
                }
            }
            return p;
        }

        internal static string Ordinal(int r)
        {
            string v = r.ToString(Core.CultureUK);
            long tens = r % 100;
            long units = tens % 10;
            tens /= 10;
            switch (tens)
            {
                case 1:
                    {
                        v += "th";
                        break;
                    }
                default:
                    {
                        switch (units)
                        {
                            case 1:
                                {
                                    v += "st";
                                    break;
                                }
                            case 2:
                                {
                                    v += "nd";
                                    break;
                                }
                            case 3:
                                {
                                    v += "rd";
                                    break;
                                }
                            default:
                                {
                                    v += "th";
                                    break;
                                }
                        }
                        break;
                    }
            }
            return v;
        }
        internal List<Tuple<int, int>> CircuitGrandPrixPairings()
        {
            List<Tuple<int, int>> retlist = new List<Tuple<int, int>>();
            foreach (RaceMeeting rm in Races.Values)
            {
                int ck = rm.CircuitKey;
                int gk = rm.RaceTitleKey;
                Tuple<int, int> pair = new Tuple<int, int>(ck, gk);
                if (!retlist.Contains(pair)) { retlist.Add(pair); }
            }
            return retlist;
        }

        private void NumberRaces()
        {
            List<RaceMeeting> pistes = _races.Values.ToList();
            pistes.Sort();
            int anno = 0;
            int annoR = 0;
            for (int r = 1; r <= pistes.Count; r++)
            {
                pistes[r - 1].SerialNumber = r;
                if (anno == pistes[r - 1].RaceDate.Year)
                {
                    annoR++;
                    pistes[r - 1].YearSerialNumber = annoR;
                }
                else
                {
                    anno = pistes[r - 1].RaceDate.Year;
                    pistes[r - 1].YearSerialNumber = annoR = 1;
                }
            }
        }

        internal static string WikiLinkPoints { get { return "https://en.wikipedia.org/wiki/List_of_Formula_One_World_Championship_points_scoring_systems"; } }

        private void UpdateDriverRuntimeProperties()
        {
            List<int> racesQualifiedKeys; // use of these lists ensures against double-counting when drivers share cars
            List<int> racesStartedKeys;
            List<int> racesFinishedKeys;
            List<int> yearsRaced;
            DateTime firstracedate;
            DateTime lastracedate;
            DateTime firstQualDate;
            DateTime lastQualDate;

            int firstrace;
            int lastrace;
            int poles;
            foreach (int k in _drivers.Keys)
            {
                racesStartedKeys = new List<int>();
                racesFinishedKeys = new List<int>();
                racesQualifiedKeys = new List<int>();
                yearsRaced = new List<int>();
                firstracedate = DateTime.MaxValue;
                lastracedate = DateTime.MinValue;
                firstQualDate = DateTime.MaxValue;
                lastQualDate = DateTime.MinValue;
                firstrace = 0;
                lastrace = 0;
                poles = 0;
                foreach (Voiture v in _voitures.Values)
                {
                    if (v.IncludesDriver(k))
                    {
                        int rr = v.RaceMeetingKey;
                        if (!racesQualifiedKeys.Contains(rr)) { racesQualifiedKeys.Add(rr); }
                        RaceMeeting mtg = _races[rr];
                        DateTime rd = mtg.RaceDate;
                        if (!yearsRaced.Contains(rd.Year)) { yearsRaced.Add(rd.Year); }
                        if (v.GridPosition == 1) { poles++; }
                        if (rd < firstQualDate) { firstQualDate = rd; }
                        if (rd > lastQualDate) { lastQualDate = rd; }
                        if (v.RacePosition != (int)Core.RaceResultConstants.DidNotStart)
                        {
                            if (!racesStartedKeys.Contains(rr)) { racesStartedKeys.Add(rr); }
                            if (rd < firstracedate) { firstracedate = rd; firstrace = rr; }
                            if (rd > lastracedate) { lastracedate = rd; lastrace = rr; }
                            if (v.RacePosition < 999)
                            {
                                if (!racesFinishedKeys.Contains(rr)) { racesFinishedKeys.Add(rr); }
                            }
                        }
                    }
                }
                yearsRaced.Sort();

                bool flag = false;
                string output = string.Empty;
                int start = 0;
                int last = yearsRaced[yearsRaced.Count - 1] + 1;
                for (int yr = yearsRaced[0]; yr <= last; yr++)
                {
                    if (yearsRaced.Contains(yr))
                    {
                        if (flag == false) { flag = true; start = yr; output += $"{yr}"; }
                    }
                    else
                    {
                        if (flag)
                        {
                            flag = false;
                            if (start == (yr - 1)) { output += $" "; } else { output += $"-{yr - 1} "; }
                        }
                    }
                }

                _drivers[k].RuntimeYears = output.Trim();

                _drivers[k].RuntimeFirstRaceStartDate = firstracedate;
                _drivers[k].RuntimeLastRaceStartDate = lastracedate;
                _drivers[k].RuntimeFirstQualifiedDate = firstQualDate;
                _drivers[k].RuntimeLastQualifiedDate = lastQualDate;
                _drivers[k].RuntimeRacesStarted = racesStartedKeys.Count;
                _drivers[k].RuntimeFirstRaceKey = firstrace;
                _drivers[k].RuntimeLastRaceKey = lastrace;
                _drivers[k].RuntimePoles = poles;
                _drivers[k].RuntimeRacesFinished = racesFinishedKeys.Count;
                _drivers[k].RuntimeRacesQualified = racesQualifiedKeys.Count;
            }
        }

        internal List<Tuple<DateTime, int, bool>> DriversDiedSinceLastRace(int raceKey)
        {
            List<Tuple<DateTime, int, bool>> returnList = new List<Tuple<DateTime, int, bool>>();
            DateTime racedate = _races[raceKey].RaceDate;
            DateTime previousracedate = DateTime.MinValue;
            foreach (RaceMeeting m in _races.Values)
            {
                DateTime rd = m.RaceDate;
                if (rd < racedate)
                {
                    if (rd > previousracedate) { previousracedate = rd; }
                }
            }
            if (previousracedate < Core.DateBase) { return returnList; } // this was the earliest race - no previous!
            previousracedate = previousracedate.AddDays(-1); // include drivers killed on last race day
            foreach (Driver d in _drivers.Values)
            {
                DateTime dd = d.DeathDate;
                if ((dd > previousracedate) && (dd < racedate))
                {
                    Tuple<DateTime, int, bool> tup = new Tuple<DateTime, int, bool>(dd, d.Key, (d.HowDied != CauseOfDeath.Natural));
                    returnList.Add(tup);
                }
            }
            returnList.Sort();
            return returnList;
        }

        internal List<Tuple<DateTime, int, bool>> DriversDiedBeforeNextRace(int raceKey)
        {
            List<Tuple<DateTime, int, bool>> returnList = new List<Tuple<DateTime, int, bool>>();
            DateTime racedate = _races[raceKey].RaceDate;
            DateTime subsequentracedate = DateTime.MaxValue;
            foreach (RaceMeeting m in _races.Values)
            {
                DateTime rd = m.RaceDate;
                if (rd > racedate)
                {
                    if (rd < subsequentracedate) { subsequentracedate = rd; }
                }
            }
            if (subsequentracedate == DateTime.MaxValue) { return returnList; } // this was the latest race - no subsequent!
            racedate = racedate.AddDays(-1); // include drivers killed on this race day
            foreach (Driver d in _drivers.Values)
            {
                DateTime dd = d.DeathDate;
                if ((dd < subsequentracedate) && (dd > racedate))
                {
                    Tuple<DateTime, int, bool> tup = new Tuple<DateTime, int, bool>(dd, d.Key, (d.HowDied != CauseOfDeath.Natural));
                    returnList.Add(tup);
                }
            }
            returnList.Sort();
            return returnList;
        }

        internal Tuple<int, int> ConstructorFirstLastYears(int team)
        {
            int fy = int.MaxValue;
            int ly = 0;
            foreach (Voiture v in _voitures.Values)
            {
                if (v.ConstructorKey == team) { int y = v.RaceDate.Year; fy = Math.Min(fy, y); ly = Math.Max(ly, y); }
            }
            return new Tuple<int, int>(fy, ly);
        }

        private bool VenueAnyOccurrence(int venu)
        {
            bool flag = false;
            foreach (int k in Races.Keys)
            {
                if (Races[k].CircuitKey == venu) { flag = true; break; }
            }
            return flag;
        }

        internal int MostRecentConstructorForDriver(int drv, DateTime asat)
        {
            List<Tuple<DateTime, int>> teamEvents = new List<Tuple<DateTime, int>>();
            foreach (Voiture v in _voitures.Values)
            {
                if (v.RaceDate < asat)
                {
                    if (v.IncludesDriver(drv)) { teamEvents.Add(new Tuple<DateTime, int>(v.RaceDate, v.ConstructorKey)); }
                }
            }
            teamEvents.Sort();
            if (teamEvents.Count < 1) { return 0; } // e.g. for new driver
            return teamEvents[teamEvents.Count - 1].Item2;
        }

        /// <summary>
        /// returns the driver's surname, with a first name only if necessary to distinguish drivers
        /// </summary>
        /// <param name="driverKey"></param>
        /// <returns></returns>
        internal string DriverShortName(int driverKey)
        {
            string shorty = _drivers[driverKey].Surname;
            int ct = 0;
            foreach (Driver d in _drivers.Values) { if (d.Surname == shorty) { ct++; } }
            if (ct > 1) { shorty = _drivers[driverKey].FullName; }
            return shorty;
        }

        private double AgeDaysOnRaceDay(int RaceKey, int DriverKey)
        {
            DateTime born = Drivers[DriverKey].BirthDate;
            DateTime race = Races[RaceKey].RaceDate;
            return (race - born).TotalDays;
        }

        public static Tuple<int, int> AgeYearsDays(DateTime birth, DateTime death)
        {
            DateTime temp = birth;
            int years = 0;
            while (temp.Year < death.Year) { years++; temp = temp.AddYears(1); }
            if (temp.DayOfYear > death.DayOfYear) { years--; temp = temp.AddYears(-1); }
            TimeSpan t = death - temp;
            int days = (int)t.TotalDays;
            return new Tuple<int, int>(years, days);
        }

        internal List<Tuple<int, int>> OldestAndYoungestDriversOnRaceDay()
        {
            List<Tuple<int, int>> TopAndTail = new List<Tuple<int, int>>();
            List<Tuple<double, int, int>> Alphas = new List<Tuple<double, int, int>>();
            List<Tuple<double, int, int>> Omegas = new List<Tuple<double, int, int>>();

            foreach (int dk in Drivers.Keys)
            {
                Tuple<int, int> alphaomega = FirstLastRacesForDriver(dk);
                Tuple<double, int, int> Q = new Tuple<double, int, int>(AgeDaysOnRaceDay(alphaomega.Item1, dk), alphaomega.Item1, dk);
                Alphas.Add(Q);
                Q = new Tuple<double, int, int>(AgeDaysOnRaceDay(alphaomega.Item2, dk), alphaomega.Item2, dk);
                Omegas.Add(Q);
            }
            Alphas.Sort();
            Omegas.Sort();
            Omegas.Reverse();
            for (int a = 0; a < 50; a++)
            {
                TopAndTail.Add(new Tuple<int, int>(Alphas[a].Item2, Alphas[a].Item3));
            }
            for (int a = 0; a < 50; a++)
            {
                TopAndTail.Add(new Tuple<int, int>(Omegas[a].Item2, Omegas[a].Item3));
            }
            return TopAndTail;
        }

        internal List<Tuple<int, int>> LongestDriverCareers()
        {
            List<Tuple<int, int>> Careers = new List<Tuple<int, int>>();

            foreach (int dk in Drivers.Keys)
            {
                DateTime df = Drivers[dk].RuntimeFirstQualifiedDate;
                DateTime dl = Drivers[dk].RuntimeLastQualifiedDate;
                TimeSpan span = dl - df;
                Tuple<int, int> Q = new Tuple<int, int>(span.Days, dk);
                Careers.Add(Q);
            }
            Careers.Sort();
            Careers.Reverse();
            return Careers;
        }

        private Tuple<int, int> FirstLastRacesForDriver(int d)
        {
            DateTime firstdate = DateTime.MaxValue;
            DateTime lastdate = DateTime.MinValue;
            int firstrace = 0;
            int lastrace = 0;
            foreach (Voiture v in Voitures.Values)
            {
                if ((v.DriverKey(0) == d) || (v.DriverKey(1) == d) || (v.DriverKey(2) == d))
                {
                    DateTime racedate = v.RaceDate;
                    if (firstdate > racedate)
                    {
                        firstdate = racedate;
                        firstrace = v.RaceMeetingKey;
                    }
                    if (lastdate < racedate)
                    {
                        lastdate = racedate;
                        lastrace = v.RaceMeetingKey;
                    }
                }
            }
            return new Tuple<int, int>(firstrace, lastrace);
        }

        internal int DriversBorn(int decade)
        {
            List<int> Q = DriversBornInDecade(decade);
            return Q.Count;
        }

        private List<int> DriversBornInDecade(int decadeStart)
        {
            List<int> lst = new List<int>();
            foreach (Driver d in Drivers.Values)
            {
                int byr = d.BirthDate.Year;
                int dif = byr - decadeStart;
                if ((dif >= 0) && (dif <= 9)) { lst.Add(d.Key); }
            }
            return lst;
        }

        internal int DriversAlive(int decade)
        {
            List<int> Q = DriversBornInDecade(decade);
            int a = 0;
            foreach (int k in Q)
            {
                if (Drivers[k].DeathDate < Core.DateBase) { a++; }
            }
            return a;
        }
        internal int DriversKilledRacing(int decade)
        {
            List<int> Q = DriversBornInDecade(decade);
            int a = 0;
            foreach (int k in Q)
            {
                if (Drivers[k].HowDied == Core.CauseOfDeath.RacingAccident) { a++; }
            }
            return a;
        }

        internal int DriversKilledRaceRelated(int decade)
        {
            List<int> Q = DriversBornInDecade(decade);
            int a = 0;
            foreach (int k in Q)
            {
                if ((Drivers[k].HowDied == CauseOfDeath.RacingAccident) || (Drivers[k].HowDied == CauseOfDeath.RacePracticeOrTestingAccident)) { a++; }
            }
            return a;
        }

        internal int DriversKilledAnyAccident(int decade)
        {
            List<int> Q = DriversBornInDecade(decade);
            int a = 0;
            foreach (int k in Q)
            {
                if ((Drivers[k].HowDied == CauseOfDeath.RacingAccident) || (Drivers[k].HowDied == CauseOfDeath.RacePracticeOrTestingAccident) || (Drivers[k].HowDied == CauseOfDeath.OtherAccident)) { a++; }
            }
            return a;
        }

        internal int DriverRaceWins(int drv)
        {
            List<int> RaceKeys = new List<int>();
            foreach (Voiture v in Voitures.Values)
            {
                if (v.RacePosition == 1)
                {
                    if (v.IncludesDriver(drv))
                    {
                        RaceKeys.Add(v.RaceMeetingKey);
                    }
                }
            }
            return RaceKeys.Count;
        }

        internal int DriverRaceWinsUpTo(int drv, DateTime dat)
        {
            List<int> RaceKeys = new List<int>();
            foreach (Voiture v in Voitures.Values)
            {
                if (v.RacePosition == 1)
                {
                    if (Races[v.RaceMeetingKey].RaceDate <= dat)
                    {
                        if (v.IncludesDriver(drv))
                        {
                            RaceKeys.Add(v.RaceMeetingKey);
                        }
                    }
                }
            }
            return RaceKeys.Count;
        }

        internal int DriverMostConsecutiveRaceWins(int drv)
        {
            List<RaceMeeting> pistes = _races.Values.ToList();
            pistes.Sort();
            int career = 0;
            foreach (RaceMeeting p in pistes)
            {
                int snake = p.ConWins.DriverRun(drv);
                if (snake > career) { career = snake; }
            }
            return career;
        }

        private List<int> RaceMeetingKeysByDate()
        {
            List<RaceMeeting> pistes = _races.Values.ToList();
            List<int> clefs = new List<int>();
            pistes.Sort();
            foreach (RaceMeeting m in pistes)
            {
                clefs.Add(m.Key);
            }
            return clefs;
        }

        internal void UpdateRaceConsecutiveRaceRecords()
        {
            List<int> pisteKeys = RaceMeetingKeysByDate();
            for (int j = 0; j < pisteKeys.Count; j++)
            {
                int m = pisteKeys[j];
                _races[m].ConWins = new ConsecutiveWins();
                if (j > 0) { int n = pisteKeys[j - 1]; _races[m].ConWins.BringForward(_races[n].ConWins); }
                Tuple<int, int, int> toppers = _races[m].PodiumOne;
                _races[m].ConWins.RegisterDrivers(toppers);
            }
        }

        internal int DriverRacePodiums(int drv)
        {
            List<int> RaceKeys = new List<int>();
            foreach (Voiture v in Voitures.Values)
            {
                if (v.RacePosition < 4)
                {
                    if (v.IncludesDriver(drv))
                    {
                        RaceKeys.Add(v.RaceMeetingKey);
                    }
                }
            }
            return RaceKeys.Count;
        }

        internal List<float> ScoringSystemForLastRaceBefore(DateTime dat)
        {
            List<RaceMeeting> meetings = Races.Values.ToList();
            meetings.Sort(); // by date order
            int j = -1;
            for (int z = 0; z < meetings.Count; z++)
            {
                if (meetings[z].RaceDate < dat) { j = z; }
            }
            // j is now the last meeting prior to given date
            bool ok = false;
            float[] scheme = Array.Empty<float>();
            if (j < 0) { return scheme.ToList(); }
            while (!ok)
            {
                scheme = DetectMeetingPointsAwardScheme(meetings[j], out bool tick);
                j--;
                ok = tick;
            }; // until we get a scheme with no holes in it
            return scheme.ToList();
        }

        internal float DriversActualAllTimePoints(int driver)
        {
            float w = 0;
            foreach (Voiture v in Voitures.Values)
            {
                float? scor = v.DriverPoints(driver);
                if (scor.HasValue) { w += scor.Value; }
            }
            return w;
        }

        internal float DriversStandardisedAllTimePoints(int driver)
        { // uses the latest scoring system, ignoring any fastest lap points
            List<float> scoring = ScoringSystemForLastRaceBefore(DateTime.Today);
            float w = 0;
            foreach (Voiture v in Voitures.Values)
            {
                if (v.IncludesDriver(driver))
                {
                    int posn = v.RacePosition;
                    if (posn < scoring.Count) // within the scoring range
                    {
                        float pts = scoring[posn];
                        w += pts / v.DriverCount; // shared points if multiple drivers
                    }
                }
            }
            return w;
        }

        internal List<Tuple<float, int>> AllTimeActualDriverScores()
        {
            List<Tuple<float, int>> answ = new List<Tuple<float, int>>();
            foreach (int k in Drivers.Keys)
            {
                float s = DriversActualAllTimePoints(k);
                Tuple<float, int> glob = new Tuple<float, int>(s, k);
                answ.Add(glob);
            }
            answ.Sort();
            answ.Reverse();
            return answ;
        }

        internal List<Tuple<float, int>> AllTimeStandardDriverScores()
        {
            List<Tuple<float, int>> answ = new List<Tuple<float, int>>();
            foreach (int k in Drivers.Keys)
            {
                float s = DriversStandardisedAllTimePoints(k);
                Tuple<float, int> glob = new Tuple<float, int>(s, k);
                answ.Add(glob);
            }
            answ.Sort();
            answ.Reverse();
            return answ;
        }

        private float[] DetectMeetingPointsAwardScheme(RaceMeeting rm, out bool valid)
        {
            List<Voiture> results = new List<Voiture>();
            foreach (Voiture v in Voitures.Values)
            {
                if (v.RaceMeetingKey == rm.Key) { results.Add(v); }
            }
            // find the maximum finishing position for which points were awarded
            int mx = 0;
            foreach (Voiture v in results)
            {
                if (v.RacePosition < 1000)
                {
                    if (v.AggregatedPositionPoints > 0)
                    {
                        if (v.RacePosition > mx) { mx = v.RacePosition; }
                    }
                }
            }
            // create and fill an array of the scores awarded to the different finishing positions
            float[] reward = new float[mx + 1];
            foreach (Voiture v in results)
            {
                int p = v.RacePosition;
                if (p <= mx) { reward[p] = v.AggregatedPositionPoints; }
            }

            // discover whether there are any gaps in the reward scheme (e.g. by a driver gaining a position but not having been allowed points for some reason)
            bool flag = true;
            for (int a = 1; a < reward.Length; a++)
            {
                if (reward[a] == 0) { flag = false; }
            }
            valid = flag;
            return reward;
        }

        internal float ScoreForFirstPlace(int anno)
        {
            // returns the best points score awarded to a driver that season for race position (excluding fastest lap bonus)
            float retval = 0;
            foreach (Voiture v in Voitures.Values)
            {
                if (v.RaceDate.Year == anno)
                {
                    if (v.RacePosition == 1)
                    {
                        for (int chap = 0; chap < 3; chap++)
                        {
                            float pts = v.PointsForPosition(chap).FloatValue;
                            if (pts > retval) { retval = pts; }
                        }
                    }
                }
            }
            return retval;
        }

        internal float ScoreForFastestLap(int anno)
        {
            // returns the best points score awarded to a driver as a fastest lap bonus
            float retval = 0;
            foreach (Voiture v in Voitures.Values)
            {
                if (v.RaceDate.Year == anno)
                {
                    for (int chap = 0; chap < 3; chap++)
                    {
                        float pts = v.PointsForFastestLap(chap).FloatValue;
                        if (pts > retval) { retval = pts; }
                    }
                }
            }
            return retval;
        }
        internal float ScoreForSprintQualifying(int anno)
        {
            // returns the best points score awarded to a driver for sprint qualifying
            float retval = 0;
            foreach (Voiture v in Voitures.Values)
            {
                if (v.RaceDate.Year == anno)
                {
                    for (int chap = 0; chap < 3; chap++)
                    {
                        float pts =v.PointsForSprintQualifying(chap).FloatValue;
                        if (pts > retval) { retval = pts; }
                    }
                }
            }
            return retval;
        }
        internal List<Tuple<DateTime, int, int>> MostWinsByDate()
        {
            List<Tuple<DateTime, int>> winnerz = new List<Tuple<DateTime, int>>();
            foreach (Voiture r in Voitures.Values)
            {
                if (r.RacePosition == 1)
                {
                    DateTime f = r.RaceDate;
                    for (int a = 0; a < 3; a++)
                    {
                        int k = r.DriverKey(a);
                        if (k > 0) { winnerz.Add(new Tuple<DateTime, int>(f, k)); }
                    }
                }
            }
            winnerz.Sort();
            int mostest = 0;
            Dictionary<int, int> driveraccumulated = new Dictionary<int, int>();
            List<Tuple<DateTime, int, int>> mostwins = new List<Tuple<DateTime, int, int>>();
            foreach (Tuple<DateTime, int> rw in winnerz)
            {
                if (driveraccumulated.ContainsKey(rw.Item2))
                {
                    driveraccumulated[rw.Item2]++;
                }
                else
                {
                    driveraccumulated.Add(rw.Item2, 1);
                }
                if (driveraccumulated[rw.Item2] >= mostest)
                {
                    mostest = driveraccumulated[rw.Item2];
                    Tuple<DateTime, int, int> record = new Tuple<DateTime, int, int>(rw.Item1, rw.Item2, mostest);
                    mostwins.Add(record);
                }
            }
            return mostwins;
        }

        internal List<Tuple<DateTime, int, int>> MostConsecutiveWinsByDate()
        {
            List<int> quays = RaceMeetingKeysByDate();
            List<Tuple<DateTime, int, int>> answre = new List<Tuple<DateTime, int, int>>();
            int bestSnake = 0;
            int bestPerson = -1;
            foreach (int w in quays)
            {
                Tuple<int, int> hero = _races[w].ConWins.BestDriverRun();
                if (hero.Item2 > bestSnake)
                {
                    bestSnake = hero.Item2;
                    bestPerson = hero.Item1;
                    answre.Add(new Tuple<DateTime, int, int>(_races[w].RaceDate, bestPerson, bestSnake));
                }
                else if ((hero.Item2 == bestSnake) && (hero.Item1 != bestPerson))
                {
                    bestPerson = hero.Item1;
                    answre.Add(new Tuple<DateTime, int, int>(_races[w].RaceDate, bestPerson, bestSnake));
                }
            }
            return answre;
        }

        internal List<Tuple<DateTime, int, int>> MostPolesByDate()
        {
            List<Tuple<DateTime, int>> winnerz = new List<Tuple<DateTime, int>>();
            foreach (Voiture r in Voitures.Values)
            {
                if (r.GridPosition == 1)
                {
                    DateTime f = r.RaceDate;
                    for (int a = 0; a < 3; a++)
                    {
                        int k = r.DriverKey(a);
                        if (k > 0) { winnerz.Add(new Tuple<DateTime, int>(f, k)); }
                    }
                }
            }
            winnerz.Sort();
            int mostest = 0;
            Dictionary<int, int> driveraccumulated = new Dictionary<int, int>();
            List<Tuple<DateTime, int, int>> mostpoles = new List<Tuple<DateTime, int, int>>();
            foreach (Tuple<DateTime, int> rw in winnerz)
            {
                if (driveraccumulated.ContainsKey(rw.Item2))
                {
                    driveraccumulated[rw.Item2]++;
                }
                else
                {
                    driveraccumulated.Add(rw.Item2, 1);
                }
                if (driveraccumulated[rw.Item2] >= mostest)
                {
                    mostest = driveraccumulated[rw.Item2];
                    Tuple<DateTime, int, int> record = new Tuple<DateTime, int, int>(rw.Item1, rw.Item2, mostest);
                    mostpoles.Add(record);
                }
            }
            return mostpoles;
        }

        internal List<Tuple<DateTime, int, int>> MostPodiumsByDate()
        {
            List<Tuple<DateTime, int>> winnerz = new List<Tuple<DateTime, int>>();
            foreach (Voiture r in Voitures.Values)
            {
                if (r.RacePosition < 4)
                {
                    DateTime f = r.RaceDate;
                    for (int a = 0; a < 3; a++)
                    {
                        int k = r.DriverKey(a);
                        if (k > 0) { winnerz.Add(new Tuple<DateTime, int>(f, k)); }
                    }
                }
            }
            winnerz.Sort();
            int mostest = 0;
            Dictionary<int, int> driveraccumulated = new Dictionary<int, int>();
            List<Tuple<DateTime, int, int>> mostpods = new List<Tuple<DateTime, int, int>>();
            foreach (Tuple<DateTime, int> rw in winnerz)
            {
                if (driveraccumulated.ContainsKey(rw.Item2))
                {
                    driveraccumulated[rw.Item2]++;
                }
                else
                {
                    driveraccumulated.Add(rw.Item2, 1);
                }
                if (driveraccumulated[rw.Item2] >= mostest)
                {
                    mostest = driveraccumulated[rw.Item2];
                    Tuple<DateTime, int, int> record = new Tuple<DateTime, int, int>(rw.Item1, rw.Item2, mostest);
                    mostpods.Add(record);
                }
            }
            return mostpods;
        }

        internal List<int> CircuitKeysForGrandPrix(int gp)
        {
            List<int> tracks = new List<int>();
            foreach (RaceMeeting rm in Races.Values)
            {
                if (rm.RaceTitleKey == gp)
                {
                    if (!tracks.Contains(rm.CircuitKey)) { tracks.Add(rm.CircuitKey); }
                }
            }
            return tracks;
        }

        internal int FirstIncompleteSeason()
        {
            int ys = 1950;
            int yf = DateTime.Today.Year;
            Dictionary<int, int> seasonMeetCount = new Dictionary<int, int>();
            for (int a = ys; a <= yf; a++) { seasonMeetCount.Add(a, 0); }
            foreach (RaceMeeting rm in Core.Instance.Races.Values)
            {
                int b = rm.RaceDate.Year;
                seasonMeetCount[b]++;
            }
            int target = 0;
            for (int a = ys; a <= yf; a++)
            {
                if (seasonMeetCount.ContainsKey(a))
                {
                    if (Seasons.ContainsKey(a))
                    {
                        if (Seasons[a].StatedNumberOfRaces > seasonMeetCount[a])
                        {
                            target = a; break;
                        }
                    }
                }
            }
            return target;
        }

        internal List<Tuple<int, int>> DriversWhoQualifiedButNeverStarted()
        {
            List<Tuple<int, int>> returnList = new List<Tuple<int, int>>();
            foreach (int dkey in Drivers.Keys)
            {
                if (Drivers[dkey].RuntimeRacesStarted == 0)
                {
                    Tuple<int, int> item = new Tuple<int, int>(Drivers[dkey].RuntimeRacesQualified, dkey);
                    returnList.Add(item);
                }
            }
            returnList.Sort();
            returnList.Reverse();
            return returnList;
        }

        internal List<Tuple<int, int>> DriversWhoStartedButNeverFinished()
        {
            List<Tuple<int, int>> returnList = new List<Tuple<int, int>>();
            foreach (int dkey in Drivers.Keys)
            {
                if (Drivers[dkey].RuntimeRacesStarted > 0)
                {
                    if (Drivers[dkey].RuntimeRacesFinished == 0)
                    {
                        Tuple<int, int> item = new Tuple<int, int>(Drivers[dkey].RuntimeRacesStarted, dkey);
                        returnList.Add(item);
                    }
                }
            }
            returnList.Sort();
            returnList.Reverse();
            return returnList;
        }

        private int SeasonsRacedIn(int driverKey)
        {
            List<int> yers = new List<int>();
            foreach (Voiture v in Voitures.Values)
            {
                if (v.IncludesDriver(driverKey))
                {
                    if (v.RacePosition < 999)
                    {
                        DateTime dat = v.RaceDate;
                        if (!yers.Contains(dat.Year)) { yers.Add(dat.Year); }
                    }
                }
            }
            return yers.Count;
        }

        internal List<Tuple<int, int>> DriversWhoRacedInMostSeasons()
        {
            List<Tuple<int, int>> returnList = new List<Tuple<int, int>>();
            foreach (int dkey in Drivers.Keys)
            {
                Tuple<int, int> item = new Tuple<int, int>(SeasonsRacedIn(dkey), dkey);
                returnList.Add(item);
            }
            returnList.Sort();
            returnList.Reverse();
            return returnList;
        }

        internal List<Tuple<int, int, int, int>> CompadresOf(int driverNumber)
        {
            // For each driver I have competed against, return Tuple with driverId, races, beathim, beatme
            List<Tuple<int, int, int, int>> results = new List<Tuple<int, int, int, int>>();
            List<int> myFellowKeys = new List<int>();
            List<int> myMeetingKeys = new List<int>();
            foreach (Voiture v in Voitures.Values) // collect all my race meetings and competitor drivers
            {
                if (v.IncludesDriver(driverNumber))
                {
                    if (!myMeetingKeys.Contains(v.RaceMeetingKey))
                    {
                        myMeetingKeys.Add(v.RaceMeetingKey);
                        foreach (Voiture w in Voitures.Values)
                        {
                            if (w.RaceMeetingKey == v.RaceMeetingKey)
                            {
                                for (int seat = 0; seat < 3; seat++)
                                {
                                    int p = w.DriverKey(seat);
                                    if ((p != driverNumber) && (p > 0) && (!myFellowKeys.Contains(p))) { myFellowKeys.Add(p); }
                                }
                            }
                        }
                    }
                }
            }
            foreach (int bro in myFellowKeys)
            {
                int starts = 0;
                int wins = 0;
                int losses = 0;
                foreach (int mtg in myMeetingKeys)
                {
                    int moi = RaceOutcome(driverNumber, mtg);
                    int toi = RaceOutcome(bro, mtg);
                    if ((moi > -1) && (toi > -1)) // neither of us failed to start the race
                    {
                        starts++;
                        if (moi > toi) { wins++; } else { if (toi > moi) { losses++; } }
                    }
                }
                if (starts > 0)
                {
                    results.Add(new Tuple<int, int, int, int>(starts, wins, losses, bro)); // sortable by number of races and of wins
                }
            }
            results.Sort();
            results.Reverse();
            return results;
        }

        internal Dictionary<string, int> DriverRacePerformance()
        {
            Dictionary<string, int> Performances = new Dictionary<string, int>();
            foreach (Voiture v in Voitures.Values) 
            {
                for (int seat = 0; seat < 3; seat++)
                {
                    int p = v.DriverKey(seat);
                    if (p > 0)
                    {
                        string qui = $"{v.RaceMeetingKey}-{p}";
                        int pos = v.RacePosition;
                        if (pos != (int)RaceResultConstants.DidNotStart)
                        {
                            if (pos > 1000) 
                            { pos = 0; } // one of the 'did not finish' constants
                            else
                            {
                                pos = 100 - pos; // mirroring the RacePosition function, making a lower (but positive) ranking better
                            }
                            if (Performances.ContainsKey(qui))
                            {
                                if (pos > Performances[qui]) { Performances[qui] = pos; }
                            }
                            else
                            {
                                Performances.Add(qui, pos);
                            }
                        }
                    }
                }
            }
            return Performances;
        }

        private int RaceOutcome(int drvr, int rmtg)
        {
            List<int> outcomes = new List<int>() { -1 };
            foreach (Voiture v in Voitures.Values)
            {
                if (v.IncludesDriver(drvr))
                {
                    if (v.RaceMeetingKey == rmtg)
                    {
                        if (v.RacePosition != (int)RaceResultConstants.DidNotStart)
                        {
                            if (v.RacePosition > 1000) // did not finish constant
                            {
                                outcomes.Add(0);
                            }
                            else
                            {
                                outcomes.Add(100 - v.RacePosition);
                            }
                        }
                    }
                }
            }
            return outcomes.Max();  // best result in case driver featured in more than one car in the race
        }

        internal static string ConstructorName(string raw)
        {
            return raw.Replace('=', '-');
        }

        internal static string GlyphLiving { get { return $"{'\x00F6'}"; } }
        internal static string GlyphFatalAccident { get { return $"{'\x0085'}"; } }

        internal static System.Windows.Controls.StackPanel FrenchFlag()
        {
            System.Windows.Controls.StackPanel s = new System.Windows.Controls.StackPanel() { Orientation = System.Windows.Controls.Orientation.Horizontal, Margin=new System.Windows.Thickness(6,0,6,0) };
            s.Children.Add( new System.Windows.Shapes.Rectangle() { Fill = System.Windows.Media.Brushes.Blue, Width = 4, Height = 10 });
            s.Children.Add(new System.Windows.Shapes.Rectangle() { Fill = System.Windows.Media.Brushes.White, Width = 4, Height = 10 });
            s.Children.Add(new System.Windows.Shapes.Rectangle() { Fill = System.Windows.Media.Brushes.Red, Width = 4, Height = 10 });
            return s;
        }
        
        public static void LaunchWebPage(string webAddress)
        {
            System.Diagnostics.ProcessStartInfo pinfo = new(webAddress) { UseShellExecute = true };
            System.Diagnostics.Process.Start(pinfo);
        }
}