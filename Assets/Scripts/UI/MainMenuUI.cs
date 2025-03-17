using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MainMenuUI : MonoBehaviour
{

    [SerializeField] private AudioClip interactSound;


    public void PlayGame()
    {
        SceneManager.LoadScene("Level1");
        Time.timeScale = 1;
        SoundManager.instance.PlaySound(interactSound);
        NetworkManager.Singleton.Shutdown();
    }

    // Main Menu button for end card
    public void MainMenuButton()
    {
        SceneManager.LoadScene(0);
        SoundManager.instance.PlaySound(interactSound);
    }

    public void QuitGame()
    {
        SoundManager.instance.PlaySound(interactSound);
        Application.Quit(); //Quits the game (only works in build)

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; //Exits play mode (will only be executed in the editor)
#endif
    }
}
