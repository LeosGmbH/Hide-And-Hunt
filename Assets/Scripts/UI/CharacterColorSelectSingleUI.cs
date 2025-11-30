using UnityEngine;
using UnityEngine.UI;

public class CharacterColorSelectSingleUI : MonoBehaviour
{
    [SerializeField] private int colorId;
    [SerializeField] private Image image;
    [SerializeField] private GameObject selectedGameObject;
    CharacterSelectPlayer characterSelectPlayer;

    private void Awake()
    {
        characterSelectPlayer = FindObjectOfType<CharacterSelectPlayer>();
        GetComponent<Button>().onClick.AddListener(() =>
        {
            GameMultiplayer.Instance.ChangePlayerColor(colorId);
            GameMultiplayer.Instance.SetCharacterSelectionServerRpc(colorId);
        });
    }

    private void Start()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultiplayer_OnPlayerDataNetworkListChanged;
        image.color = GameMultiplayer.Instance.GetPlayerColor(colorId);
        UpdateIsSelected();
    }
    private void GameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdateIsSelected();
    }
    private void UpdateIsSelected()
    {
        bool isSelected = false;

        foreach (var playerData in GameMultiplayer.Instance.GetAllPlayerData())
        {
            if (playerData.colorId == colorId)
            {
                isSelected = true;
                break;
            }
        }

        selectedGameObject.SetActive(isSelected);
    }
    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayer_OnPlayerDataNetworkListChanged;
    }
}