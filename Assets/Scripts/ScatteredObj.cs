using UnityEngine;
using System.Collections;

public class ScatteredObj : MonoBehaviour {
	public ResourceType type = ResourceType.none;
	public float canPickUpAt = 0f;
	public float disableTime = 0.5f;

	// Use this for initialization
	void Start () {
		canPickUpAt = Time.time + disableTime;
	}
	
	// Update is called once per frame
	void Update () {
		if (type == ResourceType.none) {
			throw new UnassignedReferenceException("ScatteredObj script's type is none");
		}
	}

	public bool CanPickUp() {
		return Time.time >= canPickUpAt;
	}
}
