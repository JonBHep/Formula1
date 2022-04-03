using System;

namespace Formula1;

internal class Voiture : IComparable<Voiture>
{
    [Flags]
        enum SpecialFeatures { None=0, Formula2Car=1, ConstructorPointsDisallowed=2, InvolvedInControversy = 4};

        private readonly int _key;
        private int _constructorKey;
        private int _raceMeetingKey;
        private readonly int[] _driverKey;
        private readonly RacePoints[] _pointsFastestLap;
        private readonly RacePoints[] _pointsSprintQualifyingPosition;
        private readonly RacePoints[] _pointsRacePosition;
        private int _gridPosition;
        private int _racePosition;
        private int _constructorPointsPenalty;
        private SpecialFeatures _specials;

        internal Voiture(int KeyAssigned)
        {
            _key = KeyAssigned;
            _constructorKey = 0;
            _driverKey = new int[3];
            _specials = SpecialFeatures.None;
            _gridPosition = 0;
            _raceMeetingKey = 0;
            _racePosition = 0;
            _constructorPointsPenalty = 0;
            _pointsFastestLap = new RacePoints[3];
            _pointsSprintQualifyingPosition = new RacePoints[3];
            _pointsRacePosition = new RacePoints[3];
            _pointsFastestLap[0] = new RacePoints(0);
            _pointsFastestLap[1] = new RacePoints(0);
            _pointsFastestLap[2] = new RacePoints(0);
            _pointsSprintQualifyingPosition[0] = new RacePoints(0);
            _pointsSprintQualifyingPosition[1] = new RacePoints(0);
            _pointsSprintQualifyingPosition[2] = new RacePoints(0);
            _pointsRacePosition[0] = new RacePoints(0);
            _pointsRacePosition[1] = new RacePoints(0);
            _pointsRacePosition[2] = new RacePoints(0);
        }
        internal int Key { get => _key; } // Key is assigned at runtime and is not saved in the Specification
        internal int ConstructorKey { get => _constructorKey; set => _constructorKey = value; }
        internal int RaceMeetingKey { get => _raceMeetingKey; set => _raceMeetingKey = value; }
        internal int GridPosition { get => _gridPosition; set => _gridPosition = value; }
        internal int RacePosition { get => _racePosition; set => _racePosition = value; }
        internal int ConstructorPointsPenalty { get => _constructorPointsPenalty; set => _constructorPointsPenalty=value; }
        internal int DriverKey(int driver) { return _driverKey[driver]; }
        internal void SetDriverKey(int driver, int key) { _driverKey[driver] = key; }
        internal RacePoints PointsForFastestLap(int driver) { return _pointsFastestLap[driver]; }
        internal void SetPointsForFastestLap(int driver, RacePoints pts) { _pointsFastestLap[driver] = pts; }
        internal RacePoints PointsForSprintQualifying(int driver) { return _pointsSprintQualifyingPosition[driver]; }
        internal void SetPointsForSprintQualifying(int driver, RacePoints pts) { _pointsSprintQualifyingPosition[driver] = pts; }
        internal RacePoints PointsForPosition(int driver) { return _pointsRacePosition[driver]; }
        internal void SetPointsForPosition(int driver, RacePoints pts) {_pointsRacePosition[driver] = pts; }
        
        internal float AggregatedAllPoints
        {
            get
            {
                float vlu = _pointsFastestLap[0].FloatValue;
                vlu += _pointsFastestLap[1].FloatValue;
                vlu += _pointsFastestLap[2].FloatValue;
                vlu += _pointsSprintQualifyingPosition[0].FloatValue;
                vlu += _pointsSprintQualifyingPosition[1].FloatValue;
                vlu += _pointsSprintQualifyingPosition[2].FloatValue;
                vlu += _pointsRacePosition[0].FloatValue;
                vlu += _pointsRacePosition[1].FloatValue;
                vlu += _pointsRacePosition[2].FloatValue;
                return vlu;
            }
        }
        internal float AggregatedPositionPoints
        {
            get
            {
                float vlu =  _pointsRacePosition[0].FloatValue;
                vlu += _pointsRacePosition[1].FloatValue;
                vlu += _pointsRacePosition[2].FloatValue;
                return vlu;
            }
        }
        internal bool Finished { get { return (_racePosition < 1000); } }
        internal string Specification
        {
            get
            {
                return $"{_raceMeetingKey.ToString(System.Globalization.CultureInfo.InvariantCulture)}~{_constructorKey.ToString(System.Globalization.CultureInfo.InvariantCulture)}~{_gridPosition.ToString(System.Globalization.CultureInfo.InvariantCulture)}~{_racePosition.ToString(System.Globalization.CultureInfo.InvariantCulture)}~{_driverKey[0].ToString(System.Globalization.CultureInfo.InvariantCulture)}~{_driverKey[1].ToString(System.Globalization.CultureInfo.InvariantCulture)}~{_driverKey[2].ToString(System.Globalization.CultureInfo.InvariantCulture)}~{_pointsFastestLap[0].Specification}~{_pointsFastestLap[1].Specification}~{_pointsFastestLap[2].Specification}~{_pointsSprintQualifyingPosition[0].Specification}~{_pointsSprintQualifyingPosition[1].Specification}~{_pointsSprintQualifyingPosition[2].Specification}~{_pointsRacePosition[0].Specification}~{_pointsRacePosition[1].Specification}~{_pointsRacePosition[2].Specification}~{(int)_specials}~{_constructorPointsPenalty.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            }
            set
            {
                string[] part = value.Split('~');
                _raceMeetingKey = int.Parse(part[0], System.Globalization.CultureInfo.InvariantCulture);
                _constructorKey = int.Parse(part[1], System.Globalization.CultureInfo.InvariantCulture);
                _gridPosition = int.Parse(part[2], System.Globalization.CultureInfo.InvariantCulture);
                _racePosition = int.Parse(part[3], System.Globalization.CultureInfo.InvariantCulture);
                _driverKey[0] = int.Parse(part[4], System.Globalization.CultureInfo.InvariantCulture);
                _driverKey[1] = int.Parse(part[5], System.Globalization.CultureInfo.InvariantCulture);
                _driverKey[2] = int.Parse(part[6], System.Globalization.CultureInfo.InvariantCulture);
                _pointsFastestLap[0] =new RacePoints(part[7]);
                _pointsFastestLap[1] = new RacePoints(part[8]);
                _pointsFastestLap[2] = new RacePoints(part[9]);
                _pointsSprintQualifyingPosition[0] = new RacePoints(part[10]);
                _pointsSprintQualifyingPosition[1] = new RacePoints(part[11]);
                _pointsSprintQualifyingPosition[2] = new RacePoints(part[12]);
                _pointsRacePosition[0] = new RacePoints(part[13]);
                _pointsRacePosition[1] = new RacePoints(part[14]);
                _pointsRacePosition[2] = new RacePoints(part[15]);
                _specials = (SpecialFeatures)int.Parse(part[16], System.Globalization.CultureInfo.InvariantCulture);
                _constructorPointsPenalty = int.Parse(part[17], System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        int IComparable<Voiture>.CompareTo(Voiture other)
        {
            if ((this.RacePosition > 999) && (other.RacePosition > 999))
            {
                return this.GridPosition.CompareTo(other.GridPosition);
            }
            return this.RacePosition.CompareTo(other.RacePosition);
        }

        internal bool IncludesDriver(int dkey)
        {
            if (_driverKey[0] == dkey) { return true; }
            if (_driverKey[1] == dkey) { return true; }
            if (_driverKey[2] == dkey) { return true; }
            return false;
        }

        internal int DriverCount
        {
            get
            {
                if (_driverKey[2] > 0) { return 3; }
                if (_driverKey[1] > 0) { return 2; }
                return 1;
            }
        }

        internal bool Formula2
        {
            get
            {
                return (_specials & SpecialFeatures.Formula2Car)== SpecialFeatures.Formula2Car;
            }
            set
            {
                if (value)
                {
                    _specials |= SpecialFeatures.Formula2Car;
                }
                else
                {
                    if ((_specials & SpecialFeatures.Formula2Car) == SpecialFeatures.Formula2Car)
                    {
                        _specials &= ~SpecialFeatures.Formula2Car;
                    }
                }
            }
        }
        internal bool Controversial
        {
            get
            {
                return (_specials & SpecialFeatures.InvolvedInControversy) == SpecialFeatures.InvolvedInControversy;
            }
            set
            {
                if (value)
                {
                    _specials |= SpecialFeatures.InvolvedInControversy;
                }
                else
                {
                    if ((_specials & SpecialFeatures.InvolvedInControversy) == SpecialFeatures.InvolvedInControversy)
                    {
                        _specials &= ~SpecialFeatures.InvolvedInControversy;
                    }
                }
            }
        }
        internal bool ConstructorPointsDisallowed
        {
            get
            {
                return (_specials & SpecialFeatures.ConstructorPointsDisallowed) == SpecialFeatures.ConstructorPointsDisallowed;
            }
            set
            {
                if (value)
                {
                    _specials |= SpecialFeatures.ConstructorPointsDisallowed;
                }
                else
                {
                    if ((_specials & SpecialFeatures.ConstructorPointsDisallowed)== SpecialFeatures.ConstructorPointsDisallowed)
                    {
                        _specials &= ~SpecialFeatures.ConstructorPointsDisallowed;
                    }
                }
            }
        }

        internal float? DriverPoints(int DriverKey)
        {
            if (_driverKey[0] == DriverKey) { return _pointsFastestLap[0].FloatValue + _pointsSprintQualifyingPosition[0].FloatValue + _pointsRacePosition[0].FloatValue; }
            if (_driverKey[1] == DriverKey) { return _pointsFastestLap[1].FloatValue + _pointsSprintQualifyingPosition[1].FloatValue + _pointsRacePosition[1].FloatValue; }
            if (_driverKey[2] == DriverKey) { return _pointsFastestLap[2].FloatValue + _pointsSprintQualifyingPosition[2].FloatValue + _pointsRacePosition[2].FloatValue; }
            return null;
        }
        
     internal   DateTime RaceDate { get { return Core.Instance.Races[_raceMeetingKey].RaceDate; } }
    }

    internal class RacePoints : IComparable<RacePoints>
    {
        private int _numerator;
        private int _denominator;
        internal RacePoints(Tuple<int, int> points)
        {
            _numerator = points.Item1;
            _denominator = points.Item2;
        }
        internal RacePoints(int num)
        {
            _numerator = num;
            _denominator = 1;
        }
        internal RacePoints(string spec)
        {
            Specification = spec;
        }
        internal float FloatValue { get { return _numerator / (float)_denominator; } }
        internal static Tuple<int, int> Interpret(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new Tuple<int, int>(0, 1); // a valid but zero value
            }
            int numtor = 0;
            int dentor = 0;
            if (raw.Contains('/'))
            {
                string[] part = raw.Split('/');
                if (int.TryParse(part[0], out int num))
                {
                    numtor = num;
                    if (int.TryParse(part[1], out int den))
                    {
                        dentor = den;
                    }
                }
            }
            else
            {
                if (int.TryParse(raw, out int num))
                {
                    numtor = num; dentor = 1;
                }
            }
            return new Tuple<int, int>(numtor, dentor);
        }
        internal string Specification
        {
            get
            {
                return $"{_numerator.ToString(System.Globalization.CultureInfo.InvariantCulture)}:{_denominator.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            }
            set
            {
                string[] part = value.Split(':');
                _numerator = int.Parse(part[0], System.Globalization.CultureInfo.InvariantCulture);
                _denominator = int.Parse(part[1], System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        internal string Representation
        {
            get
            {
                if (_numerator == 0) { _denominator = 1; } // if numerator is zero it cannot matter what the denominator is, so set to 1
                Simplify();
                if (_denominator == 1)
                {
                    return $"{_numerator}";
                }
                else if (_denominator == _numerator)
                {
                    return "1";
                }
                else
                {
                    return $"{_numerator}/{_denominator}";
                }
            }
        }

        private void Simplify()
        {
            int f = 2;
            while (((_numerator % f) == 0) && (_denominator % f == 0)) { _numerator /= f; _denominator /= f; }
            f = 3;
            while (((_numerator % f) == 0) && (_denominator % f == 0)) { _numerator /= f; _denominator /= f; }
            f = 5;
            while (((_numerator % f) == 0) && (_denominator % f == 0)) { _numerator /= f; _denominator /= f; }
            f = 7;
            while (((_numerator % f) == 0) && (_denominator % f == 0)) { _numerator /= f; _denominator /= f; }
        }

        int IComparable<RacePoints>.CompareTo(RacePoints other)
        {
            return this.FloatValue.CompareTo(other.FloatValue);
        }
}