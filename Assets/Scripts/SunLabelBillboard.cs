using UnityEngine;

/// <summary>
/// Keeps a world-space TextMesh label always facing the camera (billboard),
/// and scales it slightly based on distance so it's always readable.
/// Attach to any child object of a Sun that has a TextMesh.
/// </summary>
public class SunLabelBillboard : MonoBehaviour
{
    void Update()
    {
        if (Camera.main == null) return;

        // Face the camera
        transform.rotation = Quaternion.LookRotation(
            transform.position - Camera.main.transform.position
        );

        // Scale with distance so it stays comfortably readable at any range
        float dist  = Vector3.Distance(Camera.main.transform.position, transform.position);
        float scale = Mathf.Clamp(dist * 0.006f, 0.4f, 3.5f);
        transform.localScale = Vector3.one * scale;
    }
}
