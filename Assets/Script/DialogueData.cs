using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue")]
public class DialogueData : ScriptableObject
{
    public string speakerName;
    [TextArea(3, 10)]
    public string[] sentences; // 대사 내용들
}