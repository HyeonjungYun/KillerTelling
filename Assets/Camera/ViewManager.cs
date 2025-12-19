using UnityEngine;
using System.Collections;

public class ViewManager : MonoBehaviour
{
    public static ViewManager instance;

    [Header("Anchors")]
    public Transform gameAnchor;      // 앞쪽 (게임 뷰)
    public Transform dialogueAnchor;  // 뒤쪽 (대화 뷰)

    [Header("Settings")]
    public float rotateDuration = 1.0f; // 회전하는 데 걸리는 시간
    private Camera mainCamera;

    private void Awake()
    {
        if (instance == null) instance = this;
        mainCamera = Camera.main;
    }

    // 1. 대화 모드로 전환 (뒤로 돌기)
    public void SwitchToDialogueMode()
    {
        StopAllCoroutines();
        StartCoroutine(RotateRoutine(dialogueAnchor));
    }

    // 2. 게임 모드로 복귀 (앞으로 돌기)
    public void SwitchToGameMode()
    {
        StopAllCoroutines();
        StartCoroutine(RotateRoutine(gameAnchor));
    }

    // 부드럽게 회전시키는 코루틴
    IEnumerator RotateRoutine(Transform target)
    {
        Quaternion startRot = mainCamera.transform.rotation;
        Quaternion endRot = target.rotation;

        float timer = 0f;
        while (timer < rotateDuration)
        {
            timer += Time.deltaTime;
            float t = timer / rotateDuration;

            // 부드러운 움직임 (Ease-In-Out)
            t = t * t * (3f - 2f * t);

            mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        // 확실하게 각도 고정
        mainCamera.transform.rotation = endRot;
    }
}