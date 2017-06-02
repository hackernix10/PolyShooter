using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathHistory : MonoBehaviour {
  public GameObject itemPrefab;

  void Start() {
    ClearHistory();
  }

  public void Add(string text) {
    GameObject item = Instantiate(itemPrefab);
    item.transform.SetParent(transform);
    RectTransform rectTransform = item.GetComponent<RectTransform>();
    rectTransform.offsetMin = new Vector2(0, 0);
    rectTransform.offsetMax = new Vector2(0, 32);
    rectTransform.anchoredPosition = new Vector2(0, -15 - (32 * (transform.childCount - 1)));

    item.GetComponent<Text>().text = text;
    Destroy(item, 2.5f);
  }

  const string alphaTeamColorHex = "#FF6060FF";
  const string bravoTeamColorHex = "#6DC9FFFF";

  public void AddPlayerKilledPlayer(PhotonPlayer killer, PhotonPlayer victim) {
    string text = "<color=" + (killer.GetTeam() == PunTeams.Team.Alpha ? alphaTeamColorHex : bravoTeamColorHex) + ">" + killer.NickName +
                  "</color> killed <color=" + (victim.GetTeam() == PunTeams.Team.Alpha ? alphaTeamColorHex : bravoTeamColorHex) + ">" + victim.NickName + "</color>";
    Add(text);
  }

  public void ClearHistory() {
    foreach (Transform child in transform)
      Destroy(child.gameObject);
  }
}
