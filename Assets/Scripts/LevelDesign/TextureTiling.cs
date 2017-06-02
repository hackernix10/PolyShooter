using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TextureTiling : MonoBehaviour {
  public Vector2 tiling = Vector2.one;
  
  Material material;

  void Start() {
    material = GetComponent<MeshRenderer>().sharedMaterial;
    material.mainTextureScale = tiling;
  }

#if UNITY_EDITOR
  void Update() {
    material.mainTextureScale = tiling;
  }
#endif
}
