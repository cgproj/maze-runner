using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


/*
 * Job generating the maze for the game. Using burstCompile to increase performance and speed, as well as allow for multithreading. Using the growing three alorigthm
 */

[BurstCompile]
public struct GenerateMazeJob : IJob
{
    public Maze maze;

    public int seed;

    public float pickLastProbability, openDeadEndProbability, openOptionalProbability;

    public void Execute()
    {
        var random = new Random((uint)seed);
        
        //Scratchpad - used to store selected paths for a cell and it's selected neighbour
        var scratchpad = new NativeArray<(int, MazeFlags, MazeFlags)>(
            4, Allocator.Temp, NativeArrayOptions.UninitializedMemory
        );

        //List of active cells indexes (path)
        var activeIndices = new NativeArray<int>(
            maze.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory
        );

        //Backtracking indexes for the activeIndices
        int firstActiveIndex = 0, lastActiveIndex = 0;

        //Selecting the starting cell
        activeIndices[firstActiveIndex] = random.NextInt(maze.Length);

        while (firstActiveIndex <= lastActiveIndex)
		{
            bool pickLast = random.NextFloat() < pickLastProbability;
            int randomActiveIndex, index;
            if (pickLast)
            {
                //We select the last added cell to the stack
                randomActiveIndex = 0;
                index = activeIndices[lastActiveIndex];
            }
            else
            {
                //We select the random active cell from the stack
                randomActiveIndex = random.NextInt(firstActiveIndex, lastActiveIndex + 1);
                index = activeIndices[randomActiveIndex];
            }

            //Locate aviable passages
            int availablePassageCount = FindAvailablePassages(index, scratchpad);
			if (availablePassageCount <= 1)
			{
                if (pickLast)
                {
                    //No further path, backtracking to the previous cell
                    lastActiveIndex -= 1;
                }
                else
                {
                    //Select random cell from active cells list
                    //Select the first cell from the active cells and set it a the randomActiveIndex
                    activeIndices[randomActiveIndex] = activeIndices[firstActiveIndex++];
                }
            }
			if (availablePassageCount > 0)
			{
                //Selecting passage from one cell to another if any aviable
				(int, MazeFlags, MazeFlags) passage =
					scratchpad[random.NextInt(0, availablePassageCount)];
				maze.Set(index, passage.Item2);
				maze[passage.Item1] = passage.Item3;
                //Set the activeIndices at last inedex to newly created path
                activeIndices[++lastActiveIndex] = passage.Item1; 
			}
		}

        //Open dead ends
        if (openDeadEndProbability > 0)
        {
            random = OpenDeadEnds(random, scratchpad);
        }

        //Open arbitrary passages
        if (openOptionalProbability > 0)
        {
            random = OpenOptionalPassages(random);
        }
    }

    //Method checking for aviable passages. We check if the neigbour cells are univsited -> their mask is 0000 and populate them
    int FindAvailablePassages(
        int index, NativeArray<(int, MazeFlags, MazeFlags)> scratchpad
    )
    {
        int2 coordinates = maze.IndexToCoordinates(index);
        int count = 0;
        if (coordinates.x + 1 < maze.SizeWidth)
        {
            int i = index + maze.StepE;
            if (maze[i] == MazeFlags.Empty)
            {
                scratchpad[count++] = (i, MazeFlags.PassageE, MazeFlags.PassageW);
            }
        }
        if (coordinates.x > 0)
        {
            int i = index + maze.StepW;
            if (maze[i] == MazeFlags.Empty)
            {
                scratchpad[count++] = (i, MazeFlags.PassageW, MazeFlags.PassageE);
            }
        }
        if (coordinates.y + 1 < maze.SizeHeight)
        {
            int i = index + maze.StepN;
            if (maze[i] == MazeFlags.Empty)
            {
                scratchpad[count++] = (i, MazeFlags.PassageN, MazeFlags.PassageS);
            }
        }
        if (coordinates.y > 0)
        {
            int i = index + maze.StepS;
            if (maze[i] == MazeFlags.Empty)
            {
                scratchpad[count++] = (i, MazeFlags.PassageS, MazeFlags.PassageN);
            }
        }
        return count;
    }

    Random OpenDeadEnds(
        Random random, NativeArray<(int, MazeFlags, MazeFlags)> scratchpad
    )
    {
        for (int i = 0; i < maze.Length; i++)
        {
            MazeFlags cell = maze[i];
            if (cell.HasExactlyOne() && random.NextFloat() < openDeadEndProbability)
            {
                int availablePassageCount = FindClosedPassages(i, scratchpad, cell);
                (int, MazeFlags, MazeFlags) passage =
                    scratchpad[random.NextInt(0, availablePassageCount)];
                maze[i] = cell.With(passage.Item2);
                maze.Set(i + passage.Item1, passage.Item3);
            }
        }
        return random;
    }

    /*
     * Random chance of opening a closed path
     */
    Random OpenOptionalPassages(Random random)
    {
        for (int i = 0; i < maze.Length; i++)
        {
            int2 coordinates = maze.IndexToCoordinates(i);
            if (coordinates.x > 0 && random.NextFloat() < openOptionalProbability)
            {
                maze.Set(i, MazeFlags.PassageW);
                maze.Set(i + maze.StepW, MazeFlags.PassageE);
            }
            if (coordinates.y > 0 && random.NextFloat() < openOptionalProbability)
            {
                maze.Set(i, MazeFlags.PassageS);
                maze.Set(i + maze.StepS, MazeFlags.PassageN);
            }
        }
        return random;
    }

    /*
     * If we are at the dead end, we probe for closed pathes and force open them
     */
    int FindClosedPassages(
        int index, NativeArray<(int, MazeFlags, MazeFlags)> scratchpad, MazeFlags exclude
    )
    {
        int2 coordinates = maze.IndexToCoordinates(index);
        int count = 0;
        if (exclude != MazeFlags.PassageE && coordinates.x + 1 < maze.SizeWidth)
        {
            scratchpad[count++] = (maze.StepE, MazeFlags.PassageE, MazeFlags.PassageW);
        }
        if (exclude != MazeFlags.PassageW && coordinates.x > 0)
        {
            scratchpad[count++] = (maze.StepW, MazeFlags.PassageW, MazeFlags.PassageE);
        }
        if (exclude != MazeFlags.PassageN && coordinates.y + 1 < maze.SizeHeight)
        {
            scratchpad[count++] = (maze.StepN, MazeFlags.PassageN, MazeFlags.PassageS);
        }
        if (exclude != MazeFlags.PassageS && coordinates.y > 0)
        {
            scratchpad[count++] = (maze.StepS, MazeFlags.PassageS, MazeFlags.PassageN);
        }
        return count;
    }
}
