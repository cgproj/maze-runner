using TMPro;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Mathematics.math;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour
{
    [SerializeField]
    MazeVisualization visualization;

    [SerializeField]
    GameObject mazeContainer;

    [SerializeField]
    Player player;

    [SerializeField]
    Agent[] agents;

    [SerializeField]
    int2 mazeSize = int2(20, 20);

    [SerializeField, Range(0f, 1f)]
    float pickLastProbability = 0.5f;

    [SerializeField, Range(0f, 1f)]
    float openDeadEndProbability = 0.5f;

    [SerializeField, Range(0f, 1f)]
    float openOptionalProbability = 0.5f;

    [SerializeField, Tooltip("Use zero for random seed.")]
    int seed;

    [SerializeField]
    TextMeshPro displayText;

    [SerializeField]
    TextMeshPro scoreText;

    bool isPlaying;
    float elapsedTime;

    MazeCellObject[] cellObjects;

    Maze maze;
    Scent scent;

    void StartNewGame()
    {
        isPlaying = true;
        elapsedTime = 0f;
        displayText.gameObject.SetActive(false);
        maze = new Maze(mazeSize);
        scent = new Scent(maze);
        new FindDiagonalPassagesJob
        {
            maze = maze
        }.ScheduleParallel(
            maze.Length, maze.SizeWidth, new GenerateMazeJob
            {
                maze = maze,
                seed = seed != 0 ? seed : Random.Range(1, int.MaxValue),
                pickLastProbability = pickLastProbability,
                openDeadEndProbability = openDeadEndProbability,
                openOptionalProbability = openOptionalProbability
            }.Schedule()
        ).Complete();

        if (cellObjects == null || cellObjects.Length != maze.Length)
        {
            cellObjects = new MazeCellObject[maze.Length];
        }
        visualization.Visualize(maze, mazeContainer.transform, cellObjects);

        if (seed != 0)
        {
            Random.InitState(seed);
        }

        player.StartNewGame(maze.CoordinatesToWorldPosition(
            int2(Random.Range(0, mazeSize.x / 4), Random.Range(0, mazeSize.y / 4))
        ));

        int2 halfSize = mazeSize / 2;
        for (int i = 0; i < agents.Length; i++)
        {
            var coordinates =
                int2(Random.Range(0, mazeSize.x), Random.Range(0, mazeSize.y));
            if (coordinates.x < halfSize.x && coordinates.y < halfSize.y)
            {
                if (Random.value < 0.5f)
                {
                    coordinates.x += halfSize.x;
                }
                else
                {
                    coordinates.y += halfSize.y;
                }
            }
            agents[i].StartNewGame(maze, coordinates);
        }
    }

    void Update()
    {
        if (isPlaying)
        {
            UpdateGame();
            UpdateTimer();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            StartNewGame();
            UpdateGame();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
            OnDestroy();
        }
    }

    void UpdateGame()
    {
        Vector3 playerPosition = player.Move();
        NativeArray<float> currentScent = scent.Disperse(maze, playerPosition);
        for (int i = 0; i < agents.Length; i++)
        {
            Vector3 agentPosition = agents[i].Move(currentScent);
            if (
                new Vector2(
                    agentPosition.x - playerPosition.x,
                    agentPosition.z - playerPosition.z
                ).sqrMagnitude < 1f
            )
            {
                EndGame(agents[i].TriggerMessage);
                return;
            }
        }
    }

    void UpdateTimer()
    {
        elapsedTime += Time.deltaTime;
        int secondsElapsed = (int)elapsedTime;
        scoreText.text = $"{secondsElapsed}";
    }

    void EndGame(string message)
    {
        isPlaying = false;
        displayText.text = message + $"\nYour time: {elapsedTime:F2} seconds.\nPress space to restart or escape to return to main menu";
        displayText.gameObject.SetActive(true);
        for (int i = 0; i < agents.Length; i++)
        {
            agents[i].EndGame();
        }

        for (int i = 0; i < cellObjects.Length; i++)
        {
            cellObjects[i].Recycle();
        }

        OnDestroy();
    }

    void OnDestroy()
    {
        maze.Dispose();
        scent.Dispose();
    }
}
