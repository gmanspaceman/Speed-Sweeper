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

    public GameObject audioMuted;
    public GameObject audioUnMuted;

    public Animator menuAnimator;
    public Animator TurnListAnimator;
    public Animator HelpMenuAnimator;

    public GameObject menuTint;
    public GameObject skipButton;

    public GameInfoManager gameInfoManager;
    public TurnListManager turnListManager;

    public Button HUDbutton;

    private void Awake()
    {
        Networking.OnTurnList += PopulateTurnList;
        skipButton.SetActive(false);
        SetTurnBanner();
    }

    void Start()
    {
        UpdateScore(0f);
        UpdateMines(99);
    }
    private void Update()
    {
        if (PlayerPrefs.GetString("GameMode") == "Multi")
        {
            if(!skipButton.activeSelf)
                skipButton.SetActive(true);
            //print("here1");
        }
        else
        {
            if (skipButton.activeSelf)
                skipButton.SetActive(false);
            //print("here2");
        }

        if (isMenuOpen())
        {
            if (skipButton.activeSelf)
                skipButton.SetActive(false);
            //print("here3");

            if (Input.GetMouseButtonUp(0))
            {
                if (HelpMenuAnimator.GetCurrentAnimatorStateInfo(0).IsName("Open"))
                {
                    HelpMenuAnimator.SetBool("open", false);
                    return;
                }
                
                if (TurnListAnimator.GetBool("open"))
                {
                    TurnListAnimator.SetBool("open", false);
                    HUDbutton.interactable = true;
                    return;
                }

            }
        }

        

    }
    public void WhosTurn(bool myTurn)
    {
         YourTurn.SetActive(myTurn);
         TheirTurn.SetActive(!myTurn);
    }
    public void AnimatedTurnList()
    {
        ////if the help menu is open, jsut close that and move on
        //if (HelpMenuAnimator.GetBool("open"))
        //{
        //    HelpMenuAnimator.SetBool("open", false);
        //    return;
        //}

        bool isOpen = TurnListAnimator.GetBool("open");

        //if(!isOpen)
        //    skipButton.SetActive(false);

        HUDbutton.interactable = isOpen; //let the click in the update on boardgen handle close

        string gameMode = PlayerPrefs.GetString("GameMode");
        //print(isOpen);
        //print(gameMode);
        if (!isOpen && gameMode == "Multi")
        {
            //we are about to open the menu, ask the server for the info and populate it
            Networking.ServerSend_TurnList();

            
        }

        //print(PlayerPrefs.GetString("GameMode"));
        TurnListAnimator.SetBool("multiplayer", gameMode == "Multi");
        
        TurnListAnimator.SetBool("open", !isOpen);

    }
    public void PopulateTurnList(string s)
    {
        turnListManager.ClearnTurnList();
        string[] names = s.Split(',');
        for (int ii = 1; ii < names.Length; ii++)
        {
            turnListManager.AddPlayerToTurnList(names[ii]);
        }
    }
    public void AppExit()
    {
        Application.Quit();
    }
    public void AppHelp()
    {
        bool isOpen = HelpMenuAnimator.GetBool("open");

        HelpMenuAnimator.SetBool("open", !isOpen);
    }
    public void AppHome()
    {
        GameObject[] s = GameObject.FindGameObjectsWithTag("DDOL");
        foreach (GameObject g in s)
        {
            Destroy(g);
        }
        SceneManager.LoadScene(0);
    }
    public void AppToggleAudio(bool muteAudio)
    {
        
        audioMuted.SetActive(muteAudio);
        audioUnMuted.SetActive(!muteAudio);

        AudioBackgroundManager.instance.MuteBackgroundAudio(muteAudio);     
    }
    public void SkipTurn()
    {
        Networking.ServerSend_Pass();
    }
    public bool isMenuOpen()
    {
        //this can be used for any menu to block clicking tiles
        return !TurnListAnimator.GetCurrentAnimatorStateInfo(0).IsName("Empty") ||
                !HelpMenuAnimator.GetCurrentAnimatorStateInfo(0).IsName("Empty");
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
