using UnityEngine;
using System.Collections;

public class ScatteredObj : MonoBehaviour {
	public ResourceType type = ResourceType.none;
	public float canPickUpAt = 0f;
	public float disableTime = 0.5f;
	public float stopRollingAt = 0f;
	public float rollTime = 2f;
	public float speedSlow = 0.5f;

	// Use this for initialization
	void Start () {
		canPickUpAt = Time.time + disableTime;
		stopRollingAt = Time.time + rollTime;
	}
	
	// Update is called once per frame
	void Update () {
		if (type == ResourceType.none) {
			throw new UnassignedReferenceException("ScatteredObj script's type is none");
		}
		if (SlowRoll()) {
			Vector3 decrement = rigidbody.velocity;
			decrement *= speedSlow;
			rigidbody.velocity -= decrement;
		}
	}

	public bool CanPickUp() {
		return Time.time >= canPickUpAt;
	}

	public bool SlowRoll() {
		return Time.time >= stopRollingAt;
	}
}
