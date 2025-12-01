using UnityEngine;

public class CameraZoomToDart : MonoBehaviour
{
    public Transform dartBoard;     // 다트판 위치
    public float zoomedZ = -7f;     // 가까이 갈 Z 값
    public float zoomSpeed = 3f;

    private Vector3 originalPos;
    private bool zoomingIn = false;
    private bool zoomingOut = false;

    void Start()
    {
        originalPos = transform.position;
    }

    void Update()
    {
        if (zoomingIn)
        {
            Vector3 target = new Vector3(transform.position.x, transform.position.y, zoomedZ);
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * zoomSpeed);
        }

        if (zoomingOut)
        {
            transform.position = Vector3.Lerp(transform.position, originalPos, Time.deltaTime * zoomSpeed);
        }
    }

    public void ZoomIn()
    {
        zoomingOut = false;
        zoomingIn = true;
    }

    public void ZoomOut()
    {
        zoomingIn = false;
        zoomingOut = true;
    }
}
