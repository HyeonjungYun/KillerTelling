using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DartPhaseManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public RectTransform dartPanel;

    CardManager cardManager;
    DeckManager deckManager;
    Card3DSpawner card3DSpawner;   // 추가

    void Start()
    {
        cardManager = FindObjectOfType<CardManager>();
        deckManager = FindObjectOfType<DeckManager>();
        card3DSpawner = FindObjectOfType<Card3DSpawner>(); // 3D 스포너 연결
    }

    public void StartDartPhase()
    {
        List<Sprite> cards = cardManager.DrawRandomCards(5);

        // UI 5장 생성
        StartCoroutine(Spawn5(cards));

        // 3D 다트보드에 5장 생성
        if (card3DSpawner != null)
            card3DSpawner.SpawnCardsOnBoard(cards);

        // 기존 47장 덱 처리
        if (deckManager != null)
        {
            List<Sprite> usedCards = new List<Sprite>();
            usedCards.AddRange(cards);
            usedCards.AddRange(deckManager.RemovedCards);

            deckManager.ShowRemainingDeck(cardManager, usedCards);
        }
    }

    // 🔥 빠져 있던 부분: 다트 패널에 5장 뿌리는 코루틴
    IEnumerator Spawn5(List<Sprite> five)
    {
        RectTransform rtP = dartPanel;

        float halfW = rtP.rect.width * 0.5f;
        float halfH = rtP.rect.height * 0.5f;

        // 카드 크기 가정값 (UI RectTransform sizeDelta 기준)
        float cardHalfW = 100f;
        float cardHalfH = 140f;

        float minX = -halfW + cardHalfW;
        float maxX = halfW - cardHalfW;
        float minY = -halfH + cardHalfH;
        float maxY = halfH - cardHalfH;

        float minDistance = 150f;   // 카드끼리 최소 거리

        List<Vector2> usedPos = new List<Vector2>();

        foreach (var spr in five)
        {
            GameObject card = Instantiate(cardPrefab, dartPanel);
            Image img = card.GetComponent<Image>();
            img.sprite = spr;

            // 처음엔 작게 만들었다가
            card.transform.localScale = new Vector3(0.1f, 0.1f, 1);

            // 겹치지 않는 랜덤 위치 찾기
            Vector2 pos;
            bool valid;
            do
            {
                pos = new Vector2(
                    Random.Range(minX, maxX),
                    Random.Range(minY, maxY)
                );

                valid = true;
                foreach (var p in usedPos)
                {
                    if (Vector2.Distance(p, pos) < minDistance)
                    {
                        valid = false;
                        break;
                    }
                }
            } while (!valid);

            usedPos.Add(pos);
            card.GetComponent<RectTransform>().anchoredPosition = pos;

            // 스케일 애니메이션
            float t = 0f;
            Vector3 startScale = new Vector3(0.1f, 0.1f, 1);
            Vector3 targetScale = Vector3.one;

            while (t < 1f)
            {
                t += Time.deltaTime * 3f;
                card.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            yield return new WaitForSeconds(0.05f);
        }
    }
}
