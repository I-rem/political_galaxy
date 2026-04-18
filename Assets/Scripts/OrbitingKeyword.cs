using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitingKeyword : MonoBehaviour
{
    public Transform centerPoint;
    public float orbitSpeed = 20f;
    private Vector3 orbitAxis;

    // Interaction state variables
    private bool isDragging = false;
    private float zDistance;
    private Vector3 offset;

    public void SetupText(string text, Color color, int fontSize = 90, float characterSize = 0.45f)
    {
        TextMesh tm = gameObject.AddComponent<TextMesh>();
        tm.text = text;
        tm.color = color;
        tm.fontSize = fontSize;
        tm.characterSize = characterSize;
        tm.anchor = TextAnchor.MiddleCenter;
        
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if(font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        tm.font = font;
        tm.GetComponent<Renderer>().material = tm.font.material;

        orbitAxis = Random.onUnitSphere;

        // Add a BoxCollider so the text can be clicked and dragged
        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true; // Use trigger so it does not physically push the player
        // Approximate collider size based on text length
        float width = text.Length * characterSize * 1.8f;
        float height = characterSize * 5f;
        box.size = new Vector3(width, height, 0.5f);
    }

    public void ChangeColor(Color newColor)
    {
        TextMesh tm = GetComponent<TextMesh>();
        if (tm != null)
        {
            tm.color = newColor;
        }
    }

    void Update()
    {
        // Stop movement with Space key
        bool isPaused = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;

        if (centerPoint != null && !isDragging && !isPaused)
        {
            transform.RotateAround(centerPoint.position, orbitAxis, orbitSpeed * Time.deltaTime);
        }
        
        if (Camera.main != null)
        {
            // Keep text facing the camera even while paused or dragged
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
        if (Camera.main != null)
        {
            zDistance = Camera.main.WorldToScreenPoint(transform.position).z;
            offset = transform.position - GetMouseWorldPos();
        }
    }

    void OnMouseDrag()
    {
        if (isDragging && Camera.main != null)
        {
            transform.position = GetMouseWorldPos() + offset;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
        
        // When dropped, recalculate a new orbit axis so it continues orbiting naturally from its new dragged position
        if (centerPoint != null)
        {
            Vector3 centerToCurrent = (transform.position - centerPoint.position).normalized;
            orbitAxis = Vector3.Cross(centerToCurrent, Random.onUnitSphere).normalized;
            
            // Fallback just in case
            if (orbitAxis == Vector3.zero) orbitAxis = Vector3.up;
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector2 mousePoint2D = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        Vector3 mousePoint = new Vector3(mousePoint2D.x, mousePoint2D.y, zDistance);
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}
