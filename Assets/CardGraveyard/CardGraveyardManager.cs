using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardGraveyardManager : MonoBehaviour
{
    public static CardGraveyardManager Instance;

    // 🔥 코드상 입력 차단 (키보드, 마우스 이동 등)
    public static bool IsInputBlocked = false;

    // 🔥 [중요] 유니티 에디터에서 만든 '투명 패널'을 여기에 연결하세요!
    [Header("Input Blocker")]
    public GameObject inputBlockerPanel;

    [Header("Graveyard")]
    public Transform graveyardArea;
    public GameObject cardPrefab;

    [Header("UI")]
    public TextMeshPro graveyardCounterText;

    [Header("Obstacles")]
    public ShotgunObstacle shotgunObstacle;
    public MovingTargetObstacle movingTarget;
    public ChainPendulum chainPendulum;

    [Header("Drink")]
    public EnergyDrinkMover energyDrinkMover;

    private bool drinkPlayed = false;

    private List<Sprite> storedCards = new List<Sprite>();
    public List<Sprite> StoredSprites => storedCards;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 게임 시작 시 입력 차단 해제 및 패널 끄기
        IsInputBlocked = false;
        if (inputBlockerPanel != null)
            inputBlockerPanel.SetActive(false);
    }

    public void AddCards(List<Sprite> cards)
    {
        storedCards.AddRange(cards);
        UpdateGraveyardUI();
    }

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

        // 카드 배치 (기존 로직)
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

                obj.transform.localPosition = new Vector3(stackX, i * cardOffsetY, 0);
                obj.transform.localRotation = Quaternion.Euler(90, 0, 0);
                obj.transform.localScale = Vector3.one * cardScale;
            }
        }

        CheckObstacleActivation(suitGroups);
        UpdateGraveyardCounterText(suitGroups);
    }

    private void UpdateGraveyardCounterText(Dictionary<char, List<Sprite>> suits)
    {
        if (graveyardCounterText == null) return;
        int spade = suits['S'].Count;
        int heart = suits['H'].Count;
        int diamond = suits['D'].Count;
        int club = suits['C'].Count;

        graveyardCounterText.text = $"♠ {spade}   ♦ {diamond}   ♥ {heart}   ♣ {club}";
    }

    private void CheckObstacleActivation(Dictionary<char, List<Sprite>> suitGroups)
    {
        int spade = suitGroups['S'].Count;
        int heart = suitGroups['H'].Count;
        int diamond = suitGroups['D'].Count;
        int club = suitGroups['C'].Count;

        // 1. 하트: 체인
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

        // 1-1. 장애물 메쉬 활성화 여부
        GameObject obstacleRoot = GameObject.Find("ObstacleMover");
        if (obstacleRoot != null)
        {
            Transform mesh = obstacleRoot.transform.Find("ObstacleMesh");
            if (mesh != null) mesh.gameObject.SetActive(heart >= 3);
        }

        // 2. 스페이드: 샷건
        if (shotgunObstacle != null)
            shotgunObstacle.SetActiveState(spade >= 3);

        // 3. 복합: 보스 과녁
        bool bossCondition = spade >= 4 && diamond >= 3 && heart >= 2 && club >= 2;
        if (movingTarget != null)
            movingTarget.SetActive(bossCondition);


        // 4. 🔥 다이아: 에너지 드링크 (입력 차단 + 연출)
        if (!drinkPlayed && diamond >= 4 && energyDrinkMover != null)
        {
            PlayDrinkSequence();
        }
    }

    // ===========================================================
    // 🔥 연출 및 입력 차단 핵심 로직
    // ===========================================================
    private void PlayDrinkSequence()
    {
        drinkPlayed = true;

        // 1. 코드상 차단
        IsInputBlocked = true;

        // 2. 물리적 차단 (투명 패널 켜기) -> UI 클릭 방지
        if (inputBlockerPanel != null)
            inputBlockerPanel.SetActive(true);

        Debug.Log("🚫 [System] 다이아 4장 달성! 연출 시작 (전체 입력 차단)");

        // 3. 연출 실행 (끝나면 실행할 행동 전달)
        energyDrinkMover.PlayDrinkOnce(() =>
        {
            // ✅ 연출 종료 시 실행되는 부분
            IsInputBlocked = false;

            if (inputBlockerPanel != null)
                inputBlockerPanel.SetActive(false);

            Debug.Log("✅ [System] 연출 종료 (입력 차단 해제)");
        });
    }

    private char ExtractSuit(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName)) return 'S';
        char c = char.ToUpper(spriteName[spriteName.Length - 1]);
        if (c == 'S' || c == 'H' || c == 'D' || c == 'C') return c;
        return 'S';
    }

    public void OnStageChanged_KeepState()
    {
        UpdateGraveyardUI();
    }
}