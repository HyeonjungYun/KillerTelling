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
        if (renewButton != null)
            renewButton.onClick.AddListener(OnRenewClicked);
    }

    private void OnRenewClicked()
    {
        Debug.Log("🔄 [Renew] 새 카드 뽑기 (최대 5장)");

        // 1) 기존 과녁 카드 무덤으로 이동
        MoveOldCardsToGraveyard();

        // 2) 덱에서 최대 5장 가져오기
        List<Sprite> newSprites = DrawUpTo5FromDeck();

        // 덱에 남은 카드가 전혀 없다면 (더 이상 Renew 불가)
        if (newSprites == null || newSprites.Count == 0)
        {
            Debug.Log("❌ 덱이 비어서 더 이상 Renew 할 수 없습니다.");
            return;
        }

        // 3) 과녁에 새 카드 배치 (5장 미만이어도 OK)
        wallPlacer.PlaceCards(newSprites);

        Debug.Log($"✨ [Renew] 새 카드 {newSprites.Count}장 배치 완료!");
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
            if (child.name.Contains("BackGround") ||
                child.name.Contains("Background") ||
                child.name.Contains("Board") ||
                child.name.Contains("Dart"))
                continue;

            Image img = child.GetComponent<Image>();
            if (img != null && img.sprite != null)
                removeList.Add(img.sprite);

            Destroy(child.gameObject);
        }

        if (removeList.Count > 0 && CardGraveyardManager.Instance != null)
            CardGraveyardManager.Instance.AddCards(removeList);
    }

    // ---------------------------
    // 덱에서 "최대" 5장 랜덤 뽑기
    // (5장 미만이면 남은 만큼 전부)
    // ---------------------------
    private List<Sprite> DrawUpTo5FromDeck()
    {
        DeckCard[] deckCards = FindObjectsOfType<DeckCard>();

        // 아직 사용 가능한 덱 카드만 모으기
        List<DeckCard> selectable = new List<DeckCard>();
        foreach (var dc in deckCards)
        {
            Image img = dc.GetComponent<Image>();
            if (dc.CardSprite != null && img != null && img.raycastTarget)
                selectable.Add(dc);
        }

        int available = selectable.Count;
        if (available == 0)
        {
            // 정말 아무 카드도 안 남았으면 빈 리스트 반환
            return new List<Sprite>();
        }

        // 이번에 뽑을 개수: 최대 5장, 남은 만큼
        int drawCount = Mathf.Min(5, available);

        List<Sprite> picked = new List<Sprite>();

        for (int i = 0; i < drawCount; i++)
        {
            int idx = Random.Range(0, selectable.Count);
            DeckCard card = selectable[idx];
            selectable.RemoveAt(idx);

            picked.Add(card.CardSprite);

            // 덱에서 사용된 카드 → 회색 + 클릭 불가 처리
            Image img = card.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                img.raycastTarget = false;
            }
        }

        // 뽑고 나니 더 이상 선택 가능한 카드가 없다면 Renew 버튼 비활성화
        if (renewButton != null && (available - drawCount) <= 0)
        {
            renewButton.interactable = false;
            Debug.Log("🛑 덱이 모두 소진됨 → Renew 버튼 비활성화");
        }

        return picked;
    }
}
