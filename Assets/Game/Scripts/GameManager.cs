using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int initialRows = 4;
    public int initialCols = 4;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    void Start()
    {
        StartNewGame(initialRows, initialCols);
    }

    public void StartNewGame(int rows, int cols)
    {
        DeckManager.Instance.StartNewLayout(rows, cols);
        ScoreManager.Instance.ResetScore();
    }

    public void OnGameOver()
    {
        UIController.Instance.ShowGameOver(ScoreManager.Instance.Score);
        AudioManager.Instance.PlayGameOver();
        PersistenceManager.Instance.AutoSave();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) 
            PersistenceManager.Instance.AutoSave();
    }
}
