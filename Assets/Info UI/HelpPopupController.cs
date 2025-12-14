using UnityEngine;
using UnityEngine.UI;

public class HelpPopupController : MonoBehaviour
{
    public static HelpPopupController Instance;

    public GameObject helpPopup;
    public Button helpButton;
    public Button closeButton;

    // 🔊 효과음
    public AudioClip openSound;
    public AudioClip closeSound;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);

        // 🔊 AudioSource 자동 추가
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (helpPopup != null)
            helpPopup.SetActive(false);

        if (helpButton != null)
            helpButton.onClick.AddListener(OpenPopup);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePopup);
    }

    public bool IsOpen()
    {
        return helpPopup != null && helpPopup.activeSelf;
    }

    public void OpenPopup()
    {
        // 🔊 열기 사운드
        if (openSound != null)
            audioSource.PlayOneShot(openSound);

        if (helpPopup != null)
            helpPopup.SetActive(true);
    }

    public void ClosePopup()
    {
        // 🔊 닫기 사운드
        if (closeSound != null)
            audioSource.PlayOneShot(closeSound);

        if (helpPopup != null)
            helpPopup.SetActive(false);

        // ✅ Stage1 튜토 페이즈일 때만 튜토 진행 이벤트 전달
        if (StageManager.Instance != null &&
            StageManager.Instance.currentStage == 1 &&
            StageManager.Instance.IsStage1TutorialPhase &&
            TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnHelpClosed();
        }
    }

    public void ForceOpenFromTutorial()
    {
        // 🔊 튜토리얼 강제 오픈도 동일한 사운드 사용
        if (openSound != null)
            audioSource.PlayOneShot(openSound);

        if (helpPopup != null)
            helpPopup.SetActive(true);
    }
}
