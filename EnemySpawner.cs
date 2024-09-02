using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private Transform player;
    private PlayerController playerController;
    private GameManager gameManager;

    float distanceToSpanwFromPlayer = 200;

    [SerializeField]
    private GameObject drone1, drone2;

    private GameObject droneToSpawn;
    private float timer, timerThres = 5;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController>();
        gameManager = transform.GetComponent<GameManager>();
    }
    private void Update()
    {
        if (!(gameManager.gameState == GameManager.GameState.During))
            return;
        if (timer < timerThres)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0;
            SpawnEnemy();
            timerThres -= gameManager.difficulty;
        }
    }

    void SpawnEnemy()
    {
        int rand = Random.Range(0, 2);
        switch (rand)
        {
            case 0:
                droneToSpawn = drone1;
                break;
            case 1:
                droneToSpawn = drone2;
                break;
            default:
                break;
        }

        droneToSpawn = Instantiate(droneToSpawn, new Vector3(player.position.x, player.position.y, player.position.z + distanceToSpanwFromPlayer), Quaternion.identity) ;
    }
}
