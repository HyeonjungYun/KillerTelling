using UnityEngine;
using System.Collections.Generic;

public class Hand3DSpawner : MonoBehaviour
{
    public Transform handArea;       // 파란 직사각형 위치
    public GameObject card3DPrefab;

    public void SpawnHand(List<Sprite> handSprites)
    {
        float spacing = 0.5f;

        for (int i = 0; i < handSprites.Count; i++)
        {
            GameObject card = Instantiate(card3DPrefab, handArea);
            card.name = $"HandCard_{i}";

            card.transform.localPosition = new Vector3(i * spacing, 0, 0);
            card.transform.localRotation = Quaternion.Euler(90, 0, 0);

            MeshRenderer renderer = card.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Unlit/Transparent"));
            mat.mainTexture = handSprites[i].texture;
            renderer.material = mat;
        }
    }
}
