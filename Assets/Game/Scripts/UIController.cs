using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public GameObject gameOverPanel;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        Instance = this;
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
        if (gameOverPanel) 
            gameOverPanel.SetActive(true);
        UpdateScore(finalScore);
    }
}
