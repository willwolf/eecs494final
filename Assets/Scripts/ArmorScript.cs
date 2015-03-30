using UnityEngine;
using System.Collections;

public class ArmorScript : BaseUpgradeItem {

	public int STARTING_HEALTH = 4;
	public int armorHealth = 4;

	// Use this for initialization
	void Start () {
		armorHealth = STARTING_HEALTH;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	override
	public bool CanPurchase(int teamId, GameManager gm) {
		return true;
	}

	override public void MakePurchase(int teamId, GameManager gm) {
		
	}

	public void TakeDamage(int damage) {
		armorHealth -= damage;
	}

	public bool IsDestroyed() {
		return armorHealth <= 0;
	}
}
