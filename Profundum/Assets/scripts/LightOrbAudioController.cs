﻿using UnityEngine;
using System.Collections;

public class LightOrbAudioController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		AkSoundEngine.PostEvent ("Play_LightHum", gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void Audio_Shoot ()
	{
		AkSoundEngine.PostEvent("Stop_LightHum", gameObject);
		AkSoundEngine.PostEvent("Play_LightReturn", gameObject);
		AkSoundEngine.PostEvent("Play_LightHum", gameObject);
	}
	void Audio_Shoot_Fail ()
	{
		AkSoundEngine.PostEvent("Play_LightFail", gameObject);
	}
	void Audio_StopHum ()
	{
		AkSoundEngine.PostEvent("Stop_LightHum", gameObject);
	}
}