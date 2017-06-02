using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitRotation : MonoBehaviour {
  public Vector3 min = Vector3.zero;
  public Vector3 max = Vector3.zero;

  void Start() {
    Vector3 desiredRot = new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
    transform.rotation = Quaternion.Euler(desiredRot);
  }
}
