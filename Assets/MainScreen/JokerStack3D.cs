using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JokerStack3D : MonoBehaviour
{
    public static JokerStack3D Instance;

    // 🔹 전체 게임에서 공유되는 “영구 조커 수”(분모의 기준)
    private static int globalMaxJokers = -1;

    private void Awake()
    {
        Instance = this;
    }

    [Header("Prefab / Sprite")]
    public GameObject card3DPrefab;
    public Sprite jokerSprite;

    [Header("Settings")]
    public int jokerCount = 7;   // 인스펙터 기본값(1스테이지 최대 수)

    [Header("Positions")]
    public Vector3 firstCardPosition;
    public Vector3 cardRotation;
    public Vector3 cardScale;
    public float offsetX = 0.1f;
    public float offsetY = 0.0f;

    [Header("UI")]
    public TextMeshPro jokerCountText;   // 3D 텍스트 (예: 6/6)

    // 🔹 현재 스테이지에서 남은 조커 수(분자)
    private int currentJoker;

    // 🔹 테이블 위에 “아직 사용 안 한” 조커 오브젝트들
    private readonly List<Transform> spawnedJokers = new List<Transform>();

    // ================================================================
    void Start()
    {
        InitStageFromGlobal();
    }

    // 스테이지 시작 시 공통 초기화
    private void InitStageFromGlobal()
    {
        // 최초 진입이면 인스펙터 값으로 전역 초기화
        if (globalMaxJokers < 0)
            globalMaxJokers = jokerCount;

        // 이후엔 전역에서 가져옴
        jokerCount = globalMaxJokers;   // 분모
        currentJoker = jokerCount;        // 분자(처음엔 같음)

        // 이전 스택 정리
        foreach (var t in spawnedJokers)
            if (t != null) Destroy(t.gameObject);
        spawnedJokers.Clear();
        StopAllCoroutines();

        UpdateJokerText();
        StartCoroutine(SpawnJokerStackAnimated());
        Debug.Log($"🃏 [JokerStack3D] 스테이지 시작 → {currentJoker}/{jokerCount}");
    }

    // StageManager에서 호출
    public void OnStageStart()
    {
        InitStageFromGlobal();
    }

    // ================================================================
    public void UpdateJokerText()
    {
        if (jokerCountText != null)
            jokerCountText.text = $"{currentJoker}/{jokerCount}";
    }

    // ================================================================
    // ① 덱에서 카드 선택 → 조커 1개 “영구 소모”
    //    - 던지기 모드라면 손에 들고 있던 조커를 소비
    //    - 아니라면 테이블 스택에서 1장 소비
    //    - 규칙: currentJoker--, jokerCount--, globalMaxJokers--
    // ================================================================
    public void UseOneJoker()
    {
        if (jokerCount <= 0)
        {
            Debug.LogWarning("⚠ 더 이상 소모할 조커가 없습니다! (UseOneJoker)");
            return;
        }

        // 1) 던지기 모드에서 이미 손에 들고 있는 조커가 있다면 → 그걸 삭제
        if (JokerDraggable.ActiveJoker != null)
        {
            var active = JokerDraggable.ActiveJoker;
            Notify_JokerPicked(active.transform);  // 혹시 리스트에 남아있으면 제거
            Destroy(active.gameObject);
            JokerDraggable.ActiveJoker = null;
        }
        else
        {
            // 2) 손에 들고 있는 조커가 없다면 → 테이블 위 스택에서 1장 제거
            if (spawnedJokers.Count > 0)
            {
                Transform t = spawnedJokers[0];
                spawnedJokers.RemoveAt(0);
                if (t != null) Destroy(t.gameObject);
            }
        }

        // 실제 개수 감소
        currentJoker = Mathf.Max(0, currentJoker - 1);  // 분자
        jokerCount = Mathf.Max(0, jokerCount - 1);  // 분모
        globalMaxJokers = jokerCount;

        // 안전하게 정리
        currentJoker = Mathf.Min(currentJoker, jokerCount);

        UpdateJokerText();
        Debug.Log($"🟧 [JokerStack3D] 덱 사용 → 조커 1개 영구 소모 → {currentJoker}/{jokerCount}");
    }

    // ================================================================
    // ② 조커 “던지기” 시 실제 사용
    //    - 영구 수는 그대로, 현재 스테이지 남은 수만 감소
    //    - 규칙: currentJoker-- (앞자리만 감소)
    // ================================================================
    public void ReduceCountOnly()
    {
        if (currentJoker <= 0)
        {
            Debug.LogWarning("⚠ 남은 조커 없음 (ReduceCountOnly)");
            return;
        }

        currentJoker = Mathf.Max(0, currentJoker - 1);
        UpdateJokerText();

        Debug.Log($"🟦 [JokerStack3D] 조커 투척 → 남은 조커 1 감소 → {currentJoker}/{jokerCount}");
    }

    // ================================================================
    // 테이블 스택 관리: 조커 하나가 “클릭되어 손으로 이동”할 때 호출
    // (해당 조커는 이제 스택이 아니라 플레이어 손에 있는 상태)
    // ================================================================
    public void Notify_JokerPicked(Transform tr)
    {
        if (spawnedJokers.Contains(tr))
            spawnedJokers.Remove(tr);
    }

    // ================================================================
    // 스택 생성
    // ================================================================
    private IEnumerator SpawnJokerStackAnimated()
    {
        spawnedJokers.Clear();

        for (int i = 0; i < jokerCount; i++)
        {
            GameObject card = Instantiate(card3DPrefab);

            card.transform.localScale = Vector3.zero;
            card.transform.position = firstCardPosition + new Vector3(offsetX * i, offsetY * i, 0);
            card.transform.rotation = Quaternion.Euler(cardRotation);

            ApplySprite(card);
            EnsureCollider(card);
            EnsureRigidBody(card);
            EnsureDraggable(card);

            spawnedJokers.Add(card.transform);

            yield return StartCoroutine(ScaleUp(card.transform, cardScale, 0.15f));
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void ApplySprite(GameObject card)
    {
        MeshRenderer rend = card.GetComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Unlit/Transparent"));
        mat.mainTexture = jokerSprite.texture;
        rend.material = mat;
    }

    private IEnumerator ScaleUp(Transform target, Vector3 to, float duration)
    {
        float t = 0f;
        Vector3 start = Vector3.zero;

        while (t < duration)
        {
            t += Time.deltaTime;
            target.localScale = Vector3.Lerp(start, to, t / duration);
            yield return null;
        }

        target.localScale = to;
    }

    private void EnsureCollider(GameObject card)
    {
        MeshCollider wrongCol = card.GetComponent<MeshCollider>();
        if (wrongCol != null) Destroy(wrongCol);

        if (!card.GetComponent<BoxCollider>())
        {
            BoxCollider col = card.AddComponent<BoxCollider>();
            col.size = new Vector3(1f, 1.4f, 0.05f);
        }
    }

    private void EnsureRigidBody(GameObject card)
    {
        Rigidbody rb = card.GetComponent<Rigidbody>();
        if (!rb) rb = card.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = true;
    }

    private void EnsureDraggable(GameObject card)
    {
        if (!card.GetComponent<JokerDraggable>())
            card.AddComponent<JokerDraggable>();
    }
}
