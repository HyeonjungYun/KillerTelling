using System.Collections.Generic;
using UnityEngine;

public class JokerDeckSpawner : MonoBehaviour
{
    public Transform spawnArea;       // 테이블에서 빨간 영역 위치
    public GameObject card3DPrefab;   // 3D 카드 프리팹
    public int jokerCount = 7;

    void Start()
    {
        SpawnJokerDeck();
    }

    public void SpawnJokerDeck()
    {
        float cardThickness = 0.015f;

        for (int i = 0; i < jokerCount; i++)
        {
            GameObject card = Instantiate(card3DPrefab, spawnArea);
            card.name = $"Joker_{i}";

            // 세로로 쌓기
            card.transform.localPosition = new Vector3(0, i * cardThickness, 0);
            card.transform.localRotation = Quaternion.Euler(90, 0, 0);

            // 스프라이트 적용
            MeshRenderer renderer = card.GetComponent<MeshRenderer>();
            Sprite jokerSprite = CardManager.GetCardSprite("J", 0);

            if (renderer != null && jokerSprite != null)
            {
                Material mat = new Material(Shader.Find("Unlit/Transparent"));
                mat.mainTexture = jokerSprite.texture;
                renderer.material = mat;
            }
        }

        Debug.Log("🃏 Joker 7장 생성 완료");
    }
}
