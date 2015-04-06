using UnityEngine;
using System.Collections;

public class ArrowRefill : ShopItem {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public override bool CanPurchase(PlayerController p, int teamId, GameManager gm) {
		return p.arrows < p.STARTING_ARROWS && p.currentWeapon is BowScript;
	}
	public override void MakePurchase(PlayerController p, int teamId, GameManager gm) {
		p.arrows = p.STARTING_ARROWS;
	}
}
