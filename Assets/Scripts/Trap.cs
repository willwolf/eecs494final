﻿using UnityEngine;
using System.Collections;

public class Trap : MonoBehaviour {

	public GameObject owner_base;

	private GameManager gm;

	bool initilized = false;

	// Use this for initialization
	void Start () {


	}

	private void init() {
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		
		if (owner_base == null) {
			throw new System.Exception("base not set");
		}
		
		gameObject.layer = gm.teamTrapLayer[owner_base.GetInstanceID()];
	}

	
	// Update is called once per frame
	void Update () {
		if (!initilized) {
			init();
		}
	}
}