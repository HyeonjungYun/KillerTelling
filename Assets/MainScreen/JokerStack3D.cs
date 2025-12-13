using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JokerStack3D : MonoBehaviour
{
    public static JokerStack3D Instance;

    private static int globalMaxJokers = -1;

    [Header("Audio Clips")]
    public AudioClip spawnSound;      // 카드 하나 생기는 소리
    public AudioClip pickSound;       // 손으로 가져갈 때
    public AudioClip consumeSound;    // 조커 완전 삭제될 때

    private AudioSource audioSource;

    private void Awake()
    {
        Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
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
    public TextMeshPro jokerCountText;

    private int currentJoker;
    private readonly List<Transform> spawnedJokers = new List<Transform>();


    public void OnStageStart()
    {
        InitStageFromGlobal();   // 스테이지 리셋 로직 수행
    }


    private void InitStageFromGlobal()
    {
        if (globalMaxJokers < 0)
            globalMaxJokers = jokerCount;

        jokerCount = globalMaxJokers;
        currentJoker = jokerCount;

        foreach (var t in spawnedJokers)
            if (t != null) Destroy(t.gameObject);

        spawnedJokers.Clear();
        StopAllCoroutines();

        UpdateJokerText();
        StartCoroutine(SpawnJokerStackAnimated());
    }

    public void UpdateJokerText()
    {
        if (jokerCountText != null)
            jokerCountText.text = $"{currentJoker}/{jokerCount}";
    }

    // 🔥 ① 덱 선택 → 조커 1개 영구 소모
    public void UseOneJoker()
    {
        if (jokerCount <= 0) return;

        // 카드 제거 사운드
        if (consumeSound != null)
            audioSource.PlayOneShot(consumeSound);

        if (JokerDraggable.ActiveJoker != null)
        {
            var active = JokerDraggable.ActiveJoker;
            Notify_JokerPicked(active.transform);
            Destroy(active.gameObject);
            JokerDraggable.ActiveJoker = null;
        }
        else if (spawnedJokers.Count > 0)
        {
            Transform t = spawnedJokers[0];
            spawnedJokers.RemoveAt(0);
            if (t != null) Destroy(t.gameObject);
        }

        currentJoker = Mathf.Max(0, currentJoker - 1);
        jokerCount = Mathf.Max(0, jokerCount - 1);
        globalMaxJokers = jokerCount;

        UpdateJokerText();
    }

    // 🔥 ② 투척 시 앞자리 감소 (영구X)
    public void ReduceCountOnly()
    {
        if (currentJoker <= 0) return;

        currentJoker--;
        UpdateJokerText();
    }

    // 🔥 손으로 픽! 했을 때 → 스택에서 제거
    public void Notify_JokerPicked(Transform tr)
    {
        if (spawnedJokers.Contains(tr))
            spawnedJokers.Remove(tr);

        // 사운드
        if (pickSound != null)
            audioSource.PlayOneShot(pickSound);
    }

    // 🔥 ③ 스택 생성 애니메이션 (+ 사운드)
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

            // 스폰 사운드 🎵
            if (spawnSound != null)
                audioSource.PlayOneShot(spawnSound);

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
            card.AddComponent<BoxCollider>();
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
