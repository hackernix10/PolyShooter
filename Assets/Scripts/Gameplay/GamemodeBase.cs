using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GamemodeBase : MonoBehaviour {
  public virtual void OnSetup(GameManager gm) {
  }

  public virtual void OnPlayerDeath(int victimID, int killerID) {
  }

  public virtual bool IsGameFinished() {
    object startTime = 0;
    object roundTime = 0;
    if (PhotonNetwork.room.CustomProperties.TryGetValue("StartTime", out startTime) &&
        PhotonNetwork.room.CustomProperties.TryGetValue("RoundTime", out roundTime)) {
      return ((int)startTime + (int)roundTime - (int)PhotonNetwork.time) <= 0 ? true : false;
    }

    return false;
  }

  public virtual string GetWinReasonText() {
    return "";
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

  public int GetTeamPoints(PunTeams.Team team) {
    string key = "TeamAlphaPoints";

    if (team == PunTeams.Team.Bravo) {
      key = "TeamBravoPoints";
    }

    object currentPoints = 0;
    if (PhotonNetwork.room.CustomProperties.TryGetValue(key, out currentPoints)) {
      return (int)currentPoints;
    }

    return 0;
  }
}
