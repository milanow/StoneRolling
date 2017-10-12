/* Author: Tianhe Wang
 * Date: 10/09/2017
 */

using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{

    // determine when to destory the instance
    private static bool _applicationIsQuitting = false;
    // prevent generating more instance
    private static object _lock = new object();
    private static GameManager _instance;
    // public access to GameManager instance
    public static GameManager Instance
    {
        get
        {
            if (_applicationIsQuitting)
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
    public Player Player;
    // the start and end position of a level
    public Transform StartCell;
    public Transform EndCell;

    /// <summary>
    /// The index of destination in map
    /// </summary>
    public Vector2 EndCellCoordInMap
    {
        get
        {
            return GetCoordInMap(EndCell.position.x, EndCell.position.z);
        }
    }

    /// <summary>
    /// True if game is paused, false if game is running
    /// </summary>
    public bool PauseGame { get; set; }

    /// <summary>
    /// True if game is over, false if game is running
    /// </summary>
    public bool GameOver { get; set; }

    /// <summary>
    /// An event when game end fired
    /// </summary>
    public Action<GameManager> OnGameEnd;
    private bool firedGameEnd = false;

    // map records if current coordinate has cell, the mao gives an overview of what the level is like
    // since we have an irregular shape of map, but storing inside a 2D array, thus we need bool to outline the map
    bool[,] _map;
    // offsets are used to conversion between 'index of map' and 'world coordinate in Unity'
    int _rowOffset;
    int _colOffset;

    private void Awake()
    {
        // syn cells to map
        if (Floor == null)
        {
            Floor = GameObject.Find("Floors");
            if (Floor == null)
            {
                Debug.LogError("No cells/floor assigned!");
                return;
            }
        }

        if(Player == null)
        {
            Player = GameObject.Find("Player").GetComponent<Player>();
            if(Player == null)
            {
                Debug.LogError("No Player assigned!");
                return;
            }
        }

        int lengthRow = 0, lengthCol = 0;
        CalBoundary(ref lengthRow, ref lengthCol);
#if UNITY_EDITOR
        if (lengthRow == 0 || lengthCol == 0)
        {
            Debug.Log("There is no floor in the scene.");
        }
#endif
        _map = new bool[lengthRow, lengthCol];

        Transform[] cells = Floor.GetComponentsInChildren<Transform>();
        for (int i = 0; i < cells.Length; ++i)
        {
            _map[(int)(cells[i].position.x) + _rowOffset, (int)(cells[i].position.z) + _colOffset] = true;
        }

        PauseGame = false;

#if UNITY_EDITOR
        Debug.Log("map initialize succeeds!");
#endif
    }

    private void Update()
    {
        // Check if player has reach the destination
        if (GameOver && !firedGameEnd)
        {
            OnGameEnd(this);
            firedGameEnd = true;
        }
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
        _rowOffset = 0 - minRow;
        _colOffset = 0 - minCol;

#if UNITY_EDITOR
        Debug.Log("Map size: " + row + ", " + col);
        Debug.Log("Row offset: " + _rowOffset);
        Debug.Log("Col offset: " + _colOffset);
#endif
    }

    /// <summary>
    /// Transform world position to map index
    /// </summary>
    /// <param name="x"> x in world space coord </param>
    /// <param name="y"> y in world space coord </param>
    /// <returns></returns>
    public Vector2 GetCoordInMap(float x, float y)
    {
        return new Vector2(Mathf.RoundToInt(x) + _rowOffset, Mathf.RoundToInt(y) + _colOffset);
    }

    /// <summary>
    /// Transform map position to world postion
    /// </summary>
    /// <param name="x"> x index in map </param>
    /// <param name="y"> y index in map </param>
    /// <returns></returns>
    public Vector2 GetCoordInWorldAxis(float x, float y)
    {
        return new Vector2(x - _rowOffset, y - _colOffset);
    }

    /// <summary>
    /// Check if next move is valid, inputs are target positions
    /// </summary>
    /// <param name="x1"> First point's x index </param>
    /// <param name="y1"> First point's y index </param>
    /// <param name="x2"> Second point's x index </param>
    /// <param name="y2"> Second point's y index </param>
    /// <returns></returns>
    public bool ValidMove(float x1, float y1, float x2 = -2f, float y2 = -2f)
    {
        // first point
        if (x1 < 0 || y1 < 0 || x1 >= _map.GetLength(0) || y1 >= _map.GetLength(1) || !_map[(int)x1, (int)y1]) return false;

        // if new pos is 'standing'
        if (x2 == -2 && y2 == -2) return true;

        // second point
        if (x2 < 0 || y2 < 0 || x2 >= _map.GetLength(0) || y2 >= _map.GetLength(1) || !_map[(int)x2, (int)y2]) return false;

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
    public Vector3 GetRotatingPoint(InputDirections dir, float row, float col)
    {
        Vector3 res;
        if (dir == InputDirections.Up)
        {
            res = new Vector3(row - _rowOffset + .5f, 0, col - _colOffset);
        }
        else if (dir == InputDirections.Down)
        {
            res = new Vector3(row - _rowOffset - .5f, 0, col - _colOffset);
        }
        else if (dir == InputDirections.Left)
        {
            res = new Vector3(row - _rowOffset, 0, col - _colOffset + .5f);
        }
        else
        {
            res = new Vector3(row - _rowOffset, 0, col - _colOffset - .5f);
        }
        return res;
    }

    public void OnDestroy()
    {
        _applicationIsQuitting = true;
    }



}
