using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [TextArea(3, 10)]
    public string content;     // 🔥 최종적으로 화면에 해독될 텍스트 (예: "도와주세요")

    [Tooltip("비워두면 위 content를 그대로 사용합니다. 입력하면 이 텍스트로 모스 부호를 만듭니다.")]
    public string morseOverride; // 🔥 모스 부호 생성용 텍스트 (예: "SOS")
}

[System.Serializable]
public class StageDialogueProfile
{
    public int stageIndex;
    public List<DialogueLine> preStageDialogue;
    public List<DialogueLine> postStageDialogue;
}