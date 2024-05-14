using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct Maze
{
    [NativeDisableParallelForRestriction]
    NativeArray<MazeFlags> cells;

    int2 size;

    //Width * Height
    public int Length => cells.Length;

    //Width of maze East - West
    public int SizeWidth => size.x;

    //Height of the maze North - South
    public int SizeHeight => size.y;

    //Step to move to a next "row" - move the whole row to go into the lower one
    public int StepN => size.x;

    public int StepE => 1;

    //Step to move to a previous "row" - go back the whole row to go into the upper one
    public int StepS => -size.x;

    public int StepW => -1;

    public Maze(int2 size)
    {
        this.size = size;
        cells = new NativeArray<MazeFlags>(size.x * size.y, Allocator.Persistent);
    }

    public void Dispose()
    {
        if (cells.IsCreated)
        {
            cells.Dispose();
        }
    }

    public MazeFlags this[int index]
    {
        get => cells[index];
        set => cells[index] = value;
    }

    public MazeFlags Set(int index, MazeFlags mask) =>
        cells[index] = cells[index].With(mask);

    public MazeFlags Unset(int index, MazeFlags mask) =>
        cells[index] = cells[index].Without(mask);

    public int2 IndexToCoordinates(int index)
    {
        int2 coordinates;
        coordinates.y = index / size.x;
        coordinates.x = index - size.x * coordinates.y;
        return coordinates;
    }

    public Vector3 CoordinatesToWorldPosition(int2 coordinates, float y = 0f) =>
        new Vector3(
            2f * coordinates.x + 1f - size.x,
            y,
            2f * coordinates.y + 1f - size.y
        );

    public Vector3 IndexToWorldPosition(int index, float y = 0f) =>
        CoordinatesToWorldPosition(IndexToCoordinates(index), y);
}
