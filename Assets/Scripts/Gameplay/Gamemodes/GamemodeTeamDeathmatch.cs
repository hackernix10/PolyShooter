using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// TODO: gamemode object should care also for gamemode dependent ui like game time, team points, gamemode name, etc...
public class GamemodeTeamDeathmatch : GamemodeBase {
  GameManager gameManager;
  Hud hud;

  public override void OnSetup(GameManager gm) {
    base.OnSetup(gm);
    gameManager = gm;
    hud = gm.hud;

    if (PhotonNetwork.room != null && PhotonNetwork.isMasterClient) {
      Hashtable properties = new Hashtable();
      properties.Add("StartTime", (int)PhotonNetwork.time);
      properties.Add("TeamAlphaPoints", 0);
      properties.Add("TeamBravoPoints", 0);
      properties.Add("TeamAlphaName", "Alpha");
      properties.Add("TeamBravoName", "Bravo");
    
      PhotonNetwork.room.SetCustomProperties(properties);
    }

    hud.scoreboard.gamemodeName.text = "TEAM DEATHMATCH";
  }

  void Update() {
    if (PhotonNetwork.room != null) {
      object points = 0;
      
      if (PhotonNetwork.room.CustomProperties.TryGetValue("TeamAlphaPoints", out points)) {
        hud.teamAlphaPoints.text = points.ToString();
      }

      points = 0;
      if (PhotonNetwork.room.CustomProperties.TryGetValue("TeamBravoPoints", out points)) {
        hud.teamBravoPoints.text = points.ToString();
      }

      int timeLeft = gameManager.GetTimeLeft();
      if (timeLeft > -1) {
        string minutes = (timeLeft / 60 % 60).ToString();
        string seconds = (timeLeft % 60).ToString();
        hud.roundTimer.text = minutes + "m " + seconds + "s";
      } else {
        hud.roundTimer.text = "offline";
      }
    }
  }

  public override bool IsGameFinished() {
    return base.IsGameFinished();
  }

  public override void OnPlayerDeath(int victimID, int killerID) {
    base.OnPlayerDeath(victimID, killerID);

    if (PhotonNetwork.isMasterClient) {
      PhotonPlayer victim = PhotonPlayer.Find(victimID);
      PhotonPlayer killer = PhotonPlayer.Find(killerID);

      if (victim.GetTeam() != killer.GetTeam())
        IncreaseTeamPoints(PhotonPlayer.Find(killerID).GetTeam(), 100);
    }
  }

  public override string GetWinReasonText() {
    return "Team " + (GetTeamPoints(PunTeams.Team.Alpha) > GetTeamPoints(PunTeams.Team.Bravo) ? "Alpha" : "Bravo") + " won the match.";
  }
}
