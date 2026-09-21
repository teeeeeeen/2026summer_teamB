using UnityEngine;
using UnityEngine.InputSystem;

public class VerticalMovement : MonoBehaviour
{
    [Header("移動速度")]
    public float moveSpeed = 5f;
    [Header("移動制限 (Z軸)")]
    public float minZ = -20f; 
    public float maxZ = 20f;

    private PlayerControls inputActions;
    // W/Sの値（-1.0 ～ 1.0）
    private float verticalInput;

    private void Awake()
    {
        inputActions = new PlayerControls();

        // (W=1, S=-1）
        inputActions.Player.Move.performed += context => verticalInput = context.ReadValue<float>();
        
        // キーが離された時に値を0に戻す
        inputActions.Player.Move.canceled += context => verticalInput = 0f;
    }

    // オブジェクトが有効になった時に入力を有効化
    private void OnEnable()
    {
        inputActions.Enable();
    }

    // オブジェクトが無効になった時に入力を無効化
    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Update()
    {
      // 現在地はどこ？
        Vector3 newPosition = transform.position;

        // 入力に合わせてZ座標を計算
        newPosition.z += verticalInput * moveSpeed * Time.deltaTime;

        // Z座標を minZ と maxZ の間に制限
        newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

        // 制限した位置を実際のオブジェクトに適用
        transform.position = newPosition;
    }
}