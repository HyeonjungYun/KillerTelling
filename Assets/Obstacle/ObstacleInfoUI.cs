using UnityEngine;
using TMPro;

public class ObstacleInfoUI : MonoBehaviour
{
    public static ObstacleInfoUI Instance;

    [Header("UI References")]
    public CanvasGroup panelGroup;        // ObstacleInfoPanel 에 붙은 CanvasGroup
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI conditionText;
    public TextMeshProUGUI effectText;

    private void Awake()
    {
        // 싱글톤 등록
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        Hide();    // 시작 시 숨김
    }

    // ✅ 튜토리얼(연습판) 여부
    private bool IsTutorialBoard()
    {
        // StageManager가 없으면 그냥 정상 표시(안전)
        if (StageManager.Instance == null) return false;
        return StageManager.Instance.IsStage1TutorialPhase;
    }

    public void Show(string name, string cond, string effect)
    {
        // ✅ 튜토리얼에서는 설명 UI 표시 금지
        if (IsTutorialBoard())
        {
            Hide(); // 혹시 남아있으면 즉시 숨김
            return;
        }

        if (panelGroup == null) return;

        panelGroup.alpha = 1f;
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;

        if (nameText) nameText.text = name;
        if (conditionText) conditionText.text = cond;
        if (effectText) effectText.text = effect;
    }

    public void Hide()
    {
        if (panelGroup == null) return;

        panelGroup.alpha = 0f;
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;

        // 필요하면 텍스트도 비워줌
        if (nameText) nameText.text = "";
        if (conditionText) conditionText.text = "";
        if (effectText) effectText.text = "";
    }

    private void LateUpdate()
    {
        // ✅ 튜토리얼로 전환되는 순간, 떠있던 UI가 남는 것 방지(안전장치)
        if (IsTutorialBoard() && panelGroup != null && panelGroup.alpha > 0.001f)
        {
            Hide();
        }
    }
}
