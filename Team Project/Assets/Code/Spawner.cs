using Unity.VisualScripting.AssemblyQualifiedNameParser;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject objectToSpawn;
   // [SerializeField] int numToSpawn;   Since this spawner is designed to spawn one enemy at each location we just need the number of locations
    [SerializeField] int timeBetweenSpawns;
    [SerializeField] Transform[] spawnPos;

    float spawnTimer;
    int spawnCount;
    bool startSpawning;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        gameManager.instance.UpdatedGameGoal(spawnPos.Length);
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (startSpawning)
        {
            spawnTimer += Time.deltaTime;
            if (spawnCount < spawnPos.Length && spawnTimer >= timeBetweenSpawns)
            {
                spawn();
            }
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            startSpawning = true;
        }
    }

    void spawn()
    { //using spwawn count to have each enemy have its own position
      // int spawnInt = Random.Range(0, spawnPos.Length);
        GameObject spawnedEnemy = Instantiate(objectToSpawn, spawnPos[spawnCount].position, spawnPos[spawnCount].rotation);

        // Set the "Enemy" tag to the spawned enemy
        spawnedEnemy.tag = "Enemy";

        spawnCount++;
       
        spawnTimer = 0;
    }



}
