using System.Collections.Generic;
using UnityEngine;

public class CardGraveyardManager : MonoBehaviour
{
    [Header("References")]
    public Transform graveyardArea;
    public GameObject card3DPrefab;
    public int cardsPerBatch = 7;

    private List<GameObject> graveyardCards = new();
    private List<string> suits = new() { "S", "H", "D", "C" };

    public void AddCardsToGraveyard()
    {
        float cardThickness = 0.02f; // 카드 한 장의 높이
        float stackSpacing = 0.8f;   // 무늬별 간격

        // 덱 간격
        float spacing = 1.5f; // 각 덱 간의 간격

        // 덱 위치를 중앙에 배치
        Vector3 centerPosition = new Vector3(0f, 0f, 0f); // 화면 중앙 기준

        Dictionary<string, Vector3> suitPositions = new()
        {
            { "S", centerPosition + new Vector3(-spacing * 1.5f, 0, 0) },  // ♠
            { "H", centerPosition + new Vector3(-spacing * 0.5f, 0, 0) },  // ♥
            { "D", centerPosition + new Vector3(spacing * 0.5f, 0, 0) },   // ♦
            { "C", centerPosition + new Vector3(spacing * 1.5f, 0, 0) },   // ♣
        };




        for (int i = 0; i < cardsPerBatch; i++)
        {
            string suit = suits[Random.Range(0, suits.Count)];
            int rank = Random.Range(1, 14);
            Sprite sprite = CardManager.GetCardSprite(suit, rank);

            // 카드 생성
            GameObject card = Instantiate(card3DPrefab, graveyardArea);
            card.name = $"{suit}{rank}";

            // 해당 무늬 스택의 기존 카드 개수만큼 높이 계산
            int sameSuitCount = graveyardCards.FindAll(c => c != null && c.name.StartsWith(suit)).Count;
            float height = sameSuitCount * cardThickness;

            // ★ 무늬별로 일정 위치에 깔끔히 쌓기
            card.transform.localPosition = suitPositions[suit]
                               + new Vector3(0, height, 0);

            card.transform.localRotation = Quaternion.Euler(90, 0, 0); // 정방향으로 쌓기

            // 머티리얼 적용
            MeshRenderer renderer = card.GetComponent<MeshRenderer>();
            if (renderer != null && sprite != null)
            {
                Material mat = new Material(Shader.Find("Unlit/Transparent"));
                mat.mainTexture = sprite.texture;
                renderer.material = mat;
            }

            graveyardCards.Add(card);
        }

        Debug.Log($"Generated {cardsPerBatch} cards and stacked in graveyard. Total now: {graveyardCards.Count}");
    }






    // ★ 무늬별 카운트 함수 개선
    public Dictionary<string, int> GetSuitCounts()
    {
        Dictionary<string, int> counts = new()
        {
            { "S", 0 },
            { "H", 0 },
            { "D", 0 },
            { "C", 0 }
        };

        foreach (var card in graveyardCards)
        {
            if (card == null) continue;

            foreach (string suit in suits)
            {
                if (card.name.StartsWith(suit))
                    counts[suit]++;
            }
        }

        return counts;
    }
}
