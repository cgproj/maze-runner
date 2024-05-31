using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Mathematics;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform mazeParent = null;
    [SerializeField] private GameObject playerPrefab = null;
    [SerializeField] private Transform playerSpawnPoint = null;
    [SerializeField] private MenuController menuController = null;
    [SerializeField] private TMP_Text gameOverText = null;

    [Header("Maze Generation")]
    [SerializeField] private MazeVisualization mazeVisualization = null;
    [SerializeField] private int mazeSeed = 0;
    [SerializeField] private float pickLastProbability = 0.1f;
    [SerializeField] private float openDeadEndProbability = 0.1f;
    [SerializeField] private float openOptionalProbability = 0.05f;

    private GameObject playerInstance;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            GenerateMaze();
            SpawnPlayer();
        }
    }

    public void StartNewGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void LoadSavedGame()
    {
        SceneManager.LoadScene("Game");
        // Implement loading saved game logic here
    }

    public void ShowGameOver()
    {
        gameOverText.gameObject.SetActive(true);
        // Implement game over logic here
    }

    private void GenerateMaze()
    {
        Maze maze = new Maze(new int2(10, 10)); // Change the size as desired
        GenerateMazeJob job = new GenerateMazeJob
        {
            maze = maze,
            seed = mazeSeed,
            pickLastProbability = pickLastProbability,
            openDeadEndProbability = openDeadEndProbability,
            openOptionalProbability = openOptionalProbability
        };
        job.Execute();

        mazeVisualization.Visualize(maze, mazeParent);
    }

    private void SpawnPlayer()
    {
        playerInstance = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
