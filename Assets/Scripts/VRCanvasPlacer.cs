using UnityEngine;

public class VRCanvasPlacer : MonoBehaviour
{
    public float distance = 1.5f;
    public float smoothSpeed = 4f;

    void Start()
    {
        // Reduced scale significantly so it doesn't feel huge.
        // 1920x1080 canvas * 0.0008 = ~1.5 meters wide, perfect for reading at 1.5m distance.
        transform.localScale = new Vector3(0.0008f, 0.0008f, 0.0008f);
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            // Flatten the forward vector so the UI stays upright
            Vector3 forwardFlat = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z).normalized;
            
            if (forwardFlat.sqrMagnitude < 0.001f) 
                forwardFlat = cam.transform.forward;

            // Maintain the camera's Y position (eye level) instead of dropping to the floor
            Vector3 targetPosition = cam.transform.position + (forwardFlat * distance);
            targetPosition.y = cam.transform.position.y;
            
            // Use unscaledDeltaTime because Time.timeScale is 0 during the Intro!
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.unscaledDeltaTime * smoothSpeed);
            
            // Look exactly at the camera
            transform.LookAt(cam.transform.position);
            transform.Rotate(0, 180, 0); 
        }
    }
}
