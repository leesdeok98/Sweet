using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;           // 플레이어
    public Vector3 offset = new Vector3(0, 0, -10);
    [Range(0f, 1f)]
    public float followLerp = 0.1f;    // 카메라 부드러움

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, followLerp);
    }
}
