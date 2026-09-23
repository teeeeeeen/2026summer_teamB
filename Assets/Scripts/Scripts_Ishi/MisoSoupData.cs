using UnityEngine;

[CreateAssetMenu(fileName = "NewMisoSoup", menuName = "MisoSoup/ZukanItem")]
public class MisoSoupData : ScriptableObject
{
    public string soupName;       // 味噌汁の名前
    public Sprite soupIcon;       // アイコン・詳細兼用の画像
    [TextArea(3, 5)]
    public string description;    // 説明文
    public bool isUnlocked = false; // 解放フラグ
}