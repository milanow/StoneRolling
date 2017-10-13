/* Author: Tianhe Wang
 * Date: 10/13/2017
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public GameObject GameStartPanel;
    public GameObject GameEndPanel;
    public GameObject ButtonsPanel;
    public GameObject GameLevelsPanel;

    private void Awake()
    {
        GameManager.Instance.OnGameEnd += OnGameOver_UIManager;
    }

    private void OnGameOver_UIManager(GameManager sender)
    {
        GameEndPanel.SetActive(true);
    }

    public void OnGameStart_ButtonClicked()
    {
        GameManager.Instance.PauseGame = false;
        GameStartPanel.SetActive(false);
    }

    public void OnGameLevelSelect_ButtonClicked()
    {
        ButtonsPanel.SetActive(false);
        GameLevelsPanel.SetActive(true);
    }

    public void OnGameLevelSelectQuit_ButtonClicked()
    {
        ButtonsPanel.SetActive(true);
        GameLevelsPanel.SetActive(false);
    }

    public void OnGameRestart_ButtonClicked()
    {
        GameEndPanel.SetActive(false);
        GameStartPanel.GetComponent<GameStartPanelScript>().Reset();
        GameStartPanel.SetActive(true);
        GameManager.Instance.ResetGame();
    }
}
