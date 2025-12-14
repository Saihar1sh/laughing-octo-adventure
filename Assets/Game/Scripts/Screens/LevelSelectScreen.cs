using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectScreen : ScreenBase
{
    public override ScreenType ScreenType => ScreenType.LevelSelect;
    

    [SerializeField] private Button button3x3;
    [SerializeField] private Button button4x4;
    [SerializeField] private Button button5x5;
    [SerializeField] private Button button5x6;
    
    [SerializeField] private Button customLayoutButton;
    [SerializeField] private TMP_InputField rowsInput;
    [SerializeField] private TMP_InputField colsInput;

    [SerializeField] private Button backButton;


    private void Awake()
    {
        button3x3.onClick.AddListener(() => StartNewGame(3, 3));
        button4x4.onClick.AddListener(() => StartNewGame(4, 4));
        button5x5.onClick.AddListener(() => StartNewGame(5, 5));
        button5x6.onClick.AddListener(() => StartNewGame(5, 6));
        customLayoutButton.onClick.AddListener(() => StartNewGame(int.Parse(rowsInput.text), int.Parse(colsInput.text)));
        backButton.onClick.AddListener(() => UIController.Instance.ShowScreen(ScreenType.Menu));
    }

    private void StartNewGame(int rows, int cols)
    {
        GameManager.Instance.StartNewGame(rows,cols);
    }


    public override void Show()
    {
        base.Show();
        visualsParent.gameObject.SetActive(true);
    }

    public override void Hide()
    {
        base.Hide();
        visualsParent.gameObject.SetActive(false);
    }
}
