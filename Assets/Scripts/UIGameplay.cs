using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class UIGameplay : MonoBehaviour
{

    public Button BackButton;
    public Label GameplayLabel;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        BackButton = root.Q<Button>("btnBack");
        GameplayLabel = root.Q<Label>("lblUpdater");

        BackButton.clicked += VsBackButtonPress;
    }

    void VsBackButtonPress()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void UpdateLabel(string strTurn, int intSize)
    {
        GameplayLabel.text = strTurn;
        GameplayLabel.style.fontSize = intSize;
    }
}
