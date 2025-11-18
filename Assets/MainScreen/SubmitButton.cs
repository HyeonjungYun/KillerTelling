using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;    // ← 이거 추가

public class SubmitButton : MonoBehaviour
{
    public Button submitButton;
    public Transform selectedCard3DSpawnPoint;
    public TextMeshProUGUI resultText;

    private void Start()
    {
        UpdateButtonState();
        submitButton.onClick.AddListener(OnSubmit);
    }

    private void Update()
    {
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (selectedCard3DSpawnPoint == null || submitButton == null)
            return;

        submitButton.interactable = selectedCard3DSpawnPoint.childCount >= 2;
    }

    private void OnSubmit()
    {
        int count = selectedCard3DSpawnPoint.childCount;

        if (count < 2)
        {
            Debug.Log("❌ 제출 불가: 최소 2장 필요!");
            return;
        }

        Debug.Log("📤 제출 버튼 클릭");

        // 카드 데이터 저장
        List<CardData> deck = new List<CardData>();
        foreach (Transform t in selectedCard3DSpawnPoint)
        {
            Card3D card3D = t.GetComponent<Card3D>();
            if (card3D != null && card3D.cardData != null)
                deck.Add(card3D.cardData);
        }

        // 덱 평가
        string result = DeckEvaluator.EvaluateDeck(deck);
        Debug.Log("🎯 결과: " + result);

        if (resultText != null)
            resultText.text = "Result : " + result;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
