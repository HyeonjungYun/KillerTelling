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
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = sfxVolume;

        if (renewButton != null)
            renewButton.onClick.AddListener(OnRenewClicked);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.volume = sfxVolume;
        audioSource.PlayOneShot(clip);
    }

    private void OnRenewClicked()
    {
        PlaySfx(renewClickSound);

        Debug.Log("🔄 [Renew] 새 카드 뽑기 (최대 5장)");

        // ✅ Stage1 튜토 페이즈에서만 튜토 이벤트 전달
        if (StageManager.Instance != null &&
            StageManager.Instance.currentStage == 1 &&
            StageManager.Instance.IsStage1TutorialPhase &&
            TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnRenewClicked();
        }

        if (wallPlacer == null || targetArea == null)
        {
            Debug.LogError("❌ [Renew] wallPlacer 또는 targetArea 참조가 없습니다.");
            return;
        }

        // 1) 기존 과녁 카드 무덤으로 이동
        MoveOldCardsToGraveyard();

        // 2) 덱에서 최대 5장 가져오기
        List<Sprite> newSprites = DrawUpTo5FromDeck();

        if (newSprites == null || newSprites.Count == 0)
        {
            Debug.Log("❌ 덱이 비어서 더 이상 Renew 할 수 없습니다.");
            return;
        }

        // 3) 과녁에 새 카드 배치
        wallPlacer.PlaceCards(newSprites);

        Debug.Log($"✨ [Renew] 새 카드 {newSprites.Count}장 배치 완료!");
    }

    private void MoveOldCardsToGraveyard()
    {
        List<Sprite> removeList = new List<Sprite>();

        for (int i = targetArea.childCount - 1; i >= 0; i--)
        {
            Transform child = targetArea.GetChild(i);

            // 과녁 배경 제외
            if (child.name.Contains("BackGround") || child.name.Contains("Background") ||
                child.name.Contains("Board") || child.name.Contains("Dart"))
                continue;

            bool picked = false;

            // 1) UI 카드(Image)인 경우
            Image img = child.GetComponent<Image>();
            if (img != null && img.sprite != null)
            {
                removeList.Add(img.sprite);
                picked = true;
            }

            // 2) 3D 카드(Card3D)인 경우
            if (!picked)
            {
                Card3D card3D = child.GetComponent<Card3D>();
                if (card3D != null && card3D.CurrentSprite != null)
                {
                    removeList.Add(card3D.CurrentSprite);
                    picked = true;
                }
            }

            // 3) 혹시 자식에 붙어있는 형태면(Wrapper 오브젝트)
            if (!picked)
            {
                Card3D card3D = child.GetComponentInChildren<Card3D>(true);
                if (card3D != null && card3D.CurrentSprite != null)
                {
                    removeList.Add(card3D.CurrentSprite);
                    picked = true;
                }

                if (!picked)
                {
                    Image img2 = child.GetComponentInChildren<Image>(true);
                    if (img2 != null && img2.sprite != null)
                    {
                        removeList.Add(img2.sprite);
                        picked = true;
                    }
                }
            }

            Destroy(child.gameObject);
        }
        Debug.Log($"🪦 [Graveyard] 과녁→무덤 이동: {removeList.Count}장");

        if (removeList.Count > 0)
        {
            // 🔊 무덤으로 이동할 때 효과음
            PlaySfx(graveyardSound);

            if (CardGraveyardManager.Instance != null)
                CardGraveyardManager.Instance.AddCards(removeList);
        }
    }


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

            // 덱에서 사용된 카드 → 회색 + 클릭 불가 처리
            Image img = card.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                img.raycastTarget = false;
            }
        }

        if (renewButton != null && (available - drawCount) <= 0)
        {
            renewButton.interactable = false;
            Debug.Log("🛑 덱이 모두 소진됨 → Renew 버튼 비활성화");
        }

        return picked;
    }
}
