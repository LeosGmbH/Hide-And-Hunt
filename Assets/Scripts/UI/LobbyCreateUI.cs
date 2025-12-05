using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour {
    [SerializeField] private Button closeButton;
    [SerializeField] private Button createPublicButton;
    [SerializeField] private Button createPrivateButton;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    private void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });

        createPublicButton.onClick.AddListener(() =>
        {
            LoadCharacterScene();
        });
        createPrivateButton.onClick.AddListener(() =>
        {
            LoadCharacterScene();
        });

    }


    private void LoadCharacterScene()
    { 
         SceneManager.LoadScene("CharacterSelectScene");
    }

    private void Start() {
        Hide();
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

}