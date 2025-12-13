using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;   // ⭐ Singleton

    public RectTransform deckPanel;
    public GameObject cardPrefab;
    public CardManager cardManager;
    private Sprite emptySlotSprite;

    private List<GameObject> deckCards = new List<GameObject>();

    public AudioClip hoverSound;
    public AudioClip clickSound;


    // (선택) 필요하면 실제로 사용된 카드 목록을 저장해서 재사용해도 됨
    private List<Sprite> removedCards = new List<Sprite>();
    public List<Sprite> RemovedCards => removedCards;

    private float cardWidth = 50f;
    private float cardHeight = 100f;
    private float spacingX = 6f;
    private float spacingY = 8f;

    private const int columns = 13;
    private const int rows = 4;

    // Sprite 이름 → index 맵
    private Dictionary<string, int> cardNameToIndex = new Dictionary<string, int>();

    void Awake()
    {
        Instance = this;

        GenerateEmptySlotSprite();
        BuildCardIndexLookup();
    }

    // -----------------------------
    // 빈 슬롯 스프라이트 생성
    // -----------------------------
    private void GenerateEmptySlotSprite()
    {
        Texture2D tex = new Texture2D((int)cardWidth, (int)cardHeight);
        Color c = new Color(1f, 1f, 1f, 0.15f);

        for (int y = 0; y < tex.height; y++)
            for (int x = 0; x < tex.width; x++)
                tex.SetPixel(x, y, c);

        tex.Apply();

        emptySlotSprite = Sprite.Create(tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f));
    }

    // -----------------------------
    // 카드 이름 → 인덱스 매핑
    // -----------------------------
    private void BuildCardIndexLookup()
    {
        cardNameToIndex.Clear();

        for (int i = 0; i < cardManager.cardSprites.Length; i++)
        {
            Sprite spr = cardManager.cardSprites[i];
            if (spr == null) continue;

            string name = spr.name;

            if (!cardNameToIndex.ContainsKey(name))
                cardNameToIndex[name] = i;
        }
    }

    public int GetCardIndexByName(string name)
    {
        if (cardNameToIndex.TryGetValue(name, out int idx))
            return idx;

        Debug.LogWarning($"Unknown card name: {name}");
        return -1;
    }

    // -----------------------------
    // 덱 UI 표시
    // -----------------------------
    public void ShowRemainingDeck(CardManager cardManager, List<Sprite> usedCards)
    {
        if (deckPanel == null || cardPrefab == null || cardManager == null)
        {
            Debug.LogError("❌ DeckPanel, CardPrefab, CardManager 중 하나가 연결 안 됨");
            return;
        }

        RectTransform rtPanel = deckPanel;

        foreach (Transform child in rtPanel)
            Destroy(child.gameObject);

        deckCards.Clear();

        List<Sprite> allCards = new List<Sprite>(cardManager.cardSprites);
        int total = allCards.Count - 1;

        for (int i = 0; i < total; i++)
        {
            Sprite s = allCards[i];

            int col = i % columns;
            int row = i / columns;

            float x = col * (cardWidth + spacingX);
            float y = -row * (cardHeight + spacingY);

            // 빈 슬롯
            if (usedCards.Contains(s))
            {
                GameObject blank = new GameObject($"EmptySlot_{i}",
                    typeof(RectTransform), typeof(Image));
                blank.transform.SetParent(rtPanel, false);

                RectTransform rtBlank = blank.GetComponent<RectTransform>();
                rtBlank.anchorMin = new Vector2(0, 1);
                rtBlank.anchorMax = new Vector2(0, 1);
                rtBlank.pivot = new Vector2(0, 1);
                rtBlank.sizeDelta = new Vector2(cardWidth, cardHeight);
                rtBlank.anchoredPosition = new Vector2(x, y);

                Image img = blank.GetComponent<Image>();
                img.sprite = emptySlotSprite;

                continue;
            }

            // 실제 카드
            GameObject card = Instantiate(cardPrefab, rtPanel);
            Image imgCard = card.GetComponent<Image>();
            RectTransform rtCard = card.GetComponent<RectTransform>();

            imgCard.sprite = s;
            rtCard.anchorMin = new Vector2(0, 1);
            rtCard.anchorMax = new Vector2(0, 1);
            rtCard.pivot = new Vector2(0, 1);
            rtCard.sizeDelta = new Vector2(cardWidth, cardHeight);
            rtCard.anchoredPosition = new Vector2(x, y);

            DeckCard dc = card.AddComponent<DeckCard>();

            // 🔥 DeckCard 에 효과음 주입
            dc.hoverSound = hoverSound;
            dc.clickSound = clickSound;

            deckCards.Add(card);
        }

        Debug.Log($"📚 덱카드 생성 완료! {deckCards.Count}장 (전체 {total}칸)");
    }

    void Start()
    {
        if (deckPanel != null)
        {
            RectTransform rt = deckPanel;

            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);

            rt.anchoredPosition = new Vector2(-310f, -5f);
            rt.sizeDelta = new Vector2(100f, 100f);
            rt.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }

        // 첫 스테이지 시작용
        ShowRemainingDeck(cardManager, new List<Sprite>());
    }

    // ============================================================
    // 🔥 스테이지 리셋용 : 새 52장 덱으로 초기화
    // ============================================================
    public void ResetDeckForNewStage()
    {
        removedCards.Clear();
        ShowRemainingDeck(cardManager, new List<Sprite>());
        Debug.Log("🆕 [DeckManager] 새 스테이지용 덱 리셋 완료 (52장 기준)");
    }
}
