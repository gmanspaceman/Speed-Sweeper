using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Text score;
    public Text mines;

    public GameObject YourTurn;
    public GameObject TheirTurn;

    public Text TurnBanner;

    public Slider volSlider;

    public GameObject gameLose;
    public GameObject gameWin;

    public Animator menuAnimator;
    public GameObject menuTint;

    public GameInfoManager gameInfoManager;

    void Start()
    {
        UpdateScore(0f);
        UpdateMines(99);
    }
    public void WhosTurn(bool myTurn)
    {
         YourTurn.SetActive(myTurn);
         TheirTurn.SetActive(!myTurn);
    }
    public void SetTurnBanner(string s = "")
    {
        if(s == "")
            TurnBanner.text = "";
        else
            TurnBanner.text = "Turn: " + s;
    }
    public void toggleMenu()
    {
        if (menuAnimator != null)
        {
            bool isOpen = menuAnimator.GetBool("Open");
            menuAnimator.SetBool("Open", !isOpen);
            menuTint.SetActive(!isOpen);
        }
    }
    public void UpdateScore(float f)
    {
        score.text = "Time: " + Mathf.Clamp(999f-f, 0f, 999f).ToString("F0");
    }
    public void UpdateMines(int i)
    {
        mines.text = "Mines: " + i.ToString();
    }
    public void ShowHideGameEnd(GameState.GamePhase g)
    {
        switch (g)
        {
            case GameState.GamePhase.Win:
                gameWin.SetActive(true);
                gameLose.SetActive(false);
                break;
            case GameState.GamePhase.Lose:
                gameWin.SetActive(false);
                gameLose.SetActive(true);
                break;
            case GameState.GamePhase.PreGame:
            case GameState.GamePhase.Playing:
                gameWin.SetActive(false);
                gameLose.SetActive(false);                           
                break;
            default:
                break;
        }
    }
}
