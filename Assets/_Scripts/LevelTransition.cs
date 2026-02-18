using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1f;

    public void LoadGame()
    {
        //1 game
        StartCoroutine(LoadScene(1));
    }

    public void LoadMainMenu()
    {
        //0 main menu
        StartCoroutine(LoadScene(0));
        
    }

    IEnumerator LoadScene(int buildIndex)
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(buildIndex);
    }
}
