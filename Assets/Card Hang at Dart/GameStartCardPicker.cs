using UnityEngine;
using UnityEngine.UI;          // 🔹 이 줄 추가
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameStartCardPicker : MonoBehaviour
{
    public WallCardPlacer wallPlacer;

    void Start()
    {
        // ▶ StageManager를 쓰지 않는 옛 버전 대비용
        // 지금 프로젝트에는 StageManager가 있으니까
        // Start에서는 아무 것도 안 해도 됨.
        if (StageManager.Instance == null)
        {
            SetupForStage(1);
        }
    }

    /// <summary>
    /// StageManager에서 호출하는 진짜 진입점.
    /// 스테이지마다 과녁에 걸릴 초기 5장을 다시 뽑는다.
    /// </summary>
    public void SetupForStage(int stageIndex)
    {
        StopAllCoroutines();
        StartCoroutine(DelayedInit(stageIndex));
    }

    private IEnumerator DelayedInit(int stageIndex)
    {
        // DeckManager가 덱 UI를 다시 그릴 시간을 줌
        yield return new WaitForSeconds(0.1f);

        // 전체 DeckCard 가져오기 (이미 사용된 것 포함)
        DeckCard[] allCards = FindObjectsOfType<DeckCard>();

        List<DeckCard> candidates = allCards
            .Where(c => c.CardSprite != null
                        && c.TryGetComponent(out Image img)
                        && img.raycastTarget)   // 사용 가능한 카드만
            .ToList();

        if (candidates.Count < 5)
        {
            Debug.LogError($"❌ [GameStartCardPicker] 스테이지 {stageIndex} 에서 덱 카드가 5장 미만입니다!");
            yield break;
        }

        // 5장 랜덤 선택
        List<DeckCard> selected = new List<DeckCard>();
        for (int i = 0; i < 5; i++)
        {
            int idx = Random.Range(0, candidates.Count);
            selected.Add(candidates[idx]);
            candidates.RemoveAt(idx);
        }

        // 패널에서 회색 처리 + 클릭 불가 (이미 쓰인 걸로 표시)
        foreach (var card in selected)
            card.MarkAsUsed();

        // 과녁에 붙일 Sprite 리스트
        List<Sprite> sprites = selected.Select(c => c.CardSprite).ToList();

        // 과녁에 카드 배치
        wallPlacer.PlaceCards(sprites);

        Debug.Log($"🎯 [GameStartCardPicker] Stage {stageIndex} → 5장 랜덤 선택 + 벽 부착 완료");
    }
}
