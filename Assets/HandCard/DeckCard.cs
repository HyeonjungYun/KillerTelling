using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image image;

    private Vector3 normalScale = Vector3.one;
    private Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);

    public Sprite CardSprite => image != null ? image.sprite : null;

    private void Awake()
    {
        image = GetComponent<Image>();
        normalScale = transform.localScale;
    }

    // 🔥 게임 시작 시 자동 5장 선택용 함수
    public void MarkAsUsed()
    {
        if (image != null)
        {
            image.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            image.raycastTarget = false;
        }
        transform.localScale = normalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!HandManager.Instance.IsExchangeMode()) return;
        if (image.raycastTarget == false) return;

        transform.localScale = hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = normalScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!HandManager.Instance.IsExchangeMode()) return;

        if (image == null || image.sprite == null)
            return;

        HandManager.Instance.OnCardSelectedFromDeck(image.sprite);

        MarkAsUsed(); // 🔥 기존 클릭 로직 동일
    }
}
