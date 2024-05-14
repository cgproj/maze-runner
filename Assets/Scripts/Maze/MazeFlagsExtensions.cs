using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MazeFlagsExtensions
{
    public static bool Has(this MazeFlags flags, MazeFlags mask) =>
        (flags & mask) == mask;

    public static bool HasAny(this MazeFlags flags, MazeFlags mask) =>
        (flags & mask) != 0;

    public static bool HasNot(this MazeFlags flags, MazeFlags mask) =>
        (flags & mask) != mask;

    public static bool HasExactlyOne(this MazeFlags flags) =>
        flags != 0 && (flags & (flags - 1)) == 0;

    public static MazeFlags With(this MazeFlags flags, MazeFlags mask) =>
        flags | mask;

    public static MazeFlags Without(this MazeFlags flags, MazeFlags mask) =>
        flags & ~mask;

    public static MazeFlags StraightPassages(this MazeFlags flags) =>
        flags & MazeFlags.PassagesStraight;

    public static MazeFlags DiagonalPassages(this MazeFlags flags) =>
    flags & MazeFlags.PassagesDiagonal;

    public static MazeFlags RotatedDiagonalPassages(this MazeFlags flags, int rotation)
    {
        int bits = (int)(flags & MazeFlags.PassagesDiagonal);
        bits = (bits >> rotation) | (bits << (4 - rotation));
        return (MazeFlags)bits & MazeFlags.PassagesDiagonal;
    }
}
