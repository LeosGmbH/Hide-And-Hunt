using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using UnityEngine;
using System.IO;
using Unity.Services.Relay;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;

public class MatchMaker : MonoBehaviour
{
    public TextMeshProUGUI updateText;
    public TextMeshProUGUI gameText;

    public void Host()
    {
        NetworkManager.Singleton.StartHost();
        updateText.text += "\nI am Host";
    }

    public void Join()
    {
        NetworkManager.Singleton.StartClient();
        updateText.text += "\nI am Client";

    }
    void Awake()
    {
        updateText.text += "\nLoading Game";
        StartCoroutine(WarteUndFuehreMethodeAus());
    }
    IEnumerator WarteUndFuehreMethodeAus()
    {
        yield return new WaitForSeconds(3f);
        DeineMethode();
    }

    public async void DeineMethode()
    {
        updateText.text += "\nLogging in";
        _transport = Object.FindObjectOfType<UnityTransport>();

        await Login();

        // CreateLobby();

        CheckForLobbies();

    }
    public int playerLevel = 10;
    private static UnityTransport _transport;

    public async void Play()
    {
        updateText.text += "\nLogging in";
        _transport = Object.FindObjectOfType<UnityTransport>();

        //await Login();

        // CreateLobby();

       // CheckForLobbies();
    }

    public static string PlayerId { get; private set; }

    public static async Task Login()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            var options = new InitializationOptions();
            await UnityServices.InitializeAsync(options);
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            PlayerId = AuthenticationService.Instance.PlayerId;

        }
    }

    public int maxPlayers = 2;
    public string id;
    public string lobbyName = "Name";
    public const string joinKey = "j";
    public const string levelKey = "d";

    public const string gameTypeKey = "t";
    public int gameMode = 1;

    public async void CreateLobby()
    {
        updateText.text += "\nCreating Lobby";
        try
        {
            var a = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string,DataObject>
                {
                    { joinKey,new DataObject(DataObject.VisibilityOptions.Public,joinCode)},
                    { gameTypeKey,new DataObject(DataObject.VisibilityOptions.Public,gameMode.ToString(),DataObject.IndexOptions.N1)},
                    {levelKey,new DataObject(DataObject.VisibilityOptions.Public,playerLevel.ToString(),DataObject.IndexOptions.N2)}
                }
            };
            var lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

            id = lobby.Id;
            StartCoroutine(HeartBeat(lobby.Id, 15));

            NetworkManager.Singleton.StartHost();
            updateText.text = ("I am lobby Host " + playerLevel.ToString());
            gameText.enabled = false;
        }
        catch (IOException e)
        {
            Debug.LogError(e);
            updateText.text += "\nFailed to create lobby";
        }
    }

    public static IEnumerator HeartBeat(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSeconds(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            print("Beat");
            yield return delay;
        }
    }
    private void OnDestroy()
    {
        try
        {
            StopAllCoroutines();
        }
        catch
        {
            Debug.LogFormat("Failed to Destroy");
        }
    }

    public async void CheckForLobbies()
    {
        updateText.text += "\nFinding Game";

        var queryOptions = new QueryLobbiesOptions
        {
            Filters = new List<QueryFilter>
            {
                new QueryFilter(
                    field:QueryFilter.FieldOptions.AvailableSlots,
                    op:QueryFilter. OpOptions.GT,
                    value:"0"),
                new QueryFilter(
                    field:QueryFilter.FieldOptions.N1,
                    op:QueryFilter.OpOptions.EQ,
                    value:gameMode.ToString())
            }
        };
        var response = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
        var lobbies = response.Results;

        if (lobbies.Count > 0)
        {
            foreach (var lobby in lobbies)
            {
                int number = int.Parse(lobby.Data[levelKey].Value);

                if(number > (playerLevel-5)&&number < (playerLevel + 5)) //matchmaking mit leuten +- 5 Level �ber dein Level ( brauchen wir wahrscheinlich ned also entfernen)
                {
                    JoinLobby(lobby);
                    return;
                }

               // JoinLobby(lobby);
            }
            CreateLobby(); //entfernen wenn ranked matchmaking entfernt wird
        }
        else
        {
            CreateLobby();
        }

    }
    public async void JoinLobby(Lobby lobby)
    {
        int number = int.Parse(lobby.Data[levelKey].Value);


        var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[joinKey].Value);
        id = lobby.Id;

        SetTransformAsClient(a);

        NetworkManager.Singleton.StartClient();
        updateText.text = ("In a Lobby "+number.ToString()+"My level is "+playerLevel.ToString());
        gameText.enabled = false;
    }
    public void SetTransformAsClient(JoinAllocation a)
    {
        _transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
    }

}
