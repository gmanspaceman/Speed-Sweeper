using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour
{
    
    public Text titleText;
    public Transform connectStatus;

    public CanvasGroup sceneMask;

    public CanvasGroup MainMenu;

    public CanvasGroup SoloMenu;
    public CanvasGroup SoloNewGameMenu;

    public CanvasGroup MultiplayerMenu;
    public CanvasGroup MultiplayerNewGameMenu;
    public CanvasGroup MultiplayerJoinGameMenu;

    public CanvasGroup OptionsMenu;


    public GameObject username;
    public GameObject enterUsername;

    public ButtonListControl serverListControl;

    public AudioSource backgroundMusic;
    public ParticleSystem fog;
    public GameSettingSliderGroupManager SoloGameSettings;
    public GameSettingSliderGroupManager MultiGameSettings;

    public Networking n;

    // Start is called before the first frame update
    private void Awake()
    {
        //setup start to help with dev

        MainMenu.gameObject.SetActive(true);
        MainMenu.alpha = 0;

        SoloMenu.gameObject.SetActive(false);
        SoloMenu.alpha = 0;

        SoloNewGameMenu.gameObject.SetActive(false);
        SoloNewGameMenu.alpha = 0;

        MultiplayerMenu.gameObject.SetActive(false);
        MultiplayerMenu.alpha = 0;

        MultiplayerNewGameMenu.gameObject.SetActive(false);
        MultiplayerNewGameMenu.alpha = 0;

        MultiplayerJoinGameMenu.gameObject.SetActive(false);
        MultiplayerJoinGameMenu.alpha = 0;

        OptionsMenu.gameObject.SetActive(false);
        OptionsMenu.alpha = 0;
        
        OptionsMenu.GetComponentInChildren<Slider>().value = PlayerPrefs.GetFloat("Volume", 1);
        //SoloGameSettings.SetSLiders((int)PlayerPrefs.GetFloat("Cols", SoloGameSettings.cols.value),
        //                            (int)PlayerPrefs.GetFloat("Rows", SoloGameSettings.rows.value),
        //                            (int)PlayerPrefs.GetFloat("Mines", SoloGameSettings.mines.value));

        //MultiGameSettings.SetSLiders((int)PlayerPrefs.GetFloat("Cols", MultiGameSettings.cols.value),
        //                            (int)PlayerPrefs.GetFloat("Rows", MultiGameSettings.rows.value),
        //                            (int)PlayerPrefs.GetFloat("Mines", MultiGameSettings.mines.value));

        username.GetComponent<TMP_InputField>().text = PlayerPrefs.GetString("Username", "");

        sceneMask.alpha = 1;
        backgroundMusic.volume = 0;

        fog.gameObject.SetActive(true);

        DontDestroyOnLoad(backgroundMusic.transform.parent.gameObject);
        DontDestroyOnLoad(n.transform.parent.gameObject);
        
    }
    private void Update()
    {
        // Make sure user is on Android platform
        if (Application.platform == RuntimePlatform.Android)
        {
            // Check if Back was pressed this frame
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (MainMenu.alpha == 1)
                    TransitionFrom_MainMenu_To_Quit();
                else if (SoloMenu.alpha == 1)
                    TransitionFrom_SoloMenu_To_MainMenu();
                else if (SoloNewGameMenu.alpha == 1)
                    TransitionFrom_SoloNewGameMenu_To_SoloMenu();
                else if (MultiplayerMenu.alpha == 1)
                    TransitionFrom_MultuplayerMenu_To_MainMenu();
                else if (MultiplayerNewGameMenu.alpha == 1)
                    TransitionFrom_MultiplayerNewGameMenu_To_MultiplayerMenu();
                else if (MultiplayerJoinGameMenu.alpha == 1)
                    TransitionFrom_MultiplayerJoinGameMenu_To_MultiplayerMenu();
                else if (OptionsMenu.alpha == 1)
                    TransitionFrom_OptionsMenu_To_MainMenu();
            }
        }
    }
    void Start()
    {
        StartCoroutine(FadeIn(backgroundMusic, PlayerPrefs.GetFloat("Volume", 1)));
        StartCoroutine(FadeOut(sceneMask));
        StartCoroutine(AnimateTitleText("Speed \n\tSweeper"));
    }
    private void OnDestroy()
    {
       //Networking.OnGameList -= GameList;
    }

    //Main Menu Navigation
    public void TransitionFrom_MainMenu_To_SoloMenu()
    {
        PlayerPrefs.SetString("GameMode", "Solo");
        PlayerPrefs.Save();
        StartCoroutine(FadeOutIn(MainMenu, SoloMenu));
    }
    public void TransitionFrom_MainMenu_To_MultiplayerMenu()
    {
        enterUsername.SetActive(false);

        PlayerPrefs.SetString("GameMode", "Multi");
        PlayerPrefs.Save();

        //username.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("Username", "");

        StartCoroutine(FadeOutIn(MainMenu, MultiplayerMenu));
    }
    public void TransitionFrom_MainMenu_To_Options()
    {
        StartCoroutine(FadeOutIn(MainMenu, OptionsMenu));
    }
    public void TransitionFrom_MainMenu_To_Quit()
    {
        Application.Quit();
    }

    //Solo Menu Navigation
    public void TransitionFrom_SoloMenu_To_Continue()
    {
        PlayerPrefs.SetFloat("Rows", PlayerPrefs.GetFloat("LocalSave_Rows"));
        PlayerPrefs.SetFloat("Cols", PlayerPrefs.GetFloat("LocalSave_Cols"));
        PlayerPrefs.SetFloat("Mines", PlayerPrefs.GetFloat("LocalSave_Mines"));
        PlayerPrefs.Save();

        StartCoroutine(FadeInLoad(sceneMask, 1));
    }
    public void TransitionFrom_SoloMenu_To_SoloNewGameMenu()
    {
        StartCoroutine(FadeOutIn(SoloMenu, SoloNewGameMenu));
    }
    public void TransitionFrom_SoloMenu_To_MainMenu()
    {
        PlayerPrefs.DeleteKey("GameMode");
        StartCoroutine(FadeOutIn(SoloMenu, MainMenu));
    }

    //Solo New Game Navigation
    public void TransitionFrom_SoloNewGameMenu_To_Start()
    {
        if (PlayerPrefs.HasKey("LocalSave"))
        {
            PlayerPrefs.DeleteKey("LocalSave");
            PlayerPrefs.DeleteKey("LocalSave_Rows");
            PlayerPrefs.DeleteKey("LocalSave_Cols");
            PlayerPrefs.DeleteKey("LocalSave_Mines");
            PlayerPrefs.Save();
        }
        StartCoroutine(FadeInLoad(sceneMask, 1));
    }
    public void TransitionFrom_SoloNewGameMenu_To_SoloMenu()
    {
        StartCoroutine(FadeOutIn(SoloNewGameMenu, SoloMenu));
    }

    //Multiplayer Menu Navigation
    public void TransitionFrom_MultuplayerMenu_To_MultiplayerNewGameMenu()
    {
        
        if (username.GetComponent<TMP_InputField>().text.Length <= 1)
        {
            enterUsername.SetActive(true);
            return;
        }
        

        PlayerPrefs.SetString("Username", username.GetComponent<TMP_InputField>().text);
        PlayerPrefs.Save();

        connectStatus.GetComponent<TextMeshProUGUI>().color = new Color(connectStatus.GetComponent<TextMeshProUGUI>().color.r, connectStatus.GetComponent<TextMeshProUGUI>().color.g, connectStatus.GetComponent<TextMeshProUGUI>().color.b, 0f);

        StartCoroutine(WaitForServerConnect(MultiplayerMenu, MultiplayerNewGameMenu));
        //StartCoroutine(FadeOutIn(MultiplayerMenu, MultiplayerNewGameMenu));
    }
    public void TransitionFrom_MultuplayerMenu_To_MultiplayerJoinGameMenu()
    {
        PlayerPrefs.DeleteKey("JoinGame");
        PlayerPrefs.Save();

        if (username.GetComponent<TMP_InputField>().text.Length <= 1)
        {
            enterUsername.SetActive(true);
            return;
        }

        PlayerPrefs.SetString("Username", username.GetComponent<TMP_InputField>().text);
        PlayerPrefs.Save();

        connectStatus.GetComponent<TextMeshProUGUI>().color = new Color(connectStatus.GetComponent<TextMeshProUGUI>().color.r, connectStatus.GetComponent<TextMeshProUGUI>().color.g, connectStatus.GetComponent<TextMeshProUGUI>().color.b, 0f);

        ///need to populate the server list
        Networking.OnGameList += GameList;

        

        StartCoroutine(WaitForServerConnect(MultiplayerMenu, MultiplayerJoinGameMenu));
        //StartCoroutine(FadeOutIn(MultiplayerMenu, MultiplayerJoinGameMenu));
    }
    
    
    public void GameList(Dictionary<int, int> gameList)
    {
        serverListControl.RemoveAllServerButton();
        //Display the game list so user can choose what to join
        foreach (KeyValuePair<int, int> k in gameList)
        {
            print("Game " + k.Key + " has " + k.Value + " players.");
            serverListControl.AddServerButton(k.Key.ToString(), k.Value);
        }
    }


    public void TransitionFrom_MultuplayerMenu_To_MainMenu()
    {
        PlayerPrefs.DeleteKey("GameMode");
        PlayerPrefs.Save();
        StartCoroutine(FadeOutIn(MultiplayerMenu, MainMenu));
    }

    //Multiplayer New Game Menu Navigation
    public void TransitionFrom_MultiplayerNewGameMenu_To_Start()
    {
        PlayerPrefs.SetString("GameMode_Multi", "Make");
        PlayerPrefs.Save();
        StartCoroutine(FadeInLoad(sceneMask, 1));
    }
    public void TransitionFrom_MultiplayerNewGameMenu_To_MultiplayerMenu()
    {
        connectStatus.GetComponent<TextMeshProUGUI>().color = new Color(connectStatus.GetComponent<TextMeshProUGUI>().color.r, connectStatus.GetComponent<TextMeshProUGUI>().color.g, connectStatus.GetComponent<TextMeshProUGUI>().color.b, 0f);

        //username.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("Username", "");

        enterUsername.SetActive(false);
        StartCoroutine(FadeOutIn(MultiplayerNewGameMenu, MultiplayerMenu));
    }

    //Multiplayer Join Game Menu Navigation
    public void TransitionFrom_MultiplayerJoinGameMenu_To_Start()
    {
        Networking.OnGameList -= GameList;

        if (!PlayerPrefs.HasKey("JoinGame"))
        {
            print("No Join Game Id");
            return;
        }

        PlayerPrefs.SetString("GameMode_Multi", "Join");
        PlayerPrefs.Save();
        StartCoroutine(FadeInLoad(sceneMask, 1));
    }
    public void TransitionFrom_MultiplayerJoinGameMenu_To_MultiplayerMenu()
    {
        Networking.OnGameList -= GameList;

        connectStatus.GetComponent<TextMeshProUGUI>().color = new Color(connectStatus.GetComponent<TextMeshProUGUI>().color.r, connectStatus.GetComponent<TextMeshProUGUI>().color.g, connectStatus.GetComponent<TextMeshProUGUI>().color.b, 0f);

        //username.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("Username", "");

        enterUsername.SetActive(false);
        StartCoroutine(FadeOutIn(MultiplayerJoinGameMenu, MultiplayerMenu));
    }

    //Options Menu Navigation
    public void TransitionFrom_OptionsMenu_To_MainMenu()
    {
        StartCoroutine(FadeOutIn(OptionsMenu, MainMenu));
    }

   IEnumerator WaitForServerConnect(CanvasGroup from, CanvasGroup to)
    {
        n.MultiStart();
        
        //flash connecting on screen
        TextMeshProUGUI i = connectStatus.GetComponent<TextMeshProUGUI>();
        i.text = "CONNECTING";
        do
        {

            i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
            while (i.color.a < 1.0f)
            {
                i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / 1f));
                yield return null;
            }

            i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
            while (i.color.a > 0.0f)
            {
                i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / 1f));
                yield return null;
            }

        } while (!n.isConnected);

        i.color = new Color(i.color.r, i.color.g, i.color.b, 1f);
        i.text = "CONNECTED!";

        yield return null;
        //display connected or no server found

        n.ServerSend_WHOAMI();
        n.ServerSend_IAM(PlayerPrefs.GetString("Username", "NaN"));
        n.ServerSend_GetGameList();

        StartCoroutine(FadeOutIn(from, to));
    }

    public void GameModeChange(int modeKey)
    {
        SoloGameSettings.SetSlidersInteractible(false);
        MultiGameSettings.SetSlidersInteractible(false);

        switch (modeKey)
        {
            case 0:
                SoloGameSettings.SetSLiders(6,10,1);
                MultiGameSettings.SetSLiders(6,10,1);
                break;
            case 1:
                SoloGameSettings.SetSLiders(8,13, 20);
                MultiGameSettings.SetSLiders(8,13, 20);
                break;
            case 2:
                SoloGameSettings.SetSLiders(10,16, 40);
                MultiGameSettings.SetSLiders(10,16, 40);
                break;
            case 3:
                SoloGameSettings.SetSlidersInteractible(true);
                MultiGameSettings.SetSlidersInteractible(true);
                break;
            default:
                break;
        }
    }

    public void SetGameAudio(float vol)
    {
        backgroundMusic.volume = vol;
        PlayerPrefs.SetFloat("Volume", vol);
        PlayerPrefs.Save();
    }
    public void SetRows(float rows)
    {
        SoloGameSettings.rowsLabel.GetComponent<TextMeshProUGUI>().text = "ROWS: " + rows;
        MultiGameSettings.rowsLabel.GetComponent<TextMeshProUGUI>().text = "ROWS: " + rows;
        PlayerPrefs.SetFloat("Rows", rows);
        PlayerPrefs.Save();
    }
    public void SetCols(float cols)
    {
        SoloGameSettings.colsLabel.GetComponent<TextMeshProUGUI>().text = "COLUMNS: " + cols;
        MultiGameSettings.colsLabel.GetComponent<TextMeshProUGUI>().text = "COLUMNS: " + cols;
        PlayerPrefs.SetFloat("Cols", cols);
        PlayerPrefs.Save();
    }
    public void SetMines(float mines)
    {
        SoloGameSettings.minesLabel.GetComponent<TextMeshProUGUI>().text = "MINES: " + mines;
        MultiGameSettings.minesLabel.GetComponent<TextMeshProUGUI>().text = "MINES: " + mines;
        PlayerPrefs.SetFloat("Mines", mines);
        PlayerPrefs.Save();
    }
    IEnumerator FadeInLoad(CanvasGroup g, int SceneId)
    {
        g.gameObject.SetActive(true);
        while (g.alpha < 1)
        {
            float val = g.alpha;
            val += 0.05f;
            g.alpha = val;
            yield return null;
        }
        SceneManager.LoadScene(SceneId);
    }    
    IEnumerator AnimateTitleText(string name)
    {
        string textToPrintSoFar = string.Empty;

        for (int ii = 0; ii < name.Length; ii++)
        {
            textToPrintSoFar += name[ii].ToString().ToUpper();
            titleText.text = textToPrintSoFar;
            yield return new WaitForSeconds(0.05f);
        }
        StartCoroutine(FadeIn(MainMenu));
    }
    IEnumerator FadeIn(CanvasGroup g)
    {
        g.gameObject.SetActive(true);
        while (g.alpha < 1)
        {
            float val = g.alpha;
            val += 0.05f;
            g.alpha = val;
            yield return null;
        }
    }
    IEnumerator FadeOut(CanvasGroup g)
    {
        while (g.alpha > 0)
        {
            float val = g.alpha;
            val -= 0.05f;
            g.alpha = val;
            yield return null;
        }
        g.gameObject.SetActive(false);
    }
    IEnumerator FadeIn(AudioSource a, float vol)
    {
        while (a.volume < vol)
        {
            float val = a.volume;
            val += 0.05f;
            a.volume = val;
            yield return null;
        }
    }
    IEnumerator FadeOutIn(CanvasGroup outg, CanvasGroup ing)
    {
        while (outg.alpha > 0)
        {
            float val = outg.alpha;
            val -= 0.05f;
            outg.alpha = val;
            yield return null;
        }
        outg.gameObject.SetActive(false);
        ing.gameObject.SetActive(true);
        while (ing.alpha < 1)
        {
            float val = ing.alpha;
            val += 0.05f;
            ing.alpha = val;
            yield return null;
        }
        
    }
}
