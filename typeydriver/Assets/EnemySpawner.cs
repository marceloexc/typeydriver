using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int maxEnemies = 5;
    public float spawnRadius = 10f;

    private int currentEnemies = 0;

    void Start()
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        Vector3 spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;
        spawnPos.y = transform.position.y; // keep on ground level

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        healthHandler enemyScript = enemy.GetComponent<healthHandler>();
        enemyScript.spawner = this;

        currentEnemies++;
    }

    public void OnEnemyKilled()
    {
        currentEnemies--;

        if (currentEnemies < maxEnemies)
        {
            SpawnEnemy();
        }
    }
}
