using UnityEngine;
using UnityEngine.XR;

public class VRFlightController : MonoBehaviour
{
    public float flySpeed = 60f; // Hzi biraz artiralim
    public float turnSpeed = 80f;
    public bool isGameStarted = true;
    
    private Transform cameraTransform;
    private bool lastPrimaryButtonState = false;

    void Start()
    {
        flySpeed = 60f; // Force override inspector

        // Oto balat! Trigger'a gerek yok.
        IntroManager[] intros = Resources.FindObjectsOfTypeAll<IntroManager>();
        foreach(var intro in intros) if (intro.gameObject.scene.IsValid()) intro.StartGame();

        GameObject startBtn = GameObject.Find("StartButton");
        if (startBtn != null) Destroy(startBtn);
    }

    void Update()
    {
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        Transform currentCam = Camera.main != null ? Camera.main.transform : null;
        if (currentCam == null) currentCam = GetComponentInChildren<Camera>()?.transform;
        if (currentCam == null) return;

        Vector3 moveDirection = Vector3.zero;
        float currentFlySpeed = flySpeed;

        // Grip Boost (Left or Right, Button or Axis)
        bool boostPressed = false;
        float gripVal = 0f;
        bool gripBtn = false;
        if (rightHand.TryGetFeatureValue(CommonUsages.grip, out gripVal) && gripVal > 0.5f) boostPressed = true;
        if (rightHand.TryGetFeatureValue(CommonUsages.gripButton, out gripBtn) && gripBtn) boostPressed = true;
        if (leftHand.TryGetFeatureValue(CommonUsages.grip, out gripVal) && gripVal > 0.5f) boostPressed = true;
        if (leftHand.TryGetFeatureValue(CommonUsages.gripButton, out gripBtn) && gripBtn) boostPressed = true;
        if (boostPressed) {
            currentFlySpeed *= 3.5f;
        }

        // Freeze Planets Button (A/X)
        bool primaryPressed = false;
        bool rightA = false, leftX = false;
        if (rightHand.isValid) rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out rightA);
        if (leftHand.isValid) leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out leftX);
        primaryPressed = rightA || leftX;

        if (primaryPressed && !lastPrimaryButtonState)
        {
            PlanetOrbit.GlobalPause = !PlanetOrbit.GlobalPause; // Toggle!
            if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();
        }
        lastPrimaryButtonState = primaryPressed;

        // Right Trigger Forward
        float rightTrigger = 0f;
        if (rightHand.TryGetFeatureValue(CommonUsages.trigger, out rightTrigger) && rightTrigger > 0.1f)
            moveDirection += currentCam.forward * rightTrigger;

        // Left Trigger Backward
        float leftTrigger = 0f;
        if (leftHand.TryGetFeatureValue(CommonUsages.trigger, out leftTrigger) && leftTrigger > 0.1f)
            moveDirection -= currentCam.forward * leftTrigger;

        // Right Joystick: Yaw (X) & Up/Down (Y)
        Vector2 rightJoystick = Vector2.zero;
        if (rightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightJoystick))
        {
            if (Mathf.Abs(rightJoystick.x) > 0.1f)
                transform.Rotate(0, rightJoystick.x * turnSpeed * Time.deltaTime, 0, Space.World);
            
            if (Mathf.Abs(rightJoystick.y) > 0.1f)
                moveDirection += Vector3.up * rightJoystick.y;
        }

        // Left Joystick: Strafe (X) & Forward/Backward (Y)
        Vector2 leftJoystick = Vector2.zero;
        if (leftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftJoystick))
        {
            if (Mathf.Abs(leftJoystick.x) > 0.1f)
                moveDirection += currentCam.right * leftJoystick.x;
            
            if (Mathf.Abs(leftJoystick.y) > 0.1f)
                moveDirection += currentCam.forward * leftJoystick.y;
        }

        // Apply Movement
        if (moveDirection.magnitude > 0.01f)
        {
            if (moveDirection.magnitude > 1f) moveDirection.Normalize();
            transform.position += moveDirection * currentFlySpeed * Time.deltaTime;
        }
    }
}

