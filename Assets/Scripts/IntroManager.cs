using UnityEngine;

public class IntroManager : MonoBehaviour
{
    void Awake()
    {
        // VR'da artık bilek ekranı kullanıldığı için IntroManager tamamen iptal edildi.
        // gameObject'i silersek aynı objede olan PlanetManager falan da siliniyor! 
        // O yüzden sadece bu script'i (this) siliyoruz.
        Destroy(this);
    }

    public void StartGame()
    {
        // Boş bırakıldı ki diğer scriptler çağırırsa hata vermesin
    }
}
