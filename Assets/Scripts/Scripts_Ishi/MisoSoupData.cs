using UnityEngine;

[CreateAssetMenu(fileName = "NewMisoSoup", menuName = "MisoSoup/ZukanItem")]
public class MisoSoupData : ScriptableObject
{
    public string soupName;       // 味噌汁の名前
    public Sprite soupIcon;       // アイコン・詳細兼用の画像
    
    [Header("解放条件（材料）")]
    [TextArea(2, 3)]              // インスペクターで複数行入力しやすくする
    public string ingredients;    // 材料のテキスト
    [Header("特殊演出")]
    public bool isSuperLegendary = false;//最強の味噌汁？
    [TextArea(3, 5)]
    public string description;    // 説明文
    public bool isUnlocked = false; // 解放フラグ
}