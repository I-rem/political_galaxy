using UnityEngine;
using UnityEngine.InputSystem;

public class ThemeRotator : MonoBehaviour
{
    public float rotationSpeed = 2f;

    void Update()
    {
        // Allow user to stop movement with the Space key
        bool isPaused = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        if (!isPaused)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
