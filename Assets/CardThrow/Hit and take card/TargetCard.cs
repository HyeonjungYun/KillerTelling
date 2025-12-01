using UnityEngine;

public class TargetCard : MonoBehaviour
{
    public string cardID;  // 예: "D7", "H10", "S3" 등

    // 이미 플레이어가 가져간 경우 중복 방지를 위한 flag
    public bool collected = false;
}
