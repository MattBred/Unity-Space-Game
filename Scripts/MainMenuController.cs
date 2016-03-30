using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {
    private static GameObject mainController;

    public GameObject managerObj;
    private NetworkManager manager;

    private int mainMenuSceneIndex = 0;
    private int gameSceneIndex = 1;

    private Button buttonSP;
    private Button buttonHost;
    private Button buttonJoin;
    private InputField inputAddress;

    private bool showEscapeMenu = false;

    void Awake()
    {
        if (mainController == null)
        {
            mainController = gameObject;
            DontDestroyOnLoad(transform.gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

	void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == gameSceneIndex && Input.GetKeyDown(KeyCode.Escape)) showEscapeMenu = !showEscapeMenu;
    }

    void Start()
    {
        manager = managerObj.GetComponent<NetworkManager>();
        loadMainScreen();   
    }

    void OnLevelWasLoaded()
    {
        if (SceneManager.GetActiveScene().buildIndex == mainMenuSceneIndex)
        {
            loadMainScreen();
        }
    }

    private void loadMainScreen()
    {
        buttonSP = GameObject.Find("ButtonSP").GetComponent<Button>();
            buttonSP.onClick.AddListener(() => singlePlayer());
        buttonHost = GameObject.Find("ButtonHost").GetComponent<Button>();
            buttonHost.onClick.AddListener(() => host());
        buttonJoin = GameObject.Find("ButtonJoin").GetComponent<Button>();
            buttonJoin.onClick.AddListener(() => join());
        inputAddress = GameObject.Find("InputField").GetComponent<InputField>();
            inputAddress.text = manager.networkAddress;
    }

    void OnGUI()
    {
        if (showEscapeMenu)
        {
            if (NetworkClient.active || NetworkServer.active)
            {
                if (GUI.Button(new Rect(Screen.width/2 - 75,Screen.height/2,150,30), "Leave MP Game"))
                {
                    manager.StopHost();
                    showEscapeMenu = false;
                }
            } else
            {
                if (GUI.Button(new Rect(Screen.width / 2 - 75, Screen.height / 2, 150, 30), "Main Menu"))
                {
                    SceneManager.LoadScene(mainMenuSceneIndex);
                    showEscapeMenu = false;
                }
            }
        }
    }
	
    private void singlePlayer()
    {
        if (NetworkClient.active || NetworkServer.active) manager.StopHost();
        SceneManager.LoadScene(gameSceneIndex);
    }

    private void host()
    {
        if (NetworkClient.active || NetworkServer.active) manager.StopHost();
        manager.StartHost();
    }

    private void join()
    {
        if (NetworkClient.active || NetworkServer.active) manager.StopHost();
        manager.networkAddress = inputAddress.text;
        manager.StartClient();
    }
}
