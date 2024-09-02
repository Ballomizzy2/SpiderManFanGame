using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Transform player;
    private PlayerController playerController;
    private Rigidbody rb;
    private GameManager gameManager;

    [SerializeField]
    private List<GameObject> explodeVfx = new List<GameObject>();
    AudioManager audioManager;


    [SerializeField]
    private float moveSpeed;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController>();
        audioManager = FindObjectOfType<AudioManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.gameState != GameManager.GameState.During)
        {
            Explode();
            return;
        }
        Chase();
        AutoDestroy();
    }

    private void Chase()
    {
        if (transform.position.z > player.position.z)
        {
            transform.LookAt(player.position);
            transform.Translate(Vector3.forward * moveSpeed * Random.Range(1F, 3.5F) * Time.deltaTime);
        }
    }

    private void Explode()
    {
        //spawn vfx
        audioManager.PlayAudio("Explode", this.transform.position);
        Destroy(Instantiate(explodeVfx[Random.Range(0, 3)], transform.position, Quaternion.identity), 6);
        Destroy(this.gameObject);
    }

    private void AutoDestroy()
    {
        if (transform.position.z < player.position.z - 40)
            Explode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            playerController.Die();
            Explode();
        }
        else
            Explode();

    }
}
