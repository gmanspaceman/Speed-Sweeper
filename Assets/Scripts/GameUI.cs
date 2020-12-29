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

    public Slider volSlider;

    public GameObject gameLose;
    public GameObject gameWin;

    public Animator menuAnimator;
    public GameObject menuTint;

    void Start()
    {
        UpdateScore(0f);
        UpdateMines(99);



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
        score.text = "Score: " + f.ToString("F1");
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
