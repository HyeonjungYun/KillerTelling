using UnityEngine;

public class ClickableJoker : MonoBehaviour
{
    public GameObject throwCardPrefab;

    void OnMouseDown()
    {
        // 중앙에 던지는 카드 생성
        GameObject card = Instantiate(
            throwCardPrefab,
            new Vector3(0, 1.2f, -1.5f),   // 테이블 중앙
            Quaternion.identity
        );

        // Card3D의 sprite 그대로 적용
        Sprite spr = GetComponent<Card3D>().GetSprite();
        card.GetComponent<Card3D>().SetSprite(spr);

        // 이제 throwCardPrefab에는 JokerDraggable + Rigidbody 붙어 있음
    }
}
