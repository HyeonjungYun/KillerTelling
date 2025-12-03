using UnityEngine;

public class ShotgunObstacle : MonoBehaviour
{
    public Transform model;              // Mesh가 붙은 모델
    public Transform idlePosition;       // 테이블 위 원래 자리
    public Transform obstaclePosition;   // 방해물 자리 (들린 상태)
    public float moveSpeed = 4f;

    private bool isActive = false;

    public void SetActiveState(bool active)
    {
        if (isActive == active) return;

        isActive = active;

        StopAllCoroutines();
        StartCoroutine(active ? MoveUp() : MoveDown());
    }

    private System.Collections.IEnumerator MoveUp()
    {
        while (Vector3.Distance(model.position, obstaclePosition.position) > 0.01f)
        {
            model.position = Vector3.Lerp(model.position, obstaclePosition.position, Time.deltaTime * moveSpeed);
            model.rotation = Quaternion.Slerp(model.rotation, obstaclePosition.rotation, Time.deltaTime * moveSpeed);
            yield return null;
        }
    }

    private System.Collections.IEnumerator MoveDown()
    {
        while (Vector3.Distance(model.position, idlePosition.position) > 0.01f)
        {
            model.position = Vector3.Lerp(model.position, idlePosition.position, Time.deltaTime * moveSpeed);
            model.rotation = Quaternion.Slerp(model.rotation, idlePosition.rotation, Time.deltaTime * moveSpeed);
            yield return null;
        }
    }
}
