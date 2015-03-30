using UnityEngine;
using System.Collections;

public class BowScript : WeaponItem {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	override
	public bool CanPurchase(PlayerController p, int teamId, GameManager gm) {
		return !(p.currentWeapon is BowScript);
	}

	override public void MakePurchase(PlayerController p, int teamId, GameManager gm) {
		p.weapons[p.currentWeaponIndex].SetActive(false);
		p.currentWeaponIndex = 2;
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
}
