using System.Collections.Generic;
using UnityEngine;

public class Card3DSpawner : MonoBehaviour
{
    public GameObject card3DPrefab;  // 3D카드 프리팹 (Quad + Material)
    public Transform jokerStackPoint; // 조커 7장 위치 기준점

    public float stackOffsetY = 0.03f; // 카드 한 장씩 위로 쌓일 간격

    // 다트판 카드 생성
    public void SpawnCardsOnBoard(List<Sprite> sprites)
    {
        // ...
    }

    // 🔥 조커 스택 7장 생성
    public void SpawnJokerStack(Sprite jokerSprite)
    {
        for (int i = 0; i < 7; i++)
        {
            GameObject card = Instantiate(card3DPrefab);

            // parent 설정 (필수!)
            card.transform.SetParent(jokerStackPoint, false);

            // 세로로 쌓기
            Vector3 pos = jokerStackPoint.position + new Vector3(0, i * stackOffsetY, 0);
            card.transform.position = pos;

            // 정면 바라보도록
            card.transform.rotation = Quaternion.Euler(90, 0, 0);

            // 카드 이미지 변경
            MeshRenderer renderer = card.GetComponent<MeshRenderer>();
            renderer.material.mainTexture = jokerSprite.texture;
        }
    }
}
