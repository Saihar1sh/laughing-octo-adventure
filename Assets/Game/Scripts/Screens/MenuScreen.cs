using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScreen : ScreenBase
{
    public override ScreenType ScreenType => ScreenType.Menu;

    
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button exitGameButton;

    private void Start()
    {
        startGameButton.onClick.AddListener(() => UIController.Instance.ShowScreen(ScreenType.LevelSelect));
        loadGameButton.onClick.AddListener(() => PersistenceManager.Instance.Load());
        exitGameButton.onClick.AddListener(() => GameManager.Instance.ExitGame());
    }
    
    public override void Show()
    {
        base.Show();
        visualsParent.gameObject.SetActive(true);
        loadGameButton.gameObject.SetActive(PersistenceManager.Instance.LoadExists(out _));
    }

    public override void Hide()
    {
        base.Hide();
        visualsParent.gameObject.SetActive(false);
    }
}
