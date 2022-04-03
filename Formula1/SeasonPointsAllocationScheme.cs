using System.Collections.Generic;
using System.Linq;

namespace Formula1;

internal class SeasonPointsAllocationScheme
{
     private int _firstBlockSize;
        private int _firstBlockQuota;
        private int _secondBlockQuota;

        // NOTE This scheme applies to how individual scores are totted up across a season; it doesn't include the points for position awarded to drivers / constructors as this is so variable
        

        internal SeasonPointsAllocationScheme(int FirstBlockSize, int FirstBlockQuota, int SecondBlockQuota)
        {
            _firstBlockSize = FirstBlockSize;
            _firstBlockQuota = FirstBlockQuota;
            _secondBlockQuota = SecondBlockQuota;
        }
        internal SeasonPointsAllocationScheme(string Spec)
        {
            Specification = Spec;
        }
        internal string Specification
        {
            get
            {
                return $"{FirstBlockSize.ToString(System.Globalization.CultureInfo.InvariantCulture)}^{_firstBlockQuota.ToString(Core.CultureUk)}^{_secondBlockQuota.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            }
            set
            {
                string[] part = value.Split('^');
                FirstBlockSize = int.Parse(part[0], System.Globalization.CultureInfo.InvariantCulture);
                _firstBlockQuota = int.Parse(part[1], System.Globalization.CultureInfo.InvariantCulture);
                _secondBlockQuota = int.Parse(part[2], System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public bool Applies { get { return _firstBlockQuota != Core.SpecialNumber; } }
        public int FirstBlockSize { get =>_firstBlockSize; set =>_firstBlockSize=value; }
        public int FirstBlockQuota { get => _firstBlockQuota; set => _firstBlockQuota = value; }
        public int LastBlockQuota { get => _secondBlockQuota; set => _secondBlockQuota = value; }

        internal string Explanation()
        {
            if (_firstBlockQuota ==Core.SpecialNumber)
            {
                return "No championship";
            }
            else if (_firstBlockQuota == 0)
            {
                return "Points from all races are counted";
            }
            else if (_firstBlockSize == 0)
            {
                return $"The best {_firstBlockQuota} scores are counted";
            }
            else
            {
                return $"Count the best {_firstBlockQuota} scores from the first {_firstBlockSize} races and the best {_secondBlockQuota} scores from the later races";
            }
        }

        internal float ModifiedPointsTotal(List<float> scores)
        {
            // NO CHAMPIONSHIP METHOD, no competition so score zero
            if (_firstBlockQuota == Core.SpecialNumber)
            {
                return 0;
            }

            // FIRST METHOD, no quota set so count all scores
            if (_firstBlockQuota == 0)
            {
                return scores.Sum();
            }

            // SECOND METHOD, count the best X scores
            if (_firstBlockSize == 0)
            {
                // no block size set, so take the specified number of best scores
                scores.Sort();
                scores.Reverse();
                // scores have now been sorted highest first
                int howmany = _firstBlockQuota;
                float totl = 0;
                while (howmany > 0)
                {
                    howmany--;
                    if (scores.Count > 0)
                    {
                        totl += scores[0];
                        scores.RemoveAt(0);
                    }
                }
                return totl;
            }

            // THIRD METHOD, X from the first N races then Y from the later races
            List<float> FirstBlockScores = new List<float>();
            List<float> LastBlockScores = new List<float>();
            for (int z = 0; z < scores.Count; z++)
            {
                if (z < _firstBlockSize) { FirstBlockScores.Add(scores[z]); } else { LastBlockScores.Add(scores[z]); }
            }
            FirstBlockScores.Sort();
            FirstBlockScores.Reverse();
            LastBlockScores.Sort();
            LastBlockScores.Reverse();
            // Each block of scores has now been sorted highest score first

            int howmanycount = _firstBlockQuota;
            float tot = 0;
            while (howmanycount > 0)
            {
                howmanycount--;
                if (FirstBlockScores.Count > 0)
                {
                    tot += FirstBlockScores[0];
                    FirstBlockScores.RemoveAt(0);
                }
            }
            howmanycount =_secondBlockQuota;
            while (howmanycount > 0)
            {
                howmanycount--;
                if (LastBlockScores.Count > 0)
                {
                    tot += LastBlockScores[0];
                    LastBlockScores.RemoveAt(0);
                }
            }
            return tot;
        }
}