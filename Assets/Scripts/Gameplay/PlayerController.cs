using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team {
  None,
  Alpha,
  Bravo
}

public enum PlayerState {
  Alive,
  Dead,
}

public enum InventorySlot {
  Primary,
  Secondary,
  Melee
}

public class PlayerController : MonoBehaviour {
  // some cached components
  Hud hud;
  GameManager gameManager;
  FPSController fpsController;
  PhotonView photonView;
  Transform cameraTransform;
  Camera mainCamera;

  bool isTypingMessage = false;

  [Space]
  [Header("Player Info")]
  public int health = 100;
  public PlayerState playerState;

  Dictionary<InventorySlot, Weapon> inventory = new Dictionary<InventorySlot, Weapon>() {
    { InventorySlot.Primary, null },
    { InventorySlot.Secondary, null },
    { InventorySlot.Melee, null }
  };
  KeyValuePair<InventorySlot, Weapon> currentWeapon = new KeyValuePair<InventorySlot, Weapon>(InventorySlot.Primary, null);

  [Header("Effects")]
  public GameObject hitDefaultFX;

  // fp gfx
  Animator handsAnimator;
  FPWeaponManager fpWeaponManager;

  void Start() {
    hud = Hud.instance;
    gameManager = GameManager.instance;
    fpsController = GetComponent<FPSController>();
    photonView = GetComponent<PhotonView>();
    mainCamera = Camera.main;

    if (photonView.isMine) {
      foreach (Transform child in transform) {
        child.gameObject.SetActive(false);
      }

      gameManager.localPlayerController = this;
      cameraTransform = Camera.main.transform;
      handsAnimator = cameraTransform.GetChild(0).GetComponent<Animator>();
      hud.chat.messageInput.onEndEdit.AddListener(delegate { ChatInputOnEndEdit(); });
      hud.chat.StopTyping();
      transform.name = PhotonNetwork.playerName;
      fpWeaponManager = Camera.main.GetComponent<FPWeaponManager>();

      SetWeapon(InventorySlot.Secondary, fpWeaponManager.weaponHolder.GetCurrentWeapon());
      ChangeWeapon(InventorySlot.Secondary);

      hud.HideMenu();

      Respawn();
    } else {
      fpsController.enabled = false;
    }
  }

  void Update() {
    if (photonView.isMine) {
      if (gameManager.isMatchOverCalled)
        return;

      if (Input.GetKeyDown(KeyCode.P)) {
        hud.deathHistory.AddPlayerKilledPlayer(PhotonNetwork.player, PhotonNetwork.player);
      }

      if (Input.GetKeyDown(KeyCode.R)) {
        if (currentWeapon.Value != null)
          currentWeapon.Value.OnReload(photonView);
      }

      if (currentWeapon.Value != null && fpsController.IsMovementEnabled()) {
        if (currentWeapon.Value.NeedAndCanReload()) {
          currentWeapon.Value.OnReload(photonView);
        }
      }

      if (currentWeapon.Value != null && fpsController.IsMovementEnabled()) {
        if (currentWeapon.Value.isAutomatic) {
          if (Input.GetMouseButton(0)) {
            currentWeapon.Value.OnShoot(photonView);
          }
        } else {
          if (Input.GetMouseButtonDown(0)) {
            currentWeapon.Value.OnShoot(photonView);
          }
        }
      }

      if (transform.position.y < -15.0f) {
        Respawn();
      }

      HudUpdate();
    } else {
      UpdateNetworkPosition();
    }
  }

  void FixedUpdate() {
    if (photonView.isMine) {
      // ...
    }
  }

  void OnTriggerEnter(Collider trigger) {
    if (!photonView.isMine)
      return;

    if (trigger.transform.tag == "Pickup") {
      trigger.GetComponent<Pickup>().Pick(this);
      Debug.Log(trigger.GetComponent<Pickup>().pickupType.ToString());
    }
  }

  public void PickupItem(Pickup.Type pickupType) {
    if (pickupType == Pickup.Type.AidKit) {
      health = 100;
    } else {
      currentWeapon.Value.totalAmmo = currentWeapon.Value.maxTotalAmmo;
    }
  }

  public void SetWeapon(InventorySlot slot, Weapon weapon) {
    inventory[slot] = weapon;
  }

  public void ChangeWeapon(InventorySlot slot) {
    Weapon weap = GetWeapon(slot);

    if (weap == null)
      return;

    currentWeapon = new KeyValuePair<InventorySlot, Weapon>(slot, weap);
  }

  public Weapon GetWeapon(InventorySlot slot) {
    Weapon result;
    if (inventory.TryGetValue(slot, out result))
      return result;
    return null;
  }

  [PunRPC]
  public void TakeDamage(int agressorID, int targetID, int amt, string weaponName) {
    if (photonView.isMine && playerState == PlayerState.Alive) {
      health -= amt;

      if (health <= 0) {
        photonView.RPC("OnDeath", PhotonTargets.All, agressorID, targetID);
        //OnDeath(agressorID, targetID);
      }
    }
  }

  [PunRPC]
  public void OnDeath(int killerID, int victimID) {
    if (PhotonNetwork.isMasterClient) {
      gameManager.OnDeath(killerID, victimID);
    }

    PhotonPlayer killer = PhotonPlayer.Find(killerID);
    PhotonPlayer victim = PhotonPlayer.Find(victimID);
    hud.deathHistory.AddPlayerKilledPlayer(killer, victim);

    playerState = PlayerState.Dead;
    StartCoroutine(WaitForRespawn());

    if (photonView.isMine) {
      transform.position = new Vector3(10000, 10000, 10000);
      fpsController.enabled = false;
    }
  }

  IEnumerator WaitForRespawn() {
    yield return new WaitForSeconds(5.0f);
    Respawn();
  }

  void Respawn() {
    health = 100;
    playerState = PlayerState.Alive;

    if (photonView.isMine) {
      fpsController.enabled = true;
      transform.position = gameManager.GetRandomSpawnPoint(PhotonNetwork.player.GetTeam()).position;
    }
  }

  [PunRPC]
  public void OnHit(Vector3 hitPoint, Vector3 forward) {
    if (hitDefaultFX) {
      GameObject hitFX = Instantiate(hitDefaultFX);
      hitFX.transform.position = hitPoint;
      hitFX.transform.rotation = Quaternion.LookRotation(forward);
      hitFX.GetComponent<ParticleSystem>().Play();
      Destroy(hitFX, 1.0f);
    }

    //Debug.Log("Hitpoint: " + hitPoint.ToString());
  }

  // Hud things
  void HudUpdate() {
    hud.health.text = string.Format("Health: {0}/100", health);
    Hud.instance.ammunition.text = string.Format("Ammunition: {0}/{1}", currentWeapon.Value.ammoInClip, currentWeapon.Value.totalAmmo);

    if (Input.GetKeyDown(KeyCode.Return) && isTypingMessage == false) {
      Cursor.visible = true;
      Cursor.lockState = CursorLockMode.None;
      fpsController.enabled = false;

      isTypingMessage = true;
      
      hud.chat.StartTyping();
    }

    if (Input.GetKeyDown(KeyCode.Tab)) {
      hud.ShowScoreboard();
    } else if (Input.GetKeyUp(KeyCode.Tab)) {
      hud.HideScoreboard();
    }

    if (Input.GetKeyDown(KeyCode.Escape)) {
      if (hud.menu.activeSelf) {
        hud.HideMenu();
        fpsController.EnableMovement();
      } else {
        hud.ShowMenu();
        fpsController.DisableMovement();
      }

    }
  }

  public void OnHideMenu() {
    fpsController.EnableMovement();
  }

  void ChatInputOnEndEdit() {
    if (hud.chat.messageInput.text != "") {
      if (PhotonNetwork.offlineMode) {
        OnChatMessage(PhotonNetwork.player.NickName, hud.chat.messageInput.text);
      } else {
        photonView.RPC("OnChatMessage", PhotonTargets.AllViaServer, PhotonNetwork.player.NickName, hud.chat.messageInput.text);
      }
    }

    hud.chat.messageInput.text = "";
    fpsController.enabled = true;
    isTypingMessage = false;
    hud.chat.StopTyping();
  }

  [PunRPC]
  void OnChatMessage(string sender, string msg) {
    hud.chat.AddMessage(sender, msg);
  }

  // network position stuff
  Vector3 realPosition = Vector3.zero;
  Quaternion realRotation = Quaternion.identity;
  void UpdateNetworkPosition() {
    if (Vector3.Distance(transform.position, realPosition) > 10.0f) {
      transform.position = realPosition;
    } else {
      transform.position = Vector3.Lerp(transform.position, realPosition, Time.deltaTime * 5.0f);
    }
    transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, Time.deltaTime * 5.0f);
  }

  public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
    if (stream.isWriting) {
      // network movement syncing
      stream.SendNext(transform.position);
      stream.SendNext(transform.rotation);

      stream.SendNext(health);
      stream.SendNext(playerState);
    } else {
      // network movement syncing
      realPosition = (Vector3)stream.ReceiveNext();
      realRotation = (Quaternion)stream.ReceiveNext();

      health = (int)stream.ReceiveNext();
      playerState = (PlayerState)stream.ReceiveNext();
    }
  }
}
