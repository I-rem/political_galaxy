using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// VRKeywordRaycaster — Quest 3 VR version.
/// Attach this to the Player GameObject (same object as SpaceFPSController).
///
/// Behaviour:
///   - Every frame, casts a ray from the CENTER of the player's camera (head gaze).
///   - When the ray hits an OrbitingKeyword within range AND the player presses the
///     right trigger or A button, the TweetUIManager shows that keyword's tweets.
///
/// Note: For a more precise "point with controller" interaction you could use the
/// Meta XR Interaction SDK's RayInteractor component, but gaze-based selection is
/// simpler, requires no extra prefabs, and works well for this experience.
/// </summary>
public class VRKeywordRaycaster : MonoBehaviour
{
    [Tooltip("Maximum distance in world units at which a keyword can be selected.")]
    public float maxRayDistance = 2000f;

    [Tooltip("Highlight color shown on a keyword when the player's gaze is on it.")]
    public Color gazeHighlightColor = new Color(1f, 1f, 0.3f, 1f);

    private OrbitingKeyword currentGazedKeyword = null;
    private Color originalKeywordColor = Color.white;
    private bool triggerWasHeld = false;

    void Update()
    {
        DetectGaze();
        HandleSelection();
    }

    // ---------------------------------------------------------------
    void DetectGaze()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        OrbitingKeyword hitKeyword = null;

        if (Physics.Raycast(ray, out hit, maxRayDistance))
        {
            hitKeyword = hit.collider.GetComponent<OrbitingKeyword>();
        }

        // De-highlight previous keyword if gaze moved away
        if (currentGazedKeyword != null && currentGazedKeyword != hitKeyword)
        {
            currentGazedKeyword.ChangeColor(originalKeywordColor);
            currentGazedKeyword = null;
        }

        // Highlight new keyword
        if (hitKeyword != null && hitKeyword != currentGazedKeyword)
        {
            TextMesh tm = hitKeyword.GetComponent<TextMesh>();
            if (tm != null) originalKeywordColor = tm.color;
            hitKeyword.ChangeColor(gazeHighlightColor);
            currentGazedKeyword = hitKeyword;
        }
    }

    // ---------------------------------------------------------------
    void HandleSelection()
    {
        if (currentGazedKeyword == null) return;

        bool triggerPressed = false;

        // Right trigger or A button on Quest controller
        if (Gamepad.current != null)
        {
            bool triggerHeld = Gamepad.current.rightTrigger.ReadValue() > 0.7f
                            || Gamepad.current.buttonSouth.isPressed;  // A

            triggerPressed = triggerHeld && !triggerWasHeld;  // rising edge
            triggerWasHeld = triggerHeld;
        }
        else
        {
            triggerWasHeld = false;
        }

        // Keyboard fallback for editor testing
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            triggerPressed = true;

        if (!triggerPressed) return;

        TextMesh tm = currentGazedKeyword.GetComponent<TextMesh>();
        if (tm == null) return;

        string keywordStr = tm.text.Trim();
        DataLoader dl = FindObjectOfType<DataLoader>();
        if (dl != null && dl.KeywordTweets.ContainsKey(keywordStr) && TweetUIManager.Instance != null)
        {
            TweetUIManager.Instance.ShowKeywordTweets(keywordStr, dl.KeywordTweets[keywordStr]);
        }
    }
}
