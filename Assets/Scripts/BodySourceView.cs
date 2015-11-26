using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using UnityStandardAssets.Characters.ThirdPerson;

//[AddComponentMenu("ThirdPersonController/BodySourceView")]
[RequireComponent(typeof (ThirdPersonCharacter))]
public class BodySourceView : MonoBehaviour {
	
	public GameObject BodySourceManager;

	public UILabel value1Label;
	public UILabel value2Label;
	public UILabel value3Label;
	public UILabel value4Label;
	public UILabel value5Label;
	public UILabel value6Label;
	public UILabel value7Label;
	public UILabel value8Label;
	
	private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
	private BodySourceManager _BodyManager;
	private Kinect.Body trackedBody = null;

	private bool areHandsTogether;
	private bool isLeftArmOutStretched;
	private bool isRightArmOutStretched;
	private bool isLeftArmUp;
	private bool isRightArmUp;
	private bool isTurnRight;
	private bool isTurnLeft;
	private bool isRun;
	private bool isStatic;
	private float areHandsTogetherValue;
	private float leftArmOutStretchedValue;
	private float rightArmOutStretchedValue;
	private float leftArmUpValue;
	private float rightArmUpValue;
	private float turnRightValue;
	private float turnLeftValue;
	private float runValue;
	private float staticValue;

	private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
	private Transform m_Cam;                  // A reference to the main camera in the scenes transform
	private Vector3 m_CamForward;             // The current forward direction of the camera
	private Vector3 m_Move;
	private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.

	public float timer = 1.0f;
	
	// Use this for initialization
	void Start () {
		// get the transform of the main camera
		if (Camera.main != null)
		{
			m_Cam = Camera.main.transform;

		}
		else
		{
			Debug.LogWarning(
				"Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
			// we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
		}
		
		// get the third person character ( this should never be null due to require component )
		m_Character = GetComponent<ThirdPersonCharacter>();
	}
	
	// Update is called once per frame
	void Update () {
		if (BodySourceManager == null)
		{
			return;
		}
		
		_BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
		if (_BodyManager == null)
		{
			return;
		}

		Kinect.Body[] data = _BodyManager.GetData();
		if (data == null)
		{
			return;
		}

		List<ulong> trackedIds = new List<ulong>();
		foreach(var body in data)
		{
			if (body == null)
			{
				continue;
			}
			
			if(body.IsTracked)
			{
				trackedIds.Add (body.TrackingId);
				trackedBody = body;
				break;
			}
		}

		// just get the first tracked id
		if (trackedIds.Count == 0) {
			Debug.Log ("没有追踪到目标用户");
		} else {
			Debug.Log("追踪到了目标用户");
			// analysis the action of user
			analysisUserAction();
		}
	}

	private void analysisUserAction() {

		this.areHandsTogetherValue = Mathf.Abs (GetVector3FromJoint (this.trackedBody.
		     Joints [Kinect.JointType.HandRight]).y - 
			GetVector3FromJoint (this.trackedBody.Joints [Kinect.JointType.HandLeft]).y) + 
			Mathf.Abs (GetVector3FromJoint (this.trackedBody.Joints [Kinect.JointType.HandRight]).x - 
			GetVector3FromJoint (this.trackedBody.Joints [Kinect.JointType.HandLeft]).x);

		this.leftArmOutStretchedValue = GetVector3FromJoint (this.trackedBody.Joints [Kinect.JointType.HandLeft]).x - 
			GetVector3FromJoint (this.trackedBody.Joints [Kinect.JointType.ShoulderLeft]).x;

		this.rightArmOutStretchedValue = GetVector3FromJoint (this.trackedBody.Joints [Kinect.JointType.HandRight]).x - 
			GetVector3FromJoint (this.trackedBody.Joints [Kinect.JointType.ShoulderRight]).x;

		this.leftArmUpValue = GetVector3FromJoint (this.trackedBody.Joints [Kinect.JointType.HandLeft]).y - 
			GetVector3FromJoint (this.trackedBody.Joints [Kinect.JointType.Head]).y;

		this.rightArmUpValue = GetVector3FromJoint (this.trackedBody.Joints [Kinect.JointType.HandRight]).y - 
			GetVector3FromJoint (this.trackedBody.Joints [Kinect.JointType.Head]).y;

		this.turnRightValue = GetVector3FromJoint (this.trackedBody.Joints [Kinect.JointType.SpineMid]).x - 
		                                GetVector3FromJoint (this.trackedBody.Joints [Kinect.JointType.Head]).x;

		this.turnLeftValue = GetVector3FromJoint (this.trackedBody.Joints [Kinect.JointType.SpineMid]).x - 
		                                GetVector3FromJoint (this.trackedBody.Joints [Kinect.JointType.Head]).x;

		// judge whether the user is runing or not
		this.runValue = Mathf.Abs (GetVector3FromJoint (this.trackedBody.Joints [Kinect.JointType.KneeLeft]).y - 
			GetVector3FromJoint (this.trackedBody.Joints [Kinect.JointType.KneeRight]).y);

		// judge whether the user is static or not


		this.areHandsTogether = this.areHandsTogetherValue < 0.04;
		this.isLeftArmOutStretched = this.leftArmOutStretchedValue < -0.4;
		this.isRightArmOutStretched = this.rightArmOutStretchedValue > 0.4;
		this.isLeftArmUp = this.leftArmUpValue > 0;
		this.isRightArmUp = this.rightArmUpValue > 0;
		this.isTurnLeft = this.turnLeftValue > 1;
		this.isTurnRight = this.turnRightValue < -1;
		this.isRun = this.runValue > 0.5;

		value1Label.text = "" + this.areHandsTogether;
		value2Label.text = "" + this.isLeftArmOutStretched;
		value3Label.text = "" + this.isRightArmOutStretched;
		value4Label.text = "" + this.isLeftArmUp;
		value5Label.text = "" + this.isRightArmUp;
		value6Label.text = "" + this.isTurnRight;
		value7Label.text = "" + this.isTurnLeft;
		value8Label.text = "" + this.isRun;

		float h = 0;
		float v = 0;
		if (this.isTurnLeft) {
			h = -turnLeftValue;
		} else if (this.isTurnRight) {
			h = -turnRightValue;
		}

		if (isRun) {
			v = 1;
			timer = 1;
		} 
		if (timer > 0){
			timer -= Time.deltaTime;
			v = 1;
		}

		// calculate move direction to pass to character
		if (m_Cam != null)
		{
			// calculate camera relative direction to move:
			// Debug.Log (m_Cam.forward);
			m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
			m_Move = v*m_CamForward + h*m_Cam.right;
			//m_Move.x /= 3;
			Debug.Log (m_Move);
		}
		else
		{
			// we use world-relative directions in the case of no main camera
			m_Move = v*Vector3.forward + h*Vector3.right;
		}
		
		// pass all parameters to the character control script
		m_Character.Move(m_Move, false, false);
		// Debug.Log (m_Move);
	}

	private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
	{
		return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
	}
}
