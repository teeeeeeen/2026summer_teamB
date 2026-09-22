using UnityEngine;

public class QuitButton : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("ゲーム終了"); // エディタでは終了しない
    }
}