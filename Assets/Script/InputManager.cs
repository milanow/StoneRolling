﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum InputDirections
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3
}

public class InputManager : MonoBehaviour {

    public GameObject player;

    //private static bool applicationIsQuitting = false;
    //private static object _lock = new object();
    //private static InputManager _instance;
    //public static InputManager Instance
    //{
    //    get
    //    {
    //        if (applicationIsQuitting)
    //        {
    //            Debug.LogWarning("[Singleton] Instance '" + "InputManager" +
    //                "' already destroyed on application quit." +
    //                " Won't create again - returning null.");
    //            return null;
    //        }

    //        lock (_lock)
    //        {
    //            if (_instance == null)
    //            {
    //                _instance = (InputManager)FindObjectOfType(typeof(InputManager));

    //                if (FindObjectsOfType(typeof(InputManager)).Length > 1)
    //                {
    //                    Debug.LogError("[Singleton] Something went really wrong " +
    //                        " - there should never be more than 1 singleton!" +
    //                        " Reopening the scene might fix it.");
    //                    return _instance;
    //                }

    //                if (_instance == null)
    //                {
    //                    GameObject singleton = new GameObject();
    //                    _instance = singleton.AddComponent<InputManager>();
    //                    singleton.name = "(singleton) " + typeof(InputManager).ToString();

    //                    DontDestroyOnLoad(singleton);

    //                    Debug.Log("[Singleton] An instance of " + typeof(InputManager) +
    //                        " is needed in the scene, so '" + singleton +
    //                        "' was created with DontDestroyOnLoad.");
    //                }
    //                else
    //                {
    //                    Debug.Log("[Singleton] Using instance already created: " +
    //                        _instance.gameObject.name);
    //                }
    //            }

    //            return _instance;
    //        }

    //    }
    //}
    //public void OnDestroy()
    //{
    //    applicationIsQuitting = true;
    //}

    public static event Action<InputDirections> OnMove;

	// Use this for initialization
	void Start () {
	    	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.W))
        {
            OnMove(InputDirections.Up);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            OnMove(InputDirections.Down);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            OnMove(InputDirections.Left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            OnMove(InputDirections.Right);
        }
    }


}
