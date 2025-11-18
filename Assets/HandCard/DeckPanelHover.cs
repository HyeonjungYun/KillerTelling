using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckPanelHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image panelImage;

    void Start()
    {
        if (panelImage != null)
            panelImage.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (panelImage != null)
        {
            panelImage.enabled = true;
            panelImage.color = new Color(1f, 1f, 0.3f, 0.6f); // ³ë¶õºû
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (panelImage != null)
        {
            panelImage.enabled = false;
        }
    }
}
