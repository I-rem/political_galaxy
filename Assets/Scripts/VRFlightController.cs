using UnityEngine;
using UnityEngine.XR;

public class VRFlightController : MonoBehaviour
{
    public float flySpeed = 40f;
    public float turnSpeed = 80f; // Dönüş hızı
    
    public bool isGameStarted = false; // Oyunun başladığını kontrol eden bayrak
    private Transform cameraTransform;

    void Start()
    {
        if (Camera.main != null)
            cameraTransform = Camera.main.transform;
        else
            cameraTransform = GetComponentInChildren<Camera>()?.transform;
    }

    void Update()
    {
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        // Start butonuna basılmadıysa hiçbir harekete izin verme, tetik bekleyelim
        if (!isGameStarted)
        {
            bool triggerPressed = false;
            if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed) && triggerPressed)
            {
                isGameStarted = true;
                
                IntroManager[] intros = Resources.FindObjectsOfTypeAll<IntroManager>();
                foreach(var intro in intros) if (intro.gameObject.scene.IsValid()) intro.StartGame();

                GameObject startBtn = GameObject.Find("StartButton");
                if (startBtn != null) Destroy(startBtn);

                if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();
            }
            return;
        }

        // Kamerayı dinamik olarak bul
        Transform currentCam = Camera.main != null ? Camera.main.transform : null;
        if (currentCam == null) currentCam = GetComponentInChildren<Camera>()?.transform;
        if (currentCam == null) return;

        Vector3 moveDirection = Vector3.zero;

        // 1. TETİK (TRIGGER) İLE GAZ VE FREN (İLERİ/GERİ)
        // Sağ Tetik = İleri Gaz
        float rightTrigger = 0f;
        float currentFlySpeed = flySpeed;

        // Sağ Orta Parmak (Grip) = Hız Boostu (3x)
        float rightGrip = 0f;
        if (rightHand.TryGetFeatureValue(CommonUsages.grip, out rightGrip) && rightGrip > 0.5f)
        {
            currentFlySpeed *= 3f;
        }

        if (rightHand.TryGetFeatureValue(CommonUsages.trigger, out rightTrigger) && rightTrigger > 0.1f)
        {
            moveDirection += currentCam.forward * rightTrigger; // Baktığın yöne doğru itme
        }

        // Sol Tetik = Geri Fren/Geri Gitme
        float leftTrigger = 0f;
        if (leftHand.TryGetFeatureValue(CommonUsages.trigger, out leftTrigger) && leftTrigger > 0.1f)
        {
            moveDirection -= currentCam.forward * leftTrigger;
        }

        // 2. SAĞ ANALOG (YÖNÜ ÇEVİRME / YAW)
        Vector2 rightJoystick = Vector2.zero;
        if (rightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightJoystick))
        {
            if (Mathf.Abs(rightJoystick.x) > 0.1f)
            {
                transform.Rotate(0, rightJoystick.x * turnSpeed * Time.deltaTime, 0, Space.World);
            }
        }

        // 3. SOL ANALOG (STRAFE / YANLARA VE YUKARI AŞAĞI KAYMA)
        Vector2 leftJoystick = Vector2.zero;
        if (leftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftJoystick))
        {
            if (Mathf.Abs(leftJoystick.x) > 0.1f)
            {
                moveDirection += currentCam.right * leftJoystick.x; // Sağa Sola kay
            }
            if (Mathf.Abs(leftJoystick.y) > 0.1f)
            {
                moveDirection += currentCam.forward * leftJoystick.y; // Yukarı Aşağı kay
            }
        }

        // Hareketi Uygula
        if (moveDirection.magnitude > 0.01f)
        {
            if (moveDirection.magnitude > 1f) moveDirection.Normalize();
            transform.position += moveDirection * currentFlySpeed * Time.deltaTime;
        }
    }
}
