/* Author: Tianhe Wang
 * Date: 10/09/2017
 */

using UnityEngine;

public class Player : MonoBehaviour
{
    private struct PlayerPos
    {
        public float xidx1;
        public float yidx1;
        public float xidx2;
        public float yidx2;
        public PlayerPos(float x1, float y1, float x2, float y2)
        {
            xidx1 = x1;
            yidx1 = y1;
            xidx2 = x2;
            yidx2 = y2;
        }

    }
    private enum PlayerStatus { AlongYAxis, AlongXAxis, AlongZAxis }

    /// <summary>
    /// playuer's move speed
    /// </summary>
    public float RotatingSpeed = 20f;
    // these two factors are for translating object's scale to world's scale
    private float _xfactor;
    private float _yfactor;
    // whether player is moving/rotating, if true, then disable input
    private bool _isRotating;
    private float _offset;
    // these two variables reresent the instruction to rotate/move the player 
    private Vector3 _moveAxis;
    private Vector3 _movePoint;
    // player has 3 different status, stand along x, y or z axis
    private PlayerStatus _status;
    // idx1 & idx2 does not may switch role when player is moving, xidx1 always larger than xidx2, yidx1 always larger thatn yidx2
    private PlayerPos _playerPos;
    /// <summary>
    /// return the center of player object
    /// </summary>
    public Vector2 PlayerCenter
    {
        get
        {
            return GameManager.Instance.GetCoordInWorldAxis((_playerPos.xidx1 + _playerPos.xidx2) / 2f, (_playerPos.yidx1 + _playerPos.yidx2) / 2f);
        }
    }


    // Use this for initialization
    void Start()
    {
        //InitPlayer();
        // Register Rotate function to subscribe input event
        InputManager.OnMoveControl += OnPlayerMove;
    }

    public void InitPlayer()
    {
        _isRotating = false;
        _status = 0;
        _offset = 0;
        _moveAxis = new Vector3(0, 0, 0);
        _movePoint = new Vector3(0, 0, 0);

        // this has to be excecute after ini GameManager
        InitializePlayerPosAndRot();

        CalculatePlayerScale();
    }

    public void ResetPlayer()
    {
        _isRotating = false;
        _status = 0;
        _offset = 0;
        _moveAxis = new Vector3(0, 0, 0);
        _movePoint = new Vector3(0, 0, 0);

        InitializePlayerPosAndRot();

    }

    private void CalculatePlayerScale()
    {
        _xfactor = this.GetComponent<Renderer>().bounds.size.x;
        _yfactor = this.GetComponent<Renderer>().bounds.size.y;
        this.transform.localScale = new Vector3(1f / _xfactor, 2f / _yfactor, 1f / _xfactor);
    }

    private void InitializePlayerPosAndRot()
    {
        _playerPos = new PlayerPos();
        transform.position = new Vector3(GameManager.Instance.StartCell.position.x, 0, GameManager.Instance.StartCell.position.y);
        Vector2 v2 = GameManager.Instance.GetCoordInMap(transform.position.x, transform.position.z);
        _playerPos.xidx2 = _playerPos.xidx1 = (int)v2.x;
        _playerPos.yidx2 = _playerPos.yidx1 = (int)v2.y;
        transform.rotation = Quaternion.identity;
    }

    void FixedUpdate()
    {
        if (_isRotating)
        {
            _offset += 90 * Time.deltaTime * 0.1f * RotatingSpeed;
            transform.RotateAround(_movePoint, _moveAxis, 90f * Time.fixedDeltaTime * 0.1f * RotatingSpeed);

            if (_offset >= 90)
            {
                _isRotating = false;
                _offset = 0;

                // correct rotation
                transform.localRotation = Quaternion.Euler(Mathf.RoundToInt(transform.eulerAngles.x / 90.0f) * 90, Mathf.RoundToInt((transform.eulerAngles.y) / 90.0f) * 90, Mathf.RoundToInt((transform.eulerAngles.z) / 90.0f) * 90);
                // correct location
                Vector3 oldPos = transform.position;
                transform.position = new Vector3(Mathf.Round(2 * oldPos.x) / 2, Mathf.Round(2 * oldPos.y) / 2, Mathf.Round(2 * oldPos.z) / 2);

                // check if game is over
                if (_status == PlayerStatus.AlongYAxis)
                {
                    Vector2 endPos = GameManager.Instance.EndCellCoordInMap;
                    if(_playerPos.xidx1 == endPos.x && _playerPos.yidx1 == endPos.y)
                    {
                        GameManager.Instance.GameOver = true;
                    }
                }
                
            }
        }
    }

    // handle inputManager's OnMoveControl event
    void OnPlayerMove(InputDirections dir)
    {
        // Rotate
        PrepareRotate(dir);
    }

    // prepare for rotating, including updating player's position and status, also calculate movePoint & moveAxis
    void PrepareRotate(InputDirections dir)
    {
        // if rotating then not need to change direction
        if (_isRotating) return;

        // different input & status make different moveAxis & movePoint, these two variables are for correctly rotating/moving 
        // the 'player box', which is required by 'transform.RotateAround()' method. Besides, updating player's position and status
        if (dir == InputDirections.Up)
        {
            _moveAxis = Vector3.back;
            switch (_status)
            {
                case PlayerStatus.AlongYAxis:
                    if (GameManager.Instance.ValidMove(_playerPos.xidx1 + 2, _playerPos.yidx1, _playerPos.xidx2 + 1, _playerPos.yidx2))
                    {
                        _movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Up, _playerPos.xidx1, _playerPos.yidx1);
                        _playerPos.xidx1 += 2;
                        _playerPos.xidx2 += 1;
                        _status = PlayerStatus.AlongXAxis;
                    }
                    else
                    {
                        // TODO: player failure's sound
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                case PlayerStatus.AlongXAxis:
                    if (GameManager.Instance.ValidMove(_playerPos.xidx1 + 1, _playerPos.yidx1))
                    {
                        _movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Up, Mathf.Max(_playerPos.xidx1, _playerPos.xidx2), _playerPos.yidx1);
                        _playerPos.xidx1 += 1;
                        _playerPos.xidx2 += 2;
                        _status = PlayerStatus.AlongYAxis;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                case PlayerStatus.AlongZAxis:
                    if (GameManager.Instance.ValidMove(_playerPos.xidx1 + 1, _playerPos.yidx1, _playerPos.xidx2 + 1, _playerPos.yidx2))
                    {
                        _movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Up, _playerPos.xidx1, _playerPos.yidx1);
                        _playerPos.xidx1 += 1;
                        _playerPos.xidx2 += 1;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                default:
                    Debug.LogError("Player behaviour undefined");
                    break;
            }
        }
        else if (dir == InputDirections.Down)
        {
            _moveAxis = Vector3.forward;
            switch (_status)
            {
                case PlayerStatus.AlongYAxis:
                    if (GameManager.Instance.ValidMove(_playerPos.xidx1 - 1, _playerPos.yidx1, _playerPos.xidx2 - 2, _playerPos.yidx2))
                    {
                        _movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Down, _playerPos.xidx1, _playerPos.yidx1);
                        _playerPos.xidx1 -= 1;
                        _playerPos.xidx2 -= 2;
                        _status = PlayerStatus.AlongXAxis;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                case PlayerStatus.AlongXAxis:
                    if (GameManager.Instance.ValidMove(_playerPos.xidx1 - 2, _playerPos.yidx1))
                    {
                        _movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Down, Mathf.Min(_playerPos.xidx1, _playerPos.xidx2), _playerPos.yidx1);
                        _playerPos.xidx1 -= 2;
                        _playerPos.xidx2 -= 1;
                        _status = PlayerStatus.AlongYAxis;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                case PlayerStatus.AlongZAxis:
                    if (GameManager.Instance.ValidMove(_playerPos.xidx1 - 1, _playerPos.yidx1, _playerPos.xidx2 - 1, _playerPos.yidx2))
                    {
                        _movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Down, _playerPos.xidx1, _playerPos.yidx1);
                        _playerPos.xidx1 -= 1;
                        _playerPos.xidx2 -= 1;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                default:
                    Debug.LogError("Player behaviour undefined");
                    break;
            }
        }
        else if (dir == InputDirections.Left)
        {
            _moveAxis = Vector3.right;
            switch (_status)
            {
                case PlayerStatus.AlongYAxis:
                    if (GameManager.Instance.ValidMove(_playerPos.xidx1, _playerPos.yidx1 + 2, _playerPos.xidx2, _playerPos.yidx2 + 1))
                    {
                        _movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Left, _playerPos.xidx1, _playerPos.yidx1);
                        _playerPos.yidx1 += 2;
                        _playerPos.yidx2 += 1;
                        _status = PlayerStatus.AlongZAxis;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                case PlayerStatus.AlongXAxis:
                    if (GameManager.Instance.ValidMove(_playerPos.xidx1, _playerPos.yidx1 + 1, _playerPos.xidx2, _playerPos.yidx2 + 1))
                    {
                        _movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Left, _playerPos.xidx1, _playerPos.yidx1);
                        _playerPos.yidx1 += 1;
                        _playerPos.yidx2 += 1;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                case PlayerStatus.AlongZAxis:
                    if (GameManager.Instance.ValidMove(_playerPos.xidx1, _playerPos.yidx1 + 1))
                    {
                        _movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Left, _playerPos.xidx1, Mathf.Max(_playerPos.yidx1, _playerPos.yidx2));
                        _playerPos.yidx1 += 1;
                        _playerPos.yidx2 += 2;
                        _status = PlayerStatus.AlongYAxis;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                default:
                    Debug.LogError("Player behaviour undefined");
                    break;
            }
        }
        else
        {
            _moveAxis = Vector3.left;
            switch (_status)
            {
                case PlayerStatus.AlongYAxis:
                    if (GameManager.Instance.ValidMove(_playerPos.xidx1, _playerPos.yidx1 - 1, _playerPos.xidx2, _playerPos.yidx2 - 2))
                    {
                        _movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Right, _playerPos.xidx1, _playerPos.yidx1);
                        _playerPos.yidx1 -= 1;
                        _playerPos.yidx2 -= 2;
                        _status = PlayerStatus.AlongZAxis;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                case PlayerStatus.AlongXAxis:
                    if (GameManager.Instance.ValidMove(_playerPos.xidx1, _playerPos.yidx1 - 1, _playerPos.xidx2, _playerPos.yidx2 - 1))
                    {
                        _movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Right, _playerPos.xidx1, _playerPos.yidx1);
                        _playerPos.yidx1 -= 1;
                        _playerPos.yidx2 -= 1;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                case PlayerStatus.AlongZAxis:
                    if (GameManager.Instance.ValidMove(_playerPos.xidx1, _playerPos.yidx1 - 2))
                    {
                        _movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Right, _playerPos.xidx2, Mathf.Min(_playerPos.yidx1, _playerPos.yidx2));
                        _playerPos.yidx1 -= 2;
                        _playerPos.yidx2 -= 1;
                        _status = PlayerStatus.AlongYAxis;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                default:
                    Debug.LogError("Player behaviour undefined");
                    break;
            }
        }
        // TODO: play move's sound
        _isRotating = true;
        //Debug.LogFormat("idx1: {0}, idy1: {1}, idx2: {2}, idy2: {3}", playerPos.xidx1, playerPos.yidx1, playerPos.xidx2, playerPos.yidx2);
    }

    //IEnumerator RotateCoro(Vector3 point, Vector3 axis)
    //{
    //    isRotating = true;
    //    float offset = 0f;
    //    //if(moveDir == Vector3.right)
    //    //{

    //    //    //Vector3 cur = this.transform.rotation.eulerAngles;
    //    //    targetRotation = new Vector3(0, 0, -90f);
    //    //}
    //    //else if(moveDir == Vector3.left)
    //    //{
    //    //    targetRotation = new Vector3(0, 0, 90f);
    //    //}
    //    //else if (moveDir == Vector3.forward)
    //    //{
    //    //    targetRotation = new Vector3(90f, 0, 0);
    //    //}
    //    //else if (moveDir == Vector3.left)
    //    //{
    //    //    targetRotation = new Vector3(-90, 0, 0);
    //    //}

    //    while(offset <= 90f)
    //    {
    //        offset
    //        if(moveDir == Vector3.right)
    //        {

    //        }
    //    }
    //}

}
