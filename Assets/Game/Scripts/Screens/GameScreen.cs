using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScreen : ScreenBase
{
    public override ScreenType ScreenType => ScreenType.Game;

    [SerializeField] private Transform visualsParent;

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
