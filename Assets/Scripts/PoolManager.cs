using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public GameObject[] prefabs;
    List<GameObject>[] pools;


    void Awake()
    {
        pools = new List<GameObject>[prefabs.Length];

        for (int index = 0; index < pools.Length; index++)
        {
            pools[index] = new List<GameObject>();
        }
    }

    public GameObject Get(int index)
    {
        GameObject select = null;

        foreach (GameObject item in pools[index])
        {
            if (!item.activeSelf)
            {
                select = item;
                select.SetActive(true);
                break;
            }
        }

        if (!select)
        {
            select = Instantiate(prefabs[index], transform);
            pools[index].Add(select);
        }

        return select;
    }

   // PoolManager.cs

    public void ClearAllEnemies()
    {
    Debug.Log("호출됨^^");

    // pools 안의 각 풀을 순회
    for (int poolIndex = 0; poolIndex < pools.Length; poolIndex++)
    {
        List<GameObject> pool = pools[poolIndex];

        // 리스트를 뒤에서부터 도는 이유: RemoveAt 해도 인덱스 안 꼬이게
        for (int i = pool.Count - 1; i >= 0; i--)
        {
            GameObject item = pool[i];

            // 1) 이미 Destroy()된 오브젝트면 리스트에서 제거하고 패스
            if (item == null)
            {
                pool.RemoveAt(i);
                continue;
            }

            // 2) 비활성화된 애면 그냥 넘어감
            if (!item.activeSelf)
                continue;

            // 3) 활성화된 애만 로그 찍기
            Debug.Log($"활성화된 오브젝트: {item.name}, 태그: {item.tag}");

            // 4) 보스가 아니면 비활성화
            if (!item.CompareTag("Boss"))
            {
                item.SetActive(false);
            }
        }
    }
}

}
