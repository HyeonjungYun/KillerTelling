using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObstacleHover3D : MonoBehaviour
{
    [Header("설명 데이터")]
    public string obstacleName;
    [TextArea] public string conditionText;
    [TextArea] public string effectText;

    private bool IsTutorialBoard()
    {
        if (StageManager.Instance == null) return false;
        return StageManager.Instance.IsStage1TutorialPhase;
    }

    private void OnMouseEnter()
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

    private void OnMouseExit()
    {
        // Hide는 언제든 안전하게
        if (ObstacleInfoUI.Instance != null)
        {
            ObstacleInfoUI.Instance.Hide();
        }
    }

    private void OnDisable()
    {
        // 오브젝트 비활성화될 때도 남는 UI 방지
        if (ObstacleInfoUI.Instance != null)
            ObstacleInfoUI.Instance.Hide();
    }
}
