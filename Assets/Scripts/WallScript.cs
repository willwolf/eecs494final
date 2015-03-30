using UnityEngine;
using System.Collections;

public class WallScript : BaseUpgradeItem {

	// Use this for initialization
	void Start () {
	
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
}
