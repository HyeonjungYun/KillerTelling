using System.Collections.Generic;
using UnityEngine;

public class CardGraveyardManager : MonoBehaviour
{
    public static CardGraveyardManager Instance;

    public Transform graveyardArea;
    public GameObject cardPrefab;

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
        foreach (Transform child in graveyardArea)
            Destroy(child.gameObject);

        Dictionary<char, List<Sprite>> suitGroups = new Dictionary<char, List<Sprite>>()
        {
            { 'S', new List<Sprite>() },
            { 'H', new List<Sprite>() },
            { 'D', new List<Sprite>() },
            { 'C', new List<Sprite>() },
        };

        foreach (Sprite spr in storedCards)
        {
            char suit = ExtractSuit(spr.name);
            suitGroups[suit].Add(spr);
        }

        float stackStartX = -1.5f;
        float stackSpacingX = 1.3f;
        float cardOffsetY = 0.04f;
        float cardScale = 1.1f;

        char[] suitOrder = { 'S', 'H', 'D', 'C' };

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

        // ❤️ 하트 카드가 3장 이상이면 장애물 Mesh 활성화
        CheckObstacleActivation(suitGroups);
    }

    private void CheckObstacleActivation(Dictionary<char, List<Sprite>> suitGroups)
    {
        int heartCount = suitGroups['H'].Count;

        // ObstacleMover는 항상 활성화 상태라고 가정
        GameObject obstacleRoot = GameObject.Find("ObstacleMover");
        if (obstacleRoot == null)
        {
            Debug.LogWarning("ObstacleMover not found in scene!");
            return;
        }

        // 실제 움직이는 Mesh 찾기
        Transform mesh = obstacleRoot.transform.Find("ObstacleMesh");
        if (mesh == null)
        {
            Debug.LogWarning("ObstacleMesh child not found under ObstacleMover!");
            return;
        }

        // 장애물 표시 여부
        mesh.gameObject.SetActive(heartCount >= 3);
    }

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
