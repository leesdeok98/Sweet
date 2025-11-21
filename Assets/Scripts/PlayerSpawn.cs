using UnityEngine;

/// <summary>
/// 씬 시작 시 플레이어를 지정한 스폰 지점으로 옮겨주는 스크립트
/// </summary>
public class PlayerSpawn : MonoBehaviour
{
    [Header("스폰 지점 (빈 오브젝트)")]
    [SerializeField] private Transform spawnPoint;

    void Start()
    {
        // 스폰 지점이 설정되어 있으면, 그 위치로 플레이어를 이동
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
        }
        else
        {
            // 스폰 지점을 안 넣었으면, 현재 배치된 위치를 그대로 스폰 위치로 사용
            Debug.LogWarning("[PlayerSpawn] spawnPoint가 비어있습니다. 현재 위치를 스폰 위치로 사용합니다.");
        }
    }
}
