using UnityEngine;

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

    public void OnCardSelectedFromDeck(Sprite sprite)
    {
        if (!isExchangeMode)
        {
            Debug.Log("교환 모드가 아니므로 카드 클릭 무시됨");
            return;
        }

        if (sprite == null)
        {
            Debug.LogError("❌ OnCardSelectedFromDeck: 전달된 Sprite가 NULL!");
            return;
        }

        Debug.Log("🔵 선택된 덱 카드 받음: " + sprite.name);

        SpawnSelectedCard3D(sprite);

        JokerStack3D.Instance.UseOneJoker();
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
        float yOffset = 0 * childCount;

        tObj.localPosition = new Vector3(0.5f + xOffset, -6f + yOffset, 0.1f);
        tObj.localRotation = Quaternion.Euler(0f, 0f, 0f);
        tObj.localScale = new Vector3(0.25f, 0.35f, 0.25f);

        Debug.Log("✨ 선택 카드 3D 스폰 완료! (현재 수 = " + (childCount + 1) + ")");
    }
}
