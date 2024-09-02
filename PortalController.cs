using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalController : MonoBehaviour
{
    private Transform player;
    private GameManager gameManager;
    private DimensionManager dimensionManager;
    [SerializeField]
    private Outline outline;

    [SerializeField]
    private GameObject[] collisionVfx = new GameObject[3];
    [SerializeField]
    List<Color> colorList = new List<Color>();

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        dimensionManager = FindObjectOfType<DimensionManager>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        outline = player.GetComponentInChildren<Outline>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.gameState != GameManager.GameState.During)
        {
            Destroy(gameObject);
            return;
        }
        AutoDestroy();
    }

    private void Explode()
    {
        //spawn vfx
        GameObject newVFX = Instantiate(collisionVfx[Random.Range(0, 4)], player.transform.position + Vector3.forward * 10, Quaternion.identity);
        Destroy(newVFX, 3);
        StartCoroutine(SomeTransitionFX());
        Destroy(this.gameObject);
    }

    private IEnumerator SomeTransitionFX()
    {
        outline.enabled = true;
        outline.OutlineColor = colorList[Random.Range(0, colorList.Count)];
        outline.OutlineWidth = Random.Range(0f, 10f);
        yield return new WaitForSeconds(2);
        outline.enabled = false;
    }

    private void AutoDestroy()
    {
        if (transform.position.z < player.position.z - 40)
            Destroy(gameObject);
    }

    public void Collided()
    {
        Debug.Log("Called");
        dimensionManager.SwitchDimension();
        Explode();
    }
  /*  private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            dimensionManager.SwitchDimension();
            Explode();
        }
        else
            Destroy(gameObject);

    }*/
}
