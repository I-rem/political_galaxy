using UnityEngine;

public class ThemeRotator : MonoBehaviour
{
    public float rotationSpeed = 2f;

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }
}
