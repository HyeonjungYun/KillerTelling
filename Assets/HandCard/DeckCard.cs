using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image image;

    private Vector3 normalScale = Vector3.one;
    private Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f); // 살짝 확대

    private void Awake()
    {
        image = GetComponent<Image>();
        normalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!HandManager.Instance.IsExchangeMode()) return; // 🔥 교환 모드가 아니면 hover 금지
        if (image.raycastTarget == false) return;

        transform.localScale = hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = normalScale;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[DeckCard] CLICK: " + gameObject.name);

        if (HandManager.Instance == null)
        {
            Debug.LogError("[DeckCard] HandManager.Instance is NULL!");
            return;
        }

        if (image == null || image.sprite == null)
        {
            Debug.LogError("[DeckCard] No sprite found on this card!");
            return;
        }

        // 1️⃣ HandManager로 정보 전달
        HandManager.Instance.OnCardSelectedFromDeck(image.sprite);

        // 2️⃣ 선택된 카드 회색 처리
        image.color = new Color(0.5f, 0.5f, 0.5f, 1f);

        // 3️⃣ 다시 선택 못하게 하기 (hover도 꺼짐)
        image.raycastTarget = false;

        // 4️⃣ 스케일 원래대로 보정
        transform.localScale = normalScale;

        Debug.Log($"🔒 [DeckCard] '{gameObject.name}' 사용됨 → 회색 + 클릭 차단 완료");
    }
}
