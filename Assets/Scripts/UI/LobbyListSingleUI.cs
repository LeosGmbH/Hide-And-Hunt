using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListSingleUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    private void Awake() {
        GetComponent<Button>().onClick.AddListener(() => {
        });
    }
    public void SetLobby(Lobby lobby) {
        lobbyNameText.text = lobby.Name;
    }

}