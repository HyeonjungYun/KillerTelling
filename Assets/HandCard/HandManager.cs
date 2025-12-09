using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    [Header("3D 카드 패 영역")]
    public Transform selectedCard3DSpawnPoint;
    public GameObject card3DPrefab;

    [Header("Camera")]
    public CameraRotator camRot;   // 카메라 회전 스크립트 참조

    private bool isExchangeMode = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (camRot == null)
            camRot = FindFirstObjectByType<CameraRotator>();
    }

    void Update()
    {
        // B 키로 교환 모드 토글
        if (Input.GetKeyDown(KeyCode.B))
        {
            isExchangeMode = !isExchangeMode;
            Debug.Log("교환 모드: " + isExchangeMode);
        }
    }

    public bool IsExchangeMode() => isExchangeMode;

    // -----------------------------------------------------
    // 덱 클릭 → 조커 1개 영구 소모 + 패에 카드 추가
    // -----------------------------------------------------
    public void OnCardSelectedFromDeck(Sprite sprite)
    {
        if (sprite == null) return;

        JokerStack3D.Instance.UseOneJoker();
        SpawnSelectedCard3D(sprite);

        if (camRot != null)
            camRot.LookDefault();

        // 던지기 모드에서 손에 들고 있던 조커가 있으면 제거
        JokerDraggable.DestroyActiveJokerImmediately();

        isExchangeMode = false;
        Debug.Log("🔒 교환모드 자동 종료됨 (덱 선택 + 카메라 원위치)");
    }

    // -----------------------------------------------------
    // 조커로 과녁 카드를 맞췄을 때
    // -----------------------------------------------------
    public void OnCardHitByThrow(Sprite sprite)
    {
        if (sprite == null) return;

        Debug.Log("🎯 조커 명중 → 패로 이동 (조커 소모 없음)");
        SpawnSelectedCard3D(sprite);
    }

    // -----------------------------------------------------
    // 패 영역에 3D 카드 생성
    // -----------------------------------------------------
    public void SpawnSelectedCard3D(Sprite spr)
    {
        if (selectedCard3DSpawnPoint == null || card3DPrefab == null) return;

        int count = selectedCard3DSpawnPoint.childCount;
        if (count >= 7) return;

        GameObject obj = Instantiate(card3DPrefab, selectedCard3DSpawnPoint);

        if (obj.TryGetComponent(out Card3D card3D))
            card3D.SetSprite(spr);

        obj.transform.localPosition = new Vector3(
            0.5f + count * 0.15f,
            -6f,
            0.1f
        );

        obj.transform.localScale = new Vector3(0.25f, 0.35f, 0.25f);
        obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    // -----------------------------------------------------
    // 🔥 새 스테이지 시작 / 결과 확인 뒤 등에 패를 싹 비움
    // -----------------------------------------------------
    public void ClearSelectedCards3D()
    {
        if (selectedCard3DSpawnPoint == null) return;

        for (int i = selectedCard3DSpawnPoint.childCount - 1; i >= 0; i--)
        {
            Destroy(selectedCard3DSpawnPoint.GetChild(i).gameObject);
        }

        Debug.Log("🧹 HandManager: 플레이어 패 3D 카드 전체 삭제");
    }
}
