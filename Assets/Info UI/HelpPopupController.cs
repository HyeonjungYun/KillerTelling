using UnityEngine;
using UnityEngine.UI;

public class HelpPopupController : MonoBehaviour
{
    public GameObject helpPopup;
    public Button helpButton;
    public Button closeButton;

    // 🔊 효과음
    public AudioClip openSound;
    public AudioClip closeSound;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        helpPopup.SetActive(false);

        helpButton.onClick.AddListener(OpenPopup);
        closeButton.onClick.AddListener(ClosePopup);
    }

    void OpenPopup()
    {
        if (openSound != null)
            audioSource.PlayOneShot(openSound);

        helpPopup.SetActive(true);
    }

    void ClosePopup()
    {
        if (closeSound != null)
            audioSource.PlayOneShot(closeSound);

        helpPopup.SetActive(false);
    }
}
