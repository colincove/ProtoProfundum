//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
using MonsterLove.StateMachine; 

public class HeroMovementControls : StateBehaviour 
{
	//Declare which states we'd like use
	public enum HeroStates
	{
		Normal, 
		Climbing, 
		ClimbingDown, 
		Death
	}

	private float _x_move = 0;
	private float _y_move = 0;
	private bool _stride = false;
	private float _speed = 0;
	private Vector3 _bodyAngle;
	private Vector3 _angleVector;
	private bool _flipping = false;
	private Vector3 _prev_control_vector;
	private float _control_vector_delta;
	private float _prev_control_vector_delta;
	private float _control_angle;
	private float _control_angle_delta;
	private Animator animator;
	private float capsuleRadius;
	private CameraAnchorControl camAnchorControl;

	public GameObject ragDoll;
	public float maxRunSpeed = 5;
	public float maxWalkSpeed = 2.5f;
	public float friction = 1.20f;
	public float stopFriction = 1.05f;
	public GameObject cam;
	public GameObject controlAngleAnchor;
	public LayerMask mask;
	public int climbingLayer;
	public int normalLayer;
	public GameObject bloodPrefab;

	public HeroMovementControls ()
	{
	}

	void Awake()
	{
		Initialize<HeroStates>();

		//Change to our first state
		ChangeState(HeroStates.Normal);
	}

	void Start()
	{
		camAnchorControl = FindObjectOfType<CameraAnchorControl> ();
		climbingLayer = LayerMask.NameToLayer ("HeroAlt");
		normalLayer = gameObject.layer;
		animator = GetComponent<Animator>();
		cam = Camera.main.gameObject;
		capsuleRadius = GetComponent<CapsuleCollider> ().radius;
	}

	void Normal_Update()
	{


		_x_move = Input.GetAxis("JoystickLeftHorizontal"); 
		_y_move = Input.GetAxis("JoystickLeftVertical"); 

		_y_move += Input.GetKey ("up") || Input.GetKey (KeyCode.W) ? -1 : 0;
		_y_move += Input.GetKey ("down")|| Input.GetKey (KeyCode.S)  ? 1 : 0;
		_x_move += Input.GetKey ("right") || Input.GetKey (KeyCode.D) ? 1 : 0;
		_x_move += Input.GetKey ("left")|| Input.GetKey (KeyCode.A)  ? -1 : 0;
		
		//float cam_delta_x = cam.transform.position.x - controlAngleAnchor.transform.position.x;
		//float cam_delta_z = cam.transform.position.z - controlAngleAnchor.transform.position.z;

		float cam_delta_x = cam.transform.position.x - controlAngleAnchor.transform.position.x;
		float cam_delta_z = cam.transform.position.z - controlAngleAnchor.transform.position.z;
		
		float cam_angle = Mathf.Atan2(cam_delta_x, cam_delta_z) + .5f;
		//cam_angle = cam.gameObject.transform.rotation.y;
		
		float control_angle = Mathf.Atan2(_y_move, _x_move) + 1.5f;
		float control_vector = Mathf.Sqrt(_x_move * _x_move + _y_move * _y_move);
		Vector3 control_angle_vector = new Vector3(_x_move, 0, _y_move);
		if(_prev_control_vector !=null)
		{
			_control_angle_delta = Vector3.Angle(control_angle_vector, _prev_control_vector);
		}
		_control_vector_delta = Mathf.Abs(_prev_control_vector_delta - control_vector);
		_prev_control_vector_delta = control_vector;
		_prev_control_vector = control_angle_vector;
		
		float angle = cam_angle+control_angle;
		_angleVector.Set(Mathf.Cos(angle), 0, Mathf.Sin(angle));
		
		float runRange = 1;
		float walkRange = 0f;
		float turnRange = 0.03f;
		
		bool running = control_vector >= runRange ? true:false;
		bool walking = !running && control_vector >walkRange;
		bool turning = !walking && !running && control_vector > turnRange;
		
		
		float slerpSpeed = 0.0f;
		_stride = false;
		float vel = 0;

		animator.SetBool ("Running", _x_move != 0 || _y_move != 0);

		if(running)
		{
			//vel = control_vector/20;
			if(_speed>maxRunSpeed / friction) 
			{
				_speed = maxRunSpeed;
				_stride = true;
			}
			slerpSpeed = 0.1f;
		}
		else if(walking)
		{
			//vel = control_vector/50;
			if(_speed>maxWalkSpeed) 
			{
				
				_stride = true;
			}
			slerpSpeed = 0.05f;
			//speed = 0.5;
		}
		else if(turning)
		{	
			slerpSpeed = 0.01f;
			//speed = 0;
		}
		else
		{
			//speed = 0;
		}
		if(control_vector >=1)
		{
			_stride = true;
		}
		else
		{
			_stride = false;
		}
		if(!turning) vel = (control_vector - walkRange) / 20;
		Vector3 targetDir = _bodyAngle - _angleVector;
		Vector3 forward = transform.forward;
		float a = Vector3.Angle(_bodyAngle, _angleVector);
		
		if (a < 10F || _stride)
		{
			//speed += vel;
			_speed = vel*40;
		}
		
		if (a < 5F)
		{
			_flipping = false;
		}
		
		if(_flipping)
		{
			slerpSpeed = 0.4f;
			//speed=0;
			//bodyAngle = angleVector;
		}
		
		if(control_vector < turnRange)
		{
			_speed= _speed / stopFriction;
		}
		
		_speed = (_speed) / friction;
		_bodyAngle = Vector3.Slerp(_bodyAngle, _angleVector, slerpSpeed);

		var deltaAngle = Vector3.AngleBetween(_bodyAngle, _angleVector);

		float strength = Mathf.Sqrt(_x_move * _x_move + _y_move * _y_move) /10;
		Vector3 newVel = (transform.forward * (_speed * 5));//override Z and X movement
		newVel.y = GetComponent<Rigidbody>().velocity.y;//retain gravity values
		GetComponent<Rigidbody>().velocity =  newVel;
		GetComponent<Rigidbody>().transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(_bodyAngle.z, _bodyAngle.x)/(Mathf.PI/180)+180, Vector3.up);

		CheckForClimb ();
	}
	void Climbing_Enter()
	{
		GetComponent<CapsuleCollider> ().radius = (float)(capsuleRadius * 0.3);
		animator.SetBool ("DoClimb", true);
		animator.Play ("Climbing");
		GetComponent<Rigidbody> ().isKinematic = true;
		GetComponent<Rigidbody> ().useGravity = false;
		GetComponent<CapsuleCollider> ().enabled = false;
		GetComponent<Rigidbody> ().velocity = new Vector3 ();
		gameObject.layer = climbingLayer;
	}
	void ClimbingDown_Enter()
	{
		GetComponent<CapsuleCollider> ().radius = (float)(capsuleRadius * 0.3);
		animator.SetBool ("DoClimbDown", true);
		animator.Play ("ClimbDown");
		GetComponent<Rigidbody> ().isKinematic = true;
		GetComponent<Rigidbody> ().useGravity = false;
		GetComponent<CapsuleCollider> ().enabled = false;
		GetComponent<Rigidbody> ().velocity = new Vector3 ();
		gameObject.layer = climbingLayer;
	}
	void Climbing_Exit()
	{
		GetComponent<CapsuleCollider> ().radius = capsuleRadius;
	}
	void ClimbingDown_Exit()
	{
		GetComponent<CapsuleCollider> ().radius = capsuleRadius;
	}

	private void CheckForClimb()
	{
		RaycastHit hit = new RaycastHit ();
		float deploymentHeight = 10;
		Ray ray = new Ray (transform.position + transform.forward * .55f + transform.up * 2.5f, Vector3.down);
		if (Physics.Raycast (ray, out hit, deploymentHeight, mask)) {
			float distanceToGround = hit.distance;
			if (Input.GetKey (KeyCode.Space)) {
				if(distanceToGround>1.6 && distanceToGround < 2)
				{
					//climb up
					ChangeState (HeroStates.Climbing);
				}
				if(distanceToGround>5.0 && distanceToGround < 5.4)
				{
					//climb down
					ChangeState (HeroStates.ClimbingDown);
				}
			}
		}
	}

	void Climbing_Update()
	{
		if (!this.animator.GetCurrentAnimatorStateInfo(0).IsName("Climbing"))
		{

		//	animator.SetBool ("DoClimb", false);
			//ChangeState(HeroStates.Normal);
			// Avoid any reload.
		}

	}
	void ClimbComplete()
	{
		animator.SetBool ("DoClimb", false);
		animator.SetBool ("DoClimbDown", false);
		ChangeState(HeroStates.Normal);
	}
	void Normal_Enter()
	{
		gameObject.layer = normalLayer;
		GetComponent<Rigidbody> ().useGravity = true;
		GetComponent<Rigidbody> ().isKinematic = false;
		GetComponent<CapsuleCollider> ().enabled = true;
	}
	void Death_Enter()
	{

	}
	public void Death()
	{
		camAnchorControl.target = (GameObject)Instantiate (ragDoll, transform.position, transform.rotation);
		camAnchorControl.lookTarget = camAnchorControl.target;
		//camAnchorControl.target.GetComponent<Rigidbody> ().AddForce (new Vector3 (1, 1, 1));
		GetComponent<Collider> ().enabled = false;

		ChangeState(HeroStates.Death);
		GetComponent<Rigidbody> ().isKinematic = true;
		GetComponent<Rigidbody> ().freezeRotation = true;
		GetComponent<Rigidbody> ().useGravity = false;

		animator.SetBool ("Death", true);

		GameObject blood = Instantiate<GameObject> (bloodPrefab);
		blood.transform.parent = transform.parent;
		blood.transform.position = transform.position;


		SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer> ();
		Debug.Log ("DEATH: "+renderers.Length);
		foreach (SkinnedMeshRenderer renderer in renderers) {
			renderer.enabled = false;
		}
	}
}

