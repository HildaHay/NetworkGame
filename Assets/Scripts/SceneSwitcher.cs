using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher: MonoBehaviour {

    public void MainMenuScene() {
        SceneManager.LoadScene("MainMenu");
    }
    public void MultiplayerScene() {
        SceneManager.LoadScene("Multiplayer");
    }
    public void TestingScene() {
        SceneManager.LoadScene("SampleScene");
    }
}
