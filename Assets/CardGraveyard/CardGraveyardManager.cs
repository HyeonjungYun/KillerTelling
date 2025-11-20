using System.Collections.Generic;
using UnityEngine;

public class CardGraveyardManager : MonoBehaviour
{
    public static CardGraveyardManager Instance;

    public Transform graveyardArea;   // 3D 배치 부모
    public GameObject cardPrefab;     // Card3D 프리팹

    private List<Sprite> storedCards = new List<Sprite>();
    public List<Sprite> StoredSprites => storedCards;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddCards(List<Sprite> cards)
    {
        storedCards.AddRange(cards);
        UpdateGraveyardUI();
    }

    private void UpdateGraveyardUI()
    {
        // 기존 카드 제거
        foreach (Transform child in graveyardArea)
            Destroy(child.gameObject);

        // 무늬별 그룹 생성
        Dictionary<char, List<Sprite>> suitGroups = new Dictionary<char, List<Sprite>>()
        {
            { 'S', new List<Sprite>() },  // Spade
            { 'H', new List<Sprite>() },  // Heart
            { 'D', new List<Sprite>() },  // Diamond
            { 'C', new List<Sprite>() },  // Club
        };

        // 스프라이트 이름 분석 → 무늬 그룹에 넣기
        foreach (Sprite spr in storedCards)
        {
            char suit = ExtractSuit(spr.name);
            suitGroups[suit].Add(spr);
        }

        // 🔥 위치/스케일 설정
        float stackStartX = -1.5f;   // 맨 왼쪽 스택 X
        float stackSpacingX = 1.3f;  // 스택 간 간격
        float cardOffsetY = 0.04f;   // 스택 내 위로 쌓이는 간격
        float cardScale = 1.1f;      // 카드 스케일 (2배 수준)

        char[] suitOrder = { 'S', 'H', 'D', 'C' }; // 표시 순서

        // 스택 생성
        for (int s = 0; s < suitOrder.Length; s++)
        {
            char suit = suitOrder[s];
            List<Sprite> list = suitGroups[suit];

            float stackX = stackStartX + s * stackSpacingX;

            for (int i = 0; i < list.Count; i++)
            {
                Sprite spr = list[i];
                GameObject obj = Instantiate(cardPrefab, graveyardArea);

                Card3D card3D = obj.GetComponent<Card3D>();
                card3D.SetSprite(spr);

                obj.transform.localPosition = new Vector3(
                    stackX,
                    i * cardOffsetY,
                    0f
                );

                obj.transform.localRotation = Quaternion.Euler(90, 0, 0);
                obj.transform.localScale = Vector3.one * cardScale;
            }
        }
    }

    // 🔥 스프라이트 이름 마지막 글자에서 무늬 추출
    private char ExtractSuit(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
            return 'S';

        char c = char.ToUpper(spriteName[spriteName.Length - 1]);

        if (c == 'S' || c == 'H' || c == 'D' || c == 'C')
            return c;

        Debug.LogWarning("Unknown suit in sprite: " + spriteName);
        return 'S';
    }
}
