using System.IO;
using UnityEngine;

public class PersistenceManager : MonoBehaviour
{
    public static PersistenceManager Instance { get; private set; }

    string saveFileName = "cardmatch_save.json";

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    public void AutoSave()
    {
        var deck = DeckManager.Instance.CaptureDeckState();
        var wrapper = new SaveWrapper { score = ScoreManager.Instance.Score, deck = deck };
        string json = JsonUtility.ToJson(wrapper, true);
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        try
        {
            File.WriteAllText(path, json);
            Debug.Log($"Saved to {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Save failed: " + e.Message);
        }
    }

    public void Load()
    {
        if (!LoadExists(out var path)) return;
        try
        {
            string json = File.ReadAllText(path);
            var wrapper = JsonUtility.FromJson<SaveWrapper>(json);
            
            if(wrapper.deck.cards == null || wrapper.deck.cards.Count == 0) return;
            
            GameManager.Instance.StartNewGame(wrapper.deck.rows, wrapper.deck.cols, wrapper.deck.seed, wrapper.deck.cards);
            
            ScoreManager.Instance.ResetScore();
            // restore score
            while (ScoreManager.Instance.Score < wrapper.score) 
                ScoreManager.Instance.OnMatch(); 
            

            Debug.Log("Loaded save");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Load failed: " + e.Message);
        }
    }

    public bool LoadExists(out string path)
    {
        path = Path.Combine(Application.persistentDataPath, saveFileName);
        if (!File.Exists(path))
        {
            Debug.Log("No save file");
            return false;
        }
        return true;
    }

    [System.Serializable]
    class SaveWrapper 
    { 
        public int score; 
        public DeckSaveData deck; 
    }
}
