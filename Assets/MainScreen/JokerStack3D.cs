using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JokerStack3D : MonoBehaviour
{
    public static JokerStack3D Instance;

    // ⭐ 전체 게임에서 공유되는 “최대 조커 수”
    //   - 첫 스테이지에서 한 번 초기화
    //   - 덱에서 조커를 사용하면 감소, 다음 스테이지에도 이어짐
    private static int globalMaxJokers = -1;

    private void Awake()
    {
        Instance = this;
    }

    [Header("Prefab / Sprite")]
    public GameObject card3DPrefab;
    public Sprite jokerSprite;

    [Header("Settings")]
    public int jokerCount = 7;   // 인스펙터에 적어두는 “기본 최대 조커 수”

    [Header("Positions")]
    public Vector3 firstCardPosition;
    public Vector3 cardRotation;
    public Vector3 cardScale;
    public float offsetX = 0.1f;
    public float offsetY = 0.0f;

    [Header("UI")]
    public TextMeshPro jokerCountText;   // 3D 텍스트

    private int currentJoker;            // 현재 남은 조커(분자)
    private List<Transform> spawnedJokers = new List<Transform>();

    // ================================================================
    void Start()
    {
        // ⭐ 첫 진입이면 인스펙터 값으로 초기화, 그 이후 씬에서는 이전 값 유지
        if (globalMaxJokers < 0)
        {
            globalMaxJokers = jokerCount;
        }
        else
        {
            jokerCount = globalMaxJokers;
        }

        currentJoker = jokerCount;

        UpdateJokerText();
        StartCoroutine(SpawnJokerStackAnimated());
    }

    // ================================================================
    public void UpdateJokerText()
    {
        if (jokerCountText != null)
            jokerCountText.text = $"{currentJoker}/{jokerCount}";
    }

    // ================================================================
    // 덱 선택 → 조커 1개 "영구 소모" + 스택에서 제거
    // ================================================================
    public void UseOneJoker()
    {
        if (spawnedJokers.Count == 0 || jokerCount <= 0)
        {
            Debug.LogWarning("⚠ 더 이상 소모할 조커가 없습니다!");
            return;
        }

        // 실제 스택 제일 위(또는 앞) 카드 제거
        Transform first = spawnedJokers[0];
        spawnedJokers.RemoveAt(0);
        Destroy(first.gameObject);

        // 남은 조커(분자) 감소
        currentJoker = Mathf.Max(0, currentJoker - 1);

        // 전체 최대 조커 수(분모)도 감소 → 영구 소모
        jokerCount = Mathf.Max(0, jokerCount - 1);
        globalMaxJokers = jokerCount;  // 다음 스테이지에서도 유지

        // 혹시라도 분자가 분모보다 커지는 상황 방지
        currentJoker = Mathf.Min(currentJoker, jokerCount);

        UpdateJokerText();

        Debug.Log("🟧 덱 선택 → 조커 1개 영구 소모 (전역 최대 조커 수 감소)");
    }

    // ================================================================
    // 조커 클릭(픽업) → 이번 스테이지에서만 1개 소모 (영구 소모 X)
    // ================================================================
    public void ReduceCountOnly()
    {
        if (currentJoker <= 0)
        {
            Debug.LogWarning("⚠ 남은 조커 없음 (ReduceCountOnly)");
            return;
        }

        currentJoker--;
        UpdateJokerText();

        Debug.Log("🟦 조커 클릭 → 카운트만 감소 (스택 유지, 전역 최대는 그대로)");
    }

    // ================================================================
    public void Notify_JokerPicked(Transform tr)
    {
        if (spawnedJokers.Contains(tr))
            spawnedJokers.Remove(tr);
    }

    // ================================================================
    IEnumerator SpawnJokerStackAnimated()
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

    void ApplySprite(GameObject card)
    {
        MeshRenderer rend = card.GetComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Unlit/Transparent"));
        mat.mainTexture = jokerSprite.texture;
        rend.material = mat;
    }

    IEnumerator ScaleUp(Transform target, Vector3 to, float duration)
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

    void EnsureCollider(GameObject card)
    {
        MeshCollider wrongCol = card.GetComponent<MeshCollider>();
        if (wrongCol != null) Destroy(wrongCol);

        if (!card.GetComponent<BoxCollider>())
        {
            BoxCollider col = card.AddComponent<BoxCollider>();
            col.size = new Vector3(1f, 1.4f, 0.05f);
        }
    }

    void EnsureRigidBody(GameObject card)
    {
        Rigidbody rb = card.GetComponent<Rigidbody>();
        if (!rb) rb = card.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void EnsureDraggable(GameObject card)
    {
        if (!card.GetComponent<JokerDraggable>())
            card.AddComponent<JokerDraggable>();
    }
}
