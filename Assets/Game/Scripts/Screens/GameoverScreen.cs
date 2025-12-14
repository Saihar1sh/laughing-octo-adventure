using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameoverScreen : ScreenBase
{
    public override ScreenType ScreenType => ScreenType.GameOver;

    [SerializeField] private TextMeshProUGUI gameoverText;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button exitButton;


    private void Awake()
    {
        newGameButton.onClick.AddListener(() => GameManager.Instance.StartNewGame());
        exitButton.onClick.AddListener(() => UIController.Instance.ShowScreen(ScreenType.Menu));
    }

    public override void Show()
    {
        base.Show();
        visualsParent.gameObject.SetActive(true);
        gameoverText.text = $"Game Over\nScore: {ScoreManager.Instance.Score}";
    }

    public override void Hide()
    {
        base.Hide();
        visualsParent.gameObject.SetActive(false);
    }
}
