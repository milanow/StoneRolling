/* Author: Tianhe Wang
 * Date: 10/10/2017
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

    public Transform PlayerCenter;
    private Vector2 _offset;

    // Use this for initialization
    // This need to be execute after starting Player.cs
    void Start () {
#if UNITY_EDITOR
        if(PlayerCenter == null)
        {
            Debug.LogFormat("{0}: Assign Player's transform first!", GetType().ToString());
            _offset = Vector3.zero;
            return;
        }
#endif
        // calculate the center point of palyer
        _offset = new Vector2(this.transform.position.x - PlayerCenter.position.x, this.transform.position.z - PlayerCenter.position.z);
	}
	
	// Update is called once per frame
	void LateUpdate () {
        this.transform.position = new Vector3(_offset.x + PlayerCenter.position.x, this.transform.position.y, _offset.y + PlayerCenter.position.z);
	}
}
