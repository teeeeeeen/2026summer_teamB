using UnityEngine;

public class PlayerSelectManager : MonoBehaviour
{
    [Header("プレイヤーオブジェクト")]
    public GameObject keyboardPlayer;   // キーボード操作用のプレイヤー
    public GameObject controllerPlayer; // コントローラー操作用のプレイヤー

    [Header("UI設定")]
    public GameObject selectionUI;      // 選択ボタンが配置されているUIパネル

    void Start()
    {
        // 選択待ちの間、ゲームの時間を止める（障害物の生成やタイマーが進まなくなる）
        Time.timeScale = 0f; 
        
        // 選択画面を表示
        if (selectionUI != null) selectionUI.SetActive(true);
    }

    // キーボードを選ぶボタンから呼ばれる関数
    public void SelectKeyboard()
    {
        if (controllerPlayer != null) Destroy(controllerPlayer); // コントローラー用を削除
        if (keyboardPlayer != null) keyboardPlayer.SetActive(true); // キーボード用を有効化
        
        StartGame();
    }

    // コントローラーを選ぶボタンから呼ばれる関数
    public void SelectController()
    {
        if (keyboardPlayer != null) Destroy(keyboardPlayer); // キーボード用を削除
        if (controllerPlayer != null) controllerPlayer.SetActive(true); // コントローラー用を有効化
        
        StartGame();
    }

    private void StartGame()
    {
        // 選択画面を非表示にする
        if (selectionUI != null) selectionUI.SetActive(false);
        
        // 時間を動かしてゲーム本編を開始
        Time.timeScale = 1f;
    }
}