using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class TitleSecretCommand : MonoBehaviour
{
    [Header("図鑑に登録されている味噌汁")]
    public MisoSoupData[] allMisoSoups;

    [Header("成功音")]
    public AudioClip successSound;

    private AudioSource audioSource;

    // =========================
    // キーボード用
    // =========================

    private enum KeyboardKey
    {
        None,
        Down,
        Right,
        Left,
        P,
        H,
        S
    }

    // ↓ → ↓ → P
    private KeyboardKey[] keyboardUnlockCommand =
    {
        KeyboardKey.Down,
        KeyboardKey.Right,
        KeyboardKey.Down,
        KeyboardKey.Right,
        KeyboardKey.P
    };

    // → ↓ ← → H S
    private KeyboardKey[] keyboardResetCommand =
    {
        KeyboardKey.Right,
        KeyboardKey.Down,
        KeyboardKey.Left,
        KeyboardKey.Right,
        KeyboardKey.H,
        KeyboardKey.S
    };

    private int keyboardUnlockIndex = 0;
    private int keyboardResetIndex = 0;


    // =========================
    // プロコン用
    // =========================

    private enum ControllerKey
    {
        None,
        Down,
        Right,
        Left,
        Y,
        X
    }

    // ↓ → ↓ → Y
    private ControllerKey[] controllerUnlockCommand =
    {
        ControllerKey.Down,
        ControllerKey.Right,
        ControllerKey.Down,
        ControllerKey.Right,
        ControllerKey.Y
    };

    // → ↓ ← → X
    private ControllerKey[] controllerResetCommand =
    {
        ControllerKey.Right,
        ControllerKey.Down,
        ControllerKey.Left,
        ControllerKey.Right,
        ControllerKey.X
    };

    private int controllerUnlockIndex = 0;
    private int controllerResetIndex = 0;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }


    private void Update()
    {
        CheckKeyboardCommand();
        CheckControllerCommand();
    }


    // =========================================================
    // キーボード
    // =========================================================

    private void CheckKeyboardCommand()
    {
        if (Keyboard.current == null)
            return;

        KeyboardKey key = GetKeyboardKey();

        if (key == KeyboardKey.None)
            return;


        // 取得：↓ → ↓ → P
        if (key == keyboardUnlockCommand[keyboardUnlockIndex])
        {
            keyboardUnlockIndex++;

            if (keyboardUnlockIndex >= keyboardUnlockCommand.Length)
            {
                UnlockAllSoups();
                PlaySuccessSound();
                keyboardUnlockIndex = 0;
            }
        }
        else
        {
            keyboardUnlockIndex = 0;

            if (key == keyboardUnlockCommand[0])
                keyboardUnlockIndex = 1;
        }


        // リセット：→ ↓ ← → H S
        if (key == keyboardResetCommand[keyboardResetIndex])
        {
            keyboardResetIndex++;

            if (keyboardResetIndex >= keyboardResetCommand.Length)
            {
                ResetAllSoups();
                PlaySuccessSound();
                keyboardResetIndex = 0;
            }
        }
        else
        {
            keyboardResetIndex = 0;

            if (key == keyboardResetCommand[0])
                keyboardResetIndex = 1;
        }
    }


    private KeyboardKey GetKeyboardKey()
    {
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            return KeyboardKey.Down;

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            return KeyboardKey.Right;

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            return KeyboardKey.Left;

        if (Keyboard.current.pKey.wasPressedThisFrame)
            return KeyboardKey.P;

        if (Keyboard.current.hKey.wasPressedThisFrame)
            return KeyboardKey.H;

        if (Keyboard.current.sKey.wasPressedThisFrame)
            return KeyboardKey.S;

        return KeyboardKey.None;
    }


    // =========================================================
    // プロコン
    // =========================================================

    private void CheckControllerCommand()
    {
        if (Gamepad.current == null)
            return;

        ControllerKey key = GetControllerKey();

        if (key == ControllerKey.None)
            return;


        // 取得：↓ → ↓ → Y
        if (key == controllerUnlockCommand[controllerUnlockIndex])
        {
            controllerUnlockIndex++;

            if (controllerUnlockIndex >= controllerUnlockCommand.Length)
            {
                UnlockAllSoups();
                PlaySuccessSound();
                controllerUnlockIndex = 0;
            }
        }
        else
        {
            controllerUnlockIndex = 0;

            if (key == controllerUnlockCommand[0])
                controllerUnlockIndex = 1;
        }


        // リセット：→ ↓ ← → X
        if (key == controllerResetCommand[controllerResetIndex])
        {
            controllerResetIndex++;

            if (controllerResetIndex >= controllerResetCommand.Length)
            {
                ResetAllSoups();
                PlaySuccessSound();
                controllerResetIndex = 0;
            }
        }
        else
        {
            controllerResetIndex = 0;

            if (key == controllerResetCommand[0])
                controllerResetIndex = 1;
        }
    }


    private ControllerKey GetControllerKey()
    {
        if (Gamepad.current.dpad.down.wasPressedThisFrame)
            return ControllerKey.Down;

        if (Gamepad.current.dpad.right.wasPressedThisFrame)
            return ControllerKey.Right;

        if (Gamepad.current.dpad.left.wasPressedThisFrame)
            return ControllerKey.Left;

        // プロコンではX/Yが逆に認識される場合に対応
        // buttonWest → プロコンのY
        if (Gamepad.current.buttonWest.wasPressedThisFrame)
            return ControllerKey.Y;

        // buttonNorth → プロコンのX
        if (Gamepad.current.buttonNorth.wasPressedThisFrame)
            return ControllerKey.X;

        return ControllerKey.None;
    }


    // =========================================================
    // 味噌汁の取得・リセット
    // =========================================================

    private void UnlockAllSoups()
    {
        foreach (MisoSoupData data in allMisoSoups)
        {
            if (data == null)
                continue;

            PlayerPrefs.SetInt(
                "UnlockedSoup_" + data.soupName,
                1
            );
        }

        PlayerPrefs.Save();
    }


    private void ResetAllSoups()
    {
        foreach (MisoSoupData data in allMisoSoups)
        {
            if (data == null)
                continue;

            PlayerPrefs.DeleteKey(
                "UnlockedSoup_" + data.soupName
            );
        }

        PlayerPrefs.Save();
    }


    private void PlaySuccessSound()
    {
        if (successSound != null)
        {
            audioSource.PlayOneShot(successSound);
        }
    }
}