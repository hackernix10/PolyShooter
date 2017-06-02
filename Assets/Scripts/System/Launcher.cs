using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Launcher : Photon.PunBehaviour {
  public GameObject mainMenu;
  public GameObject connecting;

	void Start () {
		PhotonNetwork.autoJoinLobby = true;
    PhotonNetwork.automaticallySyncScene = true;
    PhotonNetwork.ConnectUsingSettings("1.0");
	}

  void CreateRoom(string name) {
    RoomOptions roomOptions = new RoomOptions();
    roomOptions.MaxPlayers = 8;

    Hashtable properties = new Hashtable();
    properties.Add("Map", 1);

    roomOptions.CustomRoomProperties = properties;

    if (!PhotonNetwork.JoinOrCreateRoom(name, roomOptions, TypedLobby.Default)) {
      Debug.LogError("There was error while trying create room.");
    }
  }

  // PUN's callbacks
  public override void OnCreatedRoom() {
    base.OnCreatedRoom();
    Debug.Log("OnCreatedRoom");
  }

  public override void OnJoinedRoom() {
    base.OnJoinedRoom();
    Debug.Log("OnJoinedRoom");

    //PhotonNetwork.LoadLevel(1);
  }

  public override void OnJoinedLobby() {
    base.OnJoinedRoom();
    Debug.Log("OnJoinedLobby");
  }

  public override void OnConnectedToMaster() {
    base.OnConnectedToMaster();
    Debug.Log("OnConnectedToMaster");
    Debug.Log("Region: " + PhotonNetwork.networkingPeer.CloudRegion);

    mainMenu.SetActive(true);
    connecting.SetActive(false);
  }

  public override void OnFailedToConnectToPhoton(DisconnectCause cause) {
    base.OnFailedToConnectToPhoton(cause);
    Debug.LogError("OnFailedToConnectToPhoton");
  }

  public override void OnReceivedRoomListUpdate() {
    base.OnReceivedRoomListUpdate();
    Debug.Log("UPDATED");
  }
}
