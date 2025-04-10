using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    public GameObject[] fruitPrefabs;
    public Transform[] spawnPoints;

    public float minDelay = .1f;
    public float maxDelay = 1f;

    public VoidEventSO gameEndEventSO;
    private bool continueSpawning = true;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(SpawnFruits());
    }

    private void OnEnable()
    {
        gameEndEventSO.OnEventRaised += StopSpawning;
    }

    private void OnDisable()
    {
        gameEndEventSO.OnEventRaised -= StopSpawning;
    }

    private void StopSpawning()
    {
        continueSpawning = false;
    }

    IEnumerator SpawnFruits()
    {
        while (continueSpawning)
        {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            int spawnIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[spawnIndex];

            GameObject spawnedFruit = Instantiate(fruitPrefabs[Random.Range(0, fruitPrefabs.Length)], spawnPoint.position, spawnPoint.rotation);
            Destroy(spawnedFruit, 5f);
        }

        yield break;
    }
}
