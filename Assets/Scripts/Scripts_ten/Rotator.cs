using UnityEngine;

public class Rotator : MonoBehaviour
{
    [Header("回転速度（度/秒）")]
    public float rotationSpeed = 90f;

    void Update()
    {
        // Space.Worldを指定して、親オブジェクトの傾きに関わらず常にワールドのY軸を中心に回転させます
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }
}