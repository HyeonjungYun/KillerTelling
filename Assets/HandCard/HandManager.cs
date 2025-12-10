using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    [Header("3D 카드 패 영역")]
    public Transform selectedCard3DSpawnPoint;
    public GameObject card3DPrefab;

    [Header("Camera")]
    public CameraRotator camRot;   // 카메라 회전 스크립트 참조

    // "조커를 손에 들고 있어서 덱과 교환 가능한 상태인지" 표시
    private bool isExchangeMode = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (camRot == null)
            camRot = FindFirstObjectByType<CameraRotator>();
    }

    // -----------------------------------------------------
    // 조커 쪽에서 상태를 알려줄 때 사용
    // -----------------------------------------------------
    public void SetExchangeMode(bool enable)
    {
        isExchangeMode = enable;
        Debug.Log("교환 모드: " + isExchangeMode);
    }

    public bool IsExchangeMode() => isExchangeMode;

    // -----------------------------------------------------
    // 🔥 우측 덱 클릭
    //  - 반드시 "조커를 손에 들고 있는 상태(던지기 모드)"에서만 동작
    //  - 그렇지 않으면 그냥 무시
    // -----------------------------------------------------
    public void OnCardSelectedFromDeck(Sprite sprite)
    {
        if (sprite == null) return;

        // 1) 던지기 모드가 아니면 교환 불가
        if (!isExchangeMode || JokerDraggable.ActiveJoker == null)
        {
            Debug.Log("⚠ 조커를 손에 들고 있을 때만 덱에서 카드를 가져올 수 있습니다.");
            return;
        }

        // 2) 조커 1개 영구 소모
        //    UseOneJoker 안에서 ActiveJoker가 있으면 그 조커를 테이블/씬에서 제거해줌
        if (JokerStack3D.Instance != null)
            JokerStack3D.Instance.UseOneJoker();
        else
            Debug.LogWarning("HandManager: JokerStack3D.Instance 가 없음");

        // 3) 플레이어 패에 카드 추가
        SpawnSelectedCard3D(sprite);

        // 4) 카메라 원위치 복귀
        if (camRot != null)
            camRot.LookDefault();

        // 5) 교환 모드 종료
        isExchangeMode = false;
        Debug.Log("🔒 교환모드 종료 (덱 교환 완료)");
    }

    // -----------------------------------------------------
    // 조커로 과녁 카드를 맞췄을 때 (조커 소모 X)
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
            0.2f + count * 0.15f,
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
