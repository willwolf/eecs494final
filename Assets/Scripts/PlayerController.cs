using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public int playerNum  = 0;
	public float rotateSpeed = 90f;
	public float walkSpeed = 8f;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (playerNum == 0) {
			throw new UnassignedReferenceException("PlayerController::playerNum must be non-zero");
		}
		float horizInput = Input.GetAxis("Horizontal_" + playerNum.ToString()),
			  vertInput = Input.GetAxis("Vertical_" + playerNum.ToString());

		transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime * horizInput);
		transform.localPosition += (transform.forward * walkSpeed * vertInput * Time.deltaTime);

		if (Input.GetButtonDown("Action_" + playerNum.ToString())) {
			TakeAction();
		}
	}

	void TakeAction() {
		print ("Player " + playerNum.ToString() + " is taking an action!");

		RaycastHit hitinfo;
		if (IsInRange(out hitinfo)) {
			print ("Player " + playerNum.ToString() + " is in range!");
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
		print ("Player " + playerNum.ToString() + " is chopping wood!");
		
	}
	void MineStone() {
		print ("Player " + playerNum.ToString() + " is mining!");
		
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
