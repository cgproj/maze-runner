using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MazeVisualization : ScriptableObject
{
    [SerializeField]
    MazeCellObject deadEnd, straight, cornerClosed, cornerOpen, tJunctionClosed, tJunctionClosedRight, tJunctionClosedLeft, tJunctionOpen, xJunctionClosed, xJunctionOpen, xJunctionSingle, xJunctionDoubleDiag, xJunctionDoubleStr, xJunctionTriple;

    public void Visualize(Maze maze, Transform parrent)
    {
        for (int i = 0; i < maze.Length; i++)
        {
            (MazeCellObject, int) prefabWithRotation = GetPrefab(maze[i]);
            MazeCellObject instance = prefabWithRotation.Item1.GetInstance();
            instance.transform.SetParent(parrent);
            instance.transform.SetPositionAndRotation(maze.IndexToWorldPosition(i), rotations[prefabWithRotation.Item2]);

        }
    }

    (MazeCellObject, int) GetPrefab(MazeFlags flags) => flags.StraightPassages() switch
    {
        MazeFlags.PassageN => (deadEnd, 0),
        MazeFlags.PassageE => (deadEnd, 1),
        MazeFlags.PassageS => (deadEnd, 2),
        MazeFlags.PassageW => (deadEnd, 3),

        MazeFlags.PassageN | MazeFlags.PassageS => (straight, 0),
        MazeFlags.PassageE | MazeFlags.PassageW => (straight, 1),

        MazeFlags.PassageN | MazeFlags.PassageE => GetCorner(flags, 0),
        MazeFlags.PassageE | MazeFlags.PassageS => GetCorner(flags, 1),
        MazeFlags.PassageS | MazeFlags.PassageW => GetCorner(flags, 2),
        MazeFlags.PassageW | MazeFlags.PassageN => GetCorner(flags, 3),

        MazeFlags.PassagesStraight & ~MazeFlags.PassageW => GetTJunction(flags, 0),
        MazeFlags.PassagesStraight & ~MazeFlags.PassageN => GetTJunction(flags, 1),
        MazeFlags.PassagesStraight & ~MazeFlags.PassageE => GetTJunction(flags, 2),
        MazeFlags.PassagesStraight & ~MazeFlags.PassageS => GetTJunction(flags, 3),

        _ => GetXJunction(flags)
    };

    (MazeCellObject, int) GetCorner(MazeFlags flags, int rotation) => (
        flags.HasAny(MazeFlags.PassagesDiagonal) ? cornerOpen : cornerClosed, rotation
    );

    (MazeCellObject, int) GetTJunction(MazeFlags flags, int rotation) => (
        flags.RotatedDiagonalPassages(rotation) switch
        {
            MazeFlags.Empty => tJunctionClosed,
            MazeFlags.PassageNE => tJunctionClosedRight,
            MazeFlags.PassageSE => tJunctionClosedLeft,
            _ => tJunctionOpen
        },
        rotation
    );

    (MazeCellObject, int) GetXJunction(MazeFlags flags) =>
        flags.DiagonalPassages() switch
        {
            MazeFlags.Empty => (xJunctionClosed, 0),

            MazeFlags.PassageNE => (xJunctionTriple, 0),
            MazeFlags.PassageSE => (xJunctionTriple, 1),
            MazeFlags.PassageSW => (xJunctionTriple, 2),
            MazeFlags.PassageNW => (xJunctionTriple, 3),

            MazeFlags.PassageNE | MazeFlags.PassageSE => (xJunctionDoubleStr, 0),
            MazeFlags.PassageSE | MazeFlags.PassageSW => (xJunctionDoubleStr, 1),
            MazeFlags.PassageSW | MazeFlags.PassageNW => (xJunctionDoubleStr, 2),
            MazeFlags.PassageNW | MazeFlags.PassageNE => (xJunctionDoubleStr, 3),

            MazeFlags.PassageNE | MazeFlags.PassageSW => (xJunctionDoubleDiag, 0),
            MazeFlags.PassageSE | MazeFlags.PassageNW => (xJunctionDoubleDiag, 1),

            MazeFlags.PassagesDiagonal & ~MazeFlags.PassageNE => (xJunctionSingle, 0),
            MazeFlags.PassagesDiagonal & ~MazeFlags.PassageSE => (xJunctionSingle, 1),
            MazeFlags.PassagesDiagonal & ~MazeFlags.PassageSW => (xJunctionSingle, 2),
            MazeFlags.PassagesDiagonal & ~MazeFlags.PassageNW => (xJunctionSingle, 3),

            _ => (xJunctionOpen, 0),
        };

    static Quaternion[] rotations =
    {
        Quaternion.identity,
        Quaternion.Euler(0f, 90f, 0f),
        Quaternion.Euler(0f, 180f, 0f),
        Quaternion.Euler(0f, 270f, 0f)
    };
}

