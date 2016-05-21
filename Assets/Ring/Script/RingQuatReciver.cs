using UnityEngine;
using System.Collections;

public class RingQuatReciver : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.rotation = Quaternion.Slerp (transform.rotation, RingManager.GetRingQuat(), 0.1f);

		if(RingManager.GetRingTouching()){
			Debug.Log("Touch!!!");
		}
	}
}
