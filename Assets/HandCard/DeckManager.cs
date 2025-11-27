using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    public RectTransform deckPanel;   // 카드덱 패널 (캔버스 하위)
    public GameObject cardPrefab;     // 카드 UI 프리팹 (Image)
    public CardManager cardManager;
    private Sprite emptySlotSprite;

    private List<GameObject> deckCards = new List<GameObject>();
    private List<Sprite> removedCards = new List<Sprite>();
    public List<Sprite> RemovedCards => removedCards;

    private float cardWidth = 28f;
    private float cardHeight = 45f;
    private float spacingX = 6f;
    private float spacingY = 8f;

    private const int columns = 13;
    private const int rows = 4;

    void Awake()
    {
        Texture2D tex = new Texture2D((int)cardWidth, (int)cardHeight);
        Color c = new Color(1f, 1f, 1f, 0.15f);

        for (int y = 0; y < tex.height; y++)
            for (int x = 0; x < tex.width; x++)
                tex.SetPixel(x, y, c);

        tex.Apply();

        emptySlotSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    public void ShowRemainingDeck(CardManager cardManager, List<Sprite> usedCards)
    {
        if (deckPanel == null || cardPrefab == null || cardManager == null)
        {
            Debug.LogError("❌ DeckPanel, CardPrefab, CardManager 중 하나가 연결 안 됨");
            return;
        }

        // ✔ rtPanel = deckPanel (필수)
        RectTransform rtPanel = deckPanel;

        // 기존 카드 정리
        foreach (Transform child in rtPanel)
            Destroy(child.gameObject);

        deckCards.Clear();

        List<Sprite> allCards = new List<Sprite>(cardManager.cardSprites);
        int total = Mathf.Min(allCards.Count, 52);

        for (int i = 0; i < total; i++)
        {
            Sprite s = allCards[i];

            int col = i % columns;
            int row = i / columns;

            float x = col * (cardWidth + spacingX);
            float y = -row * (cardHeight + spacingY);

            // ---- 빈 슬롯 ----
            if (usedCards.Contains(s))
            {
                GameObject blank = new GameObject($"EmptySlot_{i}", typeof(RectTransform), typeof(Image));
                blank.transform.SetParent(rtPanel, false);

                RectTransform rtBlank = blank.GetComponent<RectTransform>();
                rtBlank.anchorMin = new Vector2(0, 1);
                rtBlank.anchorMax = new Vector2(0, 1);
                rtBlank.pivot = new Vector2(0, 1);
                rtBlank.sizeDelta = new Vector2(cardWidth, cardHeight);
                rtBlank.anchoredPosition = new Vector2(x, y);

                Image img = blank.GetComponent<Image>();
                img.sprite = emptySlotSprite;
                img.color = Color.white;

                continue;
            }

            // ---- 실제 카드 ----
            GameObject card = Instantiate(cardPrefab, rtPanel);
            Image imgCard = card.GetComponent<Image>();
            RectTransform rtCard = card.GetComponent<RectTransform>();

            imgCard.sprite = s;
            rtCard.anchorMin = new Vector2(0, 1);
            rtCard.anchorMax = new Vector2(0, 1);
            rtCard.pivot = new Vector2(0, 1);
            rtCard.sizeDelta = new Vector2(cardWidth, cardHeight);
            rtCard.anchoredPosition = new Vector2(x, y);

            card.AddComponent<DeckCard>();

            deckCards.Add(card);
        }

        Debug.Log($"📚 덱카드 생성 완료! {deckCards.Count}장 (빈칸 포함 {total}칸)");
    }

    void Start()
    {
        if (deckPanel != null)
        {
            RectTransform rt = deckPanel;

            // 우측 상단에 완전 고정
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);

            // 🔥 pivot을 (1,1)로 되돌려야 함
            // 그래야 패널이 오른쪽 방향으로 확장되지 않고
            // 화면을 침범하지 않음
            rt.pivot = new Vector2(1, 1);

            // 위치 고정
            rt.anchoredPosition = new Vector2(135f, -10f);

            // 🔥 13x4 카드 완전체를 딱 맞게 보여주는 크기
            rt.sizeDelta = new Vector2(580f, 380f);

            // 🔥 흐릿해지는 localScale 제거
            rt.localScale = Vector3.one;
        }

        ShowRemainingDeck(cardManager, new List<Sprite>());
    }



}
