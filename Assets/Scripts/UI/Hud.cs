using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hud : MonoBehaviour {
  public static Hud instance = null;

  public Text ping;
  public Text health;
  public Text ammunition;

  public Text teamAlphaName;
  public Text teamBravoName;
  public Text teamAlphaPoints;
  public Text teamBravoPoints;

  public Text roundTimer;

  public GameObject hud;
  public GameObject chooseTeam;
  public GameObject menu;

  public Chat chat;
  public Scoreboard scoreboard;
  public DeathHistory deathHistory;

  void Awake() {
    if (instance == null) {
      instance = this;
    } else {
      Debug.LogError("There is already instance of hud on scene!");
      Destroy(gameObject);
    }

    ping = transform.FindChild("Hud/Ping").GetComponent<Text>();
    health = transform.FindChild("Hud/Health").GetComponent<Text>();
    ammunition = transform.Find("Hud/Ammunition").GetComponent<Text>();

    teamAlphaName = transform.FindChild("Hud/TeamA").GetComponent<Text>();
    teamBravoName = transform.FindChild("Hud/TeamB").GetComponent<Text>();

    teamAlphaPoints = teamAlphaName.transform.GetChild(0).GetComponent<Text>();
    teamBravoPoints = teamBravoName.transform.GetChild(0).GetComponent<Text>();

    roundTimer = transform.FindChild("Hud/Timer").GetComponent<Text>();

    hud = transform.FindChild("Hud").gameObject;
    chooseTeam = transform.FindChild("ChooseTeam").gameObject;
    menu = transform.FindChild("Menu").gameObject;

    chat = GetComponentInChildren<Chat>();
    scoreboard = GetComponentInChildren<Scoreboard>();
    deathHistory = GetComponentInChildren<DeathHistory>();
  }

  void Start() {
    
  }

  void Update() {
    ping.text = "Ping: " + PhotonNetwork.GetPing();
  }

  public void ShowScoreboard() {
    scoreboard.ClearScoreboard();
    scoreboard.GenerateScoreboard();
    scoreboard.panel.SetActive(true);

    hud.SetActive(false);
  }

  public void HideScoreboard() {
    scoreboard.panel.SetActive(false);

    hud.SetActive(true);
  }

  public void ShowMenu() {
    menu.SetActive(true);
  }

  public void HideMenu() {
    menu.SetActive(false);
    GameManager.instance.localPlayerController.OnHideMenu();
  }

  // Menu actions
  public void Disconnect() {
    PhotonNetwork.LeaveRoom();
    //GameObject.Find("LevelLoader").GetComponent<LevelLoader>().LoadMap(0);
    //Application.LoadLevel(0);
    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
  }

  public void Exit() {
    Application.Quit();
  }


}
