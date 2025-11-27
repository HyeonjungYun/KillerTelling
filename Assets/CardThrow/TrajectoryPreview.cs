using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPreview : MonoBehaviour
{
    private LineRenderer lr;
    public int pointCount = 25;
    public float timeStep = 0.05f;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = pointCount;
        lr.enabled = false;

        lr.startWidth = 0.03f;
        lr.endWidth = 0.01f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(1f, 0.8f, 0.3f, 1f);
        lr.endColor = new Color(1f, 0.4f, 0.1f, 0.6f);
    }

    public void ShowLine() => lr.enabled = true;
    public void HideLine() => lr.enabled = false;

    public void UpdateTrajectory(Vector3 velocity, Vector3 startPos)
    {
        Vector3 pos = startPos;
        Vector3 vel = velocity;

        for (int i = 0; i < pointCount; i++)
        {
            lr.SetPosition(i, pos);

            vel += Physics.gravity * timeStep;
            pos += vel * timeStep;
        }
    }
}
