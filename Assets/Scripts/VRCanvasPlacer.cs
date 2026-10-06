using UnityEngine;
using System.Collections;

public class VRCanvasPlacer : MonoBehaviour
{
    IEnumerator Start()
    {
        // Wait until XR Origin and Main Camera are fully initialized
        yield return new WaitForSeconds(0.5f);
        
        Camera cam = Camera.main;
        if (cam != null)
        {
            transform.position = cam.transform.position + cam.transform.forward * 3.0f;
            transform.LookAt(cam.transform);
            transform.Rotate(0, 180, 0); // Reverse so it faces camera correctly
            transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
        }
    }
}
