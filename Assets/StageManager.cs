using UnityEngine;
using System.Collections.Generic;
using System.Linq; // 리스트 제어(Shuffle 등)를 위해 필요

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("Managers")]
    public WallCardPlacer wallPlacer;
    public DeckManager deckManager;
    public JokerStack3D jokerStack;
    public CardManager cardManager; // 전체 카드 스프라이트가 들어있는 매니저

    [Header("Settings")]
    public int cardsOnWallCount = 5; // 벽에 붙일 카드 개수

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartStage();
    }

    public void StartStage()
    {
        // 1. 전체 카드 가져오기
        if (cardManager == null || cardManager.cardSprites == null)
        {
            Debug.LogError("❌ CardManager가 연결되지 않았거나 카드가 없습니다.");
            return;
        }

        List<Sprite> allCards = cardManager.cardSprites.ToList();

        // 2. 랜덤하게 섞기 (Shuffle)
        // (System.Random을 사용하여 리스트 순서를 섞음)
        System.Random rng = new System.Random();
        allCards = allCards.OrderBy(x => rng.Next()).ToList();

        // 3. 벽에 붙일 카드 뽑기 (앞에서부터 N장)
        List<Sprite> wallCards = allCards.Take(cardsOnWallCount).ToList();

        // 4. 벽에 배치 실행
        wallPlacer.PlaceCards(wallCards);
        Debug.Log($"🧱 벽에 카드 {wallCards.Count}장 배치 완료");

        // 5. 덱 UI 갱신 (벽에 붙은 카드는 '사용됨' 처리하여 덱에서 비워두기)
        // DeckManager의 ShowRemainingDeck에 wallCards를 'usedCards'로 넘김
        deckManager.ShowRemainingDeck(cardManager, wallCards);

        // 6. 조커 생성 시작
        jokerStack.InitJokers();
        Debug.Log("🃏 조커 스택 생성 시작");
    }

    private void StageClear()
    {
        Debug.Log("🎉 스테이지 클리어!");

        // 아까 만든 GameFlowManager에게 클리어 사실을 알림 (대화 재생 등)
        if (GameFlowManager.instance != null)
        {
            GameFlowManager.instance.OnStageClear();
        }
    }
}