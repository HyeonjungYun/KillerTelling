using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI")]
    public GameObject panelRoot;
    public TextMeshProUGUI tutorialText;

    [Header("Click Cue (▼) UI")]
    [Tooltip("패널 우측 하단에 배치한 TMP 텍스트(내용: ▼)를 연결하세요.")]
    public TextMeshProUGUI clickCueText;
    [Tooltip("▼ 깜빡이는 속도(초). 값이 작을수록 빠르게 깜빡입니다.")]
    public float clickCueBlinkPeriod = 0.6f;

    [Header("Help Panel (옵션)")]
    public GameObject helpPanel;

    [Header("Timing")]
    public float rightClickHintDelay = 2f;
    public float helpOpenDelay = 6.0f;

    private bool isTutorialActive = false;

    // (기존 문자열은 유지하지만, 이제는 본문에 붙이지 않고 ▼ UI로 표현)
    private const string CLICK_CUE = "\n<size=60%><alpha=#88>▼ 클릭</alpha></size>";

    private enum Step
    {
        None,
        Intro_WaitFirstJoker,
        AfterFirstJoker_ShowHold,
        AfterHold_Click_ToRightClick,
        After5Sec_ShowHit4Heart,
        WaitThrowAndCameraBack,
        ShowGoalAndOpenHelp,
        WaitHelpClosed,
        WaitSecondJokerPick,
        ExplainRenew1,
        ExplainRenew2,
        ExplainObstacle1,
        ExplainObstacle2,
        ExplainDeck1,
        ExplainDeck2,
        WaitSecondCardFromDeck,
        ExplainPenalty1,
        ExplainPenalty2,
        ExplainSubmit,
        Done
    }

    private Step currentStep = Step.None;
    private bool waitingClick = false;

    private Coroutine autoAdvanceRoutine;
    private Coroutine helpRoutine;
    private Coroutine clickCueRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        ForceHide();
    }

    public void ForceHide()
    {
        isTutorialActive = false;

        if (panelRoot) panelRoot.SetActive(false);
        if (helpPanel) helpPanel.SetActive(false);

        waitingClick = false;

        StopAllCoroutines();
        autoAdvanceRoutine = null;
        helpRoutine = null;
        clickCueRoutine = null;

        currentStep = Step.None;

        SetClickCueActive(false);
    }

    public void StartTutorial()
    {
        isTutorialActive = true;
        ShowPanel(true);
        currentStep = Step.Intro_WaitFirstJoker;
        waitingClick = false;

        SetText("튜토리얼 시작!\n테이블에 표시된 조커 중 하나를 선택하면 던지기 모드에 진입합니다.");
        SetClickCueActive(false);
    }

    private void Update()
    {
        if (!isTutorialActive) return;

        // helpPanel 방식일 때 닫힘 감지(보험)
        if (currentStep == Step.WaitHelpClosed && helpPanel != null && !helpPanel.activeSelf)
        {
            OnHelpClosed();
            return;
        }

        if (waitingClick && Input.GetMouseButtonDown(0))
        {
            waitingClick = false;
            SetClickCueActive(false);
            OnClickAnywhere();
        }
    }

    private void OnClickAnywhere()
    {
        switch (currentStep)
        {
            case Step.AfterFirstJoker_ShowHold:
                currentStep = Step.AfterHold_Click_ToRightClick;

                // ✅ 이 문구는 “클릭으로 넘기는” 문구가 아니라,
                // 바로 다음(2초 후) 자동으로 4하트 안내가 떠야 하므로 ▼ OFF
                SetText("마우스 우측 클릭을 누르면 왼쪽/오른쪽/직선 순서로 궤적을 변경할 수 있습니다.");
                SetClickCueActive(false);

                StartAutoAdvanceToHit4Heart();
                break;

            case Step.ExplainRenew1:
                currentStep = Step.ExplainRenew2;
                SetTextWithCue("과녁을 갱신하면 기존에 걸려 있던 카드들은 카드 무덤에 이동하게 됩니다.", true);
                waitingClick = true;
                break;

            case Step.ExplainRenew2:
                currentStep = Step.ExplainObstacle1;
                SetTextWithCue("카드 과녁에 특정 카드들이 쌓이면 카드 명중을 방해하는 장애물이 등장합니다!", true);
                waitingClick = true;
                break;

            case Step.ExplainObstacle1:
                currentStep = Step.ExplainObstacle2;
                SetTextWithCue("장애물은 전체 스테이지 동안 유지되기 때문에 과녁 갱신에는 신중함이 필요합니다!", true);
                waitingClick = true;
                break;

            case Step.ExplainObstacle2:
                currentStep = Step.ExplainDeck1;
                SetTextWithCue("우측 상단의 덱을 통해 아직 과녁에 등장하지 않은 카드들을 확인할 수 있습니다.", true);
                waitingClick = true;
                break;

            case Step.ExplainDeck1:
                currentStep = Step.ExplainDeck2;
                SetTextWithCue("만약 원하는 카드를 바로 얻고 싶으면 해당 카드를 클릭하면 됩니다.", true);
                waitingClick = true;
                break;

            case Step.ExplainDeck2:
                currentStep = Step.WaitSecondCardFromDeck;

                // ✅ 여기부터는 “플레이가 진행”되는 구간이므로 ▼ OFF
                SetTextWithCue("현재 덱에 있는 카드와 함께 원페어를 구성할 수 있는 카드를 선택해 보세요.", false);
                break;

            case Step.ExplainPenalty1:
                currentStep = Step.ExplainPenalty2;
                SetTextWithCue("점차 더 조합하기 어려운 덱이 요구되므로 슬기로운 선택을 하시길 바랍니다.", true);
                waitingClick = true;
                break;

            case Step.ExplainPenalty2:
                currentStep = Step.ExplainSubmit;

                // ✅ 제출 안내는 클릭 유도보단 행동 유도 → ▼ OFF
                SetTextWithCue("이제 목표 덱을 완성했으니 'Submission' 버튼을 클릭해보세요.", false);
                waitingClick = false;
                break;
        }
    }

    private void StartAutoAdvanceToHit4Heart()
    {
        if (autoAdvanceRoutine != null) StopCoroutine(autoAdvanceRoutine);
        autoAdvanceRoutine = StartCoroutine(AutoAdvanceToHit4Heart());
    }

    private IEnumerator AutoAdvanceToHit4Heart()
    {
        yield return new WaitForSeconds(rightClickHintDelay);

        if (!isTutorialActive) yield break;

        currentStep = Step.After5Sec_ShowHit4Heart;

        // ✅ 자동 안내는 클릭 유도 X → ▼ OFF
        SetTextWithCue("궤적을 참고하여 과녁에 걸린 4하트 카드를 명중시켜 보세요!", false);

        autoAdvanceRoutine = null;
    }

    public void OnJokerPicked()
    {
        if (!isTutorialActive) return;

        if (currentStep == Step.Intro_WaitFirstJoker)
        {
            currentStep = Step.AfterFirstJoker_ShowHold;

            // ✅ “이 문구만” 클릭 유도 표시 필요
            SetTextWithCue("조커를 잡은 후 클릭을 유지하면 예상 궤적을 확인할 수 있습니다.", false);
            waitingClick = true;
        }
        else if (currentStep == Step.WaitSecondJokerPick)
        {
            currentStep = Step.ExplainRenew1;

            // ✅ 사용자가 원한 긴 구간(renew~deck 설명)은 클릭으로 넘김 → ▼ ON
            SetTextWithCue("과녁에 걸린 카드로 목표 덱을 만들기 어렵다면 'Renew' 버튼을 눌러보세요.", false);
            waitingClick = true;
        }
    }

    public void OnJokerThrown()
    {
        if (!isTutorialActive) return;
        currentStep = Step.WaitThrowAndCameraBack;
        SetClickCueActive(false);
    }

    public void OnCameraBackToTable()
    {
        if (!isTutorialActive) return;
        ShowGoalThenOpenHelpDelayed();
    }

    private void ShowGoalThenOpenHelpDelayed()
    {
        currentStep = Step.ShowGoalAndOpenHelp;

        // ✅ 목표 안내는 행동 유도 문구. 클릭 유도 X → ▼ OFF
        SetTextWithCue("이번 스테이지의 목표 덱은 원페어입니다.\n도움말 버튼을 눌러 족보를 확인해보세요.", false);

        if (helpRoutine != null) StopCoroutine(helpRoutine);
        helpRoutine = StartCoroutine(OpenHelpAfterDelay());
    }

    private IEnumerator OpenHelpAfterDelay()
    {
        yield return new WaitForSeconds(helpOpenDelay);
        if (!isTutorialActive) yield break;

        if (HelpPopupController.Instance != null && !HelpPopupController.Instance.IsOpen())
            HelpPopupController.Instance.ForceOpenFromTutorial();

        currentStep = Step.WaitHelpClosed;
        helpRoutine = null;
    }

    public void OnHelpClosed()
    {
        if (!isTutorialActive) return;

        currentStep = Step.WaitSecondJokerPick;

        // ✅ 이 문구는 클릭 유도 아님(다시 조커 집기 행동) → ▼ OFF
        SetTextWithCue("그럼 두 번째 카드를 얻기 위해 다시 조커를 집어봅시다.", false);
    }

    public void OnCardTakenFromDeck(Sprite spr)
    {
        if (!isTutorialActive || currentStep != Step.WaitSecondCardFromDeck) return;
        if (HandManager.Instance == null || HandManager.Instance.selectedCard3DSpawnPoint == null) return;

        if (HandManager.Instance.selectedCard3DSpawnPoint.childCount >= 2)
        {
            currentStep = Step.ExplainPenalty1;
            SetTextWithCue("덱에서 카드를 가져오면 다음 스테이지부터 조커 개수가 줄어듭니다.", true);
            waitingClick = true;
        }
    }

    private void ShowPanel(bool show)
    {
        if (panelRoot) panelRoot.SetActive(show);
    }

    private void SetText(string msg)
    {
        if (tutorialText) tutorialText.text = msg;
    }

    private void SetTextWithCue(string msg, bool cue)
    {
        // ✅ “특정 메시지에서만” 우측하단 ▼가 켜짐
        SetText(msg);
        SetClickCueActive(cue);
    }

    // ============================================================
    // ▼ 깜빡이 UI 제어
    // ============================================================
    private void SetClickCueActive(bool on)
    {
        if (clickCueText == null) return;

        if (!on)
        {
            if (clickCueRoutine != null) { StopCoroutine(clickCueRoutine); clickCueRoutine = null; }
            clickCueText.gameObject.SetActive(false);
            return;
        }

        clickCueText.gameObject.SetActive(true);

        if (clickCueRoutine != null) StopCoroutine(clickCueRoutine);
        clickCueRoutine = StartCoroutine(BlinkClickCue());
    }

    private IEnumerator BlinkClickCue()
    {
        float t = 0f;

        while (true)
        {
            t += Time.unscaledDeltaTime;

            // 0.25 ~ 1.0 범위 알파를 사인파로 부드럽게 깜빡
            float a = 0.25f + 0.75f * Mathf.Abs(Mathf.Sin((t / clickCueBlinkPeriod) * Mathf.PI));

            Color c = clickCueText.color;
            c.a = a;
            clickCueText.color = c;

            yield return null;
        }
    }

    // ============================================================
    // 🔗 외부 스크립트 호환용 브릿지 메서드들
    // ============================================================

    // Renew 버튼 클릭 시 (RenewCardCycle.cs에서 호출)
    public void OnRenewClicked()
    {
        if (!isTutorialActive) return;
        Debug.Log("[Tutorial] Renew 버튼 클릭됨");
    }

    // JokerDraggable.cs에서 두 번째 조커 집었을 때 호출
    public void OnSecondJokerPicked()
    {
        if (!isTutorialActive) return;

        if (currentStep == Step.WaitSecondJokerPick)
        {
            currentStep = Step.ExplainRenew1;
            SetTextWithCue("과녁에 걸린 카드로 목표 덱을 만들기 어렵다면 우측 하단의 'Renew' 버튼을 눌러보세요.", false);
            waitingClick = true;
        }
    }

    // 카드 명중 후 패로 들어갔을 때 (HandManager.cs에서 호출)
    public void OnCardHitAndAddedToHand(Sprite spr)
    {
        if (!isTutorialActive) return;
        Debug.Log("[Tutorial] 카드 패에 추가됨");
    }
}
