using UnityEngine;
using TMPro;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    [Header("=== 3D 카드 표시 영역 ===")]
    public Transform selectedCard3DSpawnPoint;
    public GameObject card3DPrefab;

    private bool isExchangeMode = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("중복 HandManager 제거됨: " + gameObject.name);
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isExchangeMode = !isExchangeMode;
            Debug.Log("🔄 교환 모드 : " + (isExchangeMode ? "ON" : "OFF"));
        }
    }

    public bool IsExchangeMode()
    {
        return isExchangeMode;
    }

    // ============================================================
    // ⭐ JokerDraggable에서 명중한 카드가 들어오는 함수
    // ============================================================
    public void OnCardSelectedFromDeck(Sprite sprite)
    {
        if (sprite == null)
        {
            Debug.LogError("❌ OnCardSelectedFromDeck: 전달된 Sprite가 NULL!");
            return;
        }

        Debug.Log("🔵 명중 카드 처리: " + sprite.name);
        SpawnSelectedCard3D(sprite);

        isExchangeMode = false;
        Debug.Log("🔒 교환모드 자동 종료");
    }

    public void SpawnSelectedCard3D(Sprite spr)
    {
        if (card3DPrefab == null)
        {
            Debug.LogError("❌ Card3D 프리팹이 지정되지 않았습니다!");
            return;
        }

        if (selectedCard3DSpawnPoint == null)
        {
            Debug.LogError("❌ SelectedCard3DSpawnPoint가 지정되지 않았습니다!");
            return;
        }

        int childCount = selectedCard3DSpawnPoint.childCount;

        if (childCount >= 7)
        {
            Debug.Log("⚠ 이미 7장의 교환 카드가 선택됨!");
            return;
        }

        GameObject obj = Instantiate(card3DPrefab, selectedCard3DSpawnPoint);

        Card3D card3D = obj.GetComponent<Card3D>();
        if (card3D != null)
            card3D.SetSprite(spr);

        Transform tObj = obj.transform;

        float xOffset = 0.15f * childCount;

        tObj.localPosition = new Vector3(0.5f + xOffset, -6f, 0.1f);
        tObj.localRotation = Quaternion.identity;
        tObj.localScale = new Vector3(0.25f, 0.35f, 0.25f);

        Debug.Log("✨ 3D 카드 생성 완료! (총 " + (childCount + 1) + "장)");
    }
}
