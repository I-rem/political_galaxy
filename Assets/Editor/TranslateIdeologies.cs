using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class TranslateIdeologies
{
    [MenuItem("VR/Translate Ideologies")]
    public static void Translate()
    {
        string helperMethods = @"
    public static string GetTranslatedCategoryName(string category)
    {
        string lower = category.ToLower();
        if (lower.Contains(""religious"")) return ""Dini Aşırıcılık"";
        if (lower.Contains(""maga"") || lower.Contains(""populism"")) return ""MAGA / Popülizm"";
        if (lower.Contains(""gender"")) return ""Cinsiyet Özcülüğü"";
        if (lower.Contains(""ethno"")) return ""Etnomilliyetçilik"";
        if (lower.Contains(""eco"")) return ""Eko-Otoriteryenizm"";
        if (lower.Contains(""progressive"")) return ""İlerici Sol"";
        if (lower.Contains(""libertarian"")) return ""Özgürlükçü Sağ"";
        if (lower.Contains(""identitarian"")) return ""Kimlikçi Sol"";
        if (lower.Contains(""bridge"")) return ""Köprü Gezegeni"";
        return category;
    }
";

        // PlanetGravity.cs Update
        string pgPath = "Assets/Scripts/PlanetGravity.cs";
        string pg = File.ReadAllText(pgPath);

        // Replace GetExplanationForCategory
        string oldExplanations = @"private string GetExplanationForCategory\(string category\)[\s\S]*?A deep dive view mapping specific polarized perspectives inside social systems\."";\s*\}";
        string newExplanations = @"public string GetExplanationForCategory(string category)
    {
        string catLower = category.ToLower();
        if (catLower.Contains(""religious""))
            return ""Dini Aşırıcılık, ılımlı yorumları reddeden ve genellikle toplumun katı köktendinci yasalara göre tamamen yeniden yapılandırılmasını savunan radikal dini ideolojilerdir."";
        if (catLower.Contains(""populism"") || catLower.Contains(""maga""))
            return ""'Yozlaşmış elitlere' karşı 'sıradan halkı' desteklediğini iddia eden siyasi bir yaklaşım. Karmaşık sorunları, erdemli halk ile dürüst olmayan müesses nizam arasındaki ahlaki bir mücadeleye indirger."";
        if (catLower.Contains(""gender""))
            return ""Aşırı Cinsiyet Özcülüğü, erkeklerin ve kadınların rollerini, davranışlarını ve sosyal statülerini belirleyen sabit, doğuştan gelen ve değişmez biyolojik doğaları olduğu inancına radikal bir şekilde bağlılığı tanımlamak için kullanılan bir terimdir."";
        if (catLower.Contains(""ethno""))
            return ""Etnomilliyetçilik, ulusun ortak siyasi ilkeler veya vatandaşlıktan ziyade doğrudan ortak bir etnik kimlikle tanımlandığı bir milliyetçilik biçimidir."";
        if (catLower.Contains(""eco""))
            return ""Eko-Otoriteryenizm, demokratik sistemlerin iklim krizini ele almak için çok yavaş veya verimsiz olduğunu öne süren ve bunun yerine insanlığın hayatta kalmasını sağlamak için otoriter bir hükümetin katı çevre düzenlemeleri dayatması gerektiğini savunan siyasi bir kavramdır."";
        if (catLower.Contains(""progressive""))
            return ""İlerici Sol, eşitsizliği gidermek için ekonomik sistemlerin yapısal dönüşümünü savunur. Temel görüşleri arasında evrensel sağlık hizmetleri, aşırı servetin vergilendirilmesi, öğrenci borçlarının iptal edilmesi, işçi hakları ve güçlü iklim yasaları yer alır."";
        if (catLower.Contains(""libertarian""))
            return ""Özgürlükçü Sağ ideoloji, bireysel özgürlüğün en yüce siyasi değer olduğunu savunur. Vergiyi zorbalık olarak görür, düzenlemeye tabi olmayan serbest piyasaları savunur ve hükümet müdahalesini - sosyal veya ekonomik olsun - doğası gereği zorba olarak değerlendirir."";
        if (catLower.Contains(""identitarian""))
            return ""Kimlikçi Sol, siyaseti öncelikle ırk, sömürgecilik ve sistemik baskı merceğinden çerçeveler. Beyazlığın yapı sökümünü, tazminatları, polisin kaldırılmasını ve siyasi söylemlerde tarihsel olarak marjinalleştirilmiş sesleri merkeze almayı savunur."";

        return ""Sosyal sistemler içindeki belirli kutuplaşmış perspektiflerin haritasını çıkaran derinlemesine bir görünüm."";
    }

" + helperMethods;

        pg = Regex.Replace(pg, oldExplanations, newExplanations);

        // Also change ShowPlanetInfoOnPhone call
        pg = pg.Replace("ViewData.CategoryName,", "GetTranslatedCategoryName(ViewData.CategoryName),");
        File.WriteAllText(pgPath, pg, System.Text.Encoding.UTF8);

        // PlanetManager.cs Update
        string pmPath = "Assets/Scripts/PlanetManager.cs";
        string pm = File.ReadAllText(pmPath);
        
        // Ensure helper exists in PlanetManager too (as an instance or copy)
        // I'll just add it to the class if not exists
        if (!pm.Contains("GetTranslatedCategoryName"))
        {
            pm = pm.Replace("public class PlanetManager : MonoBehaviour\r\n{", "public class PlanetManager : MonoBehaviour\r\n{" + helperMethods);
            pm = pm.Replace("public class PlanetManager : MonoBehaviour\n{", "public class PlanetManager : MonoBehaviour\n{" + helperMethods);
        }

        pm = pm.Replace("catText.text     = category + \" ( )\";", "catText.text = GetTranslatedCategoryName(category) + \" ( )\";");
        pm = pm.Replace("\"<color=#00ccff><b>\" + categoryName + \"</b> (O)</color>\";", "\"<color=#00ccff><b>\" + GetTranslatedCategoryName(categoryName) + \"</b> (O)</color>\";");

        File.WriteAllText(pmPath, pm, System.Text.Encoding.UTF8);

        Debug.Log("Ideologies translated successfully.");
    }
}
