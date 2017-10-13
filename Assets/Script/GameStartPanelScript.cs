using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartPanelScript : MonoBehaviour {

    public GameObject GameButtonsPanel;
    public GameObject GameLevelsPanel;

    public void Reset()
    {
        GameLevelsPanel.SetActive(false);
        GameButtonsPanel.SetActive(true);
    }
}
