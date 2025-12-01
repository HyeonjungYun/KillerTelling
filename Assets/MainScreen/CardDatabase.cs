using UnityEngine;

public static class CardDatabase
{
    public static CardData GetCardDataFromSprite(Sprite spr)
    {
        if (spr == null)
        {
            Debug.LogError("❌ Sprite is NULL in CardDatabase");
            return null;
        }

        string name = spr.name;  // ex: "5D", "10H", "QS", "Card-back"

        // 카드 뒷면은 무시
        if (name.ToLower().Contains("back"))
        {
            Debug.Log("⚪ 카드 뒷면은 CardData를 생성하지 않습니다.");
            return null;
        }

        // 마지막 글자 = 문양
        char suitChar = name[name.Length - 1];
        string suit = suitChar.ToString();

        // 나머지 부분 = 랭크 문자열
        string rankStr = name.Substring(0, name.Length - 1);

        int rank = ConvertRank(rankStr);

        if (rank == -1)
        {
            Debug.LogError($"❌ Rank parse 실패! name = {name}");
            return null;
        }

        return new CardData(suit, rank, spr);
    }

    private static int ConvertRank(string rankStr)
    {
        switch (rankStr)
        {
            case "A": return 14;
            case "K": return 13;
            case "Q": return 12;
            case "J": return 11;
        }

        int num;
        if (int.TryParse(rankStr, out num))
            return num;

        return -1; // 실패
    }
}
