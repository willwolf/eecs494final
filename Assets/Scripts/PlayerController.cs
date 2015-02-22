﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

	public int player_num  = 0;
	public float rotate_speed = 90f;
	public float walk_speed = 8f;

	public int current_resources = 0;
	public int MAX_RESOURCES = 50;

	private Text resource_text;

	// Use this for initialization
	void Start () {
		if (player_num == 0) {
			throw new UnassignedReferenceException("PlayerController::playerNum must be non-zero");
		}
		resource_text = GameObject.Find("Resource " + player_num.ToString()).GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		if (player_num == 0) {
			throw new UnassignedReferenceException("PlayerController::playerNum must be non-zero");
		}
		float horizInput = Input.GetAxis("Horizontal_" + player_num.ToString()),
			  vertInput = Input.GetAxis("Vertical_" + player_num.ToString());

		transform.Rotate(Vector3.up, rotate_speed * Time.deltaTime * horizInput);
		transform.localPosition += (transform.forward * walk_speed * vertInput * Time.deltaTime);

		if (Input.GetButtonDown("Action_" + player_num.ToString())) {
			TakeAction();
		}
	}

	void TakeAction() {
		print ("Player " + player_num.ToString() + " is taking an action!");

		RaycastHit hitinfo;
		if (IsInRange(out hitinfo)) {
			print ("Player " + player_num.ToString() + " is in range!");
			Resource r = hitinfo.transform.GetComponent<Resource>();
			if (r == null) {
				throw new UnassignedReferenceException("Resourse layer object does not have Resource script attached");
			}

			switch (r.type) {
			case ResourceType.stone:
				MineStone();
				break;
			case ResourceType.wood:
				ChopWood();
				break;
			}
		}
	}
	void ChopWood() {
		print ("Player " + player_num.ToString() + " is chopping wood!");
		UpdateCurrentResources(5);
	}
	void MineStone() {
		print ("Player " + player_num.ToString() + " is mining!");
		UpdateCurrentResources(5);
	}

	void UpdateCurrentResources(int amount) {
		current_resources += amount;
		if (current_resources >= MAX_RESOURCES) {
			current_resources = MAX_RESOURCES;
		}
		if (resource_text == null) {
			throw new UnassignedReferenceException("resource_text for Player " + player_num.ToString() + " is uninitialized");
		}
		resource_text.text = "Carrying " + current_resources + " resources";
	}

	bool IsInRange(out RaycastHit hitinfo) {
		Vector3 halfWidth = transform.right / 2f;
		float halfHeight = transform.lossyScale.y / 2f;
		Vector3 center, leftCenter, rightCenter, footPos, footLeft, footRight;
		
		// Initialize center positions
		center = leftCenter = rightCenter = transform.position;
		leftCenter -= halfWidth;
		rightCenter += halfWidth;
		
		// Initialze bottom positions
		footPos = transform.position;
		footPos.y -= halfHeight;
		footLeft = footRight = footPos;
		footLeft -= halfWidth;
		footRight += halfWidth;
		
		return (CastResourceRay(center, out hitinfo) || CastResourceRay(footPos, out hitinfo) ||
		        CastResourceRay(leftCenter, out hitinfo) || CastResourceRay(footLeft, out hitinfo) ||
		        CastResourceRay(rightCenter, out hitinfo) || CastResourceRay(footRight, out hitinfo));
	}
	bool CastResourceRay(Vector3 origin, out RaycastHit info) {
		int layerMask = 1 << 8; // only collide with Resource layer
		return Physics.Raycast(origin, transform.forward, out info, 1.5f, layerMask);
	}
}
