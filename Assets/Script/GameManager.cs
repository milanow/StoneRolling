/* Author: Tianhe Wang
 * Date: 10/09/2017
 */

using UnityEngine;
using System;

public class GameManager : MonoBehaviour {

    // determine when to destory the instance
    private static bool applicationIsQuitting = false;
    // prevent generating more instance
    private static object _lock = new object();
    private static GameManager _instance;
    // public access to GameManager instance
    public static GameManager Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                Debug.LogWarning("[Singleton] Instance '" + "GameManager" +
                    "' already destroyed on application quit." +
                    " Won't create again - returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (GameManager)FindObjectOfType(typeof(GameManager));

                    if (FindObjectsOfType(typeof(GameManager)).Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong " +
                            " - there should never be more than 1 singleton!" +
                            " Reopening the scene might fix it.");
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<GameManager>();
                        singleton.name = "(singleton) " + typeof(GameManager).ToString();

                        DontDestroyOnLoad(singleton);

                        Debug.Log("[Singleton] An instance of " + typeof(GameManager) +
                            " is needed in the scene, so '" + singleton +
                            "' was created with DontDestroyOnLoad.");
                    }
                    else
                    {
                        Debug.Log("[Singleton] Using instance already created: " +
                            _instance.gameObject.name);
                    }
                }

                return _instance;
            }

        }
    }
    // a parent object of all floors/cells in the game
    public GameObject Floor;
    // the start and end position of a level
    public Vector2 StartPosition;
    public Vector2 EndPosition;

    // map records if current coordinate has cell, the mao gives an overview of what the level is like
    // since we have an irregular shape of map, but storing inside a 2D array, thus we need bool to outline the map
    bool[,] map;
    // offsets are used to conversion between 'index of map' and 'world coordinate in Unity'
    int rowOffset;
    int colOffset;

    private void Awake()
    {
        // syn cells to map
        if(Floor == null)
        {
            Floor = GameObject.Find("Floors");
            if (Floor == null)
            {
                Debug.LogError("No cells/floor assigned!");
                return;
            }
        }

        int lengthRow = 0, lengthCol = 0;
        CalBoundary(ref lengthRow, ref lengthCol);
        if(lengthRow == 0 || lengthCol == 0)
        {
            Debug.Log("There is no floor in the scene.");
        }
        map = new bool[lengthRow, lengthCol];

        Transform[] cells = Floor.GetComponentsInChildren<Transform>();
        for(int i = 0; i < cells.Length; ++i)
        {
            map[(int)(cells[i].position.x) + rowOffset, (int)(cells[i].position.z) + colOffset] = true;
        }

#if UNITY_EDITOR
        Debug.Log("map initialize succeeds!");
#endif
    }
     
    // calculate the length of 2D array that can hold the map
    private void CalBoundary(ref int row, ref int col)
    {
        int minRow = Int32.MaxValue;
        int maxRow = Int32.MinValue;
        int minCol = Int32.MaxValue;
        int maxCol = Int32.MinValue;
        Transform[] cells = Floor.GetComponentsInChildren<Transform>();
        if (cells.Length == 0) return;
        for (int i = 0; i < cells.GetLength(0); ++i)
        {
            minRow = Mathf.Min(minRow, (int)(cells[i].position.x));
            maxRow = Mathf.Max(maxRow, (int)(cells[i].position.x));
            minCol = Mathf.Min(minCol, (int)(cells[i].position.z));
            maxCol = Mathf.Max(maxCol, (int)(cells[i].position.z));
        }

        row = maxRow - minRow + 1;
        col = maxCol - minCol + 1;
        rowOffset = 0 - minRow;
        colOffset = 0 - minCol;

#if UNITY_EDITOR
        Debug.Log("Map size: " + row + ", " + col);
        Debug.Log("Row offset: " + rowOffset);
        Debug.Log("Col offset: " + colOffset);
#endif
    }

    public Vector2 GetCoordInMap(float xcoord, float ycoord)
    {
        return new Vector2(Mathf.RoundToInt(xcoord) + rowOffset, Mathf.RoundToInt(ycoord) + colOffset);
    }

    //public Vector2 GetCoordInWorldAxis(float xcoord, float ycoord)
    //{
    //    return new Vector2(xcoord - rowOffset, ycoord - colOffset);
    //}

    /// <summary>
    /// Check if next move is valid, inputs are target positions
    /// </summary>
    /// <param name="x1"> First point's x index </param>
    /// <param name="y1"> First point's y index </param>
    /// <param name="x2"> Second point's x index </param>
    /// <param name="y2"> Second point's y index </param>
    /// <returns></returns>
    public bool ValidMove(int x1, int y1, int x2 = -2, int y2 = -2)
    {
        // first point
        if (x1 < 0 || y1 < 0 || x1 >= map.GetLength(0) || y1 >= map.GetLength(1) || !map[x1, y1]) return false;

        // if new pos is 'standing'
        if (x2 == -2 && y2 == -2) return true;

        // second point
        if (x2 < 0 || y2 < 0 || x2 >= map.GetLength(0) || y2 >= map.GetLength(1) || !map[x2, y2]) return false;

        // both points are valid
        return true;
    }

    /// <summary>
    /// Get required rotating axis when taking step
    /// </summary>
    /// <param name="dir"> Input direction of this step </param>
    /// <param name="row"> The x coord of the player </param>
    /// <param name="col"> The y coord of the player </param>
    /// <returns></returns>
    public Vector3 GetRotatingPoint(InputDirections dir, int row, int col)
    {
        Vector3 res;
        if(dir == InputDirections.Up)
        {
            res = new Vector3(row - rowOffset + .5f, 0, col - colOffset);
        }
        else if(dir == InputDirections.Down)
        {
            res = new Vector3(row - rowOffset - .5f, 0, col - colOffset);
        }
        else if(dir == InputDirections.Left)
        {
            res = new Vector3(row - rowOffset, 0, col - colOffset + .5f);
        }
        else
        {
            res = new Vector3(row - rowOffset, 0, col - colOffset - .5f);
        }
        return res;
    }

    public void OnDestroy()
    {
        applicationIsQuitting = true;
    }

    

}
