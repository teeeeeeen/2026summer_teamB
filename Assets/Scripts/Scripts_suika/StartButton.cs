using UnityEngine;

public class StartButton : MonoBehaviour
{
    public SceneFade sceneFade;
    public string sceneName = "Main"; // 遷移先のシーン名を指定

    // 引数を削除し、ボタンのOnClickなどから呼び出しやすくする
    public void GoToGameScene()
    {
        // ダブルクォーテーションを外し、上で定義した変数(sceneName)を渡す
        sceneFade.LoadScene(sceneName);
    }
}