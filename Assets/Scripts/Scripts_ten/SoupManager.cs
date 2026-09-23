using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI; // Image用に必要
using TMPro; // TextMeshProを使うために必要
using UnityEngine.InputSystem; // 【追加】新しい入力システムを使うために必要

public class SoupManager : MonoBehaviour
{
    public static SoupManager instance;

    [Header("UI設定：テキスト")]
    public TextMeshProUGUI displayText;        // 投入中や完成品の名前を表示するテキスト（TMPに変更）
    public float textSpeed = 0.1f;             // 文字が流れるスピード

    [Header("UI設定：アイコン（8個）")]
    public Image[] ingredientIcons; 

    [Header("UI設定：完成品演出")]
    public Image finalSoupImage;    
    [Tooltip("0〜16のIDに対応する17枚のスプライトをセットしてください")]
    public Sprite[] finalSoupSprites; 
    public float showDuration = 3.0f;          // 表示しておく秒数

    [Header("スコア設定")]
    public TextMeshProUGUI totalScoreText;     // 合計スコアを表示するTMP
    public TextMeshProUGUI addedScoreText;     // 加算スコア（+3Pなど）を表示するTMP
    private int totalScore = 0;                // 現在の合計スコア

    [Header("リザルト（結果）設定")]
    public TextMeshProUGUI resultCurrentScoreText;    // ゲームオーバー画面等で「今回のスコア」を表示するTMP
    [Tooltip("1位から3位までを表示するTMPを3つセットしてください")]
    public TextMeshProUGUI[] resultHighScoreTexts;    // ハイスコアを表示するTMP配列（要素数3）
    private List<int> savedHighScores = new List<int>(); // 保存されているハイスコア

    [Header("オーディオ設定")]
    public AudioSource completeSound;          // 味噌汁完成時の効果音

    public enum Ingredient
    {
        Tofu, Wakame, Aburaage, Nasu, Nameko, Potato, Negi, Onion, Unknown
    }

    private int currentCount = 0;
    private List<Ingredient> currentIngredients = new List<Ingredient>();
    
    private Coroutine currentAnimationRoutine = null;
    private Coroutine currentTextRoutine = null;
    private Coroutine currentAddedScoreRoutine = null;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        ResetIcons();
        if (finalSoupImage != null)
        {
            finalSoupImage.gameObject.SetActive(false);
        }
        if (displayText != null)
        {
            displayText.text = "";
        }
        if (addedScoreText != null)
        {
            addedScoreText.gameObject.SetActive(false);
        }

        // スコアの初期化
        totalScore = 0;
        if (totalScoreText != null) totalScoreText.text = $"SCORE: {totalScore}";

        // 保存されているハイスコア（上位3件）を読み込む
        savedHighScores.Add(PlayerPrefs.GetInt("ScoreRank1", 0));
        savedHighScores.Add(PlayerPrefs.GetInt("ScoreRank2", 0));
        savedHighScores.Add(PlayerPrefs.GetInt("ScoreRank3", 0));
        
        UpdateResultUI();
    }

    void Update()
    {
        // 【修正】新しいInput Systemに対応したキー判定
        if (Keyboard.current != null)
        {
            bool isCtrl = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
            bool isShift = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            bool isR = Keyboard.current.rKey.wasPressedThisFrame;

            // Ctrl + Shift + R でランキングリセット
            if (isCtrl && isShift && isR)
            {
                PlayerPrefs.DeleteKey("ScoreRank1");
                PlayerPrefs.DeleteKey("ScoreRank2");
                PlayerPrefs.DeleteKey("ScoreRank3");
                PlayerPrefs.Save();

                savedHighScores = new List<int> { 0, 0, 0 };
                UpdateResultUI();
                Debug.Log("ハイスコアをリセットしました。");
            }
        }
    }

    // 障害物から呼ばれる処理
    public void CatchIngredient(string ingredientName, Sprite ingredientSprite)
    {
        Ingredient ing = GetIngredientFromName(ingredientName);
        if (ing == Ingredient.Unknown)
        {
            Debug.LogWarning($"未定義の具材名です: {ingredientName}");
            return;
        }

        // アイコンを灯す
        if (currentCount < ingredientIcons.Length && ingredientIcons[currentCount] != null)
        {
            ingredientIcons[currentCount].sprite = ingredientSprite;
            ingredientIcons[currentCount].color = new Color(1, 1, 1, 1); 
        }

        currentIngredients.Add(ing);
        currentCount++;

        if (currentCount < 8)
        {
            // 投入中テキストの更新（テキストアニメーション中でなければ表示）
            if (currentTextRoutine != null) StopCoroutine(currentTextRoutine);
            if (displayText != null) displayText.text = $"具材を投入中... ({currentCount}/8)";
        }
        else
        {
            // 8個集まった時の処理
            int soupId = EvaluateSoupId(currentIngredients);
            string resultText = GetSoupText(soupId);
            int earnedScore = GetSoupScore(soupId);

            // 効果音を鳴らす
            if (completeSound != null)
            {
                completeSound.Play();
            }

            // スコア加算
            totalScore += earnedScore;
            if (totalScoreText != null) totalScoreText.text = $"SCORE: {totalScore}";

            // 加算スコア（+〇P）の表示アニメーション
            if (currentAddedScoreRoutine != null) StopCoroutine(currentAddedScoreRoutine);
            currentAddedScoreRoutine = StartCoroutine(ShowAddedScoreRoutine(earnedScore));

            // リザルト画面をいつでも表示できるよう最新状態に更新
            UpdateResultUI();

            // テキスト流し開始
            if (currentTextRoutine != null) StopCoroutine(currentTextRoutine);
            currentTextRoutine = StartCoroutine(StreamText(resultText));

            // 画像アニメーション開始
            if (currentAnimationRoutine != null) StopCoroutine(currentAnimationRoutine);
            currentAnimationRoutine = StartCoroutine(ShowFinalSoupRoutine(soupId));

            // 次の収集ができるように即座にリセット
            ResetIcons();
        }
    }

    private void ResetIcons()
    {
        currentCount = 0;
        currentIngredients.Clear();
        foreach (var icon in ingredientIcons)
        {
            if (icon != null) icon.color = new Color(1, 1, 1, 0); 
        }
    }

    private Ingredient GetIngredientFromName(string rawName)
    {
        switch (rawName)
        {
            case "Tofu": case "豆腐": return Ingredient.Tofu;
            case "Wakame": case "わかめ": return Ingredient.Wakame;
            case "Aburaage": case "油揚げ": return Ingredient.Aburaage;
            case "Nasu": case "ナス": case "なす": return Ingredient.Nasu;
            case "Nameko": case "なめこ": return Ingredient.Nameko;
            case "Potato": case "じゃがいも": case "芋": return Ingredient.Potato;
            case "Negi": case "ネギ": case "ねぎ": return Ingredient.Negi;
            case "Onion": case "玉ねぎ": case "タマネギ": return Ingredient.Onion;
            default: return Ingredient.Unknown;
        }
    }

    private int EvaluateSoupId(List<Ingredient> ingredients)
    {
        Dictionary<Ingredient, int> counts = new Dictionary<Ingredient, int>();
        foreach (Ingredient ing in System.Enum.GetValues(typeof(Ingredient)))
        {
            if (ing != Ingredient.Unknown) counts[ing] = 0;
        }

        foreach (var ing in ingredients)
        {
            counts[ing]++;
        }

        int uniqueCount = counts.Count(kv => kv.Value > 0);

        if (uniqueCount == 8) return 0; // 具材の重複がない

        if (uniqueCount == 1)
        {
            if (counts[Ingredient.Tofu] == 8) return 1;
            if (counts[Ingredient.Wakame] == 8) return 2;
            if (counts[Ingredient.Aburaage] == 8) return 3;
            if (counts[Ingredient.Nasu] == 8) return 4;
            if (counts[Ingredient.Nameko] == 8) return 5;
            if (counts[Ingredient.Potato] == 8) return 6;
            if (counts[Ingredient.Negi] == 8) return 7;
            if (counts[Ingredient.Onion] == 8) return 8;
        }

        if (IsExactCombo(counts, uniqueCount, Ingredient.Tofu, Ingredient.Wakame, Ingredient.Aburaage)) return 10;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Potato, Ingredient.Onion)) return 11;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Negi, Ingredient.Onion)) return 12;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Aburaage, Ingredient.Nasu)) return 13;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Wakame, Ingredient.Nameko)) return 14;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Tofu, Ingredient.Aburaage)) return 9;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Nasu, Ingredient.Potato, Ingredient.Negi, Ingredient.Onion)) return 15;

        return 16;
    }

    private bool IsExactCombo(Dictionary<Ingredient, int> counts, int uniqueCount, params Ingredient[] required)
    {
        if (uniqueCount != required.Length) return false;
        foreach (var req in required)
        {
            if (counts[req] == 0) return false;
        }
        return true;
    }

    private string GetSoupText(int soupId)
    {
        switch (soupId)
        {
            case 0: return "超最強の味噌汁";
            case 1: return "湯豆腐";
            case 2: return "わかめスープ";
            case 3: return "きつねみそしる";
            case 4: return "超トロトロなすみそしる";
            case 5: return "なめこの餡かけみそしる";
            case 6: return "芋煮";
            case 7: return "ねぎだく薬味みそしる";
            case 8: return "和風オニオンスープ";
            case 9: return "畑の肉みそしる";
            case 10: return "実家のような安心感のみそしる";
            case 11: return "ほぼポトフ";
            case 12: return "Ｗネギみそしる";
            case 13: return "旨味スポンジみそしる";
            case 14: return "超ネバトロみそしる";
            case 15: return "畑の恵み野菜みそしる";
            case 16: return "具だくさんみそしる";
            default: return "プログラム壊れちゃった...";
        }
    }

    // スコアの配点を算出する処理
    private int GetSoupScore(int soupId)
    {
        if (soupId == 0) return 4;                    // 超最強の味噌汁
        if (soupId >= 1 && soupId <= 8) return 3;     // 同じ具材8個
        if (soupId >= 9 && soupId <= 15) return 2;    // 特定の組み合わせ
        return 1;                                     // 具だくさん（16）
    }

    // 加算スコアを表示して消すコルーチン
    private IEnumerator ShowAddedScoreRoutine(int score)
    {
        if (addedScoreText == null) yield break;

        addedScoreText.text = $"+{score}P";
        addedScoreText.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.5f); // 1.5秒間表示

        addedScoreText.gameObject.SetActive(false);
    }

    // 完成品の名前を流すコルーチン
    private IEnumerator StreamText(string text)
    {
        if (displayText == null) yield break;
        
        displayText.text = "";
        foreach (char c in text)
        {
            displayText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    // 完成品を拡縮アニメーションさせるコルーチン
    private IEnumerator ShowFinalSoupRoutine(int soupId)
    {
        if (finalSoupImage == null || finalSoupSprites == null || finalSoupSprites.Length == 0) yield break;

        int spriteIndex = Mathf.Clamp(soupId, 0, finalSoupSprites.Length - 1);
        finalSoupImage.sprite = finalSoupSprites[spriteIndex];
        
        finalSoupImage.gameObject.SetActive(true);
        finalSoupImage.enabled = true;
        finalSoupImage.transform.localScale = Vector3.one;

        float elapsed = 0f;
        
        while (elapsed < showDuration)
        {
            elapsed += Time.deltaTime;
            
            // 0 -> 1 -> 0 を1秒ごとに繰り返して拡縮
            float scale = Mathf.Lerp(1.0f, 1.1f, Mathf.PingPong(elapsed, 1.0f));
            finalSoupImage.transform.localScale = new Vector3(scale, scale, 1f);
            
            yield return null;
        }

        finalSoupImage.transform.localScale = Vector3.one;
        finalSoupImage.gameObject.SetActive(false);
    }

    // 今回のスコアを含めたハイスコアボードを更新する処理
    private void UpdateResultUI()
    {
        // 今回のスコアをリストに加えてソート（降順）
        List<int> displayScores = new List<int>(savedHighScores);
        displayScores.Add(totalScore);
        displayScores.Sort((a, b) => b.CompareTo(a));

        if (resultHighScoreTexts != null)
        {
            for (int i = 0; i < resultHighScoreTexts.Length; i++)
            {
                if (resultHighScoreTexts[i] != null && i < displayScores.Count)
                {
                    resultHighScoreTexts[i].text = $"{i + 1}位: {displayScores[i]}";
                }
            }
        }

        if (resultCurrentScoreText != null)
        {
            resultCurrentScoreText.text = $"今回のスコア: {totalScore}";
        }
    }

    // シーン遷移時（もう一度遊ぶ）やゲーム終了時に現在のスコアを含めて保存する
    void OnDestroy()
    {
        savedHighScores.Add(totalScore);
        savedHighScores.Sort((a, b) => b.CompareTo(a));
        
        PlayerPrefs.SetInt("ScoreRank1", savedHighScores[0]);
        PlayerPrefs.SetInt("ScoreRank2", savedHighScores[1]);
        PlayerPrefs.SetInt("ScoreRank3", savedHighScores[2]);
        PlayerPrefs.Save();
    }
}