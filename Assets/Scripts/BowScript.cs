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
		return true;
	}

	override public void MakePurchase(PlayerController p, int teamId, GameManager gm) {
		
	}
}
