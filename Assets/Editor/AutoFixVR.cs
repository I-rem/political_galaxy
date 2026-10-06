using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class AutoFixVR : EditorWindow
{
    [MenuItem("VR Setup/Fix VR Movement and Hands")]
    public static void FixVR()
    {
        // Unity 2.5.4 için XR Origin sınıfı: Unity.XR.CoreUtils.XROrigin veya UnityEngine.XR.Interaction.Toolkit.XROrigin olabilir.
        // Yeni sürümlerde CoreUtils.XROrigin kullanılıyor.
        Unity.XR.CoreUtils.XROrigin rig = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
        if (rig == null)
        {
            Debug.LogError("Sahnenizde 'XR Origin (VR)' bulunamadı! Lütfen Hierarchy'ye eklediğinizden emin olun.");
            return;
        }

        // 1. Standart VR Hareketlerini Temizle (Kullanıcı özel uzay uçuşu istiyor)
        var oldMove = rig.GetComponent<ActionBasedContinuousMoveProvider>();
        if (oldMove != null) DestroyImmediate(oldMove);
        var oldTurn = rig.GetComponent<ActionBasedContinuousTurnProvider>();
        if (oldTurn != null) DestroyImmediate(oldTurn);
        var locoSys = rig.GetComponent<LocomotionSystem>();
        if (locoSys != null) DestroyImmediate(locoSys);

        // 2. Özel Uzay Uçuşunu Ekle (VRFlightController)
        var spaceFlight = rig.GetComponent<VRFlightController>();
        if (spaceFlight == null) spaceFlight = rig.gameObject.AddComponent<VRFlightController>();

        // 3. Eller ve Takip Sistemi
        ActionBasedController[] controllers = rig.GetComponentsInChildren<ActionBasedController>(true);
        foreach (var ctrl in controllers)
        {
            bool isLeft = ctrl.gameObject.name.ToLower().Contains("left");
            string handStr = isLeft ? "{LeftHand}" : "{RightHand}";

            // Pozisyon ve Rotasyon takibi
            InputAction posAction = new InputAction("Position", InputActionType.Value, $"<XRController>{handStr}/devicePosition");
            posAction.expectedControlType = "Vector3";
            InputAction rotAction = new InputAction("Rotation", InputActionType.Value, $"<XRController>{handStr}/deviceRotation");
            rotAction.expectedControlType = "Quaternion";

            ctrl.positionAction = new InputActionProperty(posAction);
            ctrl.rotationAction = new InputActionProperty(rotAction);

            // Tetikleyici (UI ve objelerle etkileşim için)
            InputAction selectAction = new InputAction("Select", InputActionType.Value, $"<XRController>{handStr}/trigger");
            selectAction.expectedControlType = "Axis";
            ctrl.selectAction = new InputActionProperty(selectAction);
            ctrl.selectActionValue = new InputActionProperty(selectAction);
        }

        // Unity Editor'da değişikliklerin kaydedilmesi için sahneyi kirli olarak işaretle
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("VR hareketleri ve eller başarıyla onarıldı! Artık yürüme ve dönüş çalışacaktır.");
    }
}
