using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckCard : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image image;

    private Vector3 normalScale = Vector3.one;
    private Vector3 hoverScale = new Vector3(2.3f, 2.3f, 2.3f);   // 🔥 2.3배 확대

    private Outline outline;
    private RectTransform rt;

    // 🔊 (추가) 효과음 클립 (DeckManager가 주입)
    public AudioClip hoverSound;
    public AudioClip clickSound;
    private AudioSource audioSource;

    // 🔒 (추가) 덱 카드 여부 (덱에서 생성된 카드만 반응)
    private bool isDeckCard = false;

    private void Awake()
    {
        image = GetComponent<Image>();
        rt = GetComponent<RectTransform>();

        // 🔥 확대 시 덱 패널 밖으로 튀어나가지 않도록 위쪽 pivot 유지
        rt.pivot = new Vector2(0.5f, 1f);

        // 🔊 오디오 소스 자동 추가
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // 🔍 부모 체인 중 DeckManager 가 있으면 "덱 카드"
        isDeckCard = GetComponentInParent<DeckManager>() != null;
    }

    public Sprite CardSprite => image != null ? image.sprite : null;

    public void MarkAsUsed()
    {
        if (image != null)
        {
            image.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            image.raycastTarget = false;
        }
        transform.localScale = normalScale;

        Outline outline = GetComponent<Outline>();
        if (outline != null)
            Destroy(outline);
    }

    // ────────────────────────────────
    // 🔥 Hover Enter
    // ────────────────────────────────
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDeckCard) return;                // ✅ 덱 카드만
        if (image == null) return;
        if (image.raycastTarget == false) return; // 회색카드 제외

        // 🔥 다른 카드 위로 올라오게
        transform.SetAsLastSibling();

        // 🔥 2.3배 확대
        transform.localScale = hoverScale;

        // 🔥 테두리 얇게 수정 (2px 정도)
        outline = gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.8f, 0.1f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);

        // 🔥 밝기 증가
        image.color = Color.white * 1.15f;

        // 🔊 Hover 사운드
        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    // ────────────────────────────────
    // 🔥 Hover Exit
    // ────────────────────────────────
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDeckCard) return;
        if (image == null) return;

        // ✅ 이미 사용된(회색 처리된) 카드는 색을 건드리지 말고 그대로 두기
        if (image.raycastTarget == false)
            return;

        transform.localScale = normalScale;

        if (outline != null)
            Destroy(outline);

        image.color = Color.white;
    }

    // ────────────────────────────────
    // 클릭
    // ────────────────────────────────
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDeckCard) return;

        // 🔊 클릭 사운드
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);

        if (!HandManager.Instance.IsExchangeMode()) return;
        if (image == null || image.sprite == null) return;

        HandManager.Instance.OnCardSelectedFromDeck(image.sprite);
        MarkAsUsed();
    }
}
