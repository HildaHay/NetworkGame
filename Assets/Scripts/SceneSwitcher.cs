using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher: MonoBehaviour {
    public void MultiplayerScene() {
        SceneManager.LoadScene("MultiplayerScene");
    }
    public void TestingScene() {
        SceneManager.LoadScene("SampleScene");
    }
}
