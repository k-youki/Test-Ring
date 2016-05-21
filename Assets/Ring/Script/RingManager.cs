using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class RingManager : MonoBehaviour
{
	private static RingManager MyInstance;
	private bool RingStartFlg = false;
	private static bool RingConnecting = false;
	private static bool RingTouching = false;
	private bool QuaternionStartFlg = false;
	private static Quaternion ReceiveQuat;
	private Quaternion DefaultQuat;
	private Quaternion PrevQuat;
	private Quaternion TruePrevQuat;
	private float diffRemit = 0.01f;
	private float SlerpValue = 0.01f;
	private static bool didReceiveNewGesture = false;
	private static string ReceiveGeatureName;
	public enum RingModeType {
		GestureMode,
		QuaternionMode
	}
	[SerializeField] RingModeType RingMode = RingModeType.GestureMode;


	[DllImport ("__Internal" +
		"")]
	private static extern void _StartRing ();

	[DllImport ("__Internal")]
	private static extern void _EndRing ();

	[DllImport ("__Internal")]
	private static extern void _QuaternionMode ();
	
	[DllImport ("__Internal")]
	private static extern void _GestureMode ();

	public static void ChangeQuaternionMode(){
		_QuaternionMode ();
		MyInstance.RingMode = RingModeType.QuaternionMode;
	}

	public static void ChangeGestureMode(){
		_GestureMode ();
		MyInstance.RingMode = RingModeType.GestureMode;
	}



	public delegate void CallbackRingGestureStatus (bool connectStatus, string ReceiveGesture);
	
	[DllImport("__Internal")]
	private static extern void _GetRingGestureStatus (CallbackRingGestureStatus method);
	
	public static void GetRingGestureStatus ()
	{
		_GetRingGestureStatus (RingGestureStatusCallbackResult);
	}
	
	[AOT.MonoPInvokeCallbackAttribute(typeof(CallbackRingGestureStatus))]
	static void RingGestureStatusCallbackResult (bool connectStatus, string ReceiveGesture)
	{
		didReceiveNewGesture = true;
		ReceiveGeatureName = ReceiveGesture;
		Debug.Log("ReceiveGeatureName"+ ReceiveGeatureName + ReceiveGesture);
		RingConnecting = connectStatus;
	}

	public static string GetReceiveGestureName(){
		Debug.Log("GetReceiveGestureName"+ ReceiveGeatureName);
		return ReceiveGeatureName;
	}

	public static bool DidReceiveNewGesture(){
		bool currentStatus = didReceiveNewGesture;
		didReceiveNewGesture = false;
		return currentStatus;
	}



	public delegate void CallbackRingQuatStatus (bool connectStatus,float x,float y,float z,float w);
	
	[DllImport("__Internal")]
	private static extern void _GetRingQuatStatus (CallbackRingQuatStatus method);

	public static void GetRingQuatStatus ()
	{
		_GetRingQuatStatus (RingQuatStatusCallbackResult);
	}
	
	[AOT.MonoPInvokeCallbackAttribute(typeof(CallbackRingQuatStatus))]
	static void RingQuatStatusCallbackResult (bool connectStatus, float x, float y, float z, float w)
	{
		ReceiveQuat = new Quaternion (x, -y, -z, -w);
		RingConnecting = connectStatus;	
	}

	public static Quaternion GetRingQuat(){
		return MyInstance.TruePrevQuat;
	}



	public delegate void CallbackTouchStatus (bool touchStatus);

	[DllImport("__Internal")]
	private static extern void _GetTouchStatus (CallbackTouchStatus method);

	public static void GetRingTouchStatus ()
	{
		_GetTouchStatus (TouchStatusCallbackResult);
	}

	[AOT.MonoPInvokeCallbackAttribute(typeof(CallbackTouchStatus))]
	static void TouchStatusCallbackResult (bool touchStatus)
	{
		RingTouching = touchStatus;
	}

	public static bool GetRingTouching(){
		bool currentTouchStatus = RingTouching;
		RingTouching = false;
		return currentTouchStatus;
	}



	// Use this for initialization
	void Start ()
	{
		MyInstance = this;

		if (Application.platform != RuntimePlatform.OSXEditor) {
			_StartRing ();
			RingStartFlg = true;
			Debug.Log ("RingInitialize : Start");
			if(RingMode == RingModeType.QuaternionMode){
				_QuaternionMode ();
				Debug.Log ("QuaternionMode");
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (RingStartFlg && Application.platform != RuntimePlatform.OSXEditor) {
			GetRingTouchStatus();
			 if(RingMode == RingModeType.GestureMode){
				GetRingGestureStatus();
			}else{
				GetRingQuatStatus();
				if(RingConnecting)
					UpdateQuaternion ();
			}

		}
	}



	void UpdateQuaternion ()
	{

		if (!QuaternionStartFlg || RingTouching) {
			UnityEngine.Debug.Log ("DefaultQuat =" + ReceiveQuat);
			DefaultQuat = QuatInverse (ReceiveQuat);
			QuaternionStartFlg = true;
			PrevQuat = QuatMul (DefaultQuat, ReceiveQuat);
			TruePrevQuat = QuatMul (DefaultQuat, ReceiveQuat);
			UnityEngine.Debug.Log ("TruePrevQuat =" + TruePrevQuat);
		}

		Quaternion RevisionQuat = QuatMul (DefaultQuat, ReceiveQuat);

		if (RevisionQuat.x - PrevQuat.x < diffRemit && RevisionQuat.x - PrevQuat.x > -diffRemit) {
			RevisionQuat.x = PrevQuat.x;
		}
		
		if (RevisionQuat.y - PrevQuat.y < diffRemit && RevisionQuat.y - PrevQuat.y > -diffRemit) {
			RevisionQuat.y = PrevQuat.y;
		}
		
		if (RevisionQuat.z - PrevQuat.z < diffRemit && RevisionQuat.z - PrevQuat.z > -diffRemit) {
			RevisionQuat.z = PrevQuat.z;
		}
		
		if (RevisionQuat.w - PrevQuat.w < diffRemit && RevisionQuat.w - PrevQuat.w > -diffRemit) {
			RevisionQuat.w = PrevQuat.w;
		}

		Quaternion DiffQuat = QuatSub (RevisionQuat, PrevQuat);
		Quaternion ResultQuat = QuatAdd (TruePrevQuat, DiffQuat);


		//  Result rotation
//		this.transform.rotation = Quaternion.Slerp (transform.rotation, ResultQuat, SlerpValue);
//		UnityEngine.Debug.Log ("Result transform.rotation =" + this.transform.rotation);

		PrevQuat = QuatMul (DefaultQuat, ReceiveQuat);
		TruePrevQuat = ResultQuat;
	}


	//QuaterninMode

	Quaternion QuatMul (Quaternion q1, Quaternion q2)
	{
		Quaternion q3;
		q3.w = q1.w * q2.w - q1.x * q2.x - q1.y * q2.y - q1.z * q2.z;
		q3.x = q1.y * q2.z - q1.z * q2.y + q1.w * q2.x + q1.x * q2.w;
		q3.y = q1.z * q2.x - q1.x * q2.z + q1.w * q2.y + q1.y * q2.w;
		q3.z = q1.x * q2.y - q1.y * q2.x + q1.w * q2.z + q1.z * q2.w;
		
		return q3;
	}
	
	Quaternion QuatSub (Quaternion q1, Quaternion q2)
	{
		Quaternion q3;
		q3.w = q1.w - q2.w;
		q3.x = q1.x - q2.x;
		q3.y = q1.y - q2.y;
		q3.z = q1.z - q2.z;
		
		return q3;
	}
	
	Quaternion QuatAdd (Quaternion q1, Quaternion q2)
	{
		Quaternion q3;
		q3.w = q1.w + q2.w;
		q3.x = q1.x + q2.x;
		q3.y = q1.y + q2.y;
		q3.z = q1.z + q2.z;
		
		return q3;
	}
	
	float QuatNorm (Quaternion q)
	{
		float n = Mathf.Sqrt (q.w * q.w + q.x * q.x + q.y * q.y
			+ q.z * q.z);
		return n * n;
	}
	
	Quaternion QuatInverse (Quaternion q)
	{
		float n = QuatNorm (q);
		
		return   new Quaternion (-q.x / n, -q.y / n, -q.z / n, q.w / n);
	}

}
