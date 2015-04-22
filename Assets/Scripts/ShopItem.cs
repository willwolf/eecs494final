using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class ShopItem : MonoBehaviour {
	
	public Sprite icon;
	public string item_name;
	public int stone_cost;
	public int wood_cost;
	public string description;

	public ShopItem() {

	}

	public ShopItem(Sprite i, string n, int s_c, int w_c, string d){
		icon = i;
		item_name = n;
		stone_cost = s_c;
		wood_cost = w_c;
		description = d;
	}

 	public abstract bool CanPurchase(PlayerController p, int teamId, GameManager gm);
	public abstract void MakePurchase(PlayerController p, int teamId, GameManager gm);
}