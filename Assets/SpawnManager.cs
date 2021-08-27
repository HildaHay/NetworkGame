using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    Vector2[] spawnPoints;

    // Start is called before the first frame update
    void Start()
    {
        spawnPoints = new Vector2[4];

        spawnPoints[0] = new Vector2(-1, -1);
        spawnPoints[1] = new Vector2(-1, 1);
        spawnPoints[2] = new Vector2(1, -1);
        spawnPoints[3] = new Vector2(1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector2 GetSpawnPosition()
    {
        int r = Random.Range(0, 4);
        return spawnPoints[r];
    }
}
