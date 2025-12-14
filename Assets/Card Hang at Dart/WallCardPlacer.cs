using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private readonly float[] slotAngles =
    {
        90f,
        18f,
        -54f,
        -126f,
        162f
    };

    public void ClearTargetAreaOnly()
    {
        if (targetArea == null) return;

        for (int i = targetArea.childCount - 1; i >= 0; i--)
        {
            Transform child = targetArea.GetChild(i);

            if (child.name.Contains("Back") ||
                child.name.Contains("back") ||
                child.name.Contains("Board") ||
                child.name.Contains("Dart") ||
                child.name.Contains("Background"))
                continue;

            Destroy(child.gameObject);
        }

        Debug.Log("🧹 [WallCardPlacer] 스테이지 전환용 과녁 카드만 정리 완료");
    }

    public void PlaceCards(List<Sprite> sprites)
    {
        if (targetArea == null || cardUiPrefab == null)
        {
            Debug.LogError("WallCardPlacer: TargetArea 또는 CardUiPrefab 미지정");
            return;
        }

        ClearTargetAreaOnly();

        int count = Mathf.Min(sprites.Count, slotAngles.Length);

        for (int i = 0; i < count; i++)
        {
            float angRad = slotAngles[i] * Mathf.Deg2Rad;

            Vector2 localPos = new Vector2(
                Mathf.Cos(angRad) * radius,
                Mathf.Sin(angRad) * radius
            );

            GameObject obj = Instantiate(cardUiPrefab, targetArea);

            Image img = obj.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = sprites[i];
                img.raycastTarget = true;
            }

            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = localPos + new Vector2(offsetX, offsetY);

                float rot = Random.Range(-randomRotRange, randomRotRange);
                rt.localRotation = Quaternion.Euler(0f, 0f, rot);

                float scale = baseScale + Random.Range(-randomScaleRange, randomScaleRange);
                rt.localScale = new Vector3(scale, scale, 1f);

                rt.localPosition = new Vector3(
                    rt.localPosition.x,
                    rt.localPosition.y,
                    0.02f + 0.001f * i
                );
            }

            if (!obj.TryGetComponent<TargetCardHover>(out _))
                obj.AddComponent<TargetCardHover>();
        }

        Debug.Log($"🎯 [WallCardPlacer] 과녁에 카드 {count}장 배치 완료");
    }
}
