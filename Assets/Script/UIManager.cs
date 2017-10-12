using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public GameObject GameEndText;

    private void Awake()
    {
        GameEndText.SetActive(false);
        GameManager.Instance.OnGameEnd += OnGameOver_UIManager;
    }

    private void OnGameOver_UIManager(GameManager sender)
    {
        GameEndText.SetActive(true);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
