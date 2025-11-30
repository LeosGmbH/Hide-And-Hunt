using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() => {
            DisconnectFromLobby();
        });
    }
    public void DisconnectFromLobby()
    {
        GameLobby.Instance.DisconnectAndLeaveLobby();
        Loader.Load(Loader.Scene.StartMenü);
    }
}
