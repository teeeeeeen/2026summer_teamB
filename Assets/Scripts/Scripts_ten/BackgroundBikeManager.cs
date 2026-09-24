using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 

public class BackgroundBikeManager : MonoBehaviour
{
    [System.Serializable]
    public class BikePoint
    {
        [Tooltip("このスコアを上回ったら出現")]
        public int requiredScore;
        [Tooltip("このタイミングで出現させる固有のバイクプレハブ")]
        public GameObject bikePrefab;
        [Tooltip("画面外の生成位置（Empty等）")]
        public Transform spawnPoint;
        [Tooltip("画面内の最終目的地（Empty等）")]
        public Transform targetPoint;
        [HideInInspector]
        public bool isSpawned = false; 
        [HideInInspector]
        public GameObject spawnedBike; 
    }

    [Header("背景バイクの設定")]
    [Tooltip("バイクの出現条件と位置、固有のプレハブ（最大3つまで設定）")]
    public BikePoint[] bikePoints = new BikePoint[3];

    [Header("移動・ジャンプ設定")]
    [Tooltip("画面外から目的地へ移動するまでにかかる時間（秒）")]
    public float moveDuration = 2.0f;
    
    [Tooltip("プレイヤーと同じジャンプ力")]
    public float jumpForce = 5.0f;
    [Tooltip("ジャンプの重力（落ちる速さ）")]
    public float gravity = 9.81f;

    [Header("ウィリー設定")]
    [Tooltip("ジャンプ時の傾き角度（プラスで指定するとワールドX軸のマイナス方向へ回転します）")]
    public float wheelieAngle = 30f;    
    [Tooltip("傾いてから戻るまでの時間（秒）")]
    public float wheelieDuration = 0.6f; 

    private List<BikeState> activeBikes = new List<BikeState>();

    private class BikeState
    {
        public GameObject bikeObj;
        public float defaultY; 
        public float velocityY;
        public bool isJumping;
        public Quaternion baseRotation; // 生成時の初期回転
        public float currentPitch;     // ウィリー用の回転量
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.isPaused) return;

        CheckScoreAndSpawn();
        HandleJumpInput();
        UpdateJumpsAndRotations();
    }

    private void CheckScoreAndSpawn()
    {
        if (SoupManager.instance == null) return;

        int currentScore = SoupManager.instance.totalScore;

        foreach (var point in bikePoints)
        {
            if (!point.isSpawned && currentScore >= point.requiredScore)
            {
                point.isSpawned = true;
                
                if (point.bikePrefab != null)
                {
                    StartCoroutine(SpawnAndMoveBike(point));
                }
            }
        }
    }

    private IEnumerator SpawnAndMoveBike(BikePoint point)
    {
        if (point.spawnPoint == null || point.targetPoint == null) yield break;

        GameObject newBike = Instantiate(point.bikePrefab, point.spawnPoint.position, point.spawnPoint.rotation);
        point.spawnedBike = newBike;

        float timer = 0f;
        Vector3 startPos = point.spawnPoint.position;
        Vector3 endPos = point.targetPoint.position;

        while (timer < moveDuration)
        {
            if (GameManager.instance == null || !GameManager.instance.isPaused)
            {
                timer += Time.deltaTime;
                float t = timer / moveDuration;
                if (t > 1f) t = 1f;

                // だんだん遅くなるイージング（EaseOutCubic）
                float easeT = 1f - Mathf.Pow(1f - t, 3f);
                newBike.transform.position = Vector3.Lerp(startPos, endPos, easeT);
            }
            yield return null;
        }

        newBike.transform.position = endPos;

        BikeState newState = new BikeState
        {
            bikeObj = newBike,
            defaultY = newBike.transform.position.y,
            velocityY = 0f,
            isJumping = false,
            baseRotation = newBike.transform.rotation,
            currentPitch = 0f
        };
        activeBikes.Add(newState);
    }

    private void HandleJumpInput()
    {
        if (activeBikes.Count == 0) return;

        bool isJumpPressed = false;
        if (Keyboard.current != null) isJumpPressed |= Keyboard.current.spaceKey.wasPressedThisFrame;
        if (Gamepad.current != null) isJumpPressed |= Gamepad.current.buttonEast.wasPressedThisFrame; 

        if (isJumpPressed)
        {
            foreach (var state in activeBikes)
            {
                if (!state.isJumping)
                {
                    state.isJumping = true;
                    state.velocityY = jumpForce;

                    StartCoroutine(WheelieRoutine(state));
                }
            }
        }
    }

    private IEnumerator WheelieRoutine(BikeState state)
    {
        float elapsedTime = 0f;

        while (elapsedTime < wheelieDuration)
        {
            if (GameManager.instance == null || !GameManager.instance.isPaused)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / wheelieDuration;
                
                state.currentPitch = Mathf.Sin(t * Mathf.PI) * wheelieAngle;
            }
            yield return null;
        }

        state.currentPitch = 0f;
    }

    private void UpdateJumpsAndRotations()
    {
        foreach (var state in activeBikes)
        {
            if (state.bikeObj == null) continue;

            if (state.isJumping)
            {
                state.velocityY -= gravity * Time.deltaTime;
                
                Vector3 currentPos = state.bikeObj.transform.position;
                currentPos.y += state.velocityY * Time.deltaTime;

                if (currentPos.y <= state.defaultY)
                {
                    currentPos.y = state.defaultY;
                    state.isJumping = false;
                    state.velocityY = 0f;
                }

                state.bikeObj.transform.position = currentPos;
            }

            // 【変更】ワールド座標のX軸（Vector3.right）を中心に、マイナス方向へ回転させる
            Quaternion worldXRot = Quaternion.AngleAxis(state.currentPitch, Vector3.right);
            state.bikeObj.transform.rotation = worldXRot * state.baseRotation;
        }
    }
}