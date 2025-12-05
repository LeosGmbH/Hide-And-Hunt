using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMultiplayer : MonoBehaviour {


    public const int MAX_PLAYER_AMOUNT = 5;
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";


    public static GameMultiplayer Instance { get; private set; }


    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;


    [SerializeField] private List<Color> playerColorList;


    private string playerName;
    private Dictionary<ulong, int> clientCharacterSelections = new Dictionary<ulong, int>();


    
    public void SetCharacterSelectionServerRpc(int characterIndex, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        clientCharacterSelections[clientId] = characterIndex;
    }


    public int GetClientCharacterSelection(ulong clientId)
    {
        Debug.Log(clientCharacterSelections.TryGetValue(clientId, out int selection2) ? selection2 : 0);
        return clientCharacterSelections.TryGetValue(clientId, out int selection) ? selection : 0;
    }

    private void Awake() {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "PlayerName" + UnityEngine.Random.Range(100, 1000));

    }

    public string GetPlayerName() {
        return playerName;
    }

    public void SetPlayerName(string playerName) {
        this.playerName = playerName;

        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
    }

}