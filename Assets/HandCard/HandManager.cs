using UnityEngine;
using UnityEngine.UI;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    [Header("3D 카드 패 영역")]
    public Transform selectedCard3DSpawnPoint;
    public GameObject card3DPrefab;

    [Header("Camera")]
    public CameraRotator camRot;

    private bool isExchangeMode = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (camRot == null)
            camRot = FindFirstObjectByType<CameraRotator>();
    }

    public void SetExchangeMode(bool enable)
    {
        isExchangeMode = enable;
        Debug.Log("교환 모드: " + isExchangeMode);
    }

    public bool IsExchangeMode() => isExchangeMode;

    public void OnCardSelectedFromDeck(Sprite sprite)
    {
        if (sprite == null) return;

        if (!isExchangeMode || JokerDraggable.ActiveJoker == null)
        {
            Debug.Log("⚠ 조커를 손에 들고 있을 때만 덱에서 카드를 가져올 수 있습니다.");
            return;
        }

        if (JokerStack3D.Instance != null)
            JokerStack3D.Instance.UseOneJoker();
        else
            Debug.LogWarning("HandManager: JokerStack3D.Instance 가 없음");

        SpawnSelectedCard3D(sprite);

        if (camRot != null)
            camRot.LookDefault();

        isExchangeMode = false;
        Debug.Log("🔒 교환모드 종료 (덱 교환 완료)");

        // ✅ 튜토리얼이면 덱에서 카드 가져온 이벤트 전달
        // 덱 카드 클릭 후, 튜토리얼 이벤트 전달
        if (StageManager.Instance != null &&
            StageManager.Instance.currentStage == 1 &&
            StageManager.Instance.IsStage1TutorialPhase &&
            TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnCardTakenFromDeck(sprite);
        }

    }

    public void OnCardHitByThrow(Sprite sprite)
    {
        if (sprite == null) return;

        Debug.Log("🎯 조커 명중 → 패로 이동 (조커 소모 없음)");
        SpawnSelectedCard3D(sprite);

        if (StageManager.Instance != null &&
            StageManager.Instance.currentStage == 0 &&
            TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnCardHitAndAddedToHand(sprite);
        }
    }

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

    public void ClearSelectedCards3D()
    {
        if (selectedCard3DSpawnPoint == null) return;

        for (int i = selectedCard3DSpawnPoint.childCount - 1; i >= 0; i--)
            Destroy(selectedCard3DSpawnPoint.GetChild(i).gameObject);

        Debug.Log("🧹 HandManager: 플레이어 패 3D 카드 전체 삭제");
    }
}
