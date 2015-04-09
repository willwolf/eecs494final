using UnityEngine;
using System.Collections;

public class CatapultStoneScript : BaseUpgradeItem {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	override
	public bool CanPurchase(PlayerController p, int teamId, GameManager gm) {
		if (!GameManager.FORCE_CATAPULT_ORDER)
			return !gm.teamCatapultStatus[teamId].has_projectile;
		else
			return gm.teamCatapultStatus[teamId].has_arm && !gm.teamCatapultStatus[teamId].has_projectile;
	}

	override public void MakePurchase(PlayerController p, int teamId, GameManager gm) {
		gm.AddCatapultPart(teamId, CatapultPart.stone);
	}
}
