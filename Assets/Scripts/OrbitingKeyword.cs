using UnityEngine;

public class OrbitingKeyword : MonoBehaviour
{
    public Transform centerPoint;
    public float orbitSpeed = 20f;
    private Vector3 orbitAxis;

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
        if (centerPoint != null)
        {
            transform.RotateAround(centerPoint.position, orbitAxis, orbitSpeed * Time.deltaTime);
            
            if (Camera.main != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
            }
        }
    }
}
