using UnityEngine;

public class HowButton : MonoBehaviour
{
    public SceneFade sceneFade;
    public GameObject howToPlayPanel; // 「遊び方」パネルのGameObjectをアタッチ
    public GameObject mainMenuPanel; // メインメニューのGameObjectをアタッチ

    public void HowToPlay()
    {
        howToPlayPanel.SetActive(true); // 「遊び方」パネルを表示
        mainMenuPanel.SetActive(false); // メインメニューを非表示
    }

    public void BackToMenu()
    {
        howToPlayPanel.SetActive(false); // 「遊び方」パネルを非表示
        mainMenuPanel.SetActive(true); // メインメニューを表示
    }
}