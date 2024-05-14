using TMPro;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

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
    int2 mazeSize = int2(20, 20);

    [SerializeField, Range(0f, 1f)]
    float pickLastProbability = 0.5f;

    [SerializeField, Range(0f, 1f)]
    float openDeadEndProbability = 0.5f;

    [SerializeField, Range(0f, 1f)]
    float openOptionalProbability = 0.5f;

    [SerializeField, Tooltip("Use zero for random seed.")]
    int seed;

    Maze maze;

    void Awake()
    {
        maze = new Maze(mazeSize);

        new FindDiagonalPassagesJob
        {
            maze = maze,
        }.ScheduleParallel(maze.Length, maze.SizeWidth, 
        new GenerateMazeJob
        {
            maze = maze,
            seed = seed != 0 ? seed : Random.Range(1, int.MaxValue),
            pickLastProbability = pickLastProbability,
            openDeadEndProbability = openDeadEndProbability,
            openOptionalProbability = openOptionalProbability
        }.Schedule()
        ).Complete();

        visualization.Visualize(maze, mazeContainer.transform);

        if (seed != 0)
        {
            Random.InitState(seed);
        }

        player.StartNewGame(new Vector3(1f, 0f, 1f));

    }

    void Update()
    {
        player.Move();
    }

    void OnDestroy()
    {
        maze.Dispose();
    }
}