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

            // ⭐ Ace(1)을 14로 변환 — Evaluator와 동일 체계 유지
            int convertedRank = (r == 1) ? 14 : r;

            Sprite sp = CardManager.GetCardSprite(s, r);
            deck.Add(new CardData(s, convertedRank, sp));
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

        // 🔹 가로 정중앙
        float spacing = 20f;
        float totalWidth = 0f;

        foreach (var item in cardObjs)
        {
            RectTransform rt = item.GetComponent<RectTransform>();
            totalWidth += rt.rect.width;
        }

        totalWidth += spacing * (cardCount - 1);

        cardParent.localPosition = new Vector3(
            -totalWidth / 2f + cardObjs[0].GetComponent<RectTransform>().rect.width / 4f,
            cardParent.localPosition.y,
            0
        );

        // 조합 판정
        string rank = DeckEvaluator.EvaluateDeck(deck);
        resultText.text = $"Result: {rank}";
    }
}
