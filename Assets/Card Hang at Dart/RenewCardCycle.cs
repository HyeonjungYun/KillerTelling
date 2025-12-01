using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenewCardCycle : MonoBehaviour
{
    public WallCardPlacer wallPlacer;
    public DeckManager deckManager;
    public RectTransform targetArea;
    public Button renewButton;

    private void Start()
    {
        renewButton.onClick.AddListener(OnRenewClicked);
    }

    private void OnRenewClicked()
    {
        Debug.Log("🔄 [Renew] 새 카드 5장 뽑기 시작!");

        // 1) 기존 과녁 카드 무덤으로 이동
        MoveOldCardsToGraveyard();

        // 2) 덱에서 5장 가져오기
        List<Sprite> newSprites = Draw5FromDeck();
        if (newSprites == null || newSprites.Count < 5)
        {
            Debug.LogError("❌ 덱에서 5장을 가져오지 못했습니다!");
            return;
        }

        // 3) 과녁에 새 카드 배치
        wallPlacer.PlaceCards(newSprites);

        Debug.Log("✨ [Renew] 새로운 5장 배치 완료!");
    }

    // ---------------------------
    // 기존 과녁 카드 → 무덤 이동
    // ---------------------------
    private void MoveOldCardsToGraveyard()
    {
        List<Sprite> removeList = new List<Sprite>();

        for (int i = targetArea.childCount - 1; i >= 0; i--)
        {
            Transform child = targetArea.GetChild(i);

            // 과녁 배경 제외
            if (child.name.Contains("BackGround"))
                continue;

            Image img = child.GetComponent<Image>();
            if (img != null && img.sprite != null)
                removeList.Add(img.sprite);

            Destroy(child.gameObject);
        }

        // 🔥 GraveyardTester가 쓰는 구조와 완벽히 일치
        if (removeList.Count > 0)
            CardGraveyardManager.Instance.AddCards(removeList);
    }

    // ---------------------------
    // 덱에서 5장 랜덤 뽑기
    // ---------------------------
    private List<Sprite> Draw5FromDeck()
    {
        DeckCard[] deckCards = FindObjectsOfType<DeckCard>();

        List<DeckCard> selectable = new List<DeckCard>();
        foreach (var dc in deckCards)
        {
            if (dc.CardSprite != null && dc.GetComponent<Image>().raycastTarget)
                selectable.Add(dc);
        }

        if (selectable.Count < 5)
            return null;

        List<Sprite> picked = new List<Sprite>();

        for (int i = 0; i < 5; i++)
        {
            int idx = Random.Range(0, selectable.Count);
            DeckCard card = selectable[idx];
            selectable.RemoveAt(idx);

            picked.Add(card.CardSprite);

            Image img = card.GetComponent<Image>();
            img.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            img.raycastTarget = false;
        }

        return picked;
    }
}
