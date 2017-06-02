using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour {
  public GameObject scoreboardItemPrefab;

  public Text gamemodeName;
  public Text winReason; // NOTE: its only for end match screen

  public Text teamAlphaPoints;
  public Text teamBravoPoints;

  public Transform playersAlpha;
  public Transform playersBravo;

  public GameObject panel;

  void Awake() {
    panel = transform.FindChild("Panel").gameObject;
    gamemodeName = transform.FindChild("Panel/GamemodeName").GetComponent<Text>();
    winReason = transform.FindChild("Panel/WinReason").GetComponent<Text>();
    teamAlphaPoints = transform.FindChild("Panel/TeamAlphaPoints").GetComponent<Text>();
    teamBravoPoints = transform.FindChild("Panel/TeamBravoPoints").GetComponent<Text>();

    playersAlpha = transform.FindChild("Panel/PlayersAlpha");
    playersBravo = transform.FindChild("Panel/PlayersBravo");
  }

  void Start() {
    ClearScoreboard();
    GenerateScoreboard();
  }

  void Update() {
    if (panel.activeSelf) {
      if (PhotonNetwork.room != null) {
        object points = 0;
      
        if (PhotonNetwork.room.CustomProperties.TryGetValue("TeamAlphaPoints", out points)) {
          teamAlphaPoints.text = points.ToString();
        }

        points = 0;      
        if (PhotonNetwork.room.CustomProperties.TryGetValue("TeamBravoPoints", out points)) {
          teamBravoPoints.text = points.ToString();
        }
      }
    }
  }

  public void ClearScoreboard() {
    foreach (Transform child in playersAlpha) {
      Destroy(child.gameObject);
    }

    foreach (Transform child in playersBravo) {
      Destroy(child.gameObject);
    }
  }

  public void GenerateScoreboard() {
    int alphaPlayersCount = 0;
    int bravoPlayersCount = 0;

    foreach (PhotonPlayer player in PhotonNetwork.playerList) {
      PunTeams.Team team = player.GetTeam();
      if (team == PunTeams.Team.None)
        continue;

      GameObject item = Instantiate(scoreboardItemPrefab);

      Text nickname = item.transform.GetChild(0).GetComponent<Text>();

      nickname.text = player.NickName;

      if (player.IsMasterClient)
        nickname.color = Color.red;
      else
        nickname.color = Color.white;

      if (team == PunTeams.Team.Alpha)
        item.transform.SetParent(playersAlpha);
      else
        item.transform.SetParent(playersBravo);
      
      RectTransform trans = item.GetComponent<RectTransform>();
      trans.offsetMin = new Vector2(15, trans.offsetMin.y);
      trans.offsetMax = new Vector2(15, trans.offsetMax.y);

      if (team == PunTeams.Team.Alpha) {
        trans.anchoredPosition = new Vector2(0, -24 + (48 * alphaPlayersCount));
        ++alphaPlayersCount;
      } else {
        trans.anchoredPosition = new Vector2(0, -24 + (48 * bravoPlayersCount));
        ++bravoPlayersCount;
      }
    }
  }
}
