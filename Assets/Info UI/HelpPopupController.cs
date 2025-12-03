using UnityEngine;
using UnityEngine.UI;

public class HelpPopupController : MonoBehaviour
{
    public GameObject helpPopup;     // HelpPopup Panel
    public Button helpButton;        // 오른쪽 아래 ? 버튼
    public Button closeButton;       // 팝업 안의 X 버튼

    private void Start()
    {
        // 시작 시 팝업 비활성화
        helpPopup.SetActive(false);

        // 버튼 이벤트 연결
        helpButton.onClick.AddListener(OpenPopup);
        closeButton.onClick.AddListener(ClosePopup);
    }

    void OpenPopup()
    {
        helpPopup.SetActive(true);
    }

    void ClosePopup()
    {
        helpPopup.SetActive(false);
    }
}
