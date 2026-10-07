using System.IO;
using UnityEngine;
using UnityEditor;

public class TranslateToTurkish
{
    [MenuItem("VR/Translate To Turkish")]
    public static void Translate()
    {
        // 1. MuskTransitionManager.cs
        string muskPath = "Assets/Scripts/MuskTransitionManager.cs";
        string musk = File.ReadAllText(muskPath);

        musk = musk.Replace("SYSTEM OVERRIDE", "SSTEM DEVRE DII");
        musk = musk.Replace("PLATFORM OWNERSHIP CHANGED.\\nNEW ALGORITHM DEPLOYED.\\n\\nPolarization limits unlocked. Echo chambers amplifying.", "PLATFORM SAHPL DET.\\nYEN ALGORTMA DEVREDE.\\n\\nKutuplama snrlar kaldrld. Yank odalar gleniyor.");
        musk = musk.Replace("algorithm, x, takeover, amplification", "algoritma, x, el koyma, glendirme");
        musk = musk.Replace("ERROR: INFINITE", "HATA: SONSUZ");

        musk = musk.Replace("SYSTEM RESTORED", "SSTEM GER YKLEND");
        musk = musk.Replace("REVERTING TO LEGACY ALGORITHM.\\n\\nPolarization weights normalized. Returning to pre-2022 standards.", "ESK ALGORTMAYA DNLYOR.\\n\\nKutuplama arlklar normale dnd. 2022 ncesi standartlara dnlyor.");
        musk = musk.Replace("legacy, twitter, standard, timeline", "eski, twitter, standart, zaman tneli");
        musk = musk.Replace("NORMAL", "NORMAL");

        musk = musk.Replace("Platform X - New Order", "Platform X - Yeni Dzen");
        musk = musk.Replace("Twitter Era", "Twitter Dnemi");

        File.WriteAllText(muskPath, musk);


        // 2. PlanetManager.cs
        string pmPath = "Assets/Scripts/PlanetManager.cs";
        string pm = File.ReadAllText(pmPath);

        pm = pm.Replace("PULL RIGHT TRIGGER TO START", "BALAMAK N SA TET EKN");
        pm = pm.Replace("Explore the Solar Systems", "Güneş Sistemlerini Keşfet");
        pm = pm.Replace("Find the Bridging Planet", "Köprü Gezegeni Bul");
        pm = pm.Replace("PORTAL OPENED!\\nGo to the core.", "PORTAL AÇILDI!\\nÇekirdeğe ilerleyin.");
        pm = pm.Replace("Explore the bridge", "Köprüyü keşfet");
        pm = pm.Replace("Explanation:", "Açıklama:");
        pm = pm.Replace("Keywords:", "Anahtar Kelimeler:");

        File.WriteAllText(pmPath, pm);

        Debug.Log("Translated to Turkish!");
    }
}
