using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour {
  GameObject loadingSplashScreen;

  void Start() {
    loadingSplashScreen = transform.FindChild("LoadingScreen").gameObject;
    loadingSplashScreen.SetActive(false);
    DontDestroyOnLoad(this);
  }

  // TODO: argument that would let us choose map to load
  public void LoadMap(int sceneIndex) {
    PhotonNetwork.isMessageQueueRunning = false;
    loadingSplashScreen.SetActive(true);
    StartCoroutine(LoadingCoroutine(sceneIndex));
  }

  IEnumerator LoadingCoroutine(int sceneIndex) {
    AsyncOperation status = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
    yield return new WaitUntil(delegate { return status.isDone; });
    PhotonNetwork.isMessageQueueRunning = true;
    loadingSplashScreen.SetActive(false);
  }
}
