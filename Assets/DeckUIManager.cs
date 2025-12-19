using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform cardParent;
    public GameObject cardPrefab;
    public TextMeshProUGUI resultText;
    public Slider cardCountSlider;
    public TextMeshProUGUI sliderValueText;

    private List<string> suits = new() { "S", "H", "D", "C" };

    void Start()
    {
        cardCountSlider.minValue = 2;
        cardCountSlider.maxValue = 5;
        cardCountSlider.wholeNumbers = true;
        cardCountSlider.onValueChanged.AddListener(OnSliderValueChanged);
        OnSliderValueChanged(cardCountSlider.value);
    }

    void OnSliderValueChanged(float value)
    {
        int count = Mathf.RoundToInt(value);
        sliderValueText.text = $"Card Count: {count}";
        GenerateAndEvaluateDeck(count);
    }

    void GenerateAndEvaluateDeck(int cardCount)
    {
        // 기존 카드 삭제
        foreach (Transform child in cardParent)
            Destroy(child.gameObject);

        // 새 덱 생성
        List<CardData> deck = new();
        for (int i = 0; i < cardCount; i++)
        {
            string s = suits[Random.Range(0, suits.Count)];
            int r = Random.Range(1, 14);
            Sprite sp = CardManager.GetCardSprite(s, r);
            deck.Add(new CardData(s, r, sp));
        }

        // 카드 UI 생성
        List<GameObject> cardObjs = new();
        foreach (CardData card in deck)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardParent);
            Image img = cardObj.GetComponent<Image>();
            if (card.sprite != null)
                img.sprite = card.sprite;
            cardObjs.Add(cardObj);
        }

        // 🔹 완전 정중앙 정렬 계산
        float spacing = 20f; // HorizontalLayoutGroup과 동일해야 함
        float totalWidth = 0f;

        foreach (var cardObj in cardObjs)
        {
            RectTransform rt = cardObj.GetComponent<RectTransform>();
            totalWidth += rt.rect.width;
        }

        totalWidth += spacing * (cardCount - 1);
        // 카드 묶음의 중심이 정확히 가운데 오도록 위치 조정
        cardParent.localPosition = new Vector3(-totalWidth / 2f + cardObjs[0].GetComponent<RectTransform>().rect.width / 4f, cardParent.localPosition.y, 0);

        // 조합 판정
        string rank = DeckEvaluator.EvaluateDeck(deck);
        resultText.text = $"Result: {rank}";
    }
}
