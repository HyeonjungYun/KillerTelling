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
    public float circleRadius = 120f;
    public bool uniformInside = true;

    [Header("Offsets")]
    public float offsetX = -520f;   // 🔥 오른쪽 치우침 해결
    public float offsetY = -10f;

    [Header("Randomization")]
    public float randomRotRange = 10f;
    public float baseScale = 0.65f;
    public float randomScaleRange = 0.05f;

    [Header("Overlap Prevention")]
    public float minDistance = 90f;      // 🔥 카드끼리 최소 거리
    public int maxRetryPerCard = 50;     // 최대 반복

    private List<Vector2> placedPositions = new List<Vector2>();

    public void PlaceCards(List<Sprite> sprites)
    {
        // 1. 기존에 배치된 카드 오브젝트들 싹 지우기 (초기화)
        // targetArea 아래에 있는 모든 자식(이전에 만든 카드들)을 찾아서 파괴합니다.
        foreach (Transform child in targetArea)
        {
            Destroy(child.gameObject);
        }

        // 2. 논리적 좌표 리스트 초기화
        placedPositions.Clear();

        // 3. 새로 배치 시작

        foreach (var spr in sprites)
        {
            Vector2 pos = GenerateNonOverlappingPosition();

            GameObject obj = Instantiate(cardUiPrefab, targetArea);
            Destroy(obj.GetComponent<DeckCard>());

            Image img = obj.GetComponent<Image>();
            img.sprite = spr;

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos + new Vector2(offsetX, offsetY);

            // 🔥 여기 추가
            rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, -0.01f);


            float rot = Random.Range(-randomRotRange, randomRotRange);
            rt.localRotation = Quaternion.Euler(0, 0, rot);

            float scale = baseScale + Random.Range(-randomScaleRange, randomScaleRange);
            rt.localScale = new Vector3(scale, scale, 1);


        }
    }

    private Vector2 GenerateNonOverlappingPosition()
    {
        for (int attempt = 0; attempt < maxRetryPerCard; attempt++)
        {
            Vector2 pos = RandomPointInCircle(circleRadius, uniformInside);

            bool overlap = false;
            foreach (var p in placedPositions)
            {
                if (Vector2.Distance(pos, p) < minDistance)
                {
                    overlap = true;
                    break;
                }
            }

            if (!overlap)
            {
                placedPositions.Add(pos);
                return pos;
            }
        }

        // 실패 시 강제로 하나 넣기
        Vector2 fallback = RandomPointInCircle(circleRadius, uniformInside);
        placedPositions.Add(fallback);
        return fallback;
    }

    private Vector2 RandomPointInCircle(float radius, bool uniform)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);

        float r = uniform
            ? Mathf.Sqrt(Random.Range(0f, 1f)) * radius
            : Random.Range(radius * 0.6f, radius);

        return new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r);
    }
}
