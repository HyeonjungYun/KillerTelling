using UnityEngine;

public class Card3D : MonoBehaviour
{
    public CardData cardData;

    private MeshRenderer meshRenderer;
    private Sprite currentSprite;

    // 🔥 추가: 클릭 가능 여부
    public bool isInteractable = true;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetSprite(Sprite spr)
    {
        if (spr == null)
        {
            Debug.LogError("❌ Card3D.SetSprite: sprite is NULL");
            return;
        }

        currentSprite = spr;

        Material mat = new Material(Shader.Find("Unlit/Transparent"));
        mat.mainTexture = spr.texture;
        meshRenderer.material = mat;

        cardData = CardDatabase.GetCardDataFromSprite(spr);
    }

    public Sprite GetSprite()
    {
        return currentSprite;
    }
}
