using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int initialRows = 4;
    public int initialCols = 4;

    private bool init = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    private async void Start()
    {
        await Task.Yield();
        UIController.Instance.ShowScreen(ScreenType.Menu);
    }

    public void StartNewGame(int rows = -1, int cols = -1,int? seed = null, List<CardSaveData> saveData = null)
    {
        if (rows == -1) rows = initialRows;
        if (cols == -1) cols = initialCols;
        
        DeckManager.Instance.StartNewLayout(rows, cols, seed, saveData);
        ScoreManager.Instance.ResetScore();
        
        UIController.Instance.ShowScreen(ScreenType.Game);
        init = true;
    }

    public void OnGameOver()
    {
        UIController.Instance.ShowGameOver(ScoreManager.Instance.Score);
        AudioManager.Instance.PlayGameOver();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        //Handling auto save on first time scenario
        if(!init) return;
        
        if (!hasFocus && UIController.Instance.IsScreenActive(ScreenType.Game)) 
            PersistenceManager.Instance.AutoSave();
    }

    public void ExitGame()
    {
        Debug.Log("quiting game");
        Application.Quit();
    }
}
