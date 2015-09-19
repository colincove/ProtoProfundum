﻿using UnityEngine;
using System.Collections;

public class LightBubble : MonoBehaviour {
	public float startScale = 2;
	public float endScale = 10;
	public float animTime = 5;
	public AnimationCurve expandCurve;

	private float _spawnTime;
	private float _delta;
	private float _scale;
	// Use this for initialization
	void Start () {
		_spawnTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		_delta = Time.time - _spawnTime;
		if (_delta < animTime) {
			_scale = startScale + expandCurve.Evaluate(_delta/animTime) * (endScale - startScale);
			transform.localScale = new Vector3 (_scale, _scale, _scale);
		}

	}
}