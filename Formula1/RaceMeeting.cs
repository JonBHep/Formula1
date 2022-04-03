using System;
using System.Collections.Generic;

namespace Formula1;

internal class RaceMeeting : IComparable<RaceMeeting>
{
    private DateTime _raceDate;
        private int _key;
        private int _circuitKey;
        private int _raceTitleKey;
        private bool _countForConstructorsChampionship;
        private int _serialNumber;
        private int _yearSerialNumber;
        /// <summary>
        /// The drivers and their results can be extracted from the global list of results, using the Voiture's driver key and RaceMeeting key
        /// </summary>
        internal DateTime RaceDate { get => _raceDate; set => _raceDate = value; }
        internal int Key { get => _key; set => _key = value; }
        internal int CircuitKey { get => _circuitKey; set => _circuitKey = value; }
        internal int RaceTitleKey { get => _raceTitleKey; set => _raceTitleKey = value; }

        internal ConsecutiveWins ConWins;
        internal int Qualifiers { get { return Core.Instance.Qualifiers(_key); } }
        internal int Finishers { get { return Core.Instance.Finishers(_key); } }

        internal double KendalTau()
        {
            List<Tuple<int, int, int>> classified = Core.Instance.CompetitorRankings(_key);
            List<Tuple<int, int>> enders = new List<Tuple<int, int>>();
            foreach (Tuple<int, int, int> car in classified)
            {
                Tuple<int, int> veh = new Tuple<int, int>(car.Item1, car.Item2);
                enders.Add(veh);
            }
            int n = enders.Count;
            int pairings = (n * (n - 1)) / 2;
            // compare each pair
            int conc = 0;
            int disc = 0;
            for (int a = 0; a < (n - 1); a++)
            {
                for (int b = a + 1; b < n; b++)
                {
                    if (Concordant(enders[a], enders[b]))
                    { conc++; }
                    else
                    { disc++; }
                }
            }
            double Tau = (conc - disc) / (double)pairings;
            return Tau;
        }

        private static bool Concordant(Tuple<int, int> ta, Tuple<int, int> tb)
        {
            if (((ta.Item1 > tb.Item1) && (ta.Item2 > tb.Item2)) || ((ta.Item1 < tb.Item1) && (ta.Item2 < tb.Item2))) { return true; }
            return false;
            // we can ignore the case of equalities as no two cars will share the same grid ranking or the same race ranking
        }

        internal Core.RankingHealth GridGaps { get { return Core.Instance.GridHealth(_key); } }

        internal Tuple<int, int, int> PodiumOne { get { return Core.Instance.PlacedDriverKeys(_key, 1); } }
        internal Tuple<int, int, int> PodiumTwo { get { return Core.Instance.PlacedDriverKeys(_key, 2); } }
        internal Tuple<int, int, int> PodiumThree { get { return Core.Instance.PlacedDriverKeys(_key, 3); } }
        internal string Specification
        {
            get
            {
                return $"{_key}~{_circuitKey.ToString(System.Globalization.CultureInfo.InvariantCulture)}~{_raceTitleKey.ToString(System.Globalization.CultureInfo.InvariantCulture)}~{Core.DateToCode(_raceDate)}~{_countForConstructorsChampionship}";
            }
            set
            {
                string[] part = value.Split('~');
                _key = int.Parse(part[0], System.Globalization.CultureInfo.InvariantCulture);
                _circuitKey = int.Parse(part[1], System.Globalization.CultureInfo.InvariantCulture);
                _raceTitleKey = int.Parse(part[2], System.Globalization.CultureInfo.InvariantCulture);
                _raceDate = Core.DateFromCode(part[3]);
                _countForConstructorsChampionship = bool.Parse(part[4]);
            }
        }

        int IComparable<RaceMeeting>.CompareTo(RaceMeeting other)
        {
            return RaceDate.CompareTo(other.RaceDate);
        }

        internal string WikiLinkRace { get { return $"https://en.wikipedia.org/wiki/ { _raceDate.Year} {Core.Instance.RaceTitles[_raceTitleKey].Caption}"; } }

        internal bool CountForConstructorsChampionship { get => _countForConstructorsChampionship; set => _countForConstructorsChampionship = value; }
        internal int SerialNumber { get => _serialNumber; set => _serialNumber = value; }
        internal int YearSerialNumber { get => _yearSerialNumber; set => _yearSerialNumber = value; }

        // NOTE For the Indianapolis 500 Races in the early years I only entered the points scorers as there were many unheard-of US competitors in the race which is not usually a contributor to the drivers' championship
}