using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JokerStack3D : MonoBehaviour
{
    public static JokerStack3D Instance;

    private void Awake()
    {
        Instance = this;
    }

    [Header("Prefab / Sprite")]
    public GameObject card3DPrefab;
    public Sprite jokerSprite;

    [Header("Settings")]
    public int jokerCount = 7;

    [Header("Positions")]
    public Vector3 firstCardPosition;
    public Vector3 cardRotation;
    public Vector3 cardScale;
    public float offsetX = 0.1f;
    public float offsetY = 0.0f;

    [Header("UI")]
    public TextMeshPro jokerCountText;   // 3D 텍스트

    private int currentJoker;
    private List<Transform> spawnedJokers = new List<Transform>();


    // ================================================================
    void Start()
    {
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
    // 덱 선택 → 조커 1개 소모 + 스택 삭제
    // ================================================================
    public void UseOneJoker()
    {
        if (spawnedJokers.Count == 0)
        {
            Debug.LogWarning("⚠ 조커 없음!");
            return;
        }

        Transform first = spawnedJokers[0];
        spawnedJokers.RemoveAt(0);
        Destroy(first.gameObject);

        currentJoker--;
        UpdateJokerText();

        Debug.Log("🟧 덱 선택 → 조커 1개 소모");
    }

    // ================================================================
    // 조커 클릭(픽업) → 카운트만 감소
    // ================================================================
    public void ReduceCountOnly()
    {
        currentJoker--;
        UpdateJokerText();

        Debug.Log("🟦 조커 클릭 → 카운트만 감소 (스택 유지)");
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
