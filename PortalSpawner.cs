using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSpawner : MonoBehaviour
{
    private Transform player;
    private PlayerController playerController;
    private GameManager gameManager;

    float distanceToSpanwFromPlayer = 200;

    [SerializeField]
    private GameObject portalGO;

    private GameObject portal;

    private float timer, timerThres = 5;
    AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController>();
        gameManager = transform.GetComponent<GameManager>();
        audioManager = FindObjectOfType<AudioManager>();
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
            SpawnPortal();
        }
    }

    void SpawnPortal()
    {
        audioManager.PlayAudio("Spawn", Vector3.zero);
        float x = Random.Range(-22.3f, 52.1f);

        portal = Instantiate(portalGO, new Vector3(x, player.position.y + Random .Range(-10, 10), player.position.z + distanceToSpanwFromPlayer), Quaternion.identity);

    }
}
