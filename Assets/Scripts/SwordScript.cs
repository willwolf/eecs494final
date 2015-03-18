using UnityEngine;
using System.Collections;

public class SwordScript : WeaponItem {
	Vector3 swingStart;
	Vector3 swingEnd;
	bool upswing;
	bool downswing;

	// Use this for initialization
	void Start () {
		swingStart = new Vector3 (1, 0, 1);
		swingEnd = new Vector3 (-1, 0, 1);
		transform.localPosition = swingStart;
		upswing = false;
		downswing = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (upswing){
			transform.localPosition = Vector3.Slerp(transform.localPosition, swingEnd, 25 * Time.deltaTime);
			print ("swingup");
			if (transform.localPosition == swingEnd) {
				print ("start downswing");
				upswing = false;
				downswing = true;
			}
		}	
		if (downswing){
			print ("swingdown");
			transform.localPosition = Vector3.Slerp(transform.localPosition, swingStart, 25 * Time.deltaTime);
			if (transform.localPosition == swingStart) {	
				print ("back to start");
				downswing = false;
			}
		}
	}

	override
	public bool CanPurchase() {
		return true;
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.name == this.transform.parent.gameObject.name) {
			return;		
		}
		print ("poke");
		foreach (ContactPoint contact in collision.contacts) {
			Debug.DrawRay(contact.point, contact.normal, Color.white);
		}
	}

	void OnTriggerEnter(Collider coll) {
		print ("It's a trigger!");
	}

	public void Swing(){
		if (upswing || downswing) {
			return;
		}
		if(!upswing){
			upswing = true;
		}

	}

}
