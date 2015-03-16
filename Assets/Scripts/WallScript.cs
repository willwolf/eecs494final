using UnityEngine;
using System.Collections;

public class WallScript : ShopItem {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	override
	public bool CanPurchase() {
		return true;
	}
	override
	public ShopItemType ItemType() {
		return ShopItemType.wall;
	}
}
