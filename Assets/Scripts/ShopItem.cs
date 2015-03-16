using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ShopItemType {
	sword,
	wall
}

public abstract class ShopItem : MonoBehaviour {
	
	public Sprite icon;
	public string item_name;
	public int stone_cost;
	public int wood_cost;

	public ShopItem() {

	}

	public ShopItem(Sprite i, string n, int s_c, int w_c){
		icon = i;
		item_name = n;
		stone_cost = s_c;
		wood_cost = w_c;
	}

 	public abstract bool CanPurchase();
	public abstract ShopItemType ItemType();
}