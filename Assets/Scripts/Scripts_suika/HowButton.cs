using UnityEngine;

public class HowButton : MonoBehaviour
{
    public SceneFade sceneFade;

    public void GoToGameScene()
    {
        sceneFade.LoadScene("Main");
    }
}