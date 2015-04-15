using UnityEngine;
using System.Collections;

public class CatapultWeightScript: BaseUpgradeItem {
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	override
	public bool CanPurchase(PlayerController p, int teamId, GameManager gm) {
		if (!GameManager.FORCE_CATAPULT_ORDER)
			return !gm.teamCatapultStatus[teamId].has_weight;
		else
			return gm.teamCatapultStatus[teamId].has_legs && !gm.teamCatapultStatus[teamId].has_weight;
	}
	
	override public void MakePurchase(PlayerController p, int teamId, GameManager gm) {
		gm.AddCatapultPart(teamId, CatapultPart.weight);
	}
}
