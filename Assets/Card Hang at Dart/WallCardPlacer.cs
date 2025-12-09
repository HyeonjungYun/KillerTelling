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
    /// 스테이지 전환 시 사용.
    /// - 과녁에 붙어 있던 카드 UI만 삭제
    /// - DartBoard / Background 이미지는 그대로 유지
    /// - 카드 무덤으로는 보내지 않음
    /// </summary>
    public void ClearTargetAreaOnly()
    {
        if (targetArea == null) return;

        for (int i = targetArea.childCount - 1; i >= 0; i--)
        {
            Transform child = targetArea.GetChild(i);

            // 과녁 배경은 유지
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

    /// <summary>
    /// 과녁에 카드 걸기 (Renew, 스테이지 초기 세팅 등에서 호출)
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
        //    (Renew에서 부를 때는 이미 MoveOldCardsToGraveyard가 먼저 실행됨)
        // ------------------------------------------------------
        ClearTargetAreaOnly();

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

            GameObject obj = Instantiate(cardUiPrefab, targetArea);

            // 스프라이트 지정
            Image img = obj.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = sprites[i];
                img.raycastTarget = true;     // Hover, 클릭 등 이벤트 받기
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

            // Hover 컴포넌트 자동 부착
            if (!obj.TryGetComponent<TargetCardHover>(out _))
            {
                obj.AddComponent<TargetCardHover>();
            }
        }

        Debug.Log($"🎯 [WallCardPlacer] 과녁에 카드 {count}장 배치 완료");
    }
}
