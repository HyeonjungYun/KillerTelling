using UnityEngine;
using TMPro;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    [Header("3D 카드 패 영역")]
    public Transform selectedCard3DSpawnPoint;
    public GameObject card3DPrefab;

    private bool isExchangeMode = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isExchangeMode = !isExchangeMode;
            Debug.Log("교환 모드: " + isExchangeMode);
        }
    }

    public bool IsExchangeMode() => isExchangeMode;


    // ===============================================================
    // ① 덱 클릭 → 조커 1개 소모 + 스택 삭제 + 교환모드 종료
    // ===============================================================
    public void OnCardSelectedFromDeck(Sprite sprite)
    {
        if (sprite == null) return;

        JokerStack3D.Instance.UseOneJoker();
        SpawnSelectedCard3D(sprite);

        // ⭐ 덱 선택 → 교환모드 자동 종료
        isExchangeMode = false;
        Debug.Log("🔒 교환모드 자동 종료됨");
    }

    // ===============================================================
    // ② 조커 던져서 명중 → 조커 소모 없음 + 교환모드 유지
    // ===============================================================
    public void OnCardHitByThrow(Sprite sprite)
    {
        if (sprite == null) return;

        Debug.Log("🎯 조커 명중 → 패로 이동 (조커 소모 없음)");
        SpawnSelectedCard3D(sprite);

        // ⭐ 명중은 교환모드와 관계 없음 → isExchangeMode 변화 없음
    }

    // ===============================================================
    public void SpawnSelectedCard3D(Sprite spr)
    {
        int count = selectedCard3DSpawnPoint.childCount;
        if (count >= 7) return;

        GameObject obj = Instantiate(card3DPrefab, selectedCard3DSpawnPoint);

        if (obj.TryGetComponent(out Card3D card3D))
            card3D.SetSprite(spr);

        // 위치
        obj.transform.localPosition = new Vector3(
            0.5f + count * 0.15f,
            -6f,
            0.1f
        );

        // 크기
        obj.transform.localScale = new Vector3(0.25f, 0.35f, 0.25f);

        // 회전 보정
        obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }
}
