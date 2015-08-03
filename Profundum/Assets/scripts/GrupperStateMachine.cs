﻿using UnityEngine;
using System.Collections;
using MonsterLove.StateMachine;

public class GrupperStateMachine : StateBehaviour {
	//Declare which states we'd like use
	public enum GrupperStates
	{
		Init,
		Attacking, 
		Agro, 
		Feeding, 
		Idle, 
		Scared,
		Searching
	}

	public GameObject agroRangeObj;
	public GameObject closeRangeObj;
	public GameObject attackAreaObj;
	public float attackHeight = 8;
	public float searchHeight = 3;
	public Vector3 searchRotation = new Vector3 (0, 1, 0);


	private SphereCollider _agroCollider;
	private Vector3 _agroColliderRange;
	private Collider lightAgroCol;
	private GameObject _player;
	private bool agro = false;
	private bool close = false;
	private bool lightAgro = false;
	private bool lightClose = false;
	private bool canAttack = false;
	private float _acc = 0.01f;
	private float _speed = 0f;
	private Rigidbody rb;
	private Vector3 _impulseSpeed = new Vector3 (0.1f, 0.03f, 0.1f);
	private Vector3 _riseSpeed = new Vector3 (0.1f, 0.4f, 0.1f);
	private Vector3 _searchRotation = new Vector3 (0, 1, 0);

	private CollisionDelegate lightAgroRange, lightCloseRange, closeRange, agroRange, attackRange;

	// Use this for initialization
	void Awake () {
		Initialize<GrupperStates>();

		//Change to our first state
		ChangeState(GrupperStates.Searching);

		agroRange = agroRangeObj.AddComponent <CollisionDelegate>();
		agroRange.CollideWithPlayer += () => agro = true;   
		agroRange.ExitPlayerCollision += () => agro = false; 
		_agroCollider = agroRangeObj.GetComponent<SphereCollider> ();
		_agroColliderRange = new Vector3 (_agroCollider.radius, _agroCollider.radius, _agroCollider.radius);
		
		closeRange = closeRangeObj.AddComponent <CollisionDelegate>();
		closeRange.CollideWithPlayer += () => close = true;   
		closeRange.ExitPlayerCollision += () => close = false; 

		lightCloseRange = closeRangeObj.AddComponent <CollisionDelegate>();
		lightCloseRange.tag = "Player";
		lightCloseRange.CollideWithPlayer += () => lightClose = true;   
		lightCloseRange.ExitPlayerCollision += () => lightClose = false; 

		lightAgroRange = agroRangeObj.AddComponent <CollisionDelegate>();
		lightAgroRange.tag = "Player";
		lightAgroRange.CollideWithPlayer += () => lightAgro = true;   
		lightAgroRange.ExitPlayerCollision += () => lightAgro = false; 

		attackRange = attackAreaObj.AddComponent <CollisionDelegate>();
		attackRange.tag = "Player";
		attackRange.CollideWithPlayer += () => canAttack = true;   
		attackRange.ExitPlayerCollision += () => canAttack = false; 

		rb = GetComponent<Rigidbody>();
	}
	void Update()
	{
	}

	void DoAttack()
	{
		Debug.Log ("DoAttack");
	}

	void Agro_Update()
	{
		if (canAttack) 
		{
			ChangeState(GrupperStates.Attacking);
		}
	}

	void Scared_Update()
	{
		lightAgroCol = lightAgroRange.col;

		if (!lightAgroCol) {
			return;
		}

		if (!lightAgro || lightAgroCol.gameObject == null) {
			ChangeState(GrupperStates.Searching);
			return;
		}

		transform.LookAt(lightAgroCol.gameObject.transform.position);

		if(Vector3.Distance(transform.position, lightAgroCol.gameObject.transform.position) < _agroColliderRange.x) {
			Vector3 force = (transform.position -  lightAgroCol.gameObject.transform.position);
			
			force.x = force.x < 0 ? (force.x + _agroColliderRange.x) * -1:_agroColliderRange.x - force.x;
			force.y = force.y < 0 ? (force.y + _agroColliderRange.y) * -1:_agroColliderRange.y - force.y;
			force.z = force.z < 0 ? (force.z + _agroColliderRange.z) * -1:_agroColliderRange.z - force.z;
			
			force /= 20;
			
			force.y = force.y/3;
			
			if(force.y<0)
			{
				force.y = force.y*-1;
			}
			rb.AddForce(force, ForceMode.Impulse);
		}
	}

	void Searching_Update()
	{
		if (lightAgro ==  true) {
			ChangeState(GrupperStates.Agro);
		}
		RaycastHit hit = new RaycastHit ();
		RaycastHit closeHit = new RaycastHit ();
		float deploymentHeight = 10;
		Ray ray = new Ray (transform.position + transform.forward * 3.0f + transform.up * 1.5f, Vector3.down);
		Ray closeRay = new Ray (transform.position + transform.forward * 1.5f + transform.up * 1.5f, Vector3.down);
		rb.AddForce (Vector3.Scale (_impulseSpeed, transform.forward), ForceMode.Impulse);
		transform.Rotate (searchRotation);
		Physics.Raycast (closeRay, out closeHit, deploymentHeight);
		if (Physics.Raycast (ray, out hit, deploymentHeight)) {
			float distanceToGround = hit.distance;
			if (distanceToGround > searchHeight) {
				rb.AddForce (Vector3.Scale (_impulseSpeed, Vector3.down), ForceMode.Impulse);
			} else if (distanceToGround < searchHeight - .5) {
				rb.AddForce (Vector3.Scale (_impulseSpeed, Vector3.up), ForceMode.Impulse);
				if(distanceToGround < closeHit.distance - 0.5)
				{
					rb.AddForce (Vector3.Scale (_riseSpeed, Vector3.up), ForceMode.Impulse);
				}
			}
			Debug.DrawRay (ray.origin, ray.direction * deploymentHeight, Color.green);
		} else {
			rb.AddForce (Vector3.Scale (_impulseSpeed, Vector3.down), ForceMode.Impulse);
			Debug.DrawRay (ray.origin, ray.direction * deploymentHeight, Color.red);
		}
	}

	void Feeding_Update()
	{
	}
}
