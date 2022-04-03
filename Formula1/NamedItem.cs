using System;

namespace Formula1;

internal class NamedItem : IComparable<NamedItem>
{
    private int _key;
    private string _caption;

    internal int Key { get => _key; }
    internal string Caption { get => _caption; set => _caption = value; }
    internal NamedItem(string cap, int ky)
    {
        _key = ky;
        _caption = cap;
    }
    internal NamedItem(string spec)
    {
        Specification = spec;
    }
    int IComparable<NamedItem>.CompareTo(NamedItem other)
    {
        System.Globalization.CultureInfo eng = new System.Globalization.CultureInfo("en-GB");
        // Create a culturally sensitive sort key for str1.
        System.Globalization.SortKey sc1 = eng.CompareInfo.GetSortKey(this._caption);
        // Create a culturally sensitive sort key for str2.
        System.Globalization.SortKey sc2 = eng.CompareInfo.GetSortKey(other._caption);
        // Compare the two sort keys and display the results.
        int result = System.Globalization.SortKey.Compare(sc1, sc2);
        return result;
    }

    internal string Specification
    {
        get
        {
            return $"{_key.ToString(System.Globalization.CultureInfo.InvariantCulture)}~{_caption}";
        }
        set
        {
            string[] part = value.Split('~');
            _key = int.Parse(part[0], System.Globalization.CultureInfo.InvariantCulture);
            _caption = part[1];
        }
    }
}