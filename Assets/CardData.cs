using UnityEngine;

public class CardData
{
    public string suit;
    public int rank;
    public Sprite sprite;

    public CardData(string suit, int rank, Sprite sprite)
    {
        this.suit = suit;
        this.rank = rank;
        this.sprite = sprite;
    }
}
