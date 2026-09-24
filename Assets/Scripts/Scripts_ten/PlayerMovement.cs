using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; 

public class PlayerMovement : MonoBehaviour
{
    [Header("移動速度")]
    public float moveSpeed = 5f;
    [Header("移動制限 (Z軸)")]
    public float minZ = -20f; 
    public float maxZ = 20f;

    [Header("移動制限 (X軸)")]
    public float minX = -5f; 
    public float maxX = 5f;  
    
    [Header("ジャンプ力")]
    public float jumpForce = 5f;
    
    [Header("着地判定")]
    public float rayLength = 1.1f;         // 中心より少し上から飛ばすため、適切な長さを確保（モデルに合わせて調整）
    public float rayStartOffset = 0.5f;    // レイの発射位置を少し上にするためのY軸オフセット
    public LayerMask groundLayer;          // 地面として判定するレイヤーを指定

    [Header("バイクの傾き・回転")]
    public float MaxSideTiltAngle = 20f; // 左右の最大傾き
    public float TiltSpeed = 5f;         // 傾きのスピード

    [Header("ウィリー設定")]
    public float wheelieAngle = -30f;    // ジャンプ時の傾き角度（X軸マイナス方向）
    public float wheelieDuration = 0.6f; // 傾いてから戻るまでの時間（秒）

    public AudioSource jumpSound; // ジャンプ時の効果音

    private PlayerControls inputActions;
    private Vector2 moveInput;
    private Rigidbody rb;
    
    // 回転を独立して管理するための内部変数
    private float currentPitch = 0f; // 前後（X軸）用
    private float currentRoll = 0f;  // 左右傾き（Z軸）用
    private bool isWheelieing = false;
    
    // 回転のズレを防ぐための初期角度
    private Quaternion startRotation;
    
    private void Awake()
    {
        inputActions = new PlayerControls();

        inputActions.Player.Move.performed += context => moveInput = context.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += context => moveInput = Vector2.zero;
        inputActions.Player.Jump.started += context => Jump();
        
        rb = GetComponent<Rigidbody>();
        
        // ゲーム開始時の向きを記憶（これ以降Y軸のズレが発生しないようにする）
        startRotation = transform.rotation;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Update()
    {
        if (GameManager.instance != null && GameManager.instance.isPaused) return;

        // 傾きの目標値計算はUpdateで行う（滑らかな補間のため）
        if (isWheelieing)
        {
            // ウィリー中（ジャンプ中）は左右の傾きを徐々に0に戻し、無効化する
            currentRoll = Mathf.Lerp(currentRoll, 0f, Time.deltaTime * TiltSpeed);
        }
        else
        {
            // 地上にいる時だけ左右の移動入力で傾く
            float targetRoll = -moveInput.x * MaxSideTiltAngle;
            currentRoll = Mathf.Lerp(currentRoll, targetRoll, Time.deltaTime * TiltSpeed);
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.instance != null && GameManager.instance.isPaused) return;

        // 1. 移動処理 (物理エンジンと衝突しないよう Rigidbody.MovePosition を使用)
        Vector3 currentPos = rb.position;
        Vector3 newPosition = currentPos;

        // FixedUpdate内では Time.fixedDeltaTime を使用
        newPosition.x += moveInput.x * moveSpeed * Time.fixedDeltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);

        newPosition.z += moveInput.y * moveSpeed * Time.fixedDeltaTime;
        newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

        rb.MovePosition(newPosition);
        
        // 2. 回転処理 (Rigidbody.MoveRotation を使用)
        Quaternion targetRot = startRotation * Quaternion.Euler(currentPitch, 0f, currentRoll);
        rb.MoveRotation(targetRot);
    }

    private void Jump()
    {
        if (GameManager.instance != null && GameManager.instance.isPaused) return;

        // 地面に埋まっている状態での判定抜けを防ぐため、レイの始点をY軸上方向へオフセット
        Vector3 rayOrigin = transform.position + (Vector3.up * rayStartOffset);

        // 指定したLayerMask（地面レイヤー）のコライダーのみを対象にRaycastを計算
        if (Physics.Raycast(rayOrigin, Vector3.down, rayLength, groundLayer))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            AudioSource.PlayClipAtPoint(jumpSound.clip, transform.position); // ジャンプ音を再生
            
            // まだウィリー中でなければ処理を開始
            if (!isWheelieing)
            {
                StartCoroutine(WheelieRoutine());
            }
        }
    }

    // ジャンプ時に軽く傾いて戻るコルーチン
    private IEnumerator WheelieRoutine()
    {
        isWheelieing = true;
        float elapsedTime = 0f;

        while (elapsedTime < wheelieDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // 経過時間を0.0〜1.0に正規化
            float t = elapsedTime / wheelieDuration;
            
            // サイン波(0 → π)を利用して、滑らかに往復させる
            currentPitch = Mathf.Sin(t * Mathf.PI) * wheelieAngle;
            
            yield return null;
        }

        // 回転完了後に値を0にリセット
        currentPitch = 0f;
        isWheelieing = false;
    }
}