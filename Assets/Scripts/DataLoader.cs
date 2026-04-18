using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class PoliticalViewData
{
    public string CategoryName;
    public int TweetCount;
    public List<string> Keywords;
    public Dictionary<string, int> KeywordFrequencies;
    public string Explanation; // Yeni eklenen Sütun
    
    public PoliticalViewData(string name)
    {
        CategoryName = name;
        TweetCount = 0;
        Keywords = new List<string>();
        KeywordFrequencies = new Dictionary<string, int>();
        Explanation = ""; // Default boş kalsın
    }
}

public class DataLoader : MonoBehaviour
{
    public Dictionary<string, PoliticalViewData> PolarizingViews = new Dictionary<string, PoliticalViewData>();

    public void LoadData()
    {
        TextAsset dataset = Resources.Load<TextAsset>("polarization_tweets");
        if (dataset == null)
        {
            Debug.LogError("polarization_tweets.csv not found in Resources folder.");
            return;
        }

        string[] lines = dataset.text.Split('\n');
        Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

        // Header'dan explanation indexini bulma (Garanti çözüm)
        int explanationIndex = -1;
        if (lines.Length > 0)
        {
            string[] headers = CSVParser.Split(lines[0].Trim());
            for(int i = 0; i < headers.Length; i++)
            {
                if (headers[i].ToLower().Contains("explanation"))
                {
                    explanationIndex = i;
                }
            }
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] columns = CSVParser.Split(line);
            
            if (columns.Length < 12) continue;

            string planetCategory = columns[3].Trim('\"');
            string planetType = columns[4].Trim('\"');
            string keywordsRaw = columns[11].Trim('\"');

            if (planetType == "polarizing")
            {
                if (!PolarizingViews.ContainsKey(planetCategory))
                {
                    PolarizingViews.Add(planetCategory, new PoliticalViewData(planetCategory));
                }

                PoliticalViewData viewData = PolarizingViews[planetCategory];
                viewData.TweetCount++;

                // Eğer dataset'e Explanation sütunu eklenmişse oku:
                if (explanationIndex != -1 && columns.Length > explanationIndex)
                {
                    string parsedExplanation = columns[explanationIndex].Trim('\"');
                    if (!string.IsNullOrEmpty(parsedExplanation))
                    {
                        viewData.Explanation = parsedExplanation;
                    }
                }

                // Split keywords and add to list
                string[] kwds = keywordsRaw.Split(',');
                foreach (string kw in kwds)
                {
                    string cleanedKwd = kw.Trim();
                    if (!string.IsNullOrEmpty(cleanedKwd))
                    {
                        viewData.Keywords.Add(cleanedKwd);
                        
                        if (!viewData.KeywordFrequencies.ContainsKey(cleanedKwd))
                        {
                            viewData.KeywordFrequencies[cleanedKwd] = 0;
                        }
                        viewData.KeywordFrequencies[cleanedKwd]++;
                    }
                }
            }
        }
        
        Debug.Log("Data loading complete. Found " + PolarizingViews.Count + " polarizing categories.");
    }
}
