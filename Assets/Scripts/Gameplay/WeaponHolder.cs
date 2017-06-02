using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour {
  public int currentWeaponChildIndex = 0;

  public Weapon GetCurrentWeapon() {
    return transform.GetChild(currentWeaponChildIndex).GetComponent<Weapon>();
  }
}
