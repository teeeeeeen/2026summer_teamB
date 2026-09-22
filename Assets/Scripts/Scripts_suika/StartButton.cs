using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    public void GoToGameScene()
    {
        SceneManager.LoadScene("Main");
    }
}