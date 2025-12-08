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

    /// <summary>
    /// 과녁에 카드 걸기 (Renew 등에서 호출)
    /// </summary>
    public void PlaceCards(List<Sprite> sprites)
    {
        if (targetArea == null || cardUiPrefab == null)
        {
            Debug.LogError("WallCardPlacer: TargetArea 또는 CardUiPrefab 미지정");
            return;
        }

        // ------------------------------------------------------
        // 1. 기존 과녁 카드 삭제 (배경 DartBoard는 남겨둠)
        // ------------------------------------------------------
        for (int i = targetArea.childCount - 1; i >= 0; i--)
        {
            Transform child = targetArea.GetChild(i);

            // 과녁 배경은 이름으로 필터링해서 삭제하지 않음
            if (child.name.Contains("Back") ||
                child.name.Contains("back") ||
                child.name.Contains("Board") ||
                child.name.Contains("Dart") ||
                child.name.Contains("Background"))
            {
                continue;
            }

            Destroy(child.gameObject);
        }

        // ------------------------------------------------------
        // 2. 새 카드 배치 (최대 5장, 슬롯 각도 고정)
        // ------------------------------------------------------
        int count = Mathf.Min(sprites.Count, slotAngles.Length);

        for (int i = 0; i < count; i++)
        {
            float angRad = slotAngles[i] * Mathf.Deg2Rad;

            Vector2 localPos = new Vector2(
                Mathf.Cos(angRad) * radius,
                Mathf.Sin(angRad) * radius
            );

            // UI 카드 생성
            GameObject obj = Instantiate(cardUiPrefab, targetArea);

            // 스프라이트 지정
            Image img = obj.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = sprites[i];
                img.raycastTarget = true;     // Hover용 이벤트 받도록 보장
            }

            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt != null)
            {
                // 중심 기준 원형 배치 + 오프셋
                rt.anchoredPosition = localPos + new Vector2(offsetX, offsetY);

                // 살짝 랜덤 회전
                float rot = Random.Range(-randomRotRange, randomRotRange);
                rt.localRotation = Quaternion.Euler(0f, 0f, rot);

                // 살짝 랜덤 스케일
                float scale = baseScale + Random.Range(-randomScaleRange, randomScaleRange);
                rt.localScale = new Vector3(scale, scale, 1f);

                // z 살짝 앞으로(배경보다 앞) + 카드끼리 약간씩 차이
                rt.localPosition = new Vector3(
                    rt.localPosition.x,
                    rt.localPosition.y,
                    0.02f + 0.001f * i
                );
            }

            // --------------------------------------------------
            // 3. Hover 컴포넌트 자동 부착 (우측 덱과는 분리된 전용 스크립트)
            // --------------------------------------------------
            if (!obj.TryGetComponent<TargetCardHover>(out _))
            {
                obj.AddComponent<TargetCardHover>();
            }
        }

        Debug.Log($"🎯 WallCardPlacer → 과녁에 카드 {count}장 배치 완료");
    }
}
