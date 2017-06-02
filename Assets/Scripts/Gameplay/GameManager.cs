using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum Gamemode {
  TeamDeathmatch = 0,
  Deathmatch = 1,
  CaptureTheFlag = 2
}

public class GameManager : Photon.PunBehaviour {
  public static GameManager instance;
  public GameObject playerPrefab;

  public Hud hud;
  public PlayerController localPlayerController;

  List<GamemodeBase> gamemodes = new List<GamemodeBase>();
  int currentGamemode = 0;

  List<Transform> alphaTeamSpawnPoints = new List<Transform>();
  List<Transform> bravoTeamSpawnPoints = new List<Transform>();

  void Start() {
    if (instance == null) {
      instance = this;
    } else {
      Debug.LogError("There was already GameManager on scene");
      Destroy(instance.gameObject);
      instance = this;
      return;
    }
    //DontDestroyOnLoad(this);

    Random.InitState((int)System.DateTime.Now.Ticks);
    FindMapSpawnPoints();
    hud = Hud.instance;

    // Photon stuff
    if (PhotonNetwork.connectionState == ConnectionState.Disconnected) {
      PhotonNetwork.offlineMode = true;
    }

    // reset nickname if needed
    if (PhotonNetwork.player.NickName == "") {
      string newNickname = PlayerPrefs.GetString("Nickname");
      if (newNickname == "") {
        newNickname = "Player";
      }

      PhotonNetwork.player.NickName = newNickname;
    }

    // setup game properties if we are in room
    if (PhotonNetwork.room != null) {
      Hashtable properties = new Hashtable();
      properties.Add("StartTime", (int)PhotonNetwork.time);
      //properties.Add("RoundTime", (60 * 5));
      properties.Add("TeamAlphaPoints", 0);
      properties.Add("TeamBravoPoints", 0);
      properties.Add("TeamAlphaName", "Alpha");
      properties.Add("TeamBravoName", "Bravo");
    
      PhotonNetwork.room.SetCustomProperties(properties);
    }

    if (PhotonNetwork.offlineMode) {
      SpawnPlayer();
      hud.chooseTeam.SetActive(false);
      hud.hud.SetActive(true);
      PhotonNetwork.player.SetTeam(PunTeams.Team.Alpha);
    } else {
      hud.chooseTeam.SetActive(true);
      hud.hud.SetActive(false);
      hud.chooseTeam.transform.FindChild("TeamAlpha/JoinTeamAlpha").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { OnPlayerChoosedTeam(PunTeams.Team.Alpha); });
      hud.chooseTeam.transform.FindChild("TeamBravo/JoinTeamBravo").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { OnPlayerChoosedTeam(PunTeams.Team.Bravo); });
    }
    
    // TODO: automatically get gamemodes from this game object
    gamemodes.Add(GetComponent<GamemodeTeamDeathmatch>());

    // setup gamemode
    SetGamemode(Gamemode.TeamDeathmatch);
  }

  void Update() {
    if (PhotonNetwork.offlineMode)
      return;

    if (PhotonNetwork.isMasterClient) {
      int timeLeft = GetTimeLeft();
      if (timeLeft != -1) {
        if (timeLeft <= 0) {
          photonView.RPC("MatchOverRPC", PhotonTargets.All);
        }
      }
    }
  }

  public void SetGamemode(Gamemode gm) {
    currentGamemode = (int)gm;
    gamemodes[currentGamemode].OnSetup(this);
  }
  
  public void OnDeath(int killerID, int victimID) {
    gamemodes[currentGamemode].OnPlayerDeath(victimID, killerID);
  }

  public void IncreaseTeamPoints(PunTeams.Team team, int amt) {
    if (PhotonNetwork.isMasterClient) {
      string key = "TeamAlphaPoints";

      if (team == PunTeams.Team.Bravo) {
        key = "TeamBravoPoints";
      }

      object currentPoints = 0;

      if (PhotonNetwork.room.CustomProperties.TryGetValue(key, out currentPoints)) {
        Hashtable newProperties = new Hashtable();
        newProperties.Add(key, (int)currentPoints + amt);
        PhotonNetwork.room.SetCustomProperties(newProperties);
      } else {
        Debug.LogError("Couldn't find property \"" + key + "\" in custom room properties!");
      }
    }
  }

  public Transform GetRandomSpawnPoint(PunTeams.Team team) {
    if (team == PunTeams.Team.Alpha) {
      return alphaTeamSpawnPoints[Random.Range(0, alphaTeamSpawnPoints.Count)];
    } else if (team == PunTeams.Team.Bravo) {
      return bravoTeamSpawnPoints[Random.Range(0, bravoTeamSpawnPoints.Count)];
    } else if (team == PunTeams.Team.None) {
      Debug.Log("Wrong team!");
    }

    return null;
  }

  void FindMapSpawnPoints() {
    alphaTeamSpawnPoints.Clear();
    bravoTeamSpawnPoints.Clear();

    Transform spawnPoints = GameObject.Find("MapSpawnPoints").transform;
    Transform alphaTeamSpawnPointsParent = spawnPoints.FindChild("AlphaTeam");
    Transform bravoTeamSpawnPointsParent = spawnPoints.FindChild("BravoTeam"); 

    foreach (Transform child in alphaTeamSpawnPointsParent) {
      alphaTeamSpawnPoints.Add(child);
    }

    foreach (Transform child in bravoTeamSpawnPointsParent) {
      bravoTeamSpawnPoints.Add(child);
    }
  }

  void SpawnPlayer() {
    if (PhotonNetwork.offlineMode) {
      Instantiate(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity);
    } else {
      PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0, 3, 0), Quaternion.identity, 0);
    }
  }

  void OnPlayerChoosedTeam(PunTeams.Team team) {
    if (team == PunTeams.Team.None) {
      Debug.LogError("OnPlayerChoosedTeam: Wrong team.");
      return;
    }

    hud.chooseTeam.SetActive(false);
    hud.hud.SetActive(true);
    PhotonNetwork.player.SetTeam(team);
    SpawnPlayer();
  }

  public bool isMatchOverCalled = false;
  [PunRPC]
  public void MatchOverRPC() {
    if (isMatchOverCalled)
      return;
    isMatchOverCalled = true;

    if (localPlayerController)
      localPlayerController.GetComponent<FPSController>().enabled = false;


    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
    hud.ShowScoreboard();
    hud.scoreboard.winReason.text = gamemodes[currentGamemode].GetWinReasonText();

    gamemodes[currentGamemode].enabled = false;
    StartCoroutine(LoadNextMapDelay(15));
  }

  IEnumerator LoadNextMapDelay(int delay) {
    for (int i = 0; i < delay; i++) {
      hud.scoreboard.gamemodeName.text = (delay - i) + "s";
      yield return new WaitForSeconds(1.0f);
    }

    Debug.Log("Loading next map!");
    GameObject.Find("LevelLoader").GetComponent<LevelLoader>().LoadMap(1);
  }

  public int GetTimeLeft() {
    object startTime = 0;
    object roundTime = 0;

    if (PhotonNetwork.room.CustomProperties.TryGetValue("StartTime", out startTime) &&
        PhotonNetwork.room.CustomProperties.TryGetValue("RoundTime", out roundTime))
      return (int)startTime + (int)roundTime - (int)PhotonNetwork.time;

    return -1;
  }

  public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
  }
}
