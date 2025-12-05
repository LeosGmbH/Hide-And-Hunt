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
        Loader.Load(Loader.Scene.StartMenü);
    }
}
