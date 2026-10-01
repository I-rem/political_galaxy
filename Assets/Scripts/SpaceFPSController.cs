using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.InputSystem.XR;
using System.Collections.Generic;

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
    private OrbitingKeyword draggingKeyword = null;

    private bool prevRightTrigger = false;
    private bool vrInitialized = false;

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

    public void StopMovement()
    {
        currentVelocity = Vector3.zero;
    }

    void CreateCrosshair()
    {
        crosshairCanvas = new GameObject("CrosshairCanvas");
        Canvas canvas = crosshairCanvas.AddComponent<Canvas>();
        
        bool isVR = XRSettings.isDeviceActive;
        if (isVR)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            if (Camera.main != null)
            {
                crosshairCanvas.transform.SetParent(Camera.main.transform, false);
                crosshairCanvas.transform.localPosition = new Vector3(0, 0, 2f); // 2 meters in front
                crosshairCanvas.transform.localRotation = Quaternion.identity;
                crosshairCanvas.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
            }
        }
        else
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
        }

        CanvasScaler cs = crosshairCanvas.AddComponent<CanvasScaler>();
        if (!isVR) cs.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

        GameObject dot = new GameObject("CrosshairDot");
        dot.transform.SetParent(crosshairCanvas.transform, false);
        
        Text t = dot.AddComponent<Text>();
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.font = f;
        t.text = "+";
        t.fontSize = 18;
        t.color = new Color(1f, 1f, 1f, 0.7f);
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        
        RectTransform rt = dot.GetComponent<RectTransform>();
        if (!isVR)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(20, 20);
            rt.anchoredPosition = Vector2.zero;
        }
        else
        {
            rt.sizeDelta = new Vector2(20, 20);
            rt.anchoredPosition = Vector2.zero;
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f) return; // Prevent movement/looking while game paused (IntroScreen)

        bool isVR = XRSettings.isDeviceActive;

        // Auto-attach VR Tracking to camera if active
        if (isVR && Camera.main != null)
        {
            if (!vrInitialized)
            {
                if (Camera.main.gameObject.GetComponent<TrackedPoseDriver>() == null)
                {
                    Camera.main.gameObject.AddComponent<TrackedPoseDriver>();
                }
                
                // If XR just connected, update crosshair to pure WorldSpace 
                Canvas c = crosshairCanvas.GetComponent<Canvas>();
                if (c.renderMode != RenderMode.WorldSpace)
                {
                    c.renderMode = RenderMode.WorldSpace;
                    crosshairCanvas.transform.SetParent(Camera.main.transform, false);
                    crosshairCanvas.transform.localPosition = new Vector3(0, 0, 2f);
                    crosshairCanvas.transform.localRotation = Quaternion.identity;
                    crosshairCanvas.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
                }
                vrInitialized = true;
            }
        }

        // UI Menüsü kapalıyken (fare kilitliyken) ekran kamerayı çevir.
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (crosshairCanvas != null && !crosshairCanvas.activeSelf) 
                crosshairCanvas.SetActive(true);

            if (!isVR) // Avoid mouse look competing with VR Headset
            {
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

        if (isVR)
        {
            List<InputDevice> leftHandDevices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, leftHandDevices);
            if (leftHandDevices.Count > 0 && leftHandDevices[0].TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 leftAxis))
            {
                moveX = leftAxis.x;
                moveZ = leftAxis.y;
            }

            List<InputDevice> rightHandDevices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, rightHandDevices);
            if (rightHandDevices.Count > 0 && rightHandDevices[0].TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightAxis))
            {
                moveY = rightAxis.y; // Use right stick purely for vertical/elevation control
            }
        }
        else if (Cursor.lockState == CursorLockMode.Locked && Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed) moveZ -= 1f;
            if (Keyboard.current.dKey.isPressed) moveX += 1f;
            if (Keyboard.current.aKey.isPressed) moveX -= 1f;

            if (Keyboard.current.eKey.isPressed) moveY += 1f;
            if (Keyboard.current.qKey.isPressed) moveY -= 1f;

            if (Keyboard.current.shiftKey.isPressed) speedMultiplier = 3.5f;
        }

        Transform camTransform = Camera.main != null ? Camera.main.transform : transform;
        Vector3 moveInput = transform.right * moveX + transform.up * moveY + camTransform.forward * moveZ;
        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

        currentVelocity += moveInput * (acceleration * speedMultiplier) * Time.deltaTime;
        currentVelocity -= currentVelocity * drag * Time.deltaTime;

        float currentMaxSpeed = maxSpeed * speedMultiplier;
        if (currentVelocity.magnitude > currentMaxSpeed)
        {
            currentVelocity = currentVelocity.normalized * currentMaxSpeed;
        }

        cc.Move(currentVelocity * Time.deltaTime);

        // Raycasting for orbiting keywords manually
        bool interactDown = false;
        bool interactHeld = false;
        bool interactUp = false;

        if (isVR)
        {
            List<InputDevice> rightHandDevices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, rightHandDevices);
            if (rightHandDevices.Count > 0)
            {
                rightHandDevices[0].TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerVal);
                interactDown = triggerVal && !prevRightTrigger;
                interactHeld = triggerVal;
                interactUp = !triggerVal && prevRightTrigger;
                prevRightTrigger = triggerVal;
            }
        }
        else if (Mouse.current != null)
        {
            interactDown = Mouse.current.leftButton.wasPressedThisFrame;
            interactHeld = Mouse.current.leftButton.isPressed;
            interactUp = Mouse.current.leftButton.wasReleasedThisFrame;
        }

        if (interactDown)
        {
            Ray ray = new Ray(camTransform.position, camTransform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1500f))
            {
                OrbitingKeyword keyword = hit.collider.GetComponent<OrbitingKeyword>();
                if (keyword != null)
                {
                    draggingKeyword = keyword;
                    keyword.ManualMouseDown();
                }
            }
        }

        if (draggingKeyword != null)
        {
            if (interactHeld)
            {
                draggingKeyword.ManualMouseDrag();
            }
            if (interactUp)
            {
                draggingKeyword.ManualMouseUp();
                draggingKeyword = null;
            }
        }
    }
}
