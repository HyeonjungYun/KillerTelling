using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardGraveyardManager : MonoBehaviour
{
    public static CardGraveyardManager Instance;

    [Header("Graveyard")]
    public Transform graveyardArea;
    public GameObject cardPrefab;

    [Header("UI")]
    public TextMeshPro graveyardCounterText;

    [Header("Obstacles")]
    public ShotgunObstacle shotgunObstacle;        // 스페이드 3장 이상
    public MovingTargetObstacle movingTarget;      // 보스 과녁판
    public ChainPendulum chainPendulum;            // ♥ 3장 이상 → 체인 진자 장애물

    [Header("Drink")]
    public EnergyDrinkMover energyDrinkMover;      // ♦ 4장 이상 → 음료캔 연출

    private bool drinkPlayed = false;

    private List<Sprite> storedCards = new List<Sprite>();
    public List<Sprite> StoredSprites => storedCards;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ===========================================================
    public void AddCards(List<Sprite> cards)
    {
        storedCards.AddRange(cards);
        UpdateGraveyardUI();
    }

    // ===========================================================
    private void UpdateGraveyardUI()
    {
        foreach (Transform child in graveyardArea)
            Destroy(child.gameObject);

        Dictionary<char, List<Sprite>> suitGroups = new Dictionary<char, List<Sprite>>()
        {
            { 'S', new List<Sprite>() },
            { 'H', new List<Sprite>() },
            { 'D', new List<Sprite>() },
            { 'C', new List<Sprite>() },
        };

        foreach (Sprite spr in storedCards)
        {
            char suit = ExtractSuit(spr.name);
            suitGroups[suit].Add(spr);
        }

        float stackStartX = -1.5f;
        float stackSpacingX = 1.3f;
        float cardOffsetY = 0.04f;
        float cardScale = 1.1f;

        char[] suitOrder = { 'S', 'H', 'D', 'C' };

        for (int s = 0; s < suitOrder.Length; s++)
        {
            char suit = suitOrder[s];
            List<Sprite> list = suitGroups[suit];

            float stackX = stackStartX + s * stackSpacingX;

            for (int i = 0; i < list.Count; i++)
            {
                Sprite spr = list[i];
                GameObject obj = Instantiate(cardPrefab, graveyardArea);

                Card3D card3D = obj.GetComponent<Card3D>();
                card3D.SetSprite(spr);

                obj.transform.localPosition = new Vector3(
                    stackX,
                    i * cardOffsetY,
                    0);

                obj.transform.localRotation = Quaternion.Euler(90, 0, 0);
                obj.transform.localScale = Vector3.one * cardScale;
            }
        }

        CheckObstacleActivation(suitGroups);
        UpdateGraveyardCounterText(suitGroups);
    }

    // ===========================================================
    private void UpdateGraveyardCounterText(Dictionary<char, List<Sprite>> suits)
    {
        if (graveyardCounterText == null) return;

        int spade = suits['S'].Count;
        int heart = suits['H'].Count;
        int diamond = suits['D'].Count;
        int club = suits['C'].Count;

        graveyardCounterText.text =
            $"♠ {spade}   ♦ {diamond}   ♥ {heart}   ♣ {club}";
    }

    // ===========================================================
    private void CheckObstacleActivation(Dictionary<char, List<Sprite>> suitGroups)
    {
        int spade = suitGroups['S'].Count;
        int heart = suitGroups['H'].Count;
        int diamond = suitGroups['D'].Count;
        int club = suitGroups['C'].Count;

        GameObject obstacleRoot = GameObject.Find("ObstacleMover");
        if (obstacleRoot != null)
        {
            Transform mesh = obstacleRoot.transform.Find("ObstacleMesh");
            if (mesh != null)
                mesh.gameObject.SetActive(heart >= 3);
        }

        if (chainPendulum != null)
        {
            if (heart >= 3)
            {
                chainPendulum.transform.localPosition = new Vector3(0f, 6f, 0.3f);
                chainPendulum.transform.localEulerAngles = new Vector3(0f, 0f, -19.394f);
                chainPendulum.transform.localScale = new Vector3(2f, 1f, 2f);
                chainPendulum.SetActive(true);
            }
            else
            {
                chainPendulum.transform.localPosition = new Vector3(2f, 1f, -3f);
                chainPendulum.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
                chainPendulum.transform.localScale = new Vector3(2f, 0.6f, 2f);
                chainPendulum.SetActive(false);
            }
        }

        if (shotgunObstacle != null)
            shotgunObstacle.SetActiveState(spade >= 3);

        bool bossCondition =
            spade >= 4 &&
            diamond >= 3 &&
            heart >= 2 &&
            club >= 2;

        if (movingTarget != null)
            movingTarget.active = bossCondition;

        // 음료캔 연출 : 다이아몬드 4장 이상
        if (!drinkPlayed && diamond >= 4 && energyDrinkMover != null)
        {
            drinkPlayed = true;
            energyDrinkMover.PlayDrinkOnce();
        }
    }

    // ===========================================================
    private char ExtractSuit(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName)) return 'S';

        char c = char.ToUpper(spriteName[spriteName.Length - 1]);

        if (c == 'S' || c == 'H' || c == 'D' || c == 'C')
            return c;

        Debug.LogWarning("Unknown suit in sprite: " + spriteName);
        return 'S';
    }

    // ============================================================
    // 🔥 스테이지 전환 시 → 무덤은 그대로, UI/장애물 상태만 다시 계산하고 싶을 때
    // ============================================================
    public void OnStageChanged_KeepState()
    {
        UpdateGraveyardUI();
        Debug.Log("♻ [CardGraveyardManager] 스테이지 변경 → 무덤 상태 유지 + UI/장애물 재적용");
    }
}
