using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform[] spawnPoint;
    public EnemySpawnRule[] spawnRules;

    void Awake()
    {
        spawnPoint = GetComponentsInChildren<Transform>();

        foreach (var rule in spawnRules)
        {
            rule.lastSpawnTimer = 0f;
            rule.hasSpawned = false;
        }
    }
    void Update()
    {
        //if (GameManager.instance.hasEnemiesCleared)
        //{
        //    return;
        //}

        if (GameManager.instance.isGameWon || GameManager.instance.isGameOver)
        {
            return;
        }

        float currentGameTime = GameManager.instance.gameTime;

        foreach (var rule in spawnRules)
        {
            if (currentGameTime >= rule.startTime && currentGameTime <= rule.endTime)
            {
                if (rule.spawnInterval <= 0.1f)
                {
                    if (!rule.hasSpawned)
                    {
                        Spawn(rule);
                        rule.hasSpawned = true;
                    }
                }
                    else
                    {
                        rule.lastSpawnTimer += Time.deltaTime;

                        if (rule.lastSpawnTimer >= rule.spawnInterval)
                        {
                            rule.lastSpawnTimer = 0f;
                            Spawn(rule);
                        }
                    }
            }
        }
    }
    void Spawn(EnemySpawnRule rule)
    {
        //GameObject enemy = GameManager.instance.pool.Get(rule.prefabIndex);
        //enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;

        SpawnData dataForEnemy = new SpawnData();
        dataForEnemy.spriteType = rule.spriteType;
        dataForEnemy.health = rule.health;
        dataForEnemy.speed = rule.speed;
        dataForEnemy.dps = rule.dps;

        for (int i = 0; i < rule.spawnCount; i++)
        {
            GameObject enemy = GameManager.instance.pool.Get(rule.prefabIndex);
            enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
            enemy.GetComponent<Enemy>().Init(dataForEnemy);
        }
    }
}

[System.Serializable]
public class SpawnData
{
    public float spawnTime;
    public int spriteType;
    public int health;
    public float speed;
    public float dps;
}

[System.Serializable]
public class EnemySpawnRule
{
    public string description = "New Rule";
    public int prefabIndex;
    public float startTime;
    public float endTime;
    public int spawnCount;
    public float spawnInterval;

    public int spriteType;
    public int health;
    public float speed;
    public float dps;

    [HideInInspector]
    public bool hasSpawned = false;

    [HideInInspector]
    public float lastSpawnTimer;
}
