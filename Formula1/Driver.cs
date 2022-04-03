using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Formula1;

internal class Driver:IComparable<Driver>
{
     private string _surname;

        internal string Surname { get => _surname; set => _surname = value; }

        private string _forenames;

        internal string Forenames { get => _forenames; set => _forenames = value; }

        internal string FullName { get => $"{_forenames} {_surname}"; }
        internal string SortingName { get => $"{_surname} {_forenames}"; }

        internal Run TimeLeftRun(DateTime when)
        {
            string mm = TimeLeft(when);
            if (!string.IsNullOrWhiteSpace(mm)) { mm = $" ({mm})"; }
            Brush pinco = (mm.Contains("M")) ? Brushes.OrangeRed : Brushes.HotPink;
            return new Run(mm) { Foreground = pinco };
        }

        private int _countryKey;

        internal int CountryKey { get => _countryKey; set => _countryKey = value; }

        private int _key;

        internal int Key { get => _key; set => _key = value; }

        private DateTime _birthDate;
        private DateTime _deathDate;
        private DateTime _ceiDate;
        private string _deathModeText;
        private Core.CauseOfDeath _causeOfDeath;

        
        internal string RuntimeYears { get; set; }
        internal DateTime RuntimeFirstRaceStartDate { get; set; }
        internal DateTime RuntimeLastRaceStartDate { get; set; }
        internal DateTime RuntimeFirstQualifiedDate { get; set; }
        internal DateTime RuntimeLastQualifiedDate { get; set; }
        //internal int RuntimeLastYear { get; set; }
        internal int RuntimeRacesStarted { get; set; }
        internal int RuntimeRacesFinished { get; set; }
        internal int RuntimeRacesQualified { get; set; }
        internal int RuntimePoles { get; set; }
        internal int RuntimeFirstRaceKey { get; set; }
        internal int RuntimeLastRaceKey { get; set; }

        internal Core.CauseOfDeath HowDied { get => _causeOfDeath; set => _causeOfDeath = value; }
        
        internal string Specification
        {
            get
            {
                return $"{_key.ToString(System.Globalization.CultureInfo.InvariantCulture)}~{_surname}~{_forenames}~{_countryKey.ToString(System.Globalization.CultureInfo.InvariantCulture)}~{Core.DateToCode(_birthDate)}~{Core.DateToCode(_ceiDate)}~{Core.DateToCode(_deathDate)}~{_deathModeText}~{((int)_causeOfDeath).ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            }
            set
            {
                string[] part = value.Split('~');
                _key = int.Parse(part[0], System.Globalization.CultureInfo.InvariantCulture);
                _surname = part[1];
                _forenames = part[2];
                _countryKey = int.Parse(part[3], System.Globalization.CultureInfo.InvariantCulture);
                _birthDate = Core.DateFromCode(part[4]);
                _ceiDate = Core.DateFromCode(part[5]);
                _deathDate = Core.DateFromCode(part[6]);
                _deathModeText = part[7];
                _causeOfDeath = (Core.CauseOfDeath)int.Parse(part[8], System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        internal DateTime BirthDate { get => _birthDate; set => _birthDate = value; }
        internal DateTime DeathDate { get => _deathDate; set => _deathDate = value; }
        internal DateTime CeiDate { get => _ceiDate; set => _ceiDate = value; }
        internal string DeathMode { get => _deathModeText; set => _deathModeText = value; }

        int IComparable<Driver>.CompareTo(Driver other)
        {
            if (this.Complete && !other.Complete) { return 1; }
            if (!this.Complete && other.Complete) { return -1; }
            int q = string.Compare(this._surname, other._surname, true, Core.CultureUK);
            if (q == 0)
            {
                q =string.Compare( this._forenames, other._forenames, true, Core.CultureUK);
            }
            return q;
        }

        public bool Complete
        {
            get
            {
                bool replete = true;
                if (string.IsNullOrWhiteSpace(_surname)) { replete = false; }
                if (string.IsNullOrWhiteSpace(_forenames)) { replete = false; }
                if (string.IsNullOrWhiteSpace(_deathModeText)) { replete = false; }
                if (_birthDate < Core.DateBase) { replete = false; }
                if (CountryKey < 1) { replete = false; }
                if ((_deathDate > Core.DateBase) && (HowDied == Core.CauseOfDeath.Unknown)) { replete = false; }
                return replete;
            }
        }

        internal string WikiLinkString { get { return $"https://en.wikipedia.org/wiki/ {FullName}"; } }

        internal string DisplayAge()
        {
            int age = LatestAge();
            if (age==0) { return string.Empty; }
            if (_deathDate < Core.DateBase) { return $"Alive aged {age}"; } else { return $"Died aged {age}"; }
        }

        internal int LatestAge()
        {
            if (_birthDate < Core.DateBase) { return 0; }
            DateTime lastDate = (_deathDate < Core.DateBase) ? DateTime.Today : _deathDate;
            return AgeAsAt(lastDate);
        }

        internal int AgeAsAt(DateTime dat)
        {
            if (_birthDate < Core.DateBase) { return 0; }
            int yrs = dat.Year - _birthDate.Year;
            if (dat.Month < _birthDate.Month)
            { yrs--; }
            else if (dat.Month == _birthDate.Month)
            { if (dat.Day < _birthDate.Day) { yrs--; } }
            return yrs;
        }

        internal bool NotDiedBefore(DateTime Q)
        {
            Q = Q.AddDays(-1); // retract by one day so that drivers are not excluded from being selected on the day they died
            if (_deathDate < Core.DateBase) { return true; } // don't know when he died
            if (_deathDate > Q) { return true; } // known to have died later than this
            return false; // died before the given date
        }

        internal string TimeLeft(DateTime Q)
        {
            if (_deathDate < Core.DateBase) { return string.Empty; } // don't know when he died, or he's still alive
            if (_deathDate<Q) { return "(died)"; } // already died by now
            int mth = -1;
            DateTime a = Q;
            while (a < _deathDate) { a = a.AddMonths(1); mth++; }
            string mori = $"{mth}M";
            double y = Math.Round(mth / 12f);
            if (mth > 35) { mori = $"{y}Y"; }
            return mori;
        }

        internal float LifetimePoints()
        {
            float p = 0;
            foreach(Voiture v in Core.Instance.Voitures.Values)
            {
                float? f = v.DriverPoints(_key);
                if (f.HasValue) { p += f.Value; }
            }
            return p;
        }

        internal int WinsUpTo(DateTime when)
        {
            int p = 0;
            foreach (Voiture v in Core.Instance.Voitures.Values)
            {
                if (v.IncludesDriver(_key))
                {
                    if (v.RacePosition == 1)
                    {
                        if (v.RaceDate <= when)
                        {
                            p++;
                        }
                    }
                }
            }
            return p;
        }

        internal float SeasonPoints(int Annum)
        {
            float p = 0;
            foreach (Voiture v in Core.Instance.Voitures.Values)
            {
                if (v.RaceDate.Year==Annum)
                {
                    float? f = v.DriverPoints(_key);
                    if (f.HasValue) { p += f.Value; }
                }
            }
            return p;
        }

        internal void AppendFateGlyph(ref TextBlock t)
        {
            Run r;
            
            if ((HowDied == Core.CauseOfDeath.RacePracticeOrTestingAccident) || (HowDied == Core.CauseOfDeath.RacingAccident))
            {
                r = new Run() { FontFamily = new FontFamily("Webdings"), Text =Core.GlyphFatalAccident, FontSize = 16, Foreground = Brushes.Red }; // death mask symbol
                t.Inlines.Add(r);
                r = new Run() { Text = $" (died at {LatestAge()})", Foreground = Brushes.Red };
                t.Inlines.Add(r);
            }
         else  if (HowDied == Core.CauseOfDeath.OtherAccident)
            {
                r = new Run() { FontFamily = new FontFamily("Webdings"), Text = Core.GlyphFatalAccident, FontSize = 16, Foreground = Brushes.Black };  // death mask symbol
                t.Inlines.Add(r);
                r = new Run() { Text = $" (died at {LatestAge()})", Foreground = Brushes.Black };
                t.Inlines.Add(r);
            }
           else if ((BirthDate > Core.DateBase) && !(DeathDate > Core.DateBase)) // not died
            {
                r = new Run() { FontFamily = new FontFamily("Webdings"), Text = Core.GlyphLiving, FontSize = 16, Foreground = Brushes.Blue }; // cat symbol (9 lives)
                if (CeiDate > Core.DateBase) { r.Foreground = Brushes.Chocolate; } // change colour of cat if driver's career was ended by injury
                t.Inlines.Add(r);
                r = new Run() { Text = $" (aged {LatestAge()})", Foreground = Brushes.Blue };
                if (CeiDate > Core.DateBase) { r.Foreground = Brushes.Chocolate; } // change colour of age if driver's career was ended by injury
                t.Inlines.Add(r);
            }
            else if ((BirthDate > Core.DateBase) && (DeathDate > Core.DateBase)) // died but not accidentally
            {
                r = new Run() { Text = $" (died at {LatestAge()})", Foreground = Brushes.SeaGreen };
                t.Inlines.Add(r);
            }

            t.Inlines.Add(new Run() { Text =$" {RuntimeYears}", Foreground = Brushes.CornflowerBlue });
            
        }

        internal string TextQueries()
        {
            return TextWorry(_deathModeText);
        }

        internal static string TextWorry(string txt)
        {
            string tw = string.Empty;
            if (txt.Contains('[')) { tw = "Square brackets in text"; }
            if (txt.Contains(']')) { tw = "Square brackets in text"; }
            if (txt.Contains("  ")) { tw = "Double space in text"; }
            if (txt.Contains("\"")) { tw = "Double quotation mark in text"; }
            return tw;
        }
}