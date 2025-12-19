using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JokerStack3D : MonoBehaviour
{
    public static JokerStack3D Instance;

    private void Awake()
    {
        Instance = this;
    }

    // -------------------------------------------------------------
    // Inspector 설정값
    // -------------------------------------------------------------
    [Header("Prefab / Sprite")]
    public GameObject card3DPrefab;    // 조커 카드 3D 프리팹 (Quad 기반)
    public Sprite jokerSprite;         // 조커 이미지

    [Header("Settings")]
    public int jokerCount = 7;

    [Header("Positions")]
    public Vector3 firstCardPosition;
    public Vector3 cardRotation;
    public Vector3 cardScale;
    public float offsetX = 0.1f;   // 카드 겹치기 (X)
    public float offsetY = 0.0f;   // 카드 겹치기 (Y)

    [Header("UI")]
    public Text jokerCountText;    // "7/7" 표시용 Text UI

    private int currentJoker;
    private List<Transform> spawnedJokers = new List<Transform>();


    // -------------------------------------------------------------
    // Start
    // -------------------------------------------------------------
    public void InitJokers()
    {
        currentJoker = jokerCount;
        UpdateJokerText();
        StartCoroutine(SpawnJokerStackAnimated());
    }


    // -------------------------------------------------------------
    // 조커 7장 쫘르륵 생성 애니메이션
    // -------------------------------------------------------------
    IEnumerator SpawnJokerStackAnimated()
    {
        spawnedJokers.Clear();

        for (int i = 0; i < jokerCount; i++)
        {
            GameObject card = Instantiate(card3DPrefab);

            // 초기 상태
            card.transform.localScale = Vector3.zero;
            card.transform.position = firstCardPosition + new Vector3(offsetX * i, offsetY * i, 0);
            card.transform.rotation = Quaternion.Euler(cardRotation);

            // 스프라이트 적용
            ApplySprite(card);

            // Collider 보정 (MeshCollider 절대 금지)
            EnsureCollider(card);

            // Rigidbody 보정
            EnsureRigidBody(card);

            // 자동 드래그 기능 부착
            EnsureDraggable(card);

            spawnedJokers.Add(card.transform);

            // 등장 애니메이션
            yield return StartCoroutine(ScaleUp(card.transform, cardScale, 0.15f));

            // 다음 카드 간 텀
            yield return new WaitForSeconds(0.05f);
        }
    }


    // -------------------------------------------------------------
    // 스프라이트 적용 (Unlit/Transparent)
    // -------------------------------------------------------------
    void ApplySprite(GameObject card)
    {
        MeshRenderer rend = card.GetComponent<MeshRenderer>();

        Material mat = new Material(Shader.Find("Unlit/Transparent"));
        mat.mainTexture = jokerSprite.texture;

        rend.material = mat;
    }


    // -------------------------------------------------------------
    // 등장 애니메이션
    // -------------------------------------------------------------
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


    // -------------------------------------------------------------
    // Collider 자동 보정
    // -------------------------------------------------------------
    void EnsureCollider(GameObject card)
    {
        MeshCollider wrongCol = card.GetComponent<MeshCollider>();
        if (wrongCol != null)
        {
            Destroy(wrongCol);
        }

        if (card.GetComponent<BoxCollider>() == null)
        {
            BoxCollider col = card.AddComponent<BoxCollider>();
            col.size = new Vector3(1f, 1.4f, 0.05f);
            col.center = Vector3.zero;
        }
    }


    // -------------------------------------------------------------
    // Rigidbody 자동 보정
    // -------------------------------------------------------------
    void EnsureRigidBody(GameObject card)
    {
        Rigidbody rb = card.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = card.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;     // 드래그 중 낙하 방지
        rb.isKinematic = true;     // 드래그 중 물리 영향 X
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.mass = 0.1f;
        rb.angularDamping = 0.05f;
    }


    // -------------------------------------------------------------
    // 드래그 기능 자동 부착
    // -------------------------------------------------------------
    void EnsureDraggable(GameObject card)
    {
        if (card.GetComponent<JokerDraggable>() == null)
        {
            card.AddComponent<JokerDraggable>();
        }
    }


    // -------------------------------------------------------------
    // UI 업데이트
    // -------------------------------------------------------------
    void UpdateJokerText()
    {
        if (jokerCountText != null)
            jokerCountText.text = $"{currentJoker}/{jokerCount}";
    }


    // -------------------------------------------------------------
    // 조커 소모 (HandManager에서 호출)
    // -------------------------------------------------------------
    public void UseOneJoker()
    {
        if (spawnedJokers.Count == 0)
        {
            Debug.LogWarning("⚠ 더 이상 사용할 조커가 없습니다!");
            return;
        }

        Transform first = spawnedJokers[0];
        spawnedJokers.RemoveAt(0);

        Destroy(first.gameObject);

        currentJoker--;
        UpdateJokerText();

        Debug.Log("🃏 조커 1개 소모!");
    }
}
