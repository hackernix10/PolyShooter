using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {
  public float respawnTime = 0.0f;
  double respawnTimestamp = 0.0f;
  bool isVisible = true;
  PhotonView photonView;

  public enum Type {
    AidKit = 0,
    Glock = 1
  }
  public Type pickupType;

  void Awake() {
    photonView = GetComponent<PhotonView>();
  }

  void Update() {
    if (PhotonNetwork.isMasterClient) {
      if (PhotonNetwork.time >= respawnTimestamp && !isVisible) {
        if (PhotonNetwork.offlineMode) {
          Show();
        } else {
          photonView.RPC("Show", PhotonTargets.AllViaServer);
        }
      }
    }
  }

  public static WeaponType ToWeaponType() {
    return WeaponType.Glock;
  }

  public void Pick(PlayerController player) {
    player.PickupItem(pickupType);
    if (PhotonNetwork.offlineMode) {
      Hide();
    } else {
      photonView.RPC("Hide", PhotonTargets.AllViaServer);
    }
  }

  [PunRPC]
  void Show() {
    GetComponent<SphereCollider>().enabled = true;
    foreach (Transform child in transform) {
      child.gameObject.SetActive(true);
    }
    isVisible = true;
  }

  [PunRPC]
  void Hide() {
    GetComponent<SphereCollider>().enabled = false;
    foreach (Transform child in transform) {
      child.gameObject.SetActive(false);
    }
    respawnTimestamp = PhotonNetwork.time + (double)respawnTime;
    isVisible = false;
  }

  public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
  }
}
