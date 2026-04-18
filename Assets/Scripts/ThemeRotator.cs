using UnityEngine;

public class ThemeRotator : MonoBehaviour
{
    public float rotationSpeed = 2f;

    void Update()
    {
        // Allow user to stop movement with the Space key
        if (!Input.GetKey(KeyCode.Space))
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
