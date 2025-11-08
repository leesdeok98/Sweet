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

    public void ClearAllEnemies()
    {
        Debug.Log("호출되엇다^^");
        foreach (List<GameObject> pool in pools)
        {
            foreach (GameObject item in pool)
            {
                if (item.activeSelf)
                {
                    Debug.Log($"활성화된 몬스터: {item.name}, 태그: {item.tag}");
                }

                if (item.activeSelf && !item.CompareTag("Boss"))
                {
                    item.SetActive(false);
                }
            }
        }
    }
}
