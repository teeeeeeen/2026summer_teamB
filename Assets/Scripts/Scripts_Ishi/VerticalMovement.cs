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
    [Header("移動制限 (X軸)")]
    public float minX = -5f; 
    public float maxX = 5f;  
    [Header("ジャンプ力")]
    public float jumpForce = 5f;

    [Header("着地判定")]
    public float rayLength = 0.1f;
    [Header("バイクの傾き")]
    public float MaxTiltAngle = 30f;//最大傾き
    public float TiltSpeed = 5f;//傾きのスピード
    private Vector2 moveInput;
    private Rigidbody rb;
    private void Awake()
    {
        inputActions = new PlayerControls();

        inputActions.Player.Move.performed += context => moveInput = context.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += context => moveInput = Vector2.zero;
        inputActions.Player.Jump.started += context => Jump();
        rb = GetComponent<Rigidbody>();
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

        // 【追加】X軸（左右）の計算と制限 (A/Dキーの入力 = moveInput.x)
        newPosition.x += moveInput.x * moveSpeed * Time.deltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);

        // 【修正】Z軸（奥・手前）の計算と制限 (W/Sキーの入力 = moveInput.y)
        // ※ verticalInput を moveInput.y に変更
        newPosition.z += moveInput.y * moveSpeed * Time.deltaTime;
        newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

        // 制限した位置を実際のオブジェクトに適用
        transform.position = newPosition;
        //バイクの傾きの処理
        float targettilt=moveInput.y*MaxTiltAngle;
        Quaternion targetRotation = Quaternion.Euler(targettilt, transform.eulerAngles.y, 0f);
        transform.rotation=Quaternion.Lerp(transform.rotation,targetRotation,Time.deltaTime*TiltSpeed);

    }

    private void Jump()
    {
        // 着地判定
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayLength))
        {
            // ジャンプ力を使って上方向に力を加える
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}