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
            // Flatten the forward vector so the UI stays upright at eye level
            // rather than tilting into the floor/ceiling if the user looks up/down.
            Vector3 forwardFlat = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z).normalized;
            
            // Fallback if they are looking exactly straight up or down
            if (forwardFlat.sqrMagnitude < 0.001f) 
                forwardFlat = cam.transform.forward;

            Vector3 targetPosition = cam.transform.position + (forwardFlat * distance);
            
            // Smoothly glide the UI to stay in front of the user's gaze
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
            
            // Look exactly at the camera
            transform.LookAt(cam.transform.position);
            transform.Rotate(0, 180, 0); // Correct the Canvas backwards rendering
        }
    }
}
