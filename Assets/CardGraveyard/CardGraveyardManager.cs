using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardGraveyardManager : MonoBehaviour
{
    // CardGraveyardManager 상단에 추가
    [SerializeField] private GameObject cardPrefabAsset; // 프로젝트 Prefab 에셋만 넣기

    [Header("Shotgun Unlock State")]
    [SerializeField] private bool shotgunUnlockedEver = false;


    // 기존 cardPrefab 대신 cardPrefabAsset을 실제로 사용
    // 또는 EnsureRefs에서 cardPrefab이 null이면 cardPrefabAsset로 복구


    public static CardGraveyardManager Instance;

    public static bool IsInputBlocked = false;

    [Header("Input Blocker")]
    public GameObject inputBlockerPanel;

    [Header("Graveyard")]
    public Transform graveyardArea;      // 3D면 Transform / UI면 RectTransform
    public GameObject cardPrefab;        // 3D면 Card3D 프리팹 / UI면 Image 프리팹

    [Header("UI")]
    public TMP_Text graveyardCounterText;

    [Header("Obstacles")]
    public ShotgunObstacle shotgunObstacle;
    public MovingTargetObstacle movingTarget;
    public ChainPendulum chainPendulum;

    [Header("Drink")]
    public EnergyDrinkMover energyDrinkMover;

    [Header("Optional - Shotgun Preview Object (Table)")]
    public GameObject shotgunPreviewOnTable;

    [Header("3D Graveyard Layout")]
    public float stackStartX = -1.5f;
    public float stackSpacingX = 1.3f;
    public float cardOffsetY = 0.04f;
    public float cardScale3D = 1.1f;
    public float cardOffsetZPerCard = -0.002f;

    [Header("UI Graveyard Layout (when graveyardArea is RectTransform)")]
    public Vector2 uiStartPos = new Vector2(-180f, 20f);
    public Vector2 uiStackSpacing = new Vector2(120f, 0f);
    public float uiCardOffsetY = -8f;
    public Vector2 uiCardSize = new Vector2(55f, 75f);

    // =========================================================
    // ✅ Debug / Safety
    // =========================================================
    [Header("Debug / Safety")]
    [Tooltip("무덤 카드가 보이지 않을 때 원인 확정용 로그를 출력")]
    public bool debugLogs = true;

    [Tooltip("graveyardArea를 재바인딩할 때 비활성 오브젝트도 허용")]
    public bool allowBindInactiveArea = true;

    [Tooltip("카드 프리팹/자식까지 강제로 특정 레이어로 설정(카메라 CullingMask 이슈 확인용)")]
    public bool forceLayerToDefault = false;

    [Tooltip("forceLayerToDefault가 true일 때 적용할 레이어 이름")]
    public string forcedLayerName = "Default";

    private int forcedLayer = -1;
    // =========================================================

    private bool drinkPlayed = false;

    private readonly List<Sprite> storedCards = new List<Sprite>();
    public List<Sprite> StoredSprites => storedCards;

    // -----------------------------
    // ✅ Refresh 예약(중복 방지)
    // -----------------------------
    private Coroutine refreshRoutine;
    private bool refreshRequested;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        IsInputBlocked = false;
        if (inputBlockerPanel != null) inputBlockerPanel.SetActive(false);

        // forcedLayer 캐시
        forcedLayer = LayerMask.NameToLayer(forcedLayerName);

        EnsureRefs(true);
        ForceEmptyUI();

        // ✅ 시작 시에도 “한 번” 예약 갱신(씬 초기화/활성 토글 끝난 뒤)
        RequestRefresh("Awake");
    }

    private void OnEnable()
    {
        EnsureRefs(false);
        RequestRefresh("OnEnable");
    }

    // -------------------------------------------------
    // ✅ 외부에서 카드 추가
    // -------------------------------------------------
    public void AddCards(List<Sprite> cards)
    {
        EnsureRefs(false);

        if (cards == null || cards.Count == 0)
        {
            if (debugLogs) Debug.LogWarning("⚠ [Graveyard] AddCards: cards가 비어있음");
            return;
        }

        int added = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
            {
                storedCards.Add(cards[i]);
                added++;
            }
        }

        if (debugLogs) Debug.Log($"🪦 [Graveyard] AddCards: +{added}장 (총 {storedCards.Count}장)");

        // ✅ 즉시 그리지 말고 “한 프레임 뒤”에 그리기
        RequestRefresh("AddCards");
    }

    // -------------------------------------------------
    // ✅ 스테이지 전환 후 유지 갱신
    // -------------------------------------------------
    public void OnStageChanged_KeepState()
    {
        EnsureRefs(false);
        RequestRefresh("OnStageChanged_KeepState");
    }

    // -------------------------------------------------
    // ✅ 튜토 -> 실전 리셋
    // -------------------------------------------------
    public void ClearAll()
    {
        if (debugLogs) Debug.Log("🧹 CardGraveyardManager.ClearAll() : 무덤/장애물/입력 상태 초기화");

        storedCards.Clear();

        // 시각화 오브젝트 제거
        if (graveyardArea != null)
        {
            for (int i = graveyardArea.childCount - 1; i >= 0; i--)
                Destroy(graveyardArea.GetChild(i).gameObject);
        }

        drinkPlayed = false;

        IsInputBlocked = false;
        if (inputBlockerPanel != null) inputBlockerPanel.SetActive(false);

        ForceEmptyUI();

        // ✅ 리셋 후도 예약 갱신(혹시 씬 토글/정리 타이밍 이슈 방지)
        RequestRefresh("ClearAll");

        if (shotgunPreviewOnTable != null)
            shotgunPreviewOnTable.SetActive(shotgunUnlockedEver);

    }

    // =========================================================
    // ✅ Refresh 예약 시스템 (핵심)
    // =========================================================
    private void RequestRefresh(string reason)
    {
        refreshRequested = true;

        if (refreshRoutine == null)
        {
            refreshRoutine = StartCoroutine(RefreshEndOfFrame(reason));
        }
    }

    private IEnumerator RefreshEndOfFrame(string reason)
    {
        // 같은 프레임 안에서 여러 번 AddCards/정리 호출돼도
        // “프레임 끝”에 한번만 그리도록
        yield return new WaitForEndOfFrame();

        refreshRoutine = null;

        if (!refreshRequested) yield break;
        refreshRequested = false;

        RebuildGraveyardVisual(reason);
    }

    // =========================================================
    // ✅ 실제 그리기(검증 로그 포함)
    // =========================================================
    private void RebuildGraveyardVisual(string reason)
    {
        // --- 카메라/레이어 원인 확정용 로그 ---
        if (debugLogs)
        {
            Camera cam = Camera.main;
            Debug.Log($"[GY] reason={reason} area={graveyardArea?.name} active={graveyardArea?.gameObject.activeInHierarchy} " +
                      $"pos={graveyardArea?.position} local={graveyardArea?.localPosition} scale={graveyardArea?.lossyScale}");
            Debug.Log($"[GY-CAM] main={(cam ? cam.name : "NULL")} cullMask={(cam ? cam.cullingMask : 0)}");
            Debug.Log($"[GY-LAYER] areaLayer={(graveyardArea ? graveyardArea.gameObject.layer : -1)} prefabLayer={(cardPrefab ? cardPrefab.layer : -1)} " +
                      $"forceLayer={forceLayerToDefault} forcedLayerName={forcedLayerName} forcedLayerId={forcedLayer}");
        }

        EnsureRefs(false);

        // 1) 슈트 그룹핑
        Dictionary<char, List<Sprite>> suitGroups = new Dictionary<char, List<Sprite>>()
        {
            { 'S', new List<Sprite>() },
            { 'H', new List<Sprite>() },
            { 'D', new List<Sprite>() },
            { 'C', new List<Sprite>() },
        };

        foreach (Sprite spr in storedCards)
        {
            if (spr == null) continue;
            char suit = ExtractSuit(spr.name);
            if (!suitGroups.ContainsKey(suit)) suit = 'S';
            suitGroups[suit].Add(spr);
        }

        // 2) 카운터/장애물은 항상 갱신
        UpdateGraveyardCounterText(suitGroups);
        CheckObstacleActivation(suitGroups);

        // 3) 그리기 가능 여부 체크
        if (graveyardArea == null)
        {
            if (debugLogs) Debug.LogWarning($"⚠ [Graveyard] ({reason}) graveyardArea == null → 시각화 스킵");
            return;
        }

        if (cardPrefab == null)
        {
            if (debugLogs) Debug.LogWarning($"⚠ [Graveyard] ({reason}) cardPrefab == null → 시각화 스킵");
            return;
        }

        if (!graveyardArea.gameObject.activeInHierarchy)
        {
            if (debugLogs) Debug.LogWarning($"⚠ [Graveyard] ({reason}) graveyardArea 비활성 → 재탐색 시도");
            TryRebindGraveyardArea();
        }

        if (graveyardArea == null || !graveyardArea.gameObject.activeInHierarchy)
        {
            // allowBindInactiveArea=true면 비활성도 잡을 수 있으니 한번 더 시도
            if (allowBindInactiveArea)
                TryRebindGraveyardArea(allowInactive: true);

            if (graveyardArea == null || !graveyardArea.gameObject.activeInHierarchy)
            {
                if (debugLogs) Debug.LogWarning($"⚠ [Graveyard] ({reason}) 활성 graveyardArea 확보 실패 → 시각화 스킵");
                return;
            }
        }

        // 4) 기존 자식 제거
        int before = graveyardArea.childCount;
        for (int i = graveyardArea.childCount - 1; i >= 0; i--)
            Destroy(graveyardArea.GetChild(i).gameObject);

        // 5) UI/3D 분기
        bool areaIsUI = graveyardArea.GetComponent<RectTransform>() != null;

        int totalToDraw = suitGroups['S'].Count + suitGroups['H'].Count + suitGroups['D'].Count + suitGroups['C'].Count;
        if (debugLogs) Debug.Log($"🎴 [Graveyard] ({reason}) Rebuild 시작: stored={storedCards.Count}, draw={totalToDraw}, child(before)={before}, areaIsUI={areaIsUI}");

        if (totalToDraw <= 0)
        {
            if (debugLogs) Debug.Log($"ℹ [Graveyard] ({reason}) 그릴 카드가 0장 → 종료");
            return;
        }

        if (areaIsUI) DrawUI(suitGroups);
        else Draw3D(suitGroups);

        // 6) 생성 결과 확인
        if (debugLogs) Debug.Log($"✅ [Graveyard] ({reason}) Rebuild 완료: child(after)={graveyardArea.childCount}");
    }

    // -------------------------------------------------
    // ✅ 3D 렌더링
    // -------------------------------------------------
    private void Draw3D(Dictionary<char, List<Sprite>> suitGroups)
    {
        char[] suitOrder = { 'S', 'H', 'D', 'C' };

        for (int s = 0; s < suitOrder.Length; s++)
        {
            char suit = suitOrder[s];
            List<Sprite> list = suitGroups[suit];
            float x = stackStartX + s * stackSpacingX;

            for (int i = 0; i < list.Count; i++)
            {
                Sprite spr = list[i];
                if (spr == null) continue;

                GameObject obj = Instantiate(cardPrefab, graveyardArea, false);
                obj.SetActive(true);

                // ✅ 카메라 CullingMask/Layer 이슈 확인용: 레이어 강제
                if (forceLayerToDefault && forcedLayer >= 0)
                {
                    SetLayerRecursively(obj, forcedLayer);
                }

                // Card3D가 루트/자식 어느 쪽에 있든 잡기
                Card3D card3D = obj.GetComponent<Card3D>();
                if (card3D == null) card3D = obj.GetComponentInChildren<Card3D>(true);

                if (card3D != null)
                {
                    card3D.SetSprite(spr);
                }
                else
                {
                    // MeshRenderer 프리팹이면 텍스처라도 적용
                    var mr = obj.GetComponentInChildren<MeshRenderer>(true);
                    if (mr != null && mr.material != null)
                        mr.material.mainTexture = spr.texture;
                }

                obj.transform.localPosition = new Vector3(x, i * cardOffsetY, i * cardOffsetZPerCard);
                obj.transform.localRotation = Quaternion.Euler(90, 0, 0);
                obj.transform.localScale = Vector3.one * cardScale3D;

                // Renderer 강제 활성
                var r = obj.GetComponentInChildren<Renderer>(true);
                if (r != null) r.enabled = true;

                if (debugLogs)
                {
                    Debug.Log($"[GY-OBJ] {obj.name} layer={obj.layer} worldPos={obj.transform.position} localPos={obj.transform.localPosition}");
                }
            }
        }
    }

    // -------------------------------------------------
    // ✅ UI 렌더링
    // -------------------------------------------------
    private void DrawUI(Dictionary<char, List<Sprite>> suitGroups)
    {
        RectTransform parentRT = graveyardArea.GetComponent<RectTransform>();
        if (parentRT == null)
        {
            if (debugLogs) Debug.LogWarning("⚠ [Graveyard] UI로 그리려 했지만 graveyardArea에 RectTransform이 없습니다.");
            return;
        }

        char[] suitOrder = { 'S', 'H', 'D', 'C' };

        for (int s = 0; s < suitOrder.Length; s++)
        {
            char suit = suitOrder[s];
            List<Sprite> list = suitGroups[suit];

            Vector2 basePos = uiStartPos + uiStackSpacing * s;

            for (int i = 0; i < list.Count; i++)
            {
                Sprite spr = list[i];
                if (spr == null) continue;

                GameObject obj = Instantiate(cardPrefab, graveyardArea, false);
                obj.SetActive(true);

                Image img = obj.GetComponentInChildren<Image>(true);
                if (img != null)
                {
                    img.sprite = spr;
                    img.enabled = true;
                    img.raycastTarget = false;
                }

                RectTransform rt = obj.GetComponent<RectTransform>();
                if (rt == null) rt = obj.GetComponentInChildren<RectTransform>(true);

                if (rt != null)
                {
                    rt.localScale = Vector3.one;
                    rt.sizeDelta = uiCardSize;
                    rt.anchoredPosition = basePos + new Vector2(0f, i * uiCardOffsetY);
                }
            }
        }
    }

    // -------------------------------------------------
    // ✅ 카운터 텍스트
    // -------------------------------------------------
    private void UpdateGraveyardCounterText(Dictionary<char, List<Sprite>> suits)
    {
        EnsureRefs(false);
        if (graveyardCounterText == null) return;

        int spade = suits['S'].Count;
        int heart = suits['H'].Count;
        int diamond = suits['D'].Count;
        int club = suits['C'].Count;

        graveyardCounterText.text = $"♠ {spade}   ♦ {diamond}   ♥ {heart}   ♣ {club}";
        graveyardCounterText.gameObject.SetActive(true);
    }

    // -------------------------------------------------
    // ✅ 장애물 조건
    // -------------------------------------------------
    private void CheckObstacleActivation(Dictionary<char, List<Sprite>> suitGroups)
    {
        int spade = suitGroups['S'].Count;
        int heart = suitGroups['H'].Count;
        int diamond = suitGroups['D'].Count;
        int club = suitGroups['C'].Count;

        // ------------------------------------------------
        // 1️⃣ 소총 해금 판정 (튜토 포함, 단 한 번)
        // ------------------------------------------------
        if (spade >= 3)
            shotgunUnlockedEver = true;

        // ------------------------------------------------
        // 2️⃣ 테이블 미리보기는 "해금 기준"으로만 관리
        // ------------------------------------------------
        if (shotgunPreviewOnTable != null)
        {
            shotgunPreviewOnTable.SetActive(shotgunUnlockedEver);
        }

        // ------------------------------------------------
        // 3️⃣ 실제 장애물 소총 이동 (조건 기반)
        // ------------------------------------------------
        if (shotgunObstacle != null)
        {
            if (!shotgunObstacle.gameObject.activeSelf)
                shotgunObstacle.gameObject.SetActive(true);

            shotgunObstacle.SetActiveState(spade >= 3);
        }

        // ------------------------------------------------
        // 4️⃣ 기타 기존 로직 유지
        // ------------------------------------------------
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

        bool bossCondition = spade >= 4 && diamond >= 3 && heart >= 2 && club >= 2;
        if (movingTarget != null)
            movingTarget.SetActive(bossCondition);

        if (!drinkPlayed && diamond >= 4 && energyDrinkMover != null)
            PlayDrinkSequence();
    }


    private void PlayDrinkSequence()
    {
        drinkPlayed = true;

        IsInputBlocked = true;
        if (inputBlockerPanel != null)
            inputBlockerPanel.SetActive(true);

        energyDrinkMover.PlayDrinkOnce(() =>
        {
            IsInputBlocked = false;
            if (inputBlockerPanel != null)
                inputBlockerPanel.SetActive(false);
        });
    }

    // -------------------------------------------------
    // ✅ 참조 자동 보정
    // -------------------------------------------------
    private void EnsureRefs(bool log)
    {
        if (graveyardCounterText == null)
        {
            var go = GameObject.Find("Graveyard Counter Text");
            if (go != null) graveyardCounterText = go.GetComponent<TMP_Text>();

            if (graveyardCounterText == null)
            {
                TMP_Text[] all = FindObjectsOfType<TMP_Text>(true);
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i] != null && all[i].name.Contains("Graveyard") && all[i].name.Contains("Counter"))
                    {
                        graveyardCounterText = all[i];
                        break;
                    }
                }
            }
        }

        if (shotgunObstacle == null) shotgunObstacle = FindFirstObjectByType<ShotgunObstacle>();
        if (movingTarget == null) movingTarget = FindFirstObjectByType<MovingTargetObstacle>();
        if (chainPendulum == null) chainPendulum = FindFirstObjectByType<ChainPendulum>();
        if (energyDrinkMover == null) energyDrinkMover = FindFirstObjectByType<EnergyDrinkMover>();

        // 인스펙터에 연결된 레퍼런스는 '비활성'이어도 유지해야 안전합니다.
        if (graveyardArea == null)
            TryRebindGraveyardArea(allowInactive: allowBindInactiveArea);

        // forcedLayer 재캐시(인스펙터 변경 대비)
        forcedLayer = LayerMask.NameToLayer(forcedLayerName);

        if (log && debugLogs)
        {
            Debug.Log($"[Graveyard] graveyardArea={(graveyardArea ? graveyardArea.name : "NULL")}, " +
                      $"cardPrefab={(cardPrefab ? cardPrefab.name : "NULL")}, " +
                      $"counterText={(graveyardCounterText ? graveyardCounterText.name : "NULL")}");
        }

        if (cardPrefab == null && cardPrefabAsset != null)
            cardPrefab = cardPrefabAsset;

    }

    private void TryRebindGraveyardArea()
    {
        TryRebindGraveyardArea(allowInactive: allowBindInactiveArea);
    }

    private void TryRebindGraveyardArea(bool allowInactive)
    {
        string[] names = { "GraveyardArea3D", "GraveyardAreaUI", "GraveyardArea", "GraveyardRoot", "Graveyard" };

        for (int i = 0; i < names.Length; i++)
        {
            GameObject go = GameObject.Find(names[i]);
            if (go != null)
            {
                if (allowInactive || go.activeInHierarchy)
                {
                    graveyardArea = go.transform;
                    return;
                }
            }
        }

        Transform[] all = FindObjectsOfType<Transform>(true);
        foreach (var t in all)
        {
            if (t == null) continue;

            if (!allowInactive && !t.gameObject.activeInHierarchy)
                continue;

            if (t.name.Contains("GraveyardArea") || t.name == "GraveyardArea")
            {
                graveyardArea = t;
                return;
            }
        }
    }

    private void ForceEmptyUI()
    {
        var emptySuitGroups = new Dictionary<char, List<Sprite>>()
        {
            { 'S', new List<Sprite>() },
            { 'H', new List<Sprite>() },
            { 'D', new List<Sprite>() },
            { 'C', new List<Sprite>() },
        };

        UpdateGraveyardCounterText(emptySuitGroups);
        CheckObstacleActivation(emptySuitGroups);
    }

    private char ExtractSuit(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName)) return 'S';
        char c = char.ToUpper(spriteName[spriteName.Length - 1]);
        if (c == 'S' || c == 'H' || c == 'D' || c == 'C') return c;
        return 'S';
    }

    private void SetLayerRecursively(GameObject root, int layer)
    {
        if (root == null) return;
        root.layer = layer;

        var trs = root.GetComponentsInChildren<Transform>(true);
        foreach (var t in trs)
        {
            if (t != null) t.gameObject.layer = layer;
        }
    }

    // =========================================================
    // ✅ 디버그용: 에디터에서 우클릭 실행 가능
    // =========================================================
    [ContextMenu("DEBUG: Force Rebuild Now")]
    private void DebugForceRebuildNow()
    {
        RebuildGraveyardVisual("ContextMenu Force");
    }

    [ContextMenu("DEBUG: Toggle ForceLayerToDefault")]
    private void DebugToggleForceLayer()
    {
        forceLayerToDefault = !forceLayerToDefault;
        Debug.Log($"[GY] forceLayerToDefault={forceLayerToDefault} (forcedLayerName={forcedLayerName})");
        RequestRefresh("Toggle ForceLayer");
    }
}
