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
			//print ("swingup");
			if (transform.localPosition == swingEnd) {
			//	print ("start downswing");
				upswing = false;
				downswing = true;
			}
		}	
		if (downswing){
			//print ("swingdown");
			transform.localPosition = Vector3.Slerp(transform.localPosition, swingStart, 25 * Time.deltaTime);
			if (transform.localPosition == swingStart) {	
		//		print ("back to start");
				downswing = false;
			}
		}
	}

	override
	public bool CanPurchase(PlayerController p, int teamId, GameManager gm) {
		return !(p.currentWeapon is SwordScript);
	}

	override public void MakePurchase(PlayerController p, int teamId, GameManager gm) {
		p.weapons[p.currentWeaponIndex].SetActive(false);
		if (item_name.Contains("Stone")) {
			p.currentWeaponIndex = 0;
		} else {
			p.currentWeaponIndex = 1;
		}
		p.hasWeapon = true;
		p.currentWeapon = this;
		p.weapons[p.currentWeaponIndex].SetActive(true);
		//set to visible
		foreach (MeshRenderer renderer in p.weapons[p.currentWeaponIndex].GetComponentsInChildren<MeshRenderer>()) {
			renderer.enabled = true;
		}
		foreach (Collider collider in p.weapons[p.currentWeaponIndex].GetComponentsInChildren<Collider>()) {
			collider.enabled = true;
		}
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.name == this.transform.parent.gameObject.name) {
			return;		
		}

		foreach (ContactPoint contact in collision.contacts) {
			Debug.DrawRay(contact.point, contact.normal, Color.white);
		}
	}

	void OnTriggerEnter(Collider coll) {
//		print ("It's a trigger!");
	}

	public void Swing(AudioSource s){
		if (upswing || downswing) {
			return;
		}
		s.Play();
		if(!upswing){
			upswing = true;
		}

	}

}
