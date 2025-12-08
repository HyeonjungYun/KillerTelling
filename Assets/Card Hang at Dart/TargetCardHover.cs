using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TargetCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverScale = 1.6f;

    private Image img;
    private Vector3 originalScale;
    private Color originalColor;

    private void Awake()
    {
        img = GetComponent<Image>();
        originalScale = transform.localScale;
        originalColor = img.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * hoverScale;
        img.color = new Color(1f, 1f, 0.7f, 1f);   // »ìÂ¦ ³ë¶õ»ö
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
        img.color = originalColor;
    }
}
