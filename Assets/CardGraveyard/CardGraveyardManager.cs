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
        // 기존 카드 삭제
        foreach (Transform child in graveyardArea)
            Destroy(child.gameObject);

        // 무늬별 분류
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

        // 카드 무덤 시각화
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

        // 장애물 체크
        CheckObstacleActivation(suitGroups);

        // UI 카운터 업데이트
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

        // ---------------------------------------------------------
        // 기존 막대 장애물 (heart >= 3)
        // ---------------------------------------------------------
        GameObject obstacleRoot = GameObject.Find("ObstacleMover");
        if (obstacleRoot != null)
        {
            Transform mesh = obstacleRoot.transform.Find("ObstacleMesh");
            if (mesh != null)
                mesh.gameObject.SetActive(heart >= 3);
        }

        // ---------------------------------------------------------
        // 💖 새로운 장애물 : 체인 펜듈럼 (heart >= 3)
        // ---------------------------------------------------------
        // ---------------------------------------------------------
        // ♥ 3장 이상 → Chain 등장 + 위치 변경 + 진자 활성화
        // ---------------------------------------------------------
        if (chainPendulum != null)
        {
            if (heart >= 3)
            {
                // 위치 변경
                chainPendulum.transform.localPosition = new Vector3(0f, 6f, 0.3f);
                chainPendulum.transform.localEulerAngles = new Vector3(0f, 0f, -19.394f);
                chainPendulum.transform.localScale = new Vector3(2f, 1f, 2f);

                // 활성화
                chainPendulum.SetActive(true);
            }
            else
            {
                // 초기 상태 복귀
                chainPendulum.transform.localPosition = new Vector3(2f, 1f, -3f);
                chainPendulum.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
                chainPendulum.transform.localScale = new Vector3(2f, 0.6f, 2f);

                chainPendulum.SetActive(false);
            }
        }


        // ---------------------------------------------------------
        // 신규 총 장애물 (spade >= 3)
        // ---------------------------------------------------------
        if (shotgunObstacle != null)
            shotgunObstacle.SetActiveState(spade >= 3);

        // ---------------------------------------------------------
        // ⭐ 보스 장애물 : 회전 과녁판
        // 조건: ♠4 + ♦3 + ♥2 + ♣2
        // ---------------------------------------------------------
        bool bossCondition =
            spade >= 4 &&
            diamond >= 3 &&
            heart >= 2 &&
            club >= 2;

        if (movingTarget != null)
            movingTarget.active = bossCondition;
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
}
