using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MainMenuController : Photon.PunBehaviour {
  public GameObject mainMenu;
  public GameObject connecting;
  public GameObject setupNickname;

  public GameObject roomListItemPrefab;
  public Transform roomListParent;

#region CreateRoom
  public Dropdown gamemode;
  public Dropdown map;
  public Slider maxPlayersSlider;
  public Text maxPlayerCurrentValue;
  public Slider timeLimitSlider;
  public Text timeLimitCurrentValue;
  public InputField roomName;
#endregion

  void Start() {
    Goto("Main");
    RefreshRoomList();

    if (!PhotonNetwork.connected) {
      PhotonNetwork.autoJoinLobby = false;
      PhotonNetwork.automaticallySyncScene = true;
      PhotonNetwork.ConnectUsingSettings(GameDatabase.version);

      PhotonNetwork.sendRate = 30;
      PhotonNetwork.sendRateOnSerialize = 20;

      mainMenu.SetActive(false);
      connecting.SetActive(true);
      setupNickname.SetActive(false);
    }
  }

  void Update() {
    maxPlayerCurrentValue.text = ((int)maxPlayersSlider.value).ToString();
    timeLimitCurrentValue.text = ((int)timeLimitSlider.value).ToString();
  }

  void HideAll() {
    foreach (Transform child in transform) {
      transform.gameObject.SetActive(false);
    }
  }

  public void OnSetupNickname(string nickname) {
    Debug.Log("settip up nickname: " + nickname);
    setupNickname.SetActive(false);

    PhotonNetwork.player.NickName = nickname;
    PlayerPrefs.SetString("Nickname", nickname);
  }

  public void Goto(string where) {
    if (where == "Exit") {
      Application.Quit();
    } else {
      foreach (Transform child in transform.FindChild("MainMenu")) {
        if (child.name == where) {
          child.gameObject.SetActive(true);
        } else {
          child.gameObject.SetActive(false);
        }
      }
    }
  }

  public void RefreshRoomList() {
    foreach (Transform child in roomListParent) {
      Destroy(child.gameObject);
    }

    int roomCount = 0;
    foreach (RoomInfo roomInfo in PhotonNetwork.GetRoomList()) {
      GameObject item = Instantiate(roomListItemPrefab);

      item.transform.SetParent(roomListParent);
      item.transform.FindChild("RoomName").GetComponent<Text>().text = roomInfo.Name;
      item.transform.FindChild("NumberOfPlayers").GetComponent<Text>().text = roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;

      object gamemodeID = 0;
      object mapID = 0;

      if (roomInfo.CustomProperties.TryGetValue("Gamemode", out mapID)) {
        item.transform.FindChild("Gamemode").GetComponent<Text>().text = GameDatabase.gamemodes[(int)gamemodeID];
      }

      if (roomInfo.CustomProperties.TryGetValue("Map", out mapID)) {
        item.transform.FindChild("Map").GetComponent<Text>().text = GameDatabase.maps[(int)mapID];
      }

      RectTransform trans = item.GetComponent<RectTransform>();
      trans.offsetMin = new Vector2(15, trans.offsetMin.y);
      trans.offsetMax = new Vector2(15, trans.offsetMax.y);
      trans.anchoredPosition = new Vector2(0, -24 + (48 * roomCount));     

      item.GetComponent<Button>().onClick.AddListener(delegate { JoinRoom(roomInfo.Name); });

      ++roomCount;
    }
  }

  public void CreateRoom() {
    RoomOptions roomOptions = new RoomOptions();
    roomOptions.MaxPlayers = (byte)maxPlayersSlider.value;

    Hashtable properties = new Hashtable();
    properties.Add("Gamemode", gamemode.value);
    properties.Add("Map", map.value);
    properties.Add("RoundTime", 60 * (int)timeLimitSlider.value);

    roomOptions.CustomRoomProperties = properties;
    roomOptions.CustomRoomPropertiesForLobby = new string[] { "Gamemode", "Map" };

    if (!PhotonNetwork.JoinOrCreateRoom(roomName.text, roomOptions, TypedLobby.Default)) {
      Debug.LogError("There was error while trying create room.");
    }
  }

  void JoinRoom(string roomName) {
    PhotonNetwork.JoinRoom(roomName);
  }

  // PUN's callbacks
  public override void OnCreatedRoom() {
    base.OnCreatedRoom();
    Debug.Log("OnCreatedRoom");
  }

  public override void OnJoinedRoom() {
    base.OnJoinedRoom();
    Debug.Log("OnJoinedRoom");

    object map_index = 0;
    if (PhotonNetwork.room != null &&
        PhotonNetwork.room.CustomProperties.TryGetValue("Map", out map_index)) {
      GameObject.Find("LevelLoader").GetComponent<LevelLoader>().LoadMap((int)map_index + 1);
    }
  }

  public override void OnJoinedLobby() {
    base.OnJoinedRoom();
    Debug.Log("OnJoinedLobby");
  }

  public override void OnConnectedToMaster() {
    base.OnConnectedToMaster();
    Debug.Log("OnConnectedToMaster");
    Debug.Log("Region: " + PhotonNetwork.networkingPeer.CloudRegion);

    PhotonNetwork.JoinLobby();

    mainMenu.SetActive(true);
    connecting.SetActive(false);

    if (PlayerPrefs.GetString("Nickname") != null && PlayerPrefs.GetString("Nickname") != "") {
      Debug.Log(PlayerPrefs.GetString("Nickname"));
      PhotonNetwork.player.NickName = PlayerPrefs.GetString("Nickname");
    } else {
      setupNickname.SetActive(true);
    }
  }

  public override void OnFailedToConnectToPhoton(DisconnectCause cause) {
    base.OnFailedToConnectToPhoton(cause);
    Debug.LogError("OnFailedToConnectToPhoton");
  }

  public override void OnReceivedRoomListUpdate() {
    base.OnReceivedRoomListUpdate();
    RefreshRoomList();
  }
}
