using UnityEngine;

public class CircleDrawer : MonoBehaviour
{
    public int Segments = 32;
    public Color Color = Color.blue;
    public MovementManager mm;
    private void OnDrawGizmos()
    {
        for (float i = mm.minDist; i <= mm.maxDist; i+= mm.stepDist)
        {
            DrawEllipse(transform.position, transform.forward, transform.up, i * transform.localScale.x, i * transform.localScale.y, Segments, Color);
        }
    }

    private static void DrawEllipse(Vector3 pos, Vector3 forward, Vector3 up, float radiusX, float radiusY, int segments, Color color, float duration = 0)
    {
        float angle = 0f;
        Quaternion rot = Quaternion.LookRotation(forward, up);
        Vector3 lastPoint = Vector3.zero;
        Vector3 thisPoint = Vector3.zero;

        for (int i = 0; i < segments + 1; i++)
        {
            thisPoint.x = Mathf.Sin(Mathf.Deg2Rad * angle) * radiusX;
            thisPoint.y = Mathf.Cos(Mathf.Deg2Rad * angle) * radiusY;

            if (i > 0)
            {
                Debug.DrawLine(rot * lastPoint + pos, rot * thisPoint + pos, color, duration);
            }

            lastPoint = thisPoint;
            angle += 360f / segments;
        }
    }
}
