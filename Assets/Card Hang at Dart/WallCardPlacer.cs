using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WallCardPlacer : MonoBehaviour
{
    [Header("Target Area")]
    public RectTransform targetArea;

    [Header("Card Prefab")]
    public GameObject cardUiPrefab;

    [Header("Circle Settings")]
    public float radius = 110f;

    [Header("Offsets")]
    public float offsetX = 0f;
    public float offsetY = 0f;

    [Header("Randomization")]
    public float randomRotRange = 10f;
    public float randomScaleRange = 0.05f;
    public float baseScale = 0.65f;

    // 절대 겹치지 않는 고정 배치 슬롯 (각도 5개)
    private readonly float[] slotAngles =
    {
        90f,    // 12시 방향
        18f,    // 2시 방향
        -54f,   // 5시 방향
        -126f,  // 7시 방향
        162f    // 10시 방향
    };

    public void PlaceCards(List<Sprite> sprites)
    {
        if (targetArea == null || cardUiPrefab == null)
        {
            Debug.LogError("WallCardPlacer: 설정 오류");
            return;
        }

        // -------------------------------
        // 🔥 기존 과녁 카드 삭제되되 과녁 배경은 삭제 금지
        // -------------------------------
        for (int i = targetArea.childCount - 1; i >= 0; i--)
        {
            Transform child = targetArea.GetChild(i);

            // 💥 과녁 배경 제거 방지
            if (child.name.Contains("Back") ||
                child.name.Contains("back") ||
                child.name.Contains("Board") ||
                child.name.Contains("Dart") ||
                child.name.Contains("Background"))
                continue;

            Destroy(child.gameObject);
        }

        // -------------------------------
        // 🔥 5개 슬롯 고정 배치
        // -------------------------------
        int count = Mathf.Min(sprites.Count, slotAngles.Length);

        for (int i = 0; i < count; i++)
        {
            float ang = slotAngles[i] * Mathf.Deg2Rad;

            Vector2 pos = new Vector2(
                Mathf.Cos(ang) * radius,
                Mathf.Sin(ang) * radius
            );

            GameObject obj = Instantiate(cardUiPrefab, targetArea);
            Image img = obj.GetComponent<Image>();
            img.sprite = sprites[i];

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos + new Vector2(offsetX, offsetY);

            float rot = Random.Range(-randomRotRange, randomRotRange);
            rt.localRotation = Quaternion.Euler(0, 0, rot);

            float scale = baseScale + Random.Range(-randomScaleRange, randomScaleRange);
            rt.localScale = new Vector3(scale, scale, 1);

            rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, -0.01f);
        }
    }
}
