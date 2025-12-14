using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameoverScreen : ScreenBase
{
    public override ScreenType ScreenType => ScreenType.GameOver;

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
