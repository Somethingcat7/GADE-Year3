using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public Button VsPlayerButton;
    public Button VsAIButton;
    public Button QuitButton;
    public Button BackButton;
    public Label MenuLabel;

    bool blBool = true;

    // Start is called before the first frame update
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        VsPlayerButton = root.Q<Button>("btnVsPlayer");
        VsAIButton = root.Q<Button>("btnVsAI");
        QuitButton = root.Q<Button>("btnQuit");
        BackButton = root.Q<Button>("btnBack");
        MenuLabel = root.Q<Label>("lblMainMenu");

        VsPlayerButton.clicked += VsPlayerButtonPress;
        VsAIButton.clicked += VsAIButtonPress;
        QuitButton.clicked += QuitButtonPress;
        
    }

 void VsPlayerButtonPress()
    {
        if (blBool)
        {
            SceneManager.LoadScene("VsPlayer");
        }
        else 
        {
            SceneManager.LoadScene("VsAI");
        }
        
    }

    void VsAIButtonPress()
    {
        if (blBool)
        {
            blBool = false;
            

            VsPlayerButton.text = "Easy";
            VsAIButton.text = "Hard";
            QuitButton.text = "Back";
            MenuLabel.text = "Select Difficulty";
            MenuLabel.style.fontSize = 40;

        }
        else 
        {
            SceneManager.LoadScene("VsAI");
        }
        
    }

    void QuitButtonPress()
    {
        if (blBool)
        {
            Application.Quit();

        }
        else
        {
            blBool = true;
            VsPlayerButton.text = "VS PLAYER";
            VsAIButton.text = "VS AI";
            QuitButton.text = "QUIT";
            MenuLabel.text = "Main Menu";
            MenuLabel.style.fontSize = 50;

        }
        
    }
}
