using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem; // 最新のInput System用の名前空間

public class SceneFadeInController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("フェード用マスクとして使用するRawImage")]
    [SerializeField] private RawImage fadeMask;

    [Tooltip("フェードインにかかる時間（秒）")]
    [SerializeField] private float fadeDuration = 1.5f;

    [Tooltip("フェード中に入力を無効化するプレイヤーのPlayerInput（時間停止中の入力蓄積を防ぐ）")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Events")]
    [Tooltip("フェードが完全に終了したタイミング（ゲームスタート時）に呼ばれる処理")]
    [SerializeField] private UnityEvent onFadeComplete;

    private void Start()
    {
        // マスクが設定されていない場合は警告を出して処理を中断
        if (fadeMask == null)
        {
            Debug.LogWarning("Fade Mask (RawImage) が設定されていません。インスペクターから設定してください。");
            return;
        }

        // シーン起動時（Start時）にフェードイン処理のコルーチンを開始
        StartCoroutine(FadeInCoroutine());
    }

    private IEnumerator FadeInCoroutine()
    {
        // フェード開始時にゲーム内の時間を停止する
        Time.timeScale = 0f;

        // 【バグ修正】フェード中はPlayerInputからの入力受付を停止し、入力の蓄積を防ぐ
        if (playerInput != null)
        {
            playerInput.DeactivateInput();
        }

        // 初期状態を完全に不透明（Alpha = 1）に設定して表示
        Color maskColor = fadeMask.color;
        maskColor.a = 1f;
        fadeMask.color = maskColor;
        
        // 誤って非表示になっていた場合を考慮してアクティブにする
        fadeMask.gameObject.SetActive(true);

        float elapsedTime = 0f;

        // 指定した時間が経過するまでループ
        while (elapsedTime < fadeDuration)
        {
            // 時間停止中（Time.timeScale = 0）でも進む unscaledDeltaTime を加算する
            elapsedTime += Time.unscaledDeltaTime;

            // 経過時間に応じた進行度 (0.0 〜 1.0) を計算
            float normalizedTime = elapsedTime / fadeDuration;

            // Alpha値を 1.0(不透明) から 0.0(透明) へ補間して下げる
            maskColor.a = Mathf.Lerp(1f, 0f, normalizedTime);
            fadeMask.color = maskColor;

            // 1フレーム待機
            yield return null;
        }

        // 誤差対策として、ループ終了後に確実にAlpha値を0にする
        maskColor.a = 0f;
        fadeMask.color = maskColor;

        // フェード完了後、見えないRawImageがクリック判定などを阻害しないよう非アクティブ化
        fadeMask.gameObject.SetActive(false);

        // 【バグ修正】フェード完了後、入力受付を再開する
        if (playerInput != null)
        {
            playerInput.ActivateInput();
        }

        // ゲーム内の時間の停止を解除（通常速度に戻す）
        Time.timeScale = 1f;

        // ゲームスタートの処理（イベント）を呼び出す
        if (onFadeComplete != null)
        {
            onFadeComplete.Invoke();
        }
    }
}