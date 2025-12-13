using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckCard : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image image;

    private Vector3 normalScale = Vector3.one;
    private Vector3 hoverScale = new Vector3(2.3f, 2.3f, 2.3f);

    private Outline outline;
    private RectTransform rt;

    // 🔥 효과음 (DeckManager가 런타임에 자동 주입할 예정)
    public AudioClip hoverSound;
    public AudioClip clickSound;
    private AudioSource audioSource;

    // 🔥 덱 카드 여부
    private bool isDeckCard = false;

    private void Awake()
    {
        image = GetComponent<Image>();
        rt = GetComponent<RectTransform>();
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

        if (outline != null)
            Destroy(outline);
    }

    // ────────────────────────────────
    // Hover Enter
    // ────────────────────────────────
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDeckCard) return;                 // 🔒 덱 카드만 반응
        if (image == null) return;
        if (!image.raycastTarget) return;        // 사용된 카드 제외

        transform.SetAsLastSibling();
        transform.localScale = hoverScale;

        outline = gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.8f, 0.1f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);

        image.color = Color.white * 1.15f;

        // 🔊 Hover 사운드
        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    // ────────────────────────────────
    // Hover Exit
    // ────────────────────────────────
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDeckCard) return;
        if (image == null) return;
        if (!image.raycastTarget) return;

        transform.localScale = normalScale;

        if (outline != null)
            Destroy(outline);

        image.color = Color.white;
    }

    // ────────────────────────────────
    // Click
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
