using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSize : MonoBehaviour {

    public GameObject target;

    private float xfactor = 1f;
    private float yfactor = 1f;

	// Use this for initialization
	void Start () {
		if(target == null)
        {
            Debug.LogError("Please specify target obejct first!");
        }

        xfactor = this.GetComponent<Renderer>().bounds.size.x;
        yfactor = this.GetComponent<Renderer>().bounds.size.y;
        this.transform.localScale = new Vector3(1f / xfactor, 2f / yfactor, 1f / xfactor);


    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed!");
            Vector3 point = new Vector3(this.transform.position.x, 0, this.transform.position.z - .5f);
            Debug.Log(point);
            transform.RotateAround(point, new Vector3(1f, 0, 0), 270f);
        }
		//this.transform.RotateAround()
	}
}
