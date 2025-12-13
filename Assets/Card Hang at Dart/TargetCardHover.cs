using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TargetCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverScale = 1.6f;

    private Image img;
    private Color originalColor;

    private Vector3 baseScale;     // 🔥 “기준 스케일”
    private bool isHovering = false;

    public AudioClip hoverSound;
    public AudioClip exitSound;
    private AudioSource audioSource;

    private void Awake()
    {
        img = GetComponent<Image>();
        originalColor = img.color;

        // 🔥 기준 스케일은 Awake가 아니라 Start에서 잡는다
        // (WallCardPlacer가 스케일 세팅한 뒤)
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        baseScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHovering) return;
        isHovering = true;

        transform.localScale = baseScale * hoverScale;
        img.color = new Color(1f, 1f, 0.7f, 1f);

        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHovering) return;

        // 🔥 사용된 카드(덱 카드 회색 처리)는 건드리지 않는다
        if (!img.raycastTarget) return;

        isHovering = false;

        transform.localScale = baseScale;
        img.color = originalColor;

        if (exitSound != null)
            audioSource.PlayOneShot(exitSound);
    }
    private void OnDisable()
    {
        if (!img.raycastTarget) return;

        transform.localScale = baseScale;
        img.color = originalColor;
        isHovering = false;
    }

}
