using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType {
  Glock = 0
}

public enum WeaponSlot {
  Unknown,
  Primary,
  Secondary,
  Melee,
  Num
}

public class Weapon : MonoBehaviour {
  public string weaponName = "Weapon";
  public WeaponType type;
  public WeaponSlot slot;
  public bool isHolding = false;

  [Header("Ammo")]
  public int clipCapacity = 20;
  public float reloadTime = 1.5f;
  public int maxTotalAmmo = 80;
  public int totalAmmo = 80;
  public int ammoInClip = 20;

  [Header("Shooting & recoil")]
  public bool isAutomatic = false;
  public float fireRate = 2f;

  [Header("Damage")]
  public float baseDamage = 20.0f;
  public float headMultipler = 10.0f;

  [Header("Weapon FX")]
  public ParticleSystem muzzleEffect;

  bool canShoot = false;
  bool isReloading = false;

  float nextFireTime = 0.0f;

  Animator animator;
  Animator fpHandsAnimator;
  AudioSource soundFX;

  void Start() {
    animator = GetComponent<Animator>();
    soundFX = GetComponent<AudioSource>();
    fpHandsAnimator = Camera.main.transform.FindChild("FP Hands").GetComponent<Animator>();
  }
 
  void Update() {

  }

  public void OnReload(PhotonView photonView) {
    if (!photonView.isMine)
      return;

    if (type == WeaponType.Glock) {
      if (totalAmmo > 0 && ammoInClip < clipCapacity) {
        StopCoroutine("Reload");
        StartCoroutine(Reload());
        fpHandsAnimator.SetTrigger("Reload");
      }
    }
  }

  public void OnShoot(PhotonView photonView) {
    if (!photonView.isMine)
      return;

    if (isReloading && !canShoot)
      return;

    if (type == WeaponType.Glock) {
      if (ammoInClip <= 0)
        return;

      // if (PhotonNetwork.time < nextFireTime)
      //   return;
      // nextFireTime = (float)PhotonNetwork.time + (1.0f / fireRate);

      if (muzzleEffect) {
        muzzleEffect.Play();
      }

      PlayShootEffects();
      ammoInClip--;

      Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
      RaycastHit hit;

      ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);//+ new Vector3(0, recoildMaxY * currentRecoil, 0));
      /*if (currentRecoil > 0.1f) {
        ray.direction = ray.direction;
      }

      currentRecoil += 0.1f;
      currentRecoil = Mathf.Clamp(currentRecoil, 0.0f, 1.0f);*/

      if (Physics.Raycast(ray, out hit, 500.0f)) {
        Vector3 forward = Vector3.Normalize(transform.position - hit.point);

        if (PhotonNetwork.offlineMode) {
          photonView.GetComponent<PlayerController>().OnHit(hit.point, forward);
        } else {
          photonView.RPC("OnHit", PhotonTargets.All, hit.point, forward);
        }
    
        if (hit.transform.tag == "Player") {
          if (!PhotonNetwork.offlineMode) {
            PhotonView targetPhotonView = hit.transform.GetComponent<PhotonView>();
            targetPhotonView.RPC("TakeDamage", targetPhotonView.owner, photonView.ownerId, targetPhotonView.ownerId, (int)baseDamage, weaponName);
          }
        }
      }
    }
  }

  void PlayShootEffects() {
    soundFX.Play();
    animator.Play("Shoot");
  }

  void PlayReloadEffects() {
  }

  public bool NeedAndCanReload() {
    if (totalAmmo > 0 && ammoInClip == 0 && !isReloading)
      return true;
    return false;
  }

  IEnumerator ShootDelay() {
    yield return new WaitForSeconds(0.1f);
    canShoot = true;
  }

  IEnumerator Reload() {
    if (totalAmmo <= 0)
      yield return 0;

    isReloading = true;
    yield return new WaitForSeconds(reloadTime);

    int ammoToFill = (clipCapacity - ammoInClip);

    if (totalAmmo - ammoToFill > 0) {
      totalAmmo -= ammoToFill;
      ammoInClip = clipCapacity;
    } else {
      ammoInClip = totalAmmo;
      totalAmmo = 0;
    }

    isReloading = false;
  }
}
