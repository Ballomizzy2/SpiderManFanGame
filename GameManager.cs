using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{

    private const float METERS = 1000;
    public float difficulty, distanceTraveled, initDistance, timeTaken, highScore = 0;
    private Transform player;

    public enum GameState
    {
        Begin, During, End
    }

    public GameState gameState;

    //Infinite Tile Spawning
    [SerializeField]
    private GameObject floorTile;
    [SerializeField]
    private List <GameObject> tiles = new List<GameObject>();
    [SerializeField]
    private List <GameObject> tilesPool = new List<GameObject>();
    
    private int currentPoolIndex = 0;
    UIManager UI;

    private void Awake()
    {
        InstantiatePool();
        UI = FindObjectOfType<UIManager>();
    }
    private void Start()
    {
        gameState = GameState.Begin;
        player = FindObjectOfType<PlayerController>().transform;
        initDistance = player.position.z;        
    }

    private void Update()
    {
        timeTaken += Time.deltaTime * METERS + Random.Range(0, 50);
        distanceTraveled = Mathf.Abs((player.transform.position.z + 5) * METERS / 100);
        
        if(difficulty > 0.2)
            difficulty = timeTaken/ (METERS * 1000);

        if(player.position.y < -200 || player.position.y > 400)
        {
            UI.RestartGame();
        }

    }

    public void StartGame()
    {
        gameState = GameState.During;
    }

    public void StopGame()
    {
        gameState = GameState.End;

        //UPDATE HIGHSCORE
       /* if(distanceTraveled > highScore)
        {
            highScore = distanceTraveled;
        }*/
       StartCoroutine(UI.LoseGame());
    }

    public void SpawnNewFloorTile(GameObject _currentTile)
    {
        GameObject newTile;
        newTile = InstantiateFromPool(_currentTile, new Vector3(_currentTile.transform.position.x, _currentTile.transform.position.y, _currentTile.transform.position.z + 1387), Quaternion.identity);

        tiles.Add(newTile);
        tiles.Add(_currentTile);

        foreach(GameObject tile in tiles)
        {
            if(tile != _currentTile.gameObject && tile != newTile.gameObject)
            {
                tiles.Remove(tile);
                //Destroy(tile);
                StartCoroutine(RecycleObject(tile));
                //tileToDestroy = tile;
                //Task.Run(DestroyObjectInBackground);
            }
        }
    }

    private void InstantiatePool()
    {
        for(int i = 0; i <= 3; i++)
        {
            tilesPool.Add(Instantiate(floorTile, transform));
            tilesPool[i].SetActive(false);
        }
    }

    private GameObject InstantiateFromPool(GameObject recycleObject, Vector3 pos, Quaternion rot)
    {
        if(currentPoolIndex > tilesPool.Count)
            currentPoolIndex = 0;
        GameObject tileToReturn = tilesPool[currentPoolIndex].gameObject;
        tileToReturn.SetActive(true);
        tileToReturn.transform.position = pos;
        tileToReturn.transform.rotation = rot;

        currentPoolIndex++;
        return tileToReturn;
    }

    private IEnumerator RecycleObject(GameObject obj)
    {
        yield return new WaitForSeconds(2);
        obj.SetActive(false);
        tilesPool[currentPoolIndex] = obj;
    }
}
