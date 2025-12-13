using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenewCardCycle : MonoBehaviour
{
    public WallCardPlacer wallPlacer;
    public DeckManager deckManager;
    public RectTransform targetArea;
    public Button renewButton;

    [Header("SFX")]
    public AudioClip renewClickSound;
    public AudioClip graveyardSound;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (renewButton != null)
            renewButton.onClick.AddListener(OnRenewClicked);
    }

    private void OnRenewClicked()
    {
        // 🔊 클릭 사운드
        if (renewClickSound != null)
            audioSource.PlayOneShot(renewClickSound);

        Debug.Log("🔄 [Renew] 새 카드 뽑기 (최대 5장)");

        // 1) 기존 과녁 카드 → 무덤 이동
        MoveOldCardsToGraveyard();

        // 2) 덱에서 최대 5장 가져오기
        List<Sprite> newSprites = DrawUpTo5FromDeck();

        if (newSprites == null || newSprites.Count == 0)
        {
            Debug.Log("❌ 덱이 비어서 더 이상 Renew 할 수 없습니다.");
            return;
        }

        // 3) 새 카드 배치
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

        if (removeList.Count > 0)
        {
            // 🔊 무덤으로 카드 떨어지는 소리
            if (graveyardSound != null)
                audioSource.PlayOneShot(graveyardSound);

            if (CardGraveyardManager.Instance != null)
                CardGraveyardManager.Instance.AddCards(removeList);
        }
    }

    // ---------------------------
    // 덱에서 최대 5장 뽑기
    // ---------------------------
    private List<Sprite> DrawUpTo5FromDeck()
    {
        DeckCard[] deckCards = FindObjectsOfType<DeckCard>();

        List<DeckCard> selectable = new List<DeckCard>();
        foreach (var dc in deckCards)
        {
            Image img = dc.GetComponent<Image>();
            if (dc.CardSprite != null && img != null && img.raycastTarget)
                selectable.Add(dc);
        }

        int available = selectable.Count;
        if (available == 0)
            return new List<Sprite>();

        int drawCount = Mathf.Min(5, available);
        List<Sprite> picked = new List<Sprite>();

        for (int i = 0; i < drawCount; i++)
        {
            int idx = Random.Range(0, selectable.Count);
            DeckCard card = selectable[idx];
            selectable.RemoveAt(idx);

            picked.Add(card.CardSprite);

            Image img = card.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                img.raycastTarget = false;
            }
        }

        if (renewButton != null && available - drawCount <= 0)
        {
            renewButton.interactable = false;
            Debug.Log("🛑 덱이 모두 소진됨 → Renew 버튼 비활성화");
        }

        return picked;
    }
}
