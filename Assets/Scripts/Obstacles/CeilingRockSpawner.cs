using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeilingRockSpawner : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float spawnTimeMin = 1; 
    [SerializeField] private float spawnTimeMax = 3;
    [SerializeField] private float timeBetwenRocksMin = 0.4f;
    [SerializeField] private float timeBetwenRocksMax = 0.9f;

    [Header("Inspector Things")]
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private Transform[] rockSpawnTransforms;

    [Header("Listen Events")]
    [SerializeField] private VoidEventSO rockSpawnEventSO;

    private float SpawnTime => Random.Range(spawnTimeMin, spawnTimeMax);
    private float RockSpawnCooldownTime => Random.Range(timeBetwenRocksMin, timeBetwenRocksMax);

    private float overallSpawningTimer;
    private float rockSpawnCooldownTimer;

    private void OnEnable()
    {
        rockSpawnEventSO.OnEventRaised += SpawnRocks;
    }

    private void OnDisable()
    {
        rockSpawnEventSO.OnEventRaised -= SpawnRocks;
    }

    [ContextMenu("Spawn Rocks")]
    private void SpawnRocks()
    {
        overallSpawningTimer += SpawnTime;
    }

    private void Update()
    {
        if (overallSpawningTimer > 0)
        {
            overallSpawningTimer -= Time.deltaTime;
            rockSpawnCooldownTimer -= Time.deltaTime;

            if (rockSpawnCooldownTimer <= 0)
            {
                SpawnARock();
                rockSpawnCooldownTimer = RockSpawnCooldownTime;
            }
        }
    }

    private void SpawnARock()
    {
        Transform spawnTransform = GetRandomSpawnTransform();
        if (spawnTransform == null) return;

        Quaternion rot = Quaternion.identity;
        rot.z = Random.rotation.z;
        Instantiate(rockPrefab, spawnTransform.position, rot);
    }

    private Transform GetRandomSpawnTransform()
    {
        if (rockSpawnTransforms == null || rockSpawnTransforms.Length == 0) return null;

        return rockSpawnTransforms[Random.Range(0, rockSpawnTransforms.Length)];
    }
}
