using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum MazeFlags
{
    Empty = 0,

    PassageN = 0b0001,
    PassageE = 0b0010,
    PassageS = 0b0100,
    PassageW = 0b1000,

    PassagesStraight = 0b1111,

    PassageNE = 0b0001_0000,
    PassageSE = 0b0010_0000,
    PassageSW = 0b0100_0000,
    PassageNW = 0b1000_0000,

    PassagesDiagonal = 0b1111_0000
}