using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : MonoBehaviour
{
    public void TransitionToScene(int sceneNum)
    {
        SceneManager.LoadScene(sceneNum);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
