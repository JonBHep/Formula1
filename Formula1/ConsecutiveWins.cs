using System;
using System.Collections.Generic;

namespace Formula1;

internal class ConsecutiveWins
{
    private  Dictionary<int, int> _winRuns = new Dictionary<int, int>();

    internal void RegisterDrivers(Tuple<int, int, int> dkeys)
    {
        List<int> driverlist = new List<int>();
        if (dkeys.Item1 > 0) { driverlist.Add(dkeys.Item1); }
        if (dkeys.Item2 > 0) { driverlist.Add(dkeys.Item2); }
        if (dkeys.Item3 > 0) { driverlist.Add(dkeys.Item3); }
        foreach(int driverKey in driverlist)
        {
            if (_winRuns.ContainsKey(driverKey)) { _winRuns[driverKey]++; } else { _winRuns.Add(driverKey, 1); }
        }
        List<int> discontinued = new List<int>();
        foreach(int quay in _winRuns.Keys)
        {
            if (!driverlist.Contains(quay)) { discontinued.Add(quay); }
        }
        foreach(int clef in discontinued)
        {
            _winRuns.Remove(clef);
        }
    }

    internal Tuple<int,int> BestDriverRun()
    {
        int bestL = 0; int bestD = 0;
        foreach (int quay in _winRuns.Keys)
        {
            if (_winRuns[quay]>bestL) { bestL = _winRuns[quay]; bestD = quay; }
        }
        return new Tuple<int, int>(bestD, bestL);
    }

    internal int DriverRun(int driver)
    {
        if (_winRuns.ContainsKey(driver)) { return _winRuns[driver]; }
        return 0;
    }

    internal void BringForward(ConsecutiveWins cwinz)
    {
        _winRuns.Clear();
        foreach (int k in cwinz._winRuns.Keys)
        {
            _winRuns.Add(k, cwinz._winRuns[k]);
        }
    }

}