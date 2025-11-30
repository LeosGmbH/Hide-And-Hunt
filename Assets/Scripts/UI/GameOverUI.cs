using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour {
    [SerializeField] private Button mainMenuButton;
    private void Awake() {
        mainMenuButton.onClick.AddListener(() => {
            GameLobby.Instance.DisconnectAndLeaveLobby();
            Loader.Load(Loader.Scene.StartMenü);
        });
    }

    private void Start() {
        Hide();
    }

    private void Show() {
        gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Hide() {
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}