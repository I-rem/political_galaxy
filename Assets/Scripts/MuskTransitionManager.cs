using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class MuskTransitionManager : MonoBehaviour
{
    private bool isXEra = false;
    private bool isTransitioning = false;
    private bool wasTriggerPressed = false;

    private Dictionary<PlanetGravity, Vector3> originalScales = new Dictionary<PlanetGravity, Vector3>();
    private Dictionary<PlanetGravity, float> originalGravities = new Dictionary<PlanetGravity, float>();

    void Start()
    {
        // Başlangıç değerlerini kaydet
        StartCoroutine(RecordInitialStates());
    }

    IEnumerator RecordInitialStates()
    {
        yield return new WaitForSeconds(1f); // Gezegenlerin oluşmasını bekle
        PlanetGravity[] allPlanets = FindObjectsOfType<PlanetGravity>();
        foreach (var p in allPlanets)
        {
            originalScales[p] = p.transform.localScale;
            originalGravities[p] = p.gravityForce;
        }
    }

    void Update()
    {
        // SAĞ eldeki "A" tuşuna (Primary Button) basıldığında geçiş yap
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        bool primaryButtonValue;
        
        bool isPressed = false;
        if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonValue) && primaryButtonValue) 
        {
            isPressed = true;
        }

        // Space tuşu ile PC'den test için yedek
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.isPressed) isPressed = true;

        if (isPressed && !wasTriggerPressed)
        {
            wasTriggerPressed = true;
            if (!isTransitioning)
            {
                ToggleEra();
            }
        }
        else if (!isPressed)
        {
            wasTriggerPressed = false;
        }
    }

    void ToggleEra()
    {
        isXEra = !isXEra;
        StartCoroutine(TransitionRoutine(isXEra));
    }

    IEnumerator TransitionRoutine(bool toXEra)
    {
        isTransitioning = true;
        
        Camera[] cams = FindObjectsOfType<Camera>();
        Color[] origColors = new Color[cams.Length];
        CameraClearFlags[] origFlags = new CameraClearFlags[cams.Length];

        for (int i = 0; i < cams.Length; i++)
        {
            origColors[i] = cams[i].backgroundColor;
            origFlags[i] = cams[i].clearFlags;
            cams[i].clearFlags = CameraClearFlags.SolidColor;
        }

        // DRAMATİK GLITCH FLAŞI
        int flashes = toXEra ? 6 : 3;
        Color flashColor = toXEra ? new Color(0.9f, 0.1f, 0.1f, 1f) : new Color(0.1f, 0.6f, 1f, 1f); // X için kırmızı, Twitter için mavi
        
        for (int j = 0; j < flashes; j++)
        {
            for (int i = 0; i < cams.Length; i++) cams[i].backgroundColor = flashColor * Random.Range(0.6f, 1f);
            yield return new WaitForSeconds(0.06f);
            for (int i = 0; i < cams.Length; i++) cams[i].backgroundColor = Color.black;
            yield return new WaitForSeconds(0.04f);
        }

        // Telefona bildirim gönder
        if (PlanetManager.Instance != null)
        {
            if (toXEra)
            {
                PlanetManager.Instance.ShowPlanetInfoOnPhone(
                    "<color=#ff0000>SYSTEM OVERRIDE</color>",
                    "<b>PLATFORM OWNERSHIP CHANGED.\nNEW ALGORITHM DEPLOYED.</b>\n\nPolarization limits unlocked. Echo chambers amplifying.",
                    "algorithm, x, takeover, amplification",
                    "<color=#ff0000>ERROR: INFINITE</color>"
                );
            }
            else
            {
                PlanetManager.Instance.ShowPlanetInfoOnPhone(
                    "<color=#00ccff>SYSTEM RESTORED</color>",
                    "<b>REVERTING TO LEGACY ALGORITHM.</b>\n\nPolarization weights normalized. Returning to pre-2022 standards.",
                    "legacy, twitter, standard, timeline",
                    "<color=#00ccff>NORMAL</color>"
                );
            }
        }

        for (int i = 0; i < cams.Length; i++) cams[i].backgroundColor = flashColor;
        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < cams.Length; i++)
        {
            cams[i].backgroundColor = origColors[i];
            cams[i].clearFlags = origFlags[i];
        }

        // GEZEGENLERİ ÖLÇEKLENDİR
        PlanetGravity[] allPlanets = FindObjectsOfType<PlanetGravity>();
        float duration = 2.5f;
        float elapsed = 0f;

        Dictionary<PlanetGravity, Vector3> currentScales = new Dictionary<PlanetGravity, Vector3>();
        Dictionary<PlanetGravity, float> currentGravities = new Dictionary<PlanetGravity, float>();
        
        foreach (var p in allPlanets)
        {
            currentScales[p] = p.transform.localScale;
            currentGravities[p] = p.gravityForce;
            
            if (p.PlanetPS != null)
            {
                var em = p.PlanetPS.emission;
                em.rateOverTime = toXEra ? em.rateOverTime.constant * 2f : em.rateOverTime.constant / 2f;
            }
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Daha yumuşak animasyon için SmoothStep
            t = t * t * (3f - 2f * t);
            
            foreach (var p in allPlanets)
            {
                if (!originalScales.ContainsKey(p)) continue;
                Vector3 targetScale = toXEra ? originalScales[p] * 1.666f : originalScales[p];
                float targetGrav = toXEra ? 156f : originalGravities[p];

                p.transform.localScale = Vector3.Lerp(currentScales[p], targetScale, t);
                p.gravityForce = Mathf.Lerp(currentGravities[p], targetGrav, t);
            }
            yield return null;
        }

        // LOGOYU VE METNİ DEĞİŞTİR
        GameObject canvas = GameObject.Find("ChecklistCanvas");
        if (canvas != null)
        {
            Transform titleTrans = canvas.transform.Find("PhoneFrame/ChecklistBG/TitleText");
            if (titleTrans != null)
            {
                Text t = titleTrans.GetComponent<Text>();
                if (t != null)
                {
                    t.text = toXEra ? "<b><size=30>Platform X - New Order</size></b>" : "<b><size=30>Twitter Era</size></b>";
                    t.color = toXEra ? new Color(0.9f, 0.2f, 0.2f) : Color.white;
                }
            }
            
            Transform bgTrans = canvas.transform.Find("PhoneFrame/ChecklistBG");
            if (bgTrans != null)
            {
                Image bgImg = bgTrans.GetComponent<Image>();
                if (bgImg != null) bgImg.color = toXEra ? new Color(0.02f, 0.05f, 0.1f, 0.95f) : new Color(0.05f, 0.15f, 0.25f, 0.9f);
            }
        }

        yield return new WaitForSeconds(3f);

        if (PlanetManager.Instance != null)
        {
            PlanetManager.Instance.HidePlanetInfoOnPhone();
        }

        isTransitioning = false;
    }
}
