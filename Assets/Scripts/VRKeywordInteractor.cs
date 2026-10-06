using UnityEngine;
using UnityEngine.XR;

public class VRKeywordInteractor : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private bool wasTriggerPressed = false;

    void Start()
    {
        // Lazer işaretçisi için LineRenderer oluştur (Diğer bileşenlerle çakışmayı önlemek için alt obje kullanıyoruz)
        GameObject laserObj = new GameObject("CustomVRLaser");
        laserObj.transform.SetParent(this.transform, false);
        
        lineRenderer = laserObj.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.015f;
        lineRenderer.endWidth = 0.015f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(0f, 1f, 1f, 0.6f);
        lineRenderer.endColor = new Color(0f, 1f, 1f, 0.0f); // Uca doğru kaybolan lazer
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        // Sağ el kontrolcüsünü bul
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        
        bool triggerPressed = false;
        rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed);

        RaycastHit hit;
        bool hitSomething = Physics.Raycast(transform.position, transform.forward, out hit, 2000f);

        // Lazeri çiz
        if (hitSomething)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + transform.forward * 200f);
        }

        // Tetiğe basıldığında
        if (triggerPressed && !wasTriggerPressed)
        {
            if (hitSomething)
            {
                OrbitingKeyword kw = hit.collider.GetComponent<OrbitingKeyword>();
                if (kw != null)
                {
                    TextMesh tm = kw.GetComponent<TextMesh>();
                    if (tm != null && TweetUIManager.Instance != null)
                    {
                        string keywordStr = tm.text.Trim();
                        DataLoader dl = FindObjectOfType<DataLoader>();
                        if (dl != null && dl.KeywordTweets.ContainsKey(keywordStr))
                        {
                            TweetUIManager.Instance.ShowKeywordTweets(keywordStr, dl.KeywordTweets[keywordStr]);
                        }
                    }
                }
            }
        }
        wasTriggerPressed = triggerPressed;
    }
}
