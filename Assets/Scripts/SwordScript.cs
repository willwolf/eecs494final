using UnityEngine;
using System.Collections;

public class SwordScript : ShopItem {
	Vector3 swingStart;
	Vector3 swingEnd;

	bool upswing;
	bool downswing;

	Transform swordTransform;
	Quaternion r0;
	Quaternion r1;


	// Use this for initialization
	void Start () {
		swingStart = new Vector3 (1, 0, 1);
		swingEnd = new Vector3 (-1, 0, 1);
		transform.localPosition = swingStart;
		//a = player.trans.pos + forward + v3up
//		swingStart = GetComponentInParent<Transform> ();
//		swingEnd = GetComponentInParent<Transform> ();
//		swingStart.position = GetComponentInParent<Transform> ().position + GetComponentInParent<Transform>().forward;
//		swingEnd.position = GetComponentInParent<Transform> ().position + GetComponentInParent<Transform>().right;
//		swordTransform = GetComponentInParent<Transform>();
//		swordTransform.transform.position = new Vector3 (swordTransform.transform.position.x, 1, swordTransform.transform.position.z);
//		swordTransform.LookAt(swingStart, Vector3.up);
//		r0 = swordTransform.rotation;
//		swordTransform.LookAt(swingEnd, Vector3.right);
//		r1 = swordTransform.rotation;

		upswing = false;
		downswing = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (upswing){






			transform.localPosition = Vector3.Slerp(transform.localPosition, swingEnd, 25 * Time.deltaTime);
			print ("swingup");
			//if(Mathf.Approximately(transform.localPosition.x, -1) || 
			//   transform.localPosition.x < -1){
			if (transform.localPosition == swingEnd) {
				print ("start downswing");
				upswing = false;
				downswing = true;
			}
		}	
		if (downswing){
			print ("swingdown");
			transform.localPosition = Vector3.Slerp(transform.localPosition, swingStart, 25 * Time.deltaTime);
			//if(Mathf.Approximately(transform.localPosition.x, 1) || 
			//   transform.localPosition.x > 1){
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

	public void swing(){
		if (upswing || downswing) {
			return;
		}
		if(!upswing){
			upswing = true;
		}

	}

	/*
	public bool swing(){
		swingStart.position = GetComponentInParent<Transform> ().position + GetComponentInParent<Transform>().forward;
		swingEnd.position = GetComponentInParent<Transform> ().position + GetComponentInParent<Transform>().right;

		swordTransform.LookAt(swingStart, Vector3.up);
		r0 = swordTransform.rotation;
		swordTransform.LookAt(swingEnd, Vector3.right);
		r1 = swordTransform.rotation;
		if (upswing) {
			//a = player.trans.pos + forward + v3up
			//b = p.t.pos + p.t.right + v3up/2
			swordTransform.transform.localPosition = Vector3.Slerp(swingStart.localPosition, new Vector3(1, 0, 1), .1f);
		//	swordTransform.transform.localRotation = Quaternion.Lerp(r0, r1, Time.deltaTime*10f);		
			//swordTransform.transform.localRotation =  Quaternion.Lerp(transform.localRotation, Quaternion.Euler(34, -111, 254), Time.deltaTime * 10f);
			if(swordTransform.transform.localPosition >= swingEnd.position){
			//if(swordTransform.transform.localRotation.w >= r1.w){
				upswing = false;
			}
			return true;
		}
		else{
			//swordTransform.transform.localPosition = Vector3.Slerp(swingEnd.localPosition, swingStart.localPosition, .1f);
		//	swordTransform.transform.localRotation = Quaternion.Slerp(r1, r0, 0.001f);		
			if(swordTransform.transform.localPosition <= swingStart.transform.position){
			//if(swordTransform.transform.localRotation.w <= r0.w){
				upswing = true;
				return false;
			}
			return true;
		}
	}*/

	public void setSwordTransform(){

	}

	public void setUpswing(bool swing){
		upswing = swing;
	}

}
