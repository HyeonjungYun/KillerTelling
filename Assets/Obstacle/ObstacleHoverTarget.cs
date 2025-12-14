using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObstacleHoverTarget : MonoBehaviour
{
    [Header("설명 데이터")]
    [SerializeField] private string obstacleName = "Obstacle";
    [TextArea] [SerializeField] private string conditionText = "";
    [TextArea] [SerializeField] private string effectText = "";

    private bool IsTutorialBoard()
    {
        if (StageManager.Instance == null) return false;
        return StageManager.Instance.IsStage1TutorialPhase;
    }

    // HoverDetector에서 호출
    public void OnHoverEnter()
    {
        // ✅ 튜토리얼에서는 설명 UI 금지
        if (IsTutorialBoard())
        {
            if (ObstacleInfoUI.Instance != null)
                ObstacleInfoUI.Instance.Hide();
            return;
        }

        if (ObstacleInfoUI.Instance != null)
        {
            ObstacleInfoUI.Instance.Show(obstacleName, conditionText, effectText);
        }
    }

    public void OnHoverExit()
    {
        if (ObstacleInfoUI.Instance != null)
        {
            ObstacleInfoUI.Instance.Hide();
        }
    }

    private void OnDisable()
    {
        if (ObstacleInfoUI.Instance != null)
            ObstacleInfoUI.Instance.Hide();
    }
}
