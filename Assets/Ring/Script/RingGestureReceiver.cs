using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RingGestureReceiver : MonoBehaviour
{

	public Text ResultGestureDisplay;

	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
		if (RingManager.DidReceiveNewGesture ()) {
			string gestureName = RingManager.GetReceiveGestureName ();
			Debug.Log ("GestureReceived!!" + gestureName);
			ResultGestureDisplay.text = "GestureReceived!! " + gestureName;

			if (gestureName.Equals ("CIRCLE")) {

				//your code

			} else if (gestureName.Equals ("TRIANGLE")) {

				//your code

			} else if (gestureName.Equals ("HEART")) {

				//your code

			} else if (gestureName.Equals (@"PIGTALE")) {

				//your code

			} else if (gestureName.Equals ("UP")) {

				//your code

			} else if (gestureName.Equals ("DOWN")) {

				//your code

			} else if (gestureName.Equals ("LEFT")) {

				//your code

			} else if (gestureName.Equals ("RIGHT")) {

				//your code
				
			} else {
				ResultGestureDisplay.text = "Gesture NOTFOUND!";
			}

		}

		if (RingManager.GetRingTouching ()) {
			Debug.Log ("Touch!!!");
		}
	}
}
