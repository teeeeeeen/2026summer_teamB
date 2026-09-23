using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFade : MonoBehaviour
{
    [Header("フェード画像")]
    public RawImage fadeImage;

    [Header("フェード時間")]
    public float fadeDuration = 2.0f;

    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        float time = 0f;

        Color color = fadeImage.color;

        // 透明 → 完全に表示
        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            color.a = Mathf.Lerp(0f, 1f, time / fadeDuration);
            fadeImage.color = color;

            yield return null;
        }

        // 完全に表示
        color.a = 1f;
        fadeImage.color = color;

        // そのまま次のシーンへ
        SceneManager.LoadScene(sceneName);
    }
}