using UnityEngine;
using System.Collections;

public class CatapultArmScript: BaseUpgradeItem {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	override
	public bool CanPurchase(int teamId, GameManager gm) {
		return !gm.teamCatapultStatus[teamId].has_arm;
	}

	override public void MakePurchase(int teamId, GameManager gm) {
		gm.teamCatapultStatus[teamId].has_arm = true;
	}
}
