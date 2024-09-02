using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private GameManager gameManager;


    [SerializeField]
    private TextMeshProUGUI distanceTravelled, highScore;

    [SerializeField]
    private GameObject menuUI;
    
    [SerializeField]private Animator menuAnim;
    [SerializeField]
    private Button play, exit;

    AudioManager audioManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        audioManager = FindObjectOfType<AudioManager>();
    }

    private void Update()
    {
        if(gameManager.gameState == GameManager.GameState.During)
        {
            distanceTravelled.text = gameManager.distanceTraveled.ToString("N0");
            highScore.text = "HI: " + gameManager.highScore.ToString("N0");
        }
    }

    public void ExitGame()
    {
        audioManager.PlayAudio("Click", Vector3.zero);
        Application.Quit();
    }

    public void RestartGame()
    {
        SceneManager.LoadSceneAsync(0);
    }

    public void StartGame()
    {
        audioManager.PlayAudio("Click", Vector3.zero);
        if (gameManager.gameState == GameManager.GameState.End)
            RestartGame();
        menuAnim.SetTrigger("End");
        Invoke("DisableMenu", 2);
        gameManager.StartGame();
    }

    private void DisableMenu()
    {
        menuUI.SetActive(false);
    }

    public IEnumerator LoseGame()
    {
        yield return new WaitForSeconds(2);
        menuUI.SetActive(true);
    }

}
