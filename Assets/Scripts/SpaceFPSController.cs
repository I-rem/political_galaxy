using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SpaceFPSController : MonoBehaviour
{
    public float acceleration = 90f; // Karakter ivmesi hafif arttırıldı
    public float maxSpeed = 160f; // Tavan hız arttırıldı
    public float drag = 1.0f; 
    public float mouseSensitivity = 1.2f; // Mouse hassasiyeti çok düşürüldü

    private CharacterController cc;
    private float verticalRotation = 0f;
    private Vector3 currentVelocity = Vector3.zero;
    
    private GameObject crosshairCanvas;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.SetParent(transform);
            mainCam.transform.localPosition = new Vector3(0, 0.8f, 0);
            mainCam.transform.localRotation = Quaternion.identity;
        }

        CreateCrosshair();
    }

    void CreateCrosshair()
    {
        crosshairCanvas = new GameObject("CrosshairCanvas");
        Canvas canvas = crosshairCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler cs = crosshairCanvas.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

        GameObject dot = new GameObject("CrosshairDot");
        dot.transform.SetParent(crosshairCanvas.transform, false);
        
        // Image default sprite olmadan kullanılırsa beyaz dikdörtgen görünür
        // Bu yüzden Image yerine Text ile küçük + işareti kullanıyoruz
        Text t = dot.AddComponent<Text>();
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.font = f;
        t.text = "+";
        t.fontSize = 18;
        t.color = new Color(1f, 1f, 1f, 0.7f);
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        
        RectTransform rt = dot.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(20, 20);
        rt.anchoredPosition = Vector2.zero;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return; // Prevent movement/looking while game paused (IntroScreen)

        // UI Menüsü kapalıyken (fare kilitliyken) ekran kamerayı çevir.
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (crosshairCanvas != null && !crosshairCanvas.activeSelf) 
                crosshairCanvas.SetActive(true);

            float mouseX = 0f;
            float mouseY = 0f;

            if (Mouse.current != null)
            {
                mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity * 0.05f;
                mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity * 0.05f;
            }

            transform.Rotate(0, mouseX, 0);
            
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
            if (Camera.main != null)
            {
                Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
            }
        }
        else
        {
            // Eğer UI açık ve menüdeysek crosshair ortadan kalksın
            if (crosshairCanvas != null && crosshairCanvas.activeSelf) 
                crosshairCanvas.SetActive(false);
        }

        float moveX = 0f;
        float moveZ = 0f;
        float moveY = 0f;
        float speedMultiplier = 1f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed) moveZ -= 1f;
            if (Keyboard.current.dKey.isPressed) moveX += 1f;
            if (Keyboard.current.aKey.isPressed) moveX -= 1f;

            if (Keyboard.current.eKey.isPressed) moveY += 1f;
            if (Keyboard.current.qKey.isPressed) moveY -= 1f;

            if (Keyboard.current.shiftKey.isPressed) speedMultiplier = 3.5f;
        }

        Vector3 moveInput = transform.right * moveX + transform.up * moveY + transform.forward * moveZ;
        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

        currentVelocity += moveInput * (acceleration * speedMultiplier) * Time.deltaTime;
        currentVelocity -= currentVelocity * drag * Time.deltaTime;

        float currentMaxSpeed = maxSpeed * speedMultiplier;
        if (currentVelocity.magnitude > currentMaxSpeed)
        {
            currentVelocity = currentVelocity.normalized * currentMaxSpeed;
        }

        cc.Move(currentVelocity * Time.deltaTime);
    }
}
