using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chat : MonoBehaviour {
  public InputField messageInput;
  public Text messages;

  void Start() {
    messageInput = transform.FindChild("MessageInput").GetComponent<InputField>();
    messages = transform.FindChild("MessagesView/Viewport/Content/Messages").GetComponent<Text>();
    messages.text = "";
  }

  public void AddMessage(string sender, string message) {
    messages.text += sender + ": " + message + "\n";
  }

  public void StartTyping() {
    messageInput.gameObject.SetActive(true);
    messageInput.Select();
  }

  public void StopTyping() {
    messageInput.gameObject.SetActive(false);
  }
}
