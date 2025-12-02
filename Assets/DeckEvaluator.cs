using System.Collections.Generic;
using System.Linq;

public static class DeckEvaluator
{
    public static string EvaluateDeck(List<CardData> deck)
    {
        if (deck == null || deck.Count < 2)
            return "Not enough cards";

        // 랭크/문양 분리
        var ranks = deck.Select(c => c.rank).OrderBy(x => x).ToList();
        var suits = deck.Select(c => c.suit).ToList();

        int n = deck.Count;

        // -----------------------
        // 🔥 Flush 판정
        // -----------------------
        bool flush = suits.Distinct().Count() == 1;

        // -----------------------
        // 🔥 Straight 판정 (A2345 포함)
        // -----------------------
        bool straight = IsStraight(ranks);

        // -----------------------
        // 🔥 Rank 그룹 (페어, 트리플, 포카드 판정용)
        // -----------------------
        var rankGroups = deck
            .GroupBy(c => c.rank)
            .Select(g => g.Count())
            .OrderByDescending(c => c)
            .ToList();

        // -----------------------
        // 🔥 2장 덱 규칙
        // -----------------------
        if (n == 2)
        {
            if (rankGroups[0] == 2) return "OnePair";
            return "HighCard";
        }

        // -----------------------
        // 🔥 3장 덱 규칙
        // -----------------------
        if (n == 3)
        {
            if (rankGroups[0] == 3) return "ThreeOfAKind";
            if (rankGroups[0] == 2) return "OnePair";
            return "HighCard";
        }

        // -----------------------
        // 🔥 4장 덱 규칙
        // -----------------------
        if (n == 4)
        {
            if (rankGroups[0] == 4) return "FourOfAKind";
            if (rankGroups[0] == 3) return "ThreeOfAKind";
            if (rankGroups[0] == 2 && rankGroups[1] == 2) return "TwoPair";
            if (rankGroups[0] == 2) return "OnePair";
            return "HighCard";
        }

        // ============================================================
        // 🔥 5장 포커 전체 족보 (정확히 수정됨)
        // ============================================================

        bool isRoyal =
            straight &&
            flush &&
            (IsRoyalStraight(ranks));

        if (isRoyal) return "RoyalFlush";
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

    // ============================================================
    // 🔥 스트레이트 판정 함수 (A2345 지원)
    // ============================================================
    private static bool IsStraight(List<int> ranks)
    {
        // 일반 스트레이트 체크
        bool normal =
            ranks.Zip(ranks.Skip(1), (a, b) => b - a)
                 .All(diff => diff == 1);

        if (normal) return true;

        // A를 1로 취급한 스트레이트 체크 (A2345)
        List<int> aceLow = ranks.Select(r => r == 14 ? 1 : r).OrderBy(x => x).ToList();

        bool aceStraight =
            aceLow.Zip(aceLow.Skip(1), (a, b) => b - a)
                  .All(diff => diff == 1);

        return aceStraight;
    }

    // ============================================================
    // 🔥 로열 스트레이트 판정 (A-K-Q-J-10)
    // ============================================================
    private static bool IsRoyalStraight(List<int> ranks)
    {
        // A K Q J 10 → 14 13 12 11 10   
        int[] royal = { 10, 11, 12, 13, 14 };
        return ranks.OrderBy(x => x).SequenceEqual(royal);
    }

    public static int GetRankValue(string rank)
    {
        return rank switch
        {
            "HighCard" => 1,
            "OnePair" => 2,
            "TwoPair" => 3,
            "ThreeOfAKind" => 4,
            "Straight" => 5,
            "Flush" => 6,
            "FullHouse" => 7,
            "FourOfAKind" => 8,
            "StraightFlush" => 9,
            "RoyalFlush" => 10,
            _ => 0
        };
    }
}
