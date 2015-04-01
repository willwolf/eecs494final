using UnityEngine;
using System.Collections;

public class HealthScript : ShopItem {


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public override bool CanPurchase(PlayerController p, int teamId, GameManager gm) {
		return true;
	}

	public override void MakePurchase(PlayerController p, int teamId, GameManager gm) {
		p.health = p.startingHealth;
	}
}
