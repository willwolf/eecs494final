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
	public bool CanPurchase(PlayerController p, int teamId, GameManager gm) {
		return p.playerArmor == null;
	}

	override public void MakePurchase(PlayerController p, int teamId, GameManager gm) {
		p.armor = Instantiate(p.armorPrefab, p.transform.position, p.transform.rotation) as GameObject;
		p.armor.transform.parent = p.transform;
		p.playerArmor = p.armor.GetComponent<ArmorScript>();
		p.armor_slider.value = p.playerArmor.armorHealth;
	}

	public void TakeDamage(int damage) {
		armorHealth -= damage;
	}

	public bool IsDestroyed() {
		return armorHealth <= 0;
	}
}
