using System;
using System.Collections.Generic;

namespace Formula1;

internal class Circuit : IComparable<Circuit>
{
    private List< string> _circuitName=new List<string>();
        private int _countryKey;
        private int _key;
        internal int CircuitTitleCount { get { return CircuitName.Count; } }
        internal string CircuitSingleTitle(int appel)
        {
            if (CircuitName.Count < 2) { return CircuitName[0]; }
            string rv = CircuitName[appel]+" (";
            for(int z = 0; z < CircuitName.Count; z++)
            {
                if (z != appel) { rv += CircuitName[z] + ", "; }
            }
            rv = rv.Substring(0, rv.Length - 2);
            rv += ")";
            return rv;
        }
            
        internal int CountryKey { get => _countryKey; set => _countryKey = value; }
        internal int Key { get => _key; set => _key = value; }
        
        internal string Specification
        {
            get
            {
                string circuittitles = string.Join(">", CircuitName.ToArray());
                return $"{_key.ToString(System.Globalization.CultureInfo.InvariantCulture)}~{circuittitles}~{_countryKey.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            }
            set
            {
                string[] part = value.Split('~');
                _key = int.Parse(part[0], System.Globalization.CultureInfo.InvariantCulture);
                string noms = part[1];
                string[] namen = noms.Split('>');
                CircuitName.Clear();
                foreach(string nm in namen) { CircuitName.Add(nm); }
                _countryKey = int.Parse(part[2], System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public List<string> CircuitName { get => _circuitName; set => _circuitName = value; }

        int IComparable<Circuit>.CompareTo(Circuit other)
        {
            System.Globalization.CultureInfo eng = new System.Globalization.CultureInfo("en-GB");
            // Create a culturally sensitive sort key for str1.
            System.Globalization.SortKey sc1 = eng.CompareInfo.GetSortKey(CircuitName[0]);
            // Create a culturally sensitive sort key for str2.
            System.Globalization.SortKey sc2 = eng.CompareInfo.GetSortKey(other.CircuitName[0]);
            // Compare the two sort keys and return the result
            int result = System.Globalization.SortKey.Compare(sc1, sc2);
            return result;
        }

        internal bool NameMatch(List<string> namelist)
        {
            bool flag = false;
            foreach(string moi in CircuitName)
            {
                foreach(string toi in namelist)
                {
                    if (moi.Equals(toi, StringComparison.CurrentCultureIgnoreCase)) { flag = true; }
                }
            }
            return flag;
        }

}