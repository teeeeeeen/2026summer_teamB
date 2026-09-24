using UnityEngine;
using TMPro; // TextMeshProを使うために必要

public class GameOverMessageController : MonoBehaviour
{
    [Header("メッセージを表示するテキスト")]
    [SerializeField] private TextMeshProUGUI messageText;
    [Header("スコア持ってるスクリプト")]
    [SerializeField] private SoupManager soupManager;
    private void OnEnable()
    {
        // 1. スコアを取得する
        int currentScore = GetScore(); 
        ChangeMessage(currentScore);
    }

    private int GetScore()
    {
        if (soupManager != null)
        {
            return soupManager.totalScore;  // SoupManagerからスコアを取得
        }
        else
        {
            Debug.Log("SoupManagerが設定されていません。");
            return 0; // デフォルト値として0を返す
        }
    }

    private void ChangeMessage(int score)
    {
        if (messageText == null) return;

        // スコアに応じた条件分岐
        if (score < 0)
        {
            messageText.text = "このゲーム壊れちゃった...";
        }
        else if (score ==0)
        {
            messageText.text = "もしかしてわざとやってる？";
        }
        else if (score <100)
        {
            messageText.text = "かけだしみそすーぷくりえいたー";
        }
        else if (score <200)
        {
            messageText.text = "一人前のみそしるクリエイター";
        }
        else if (score <300)
        {
            messageText.text = "熟練の味噌汁職人";
        }
        else if (score <400)
        {
            messageText.text = "旨味の境地を開く者";
        }
        else if (score <500)
        {
            messageText.text = "奇跡のmisosoupmaster";
        }
        else if (score<30000)
        {
            messageText.text = "味噌と具材を滅する者";
        }
        else
        {
            messageText.text = "不正はよくないと思うよ";
        }
    }
}
    