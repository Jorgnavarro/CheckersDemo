using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class MainMenuUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Dropdown gameModeDropdown;
    public GameManager gameManager;
    public GameObject mainMenuUI;
    public GameObject HUD;

    private void Start()
    {
        //config of the dropdown Options
        gameModeDropdown.ClearOptions();
        gameModeDropdown.AddOptions(new List<string>{"Player vs Player", "Player vs AI"});
        
        //Choose the game mode selected
        gameModeDropdown.value = (int)gameManager.currentGameMode;
        gameModeDropdown.onValueChanged.AddListener(SetGameMode);
        HUD.SetActive(false);
    }

    private void SetGameMode(int index)
    {
        gameManager.currentGameMode = (GameMode)index;
        Debug.Log("Game mode selected: " + gameManager.currentGameMode);
    }

    public void StartGame()
    {
        mainMenuUI.SetActive(false);
        HUD.SetActive(true);
        BoardManager.Instance.InitializeGame();
    }
}
