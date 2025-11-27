using System.Collections.Generic;
using System.Linq;

public static class DeckEvaluator
{
    public static string EvaluateDeck(List<CardData> deck)
    {
        if (deck == null || deck.Count < 2)
            return "Not enough cards";

        var ranks = deck.Select(c => c.rank).OrderBy(x => x).ToList();
        var suits = deck.Select(c => c.suit).ToList();
        bool flush = suits.Distinct().Count() == 1;
        bool straight = ranks.Zip(ranks.Skip(1), (a, b) => b - a).All(diff => diff == 1);
        var rankGroups = deck.GroupBy(c => c.rank)
                             .Select(g => g.Count())
                             .OrderByDescending(c => c).ToList();

        int n = deck.Count;

        // 카드 개수별 평가 범위 제한
        if (n == 2)
        {
            if (rankGroups[0] == 2) return "OnePair";
            return "HighCard";
        }
        else if (n == 3)
        {
            if (rankGroups[0] == 3) return "ThreeOfAKind";
            if (rankGroups[0] == 2) return "OnePair";
            return "HighCard";
        }
        else if (n == 4)
        {
            if (rankGroups[0] == 4) return "FourOfAKind";
            if (rankGroups[0] == 3) return "ThreeOfAKind";
            if (rankGroups[0] == 2 && rankGroups.Count > 1 && rankGroups[1] == 2)
                return "TwoPair";
            return "HighCard";
        }
        else // 5장
        {
            if (straight && flush && ranks.Max() == 13) return "RoyalFlush";
            if (straight && flush) return "StraightFlush";
            if (rankGroups[0] == 4) return "FourOfAKind";
            if (rankGroups[0] == 3 && rankGroups[1] == 2) return "FullHouse";
            if (flush) return "Flush";
            if (straight) return "Straight";
            if (rankGroups[0] == 3) return "ThreeOfAKind";
            if (rankGroups[0] == 2 && rankGroups[1] == 2) return "TwoPair";
            if (rankGroups[0] == 2) return "OnePair";
            return "HighCard";
        }
    }
}
