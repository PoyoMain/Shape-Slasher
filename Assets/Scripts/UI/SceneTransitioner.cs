using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : MonoBehaviour
{
    [SerializeField] private float timeToTransition = 0;
    private int sceneNumber;

    public void TransitionToScene(int sceneNum)
    {
        sceneNumber = sceneNum;
        Invoke(nameof(Transition), timeToTransition);
    }

    private void Transition()
    {
        SceneManager.LoadScene(sceneNumber);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
