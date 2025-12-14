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
        StartNewGame();
    }

    public void StartNewGame(int rows = -1, int cols = -1)
    {
        if (rows == -1) rows = initialRows;
        if (cols == -1) cols = initialCols;
        
        DeckManager.Instance.StartNewLayout(rows, cols);
        ScoreManager.Instance.ResetScore();
        
        UIController.Instance.ShowScreen(ScreenType.Game);
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

    public void ExitGame()
    {
        Debug.Log("quiting game");
        Application.Quit();
    }
}
