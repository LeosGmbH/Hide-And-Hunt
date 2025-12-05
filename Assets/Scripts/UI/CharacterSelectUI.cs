using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour {
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button survivorButton;
    [SerializeField] private Button KillerButton;
    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.StartMenü);
        });

        survivorButton.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.InGame);

        });
        KillerButton.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.InGameKiller);

        });
    }
}