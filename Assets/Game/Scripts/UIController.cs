using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public GameObject gameOverPanel;

    private Dictionary<ScreenType, ScreenBase> _screensDict;
    private ScreenBase _currentScreen;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        Instance = this;
    }

    private void Start()
    {
        _screensDict = new Dictionary<ScreenType, ScreenBase>();
        foreach (var obj in FindObjectsByType<ScreenBase>(FindObjectsSortMode.None))
        {
            _screensDict.TryAdd(obj.ScreenType, obj);
            obj.Hide();
        }
        
        ShowScreen(ScreenType.Game);
    }

    public void ShowScreen(ScreenType screenType)
    {
        if(_currentScreen!=null) 
            _currentScreen.Hide();
        
        _currentScreen = _screensDict[screenType];
        _currentScreen.Show();
    }
    
    public void UpdateScore(int score)
    {
        if (scoreText) 
            scoreText.text = $"Score: {score}";
    }

    public void ShowCombo(int combo)
    {
        if (comboText == null) return;
        if (combo <= 1)
        {
            comboText.text = ""; 
            return;
        }
        comboText.text = $"Combo x{combo}";
    }

    public void ShowGameOver(int finalScore)
    {
        ShowScreen(ScreenType.GameOver);
        UpdateScore(finalScore);
        //Reset Combo Text
        ShowCombo(0);
    }
}
[System.Serializable]
public enum ScreenType
{
    Menu,
    Game,
    GameOver
}

public class ScreenBase : MonoBehaviour, IScreen
{
    public virtual ScreenType ScreenType { get; }

    public virtual void Show()
    {
        
    }

    public virtual void Hide()
    {
    }
}
public interface IScreen
{
    ScreenType ScreenType { get; }
    void Show();
    void Hide();
}