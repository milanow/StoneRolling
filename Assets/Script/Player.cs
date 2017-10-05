﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour
{

    public float rotatingSpeed = 10f;

    private struct PlayerPos
    {
        public int xidx1;
        public int yidx1;
        public int xidx2;
        public int yidx2;
        public PlayerPos(int x1, int y1, int x2, int y2)
        {
            xidx1 = x1;
            yidx1 = y1;
            xidx2 = x2;
            yidx2 = y2;
        }

    }

    private enum PlayerStatus { AlongYAxis, AlongXAxis, AlongZAxis }

    private float xfactor = 1f;
    private float yfactor = 1f;
    private bool isRotating;
    private float offset;
    private Vector3 moveAxis;
    private Vector3 movePoint;
    private PlayerStatus status;
    private PlayerPos playerPos;

    // Use this for initialization
    void Start()
    {

        isRotating = false;
        status = 0;
        offset = 0;
        moveAxis = new Vector3(0, 0, 0);
        movePoint = new Vector3(0, 0, 0);
        // this has to be excecute after ini GameManager
        playerPos = new PlayerPos();
        Vector2 v2 = GameManager.Instance.GetCoordInMap(transform.position.x, transform.position.z);
        Debug.Log(v2);
        playerPos.xidx2 = playerPos.xidx1 = (int)v2.x;
        playerPos.yidx2 = playerPos.yidx1 = (int)v2.y;

        // Register Rotate function to subscribe input event
        InputManager.OnMove += Rotate;

        xfactor = this.GetComponent<Renderer>().bounds.size.x;
        yfactor = this.GetComponent<Renderer>().bounds.size.y;
        this.transform.localScale = new Vector3(1f / xfactor, 2f / yfactor, 1f / xfactor);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isRotating)
        {
            offset += 90 * Time.deltaTime * 0.1f * rotatingSpeed;
            transform.RotateAround(movePoint, moveAxis, 90f * Time.fixedDeltaTime * 0.1f * rotatingSpeed);

            if (offset >= 90)
            {
                isRotating = false;
                offset = 0;

                // correct rotation
                transform.localRotation = Quaternion.Euler(Mathf.RoundToInt(transform.eulerAngles.x / 90.0f) * 90, Mathf.RoundToInt((transform.eulerAngles.y) / 90.0f) * 90, Mathf.RoundToInt((transform.eulerAngles.z) / 90.0f) * 90);
                // correct location
                Vector3 oldPos = transform.position;
                transform.position = new Vector3(Mathf.Round(2 * oldPos.x) / 2, Mathf.Round(2 * oldPos.y) / 2, Mathf.Round(2 * oldPos.z) / 2);
            }
        }
    }


    void Rotate(InputDirections dir)
    {
        // if rotating then not need to change direction
        if (isRotating) return;

        if (dir == InputDirections.Up)
        {
            moveAxis = Vector3.back;
            switch (status)
            {
                case PlayerStatus.AlongYAxis:
                    if (GameManager.Instance.ValidMove(playerPos.xidx1 + 2, playerPos.yidx1, playerPos.xidx2 + 1, playerPos.yidx2))
                    {
                        movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Up, playerPos.xidx1, playerPos.yidx1);
                        playerPos.xidx1 += 2;
                        playerPos.xidx2 += 1;
                        Debug.LogFormat("xidx1: {0}, xidx2: {1}", playerPos.xidx1, playerPos.xidx2);
                        status = PlayerStatus.AlongXAxis;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                case PlayerStatus.AlongXAxis:
                    if (GameManager.Instance.ValidMove(playerPos.xidx1 + 1, playerPos.yidx1, playerPos.xidx2 + 2, playerPos.yidx2))
                    {
                        movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Up, playerPos.xidx1, playerPos.yidx1);
                        playerPos.xidx1 += 1;
                        playerPos.xidx2 += 2;
                        Debug.LogFormat("xidx1: {0}, xidx2: {1}", playerPos.xidx1, playerPos.xidx2);

                        status = PlayerStatus.AlongYAxis;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                case PlayerStatus.AlongZAxis:
                    if (GameManager.Instance.ValidMove(playerPos.xidx1 + 1, playerPos.yidx1, playerPos.xidx2 + 1, playerPos.yidx2))
                    {
                        movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Up, playerPos.xidx1, playerPos.yidx1);
                        playerPos.xidx1 += 1;
                        playerPos.xidx2 += 1;
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
            moveAxis = Vector3.forward;
            switch (status)
            {
                case PlayerStatus.AlongYAxis:
                    if (GameManager.Instance.ValidMove(playerPos.xidx1 - 1, playerPos.yidx1, playerPos.xidx2 - 2, playerPos.yidx2))
                    {
                        movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Down, playerPos.xidx1, playerPos.yidx1);
                        playerPos.xidx1 -= 1;
                        playerPos.xidx2 -= 2;
                        status = PlayerStatus.AlongXAxis;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                case PlayerStatus.AlongXAxis:
                    if (GameManager.Instance.ValidMove(playerPos.xidx1 - 2, playerPos.yidx1, playerPos.xidx2 - 1, playerPos.yidx2))
                    {
                        movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Down, playerPos.xidx2, playerPos.yidx2);
                        playerPos.xidx1 -= 2;
                        playerPos.xidx2 -= 1;
                        status = PlayerStatus.AlongYAxis;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                case PlayerStatus.AlongZAxis:
                    if (GameManager.Instance.ValidMove(playerPos.xidx1 - 1, playerPos.yidx1, playerPos.xidx2 - 1, playerPos.yidx2))
                    {
                        movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Down, playerPos.xidx1, playerPos.yidx1);
                        playerPos.xidx1 -= 1;
                        playerPos.xidx2 -= 1;
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
            moveAxis = Vector3.right;
            switch (status)
            {
                case PlayerStatus.AlongYAxis:
                    if (GameManager.Instance.ValidMove(playerPos.xidx1, playerPos.yidx1 + 2, playerPos.xidx2, playerPos.yidx2 + 1))
                    {
                        movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Left, playerPos.xidx1, playerPos.yidx1);
                        playerPos.yidx1 += 2;
                        playerPos.yidx2 += 1;
                        status = PlayerStatus.AlongZAxis;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                case PlayerStatus.AlongXAxis:
                    if (GameManager.Instance.ValidMove(playerPos.xidx1, playerPos.yidx1 + 1, playerPos.xidx2, playerPos.yidx2 + 1))
                    {
                        movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Left, playerPos.xidx1, playerPos.yidx1);
                        playerPos.yidx1 += 1;
                        playerPos.yidx2 += 1;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                case PlayerStatus.AlongZAxis:
                    if (GameManager.Instance.ValidMove(playerPos.xidx1, playerPos.yidx1 + 1, playerPos.xidx2, playerPos.yidx2 + 2))
                    {
                        movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Left, playerPos.xidx1, playerPos.yidx1);
                        playerPos.yidx1 += 1;
                        playerPos.yidx2 += 2;
                        status = PlayerStatus.AlongYAxis;
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
            moveAxis = Vector3.left;
            switch (status)
            {
                case PlayerStatus.AlongYAxis:
                    if (GameManager.Instance.ValidMove(playerPos.xidx1, playerPos.yidx1 - 1, playerPos.xidx2, playerPos.yidx2 - 2))
                    {
                        movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Right, playerPos.xidx1, playerPos.yidx1);
                        playerPos.yidx1 -= 1;
                        playerPos.yidx2 -= 2;
                        status = PlayerStatus.AlongZAxis;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                case PlayerStatus.AlongXAxis:
                    if (GameManager.Instance.ValidMove(playerPos.xidx1, playerPos.yidx1 - 1, playerPos.xidx2, playerPos.yidx2 - 1))
                    {
                        movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Right, playerPos.xidx1, playerPos.yidx1);
                        playerPos.yidx1 -= 1;
                        playerPos.yidx2 -= 1;
                    }
                    else
                    {
                        Debug.Log("Not valid move");
                        return;
                    }
                    break;
                case PlayerStatus.AlongZAxis:
                    if (GameManager.Instance.ValidMove(playerPos.xidx1, playerPos.yidx1 - 2, playerPos.xidx2, playerPos.yidx2 - 1))
                    {
                        movePoint = GameManager.Instance.GetRotatingPoint(InputDirections.Right, playerPos.xidx2, playerPos.yidx2);
                        playerPos.yidx1 -= 2;
                        playerPos.yidx2 -= 1;
                        status = PlayerStatus.AlongXAxis;
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
        isRotating = true;

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
