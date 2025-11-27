using UnityEngine;

public class Card3D : MonoBehaviour
{
    public CardData cardData;

    private MeshRenderer meshRenderer;

    // 현재 적용된 Sprite 저장
    private Sprite currentSprite;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // --------------------------------------------------------------------
    //  카드 스프라이트 적용
    // --------------------------------------------------------------------
    public void SetSprite(Sprite spr)
    {
        if (spr == null)
        {
            Debug.LogError("❌ Card3D.SetSprite: sprite is NULL");
            return;
        }

        currentSprite = spr;  // ⭐ Sprite 저장

        // 재질 생성 (Unlit/Transparent)
        Material mat = new Material(Shader.Find("Unlit/Transparent"));
        mat.mainTexture = spr.texture;
        meshRenderer.material = mat;

        // CardData 자동 생성
        cardData = CardDatabase.GetCardDataFromSprite(spr);
    }

    // --------------------------------------------------------------------
    //  ⭐ JokerDraggable에서 호출하는 Sprite Getter
    // --------------------------------------------------------------------
    public Sprite GetSprite()
    {
        return currentSprite;
    }
}
