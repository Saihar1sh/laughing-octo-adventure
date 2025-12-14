using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score { get; private set; }
    public int comboCount = 0;
    public float comboWindow = 2.0f; // seconds
    float lastMatchTime = -10f;

    public int matchPoints = 100;
    public int mismatchPenalty = 25;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    public void ResetScore()
    {
        Score = 0; comboCount = 0; lastMatchTime = -10f;
        UIController.Instance.UpdateScore(Score);
    }

    public void OnMatch()
    {
        float t = Time.time;
        
        if (t - lastMatchTime <= comboWindow) 
            comboCount++; 
        else 
            comboCount = 1;
        
        lastMatchTime = t;

        int added = matchPoints * comboCount;
        Score += added;
        UIController.Instance.UpdateScore(Score);
        UIController.Instance.ShowCombo(comboCount);
    }

    public void OnMismatch()
    {
        comboCount = 0;
        Score = Mathf.Max(0, Score - mismatchPenalty);
        UIController.Instance.UpdateScore(Score);
    }
}