using System;

namespace Formula1;

internal class IndividualScore : IComparable<IndividualScore>
{
    internal int IndividualKey { get; private set; }
    internal float Score { get; set; }
    internal float CumulativeScore { get; set; }
    internal float CumulativeScoreCounted { get; set; }
    internal float MaxTheoreticalSeasonTotal { get; set; }

    internal IndividualScore(int IndivKey)
    {
        IndividualKey = IndivKey;
        Score = 0;
        CumulativeScore = 0;
        CumulativeScoreCounted = 0;
        MaxTheoreticalSeasonTotal = 0;
    }

    int IComparable<IndividualScore>.CompareTo(IndividualScore? other)
    {
        if (other is { } autre)
        {
            int x =autre.CumulativeScoreCounted.CompareTo(CumulativeScoreCounted);
            if (x == 0) { x = other.CumulativeScore.CompareTo(CumulativeScore); }
            if (x == 0) { x = other.Score.CompareTo(Score); }
            return x;    
        }

        return 0;
    }
}