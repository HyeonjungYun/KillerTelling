using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraveyardTester : MonoBehaviour
{
    public Button testButton;
    public CardGraveyardManager graveyardManager;
    public TextMeshPro suitCountText;   // ← 여기 수정!

    void Start()
    {
        testButton.onClick.AddListener(OnClickGenerateCards);
    }

    void OnClickGenerateCards()
    {
        graveyardManager.AddCardsToGraveyard();

        var counts = graveyardManager.GetSuitCounts();
        suitCountText.text = $"♠ {counts["S"]}   ♥ {counts["H"]}   ♦ {counts["D"]}   ♣ {counts["C"]}";
    }
}
