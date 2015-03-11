using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShopItem : MonoBehaviour {
	
	public Sprite icon;
	public string name;
	public int stone_cost;
	public int wood_cost;
	
	public ShopItem(Sprite i, string n, int s_c, int w_c){
		icon = i;
		name = n;
		stone_cost = s_c;
		wood_cost = w_c;
	}
	
}

public class ShopItemList {
	public List<ShopItem> items = new List<ShopItem>();
	
	public ShopItemList() {
		addItem (Resources.LoadAssetAtPath<Sprite>("Assets/Sprites/Sword.png"), 
		         "Sword", 3, 3);
		addItem (Resources.LoadAssetAtPath<Sprite>("Assets/Sprites/Wall.png"), 
		         "Wall", 0, 10);
		addItem (Resources.LoadAssetAtPath<Sprite>("Assets/Sprites/Turret.png"), 
		         "Turret", 10, 0);
		//		addItem ("hiofd", "jiofds", "inodfs", "jiofds");
//		addItem ("hiofd", "jiofds", "inodfs", "jiofds");
	}
	
	public void addItem(Sprite i, string n, int s_c, int w_c){
		items.Add (new ShopItem (i, n, s_c, w_c));
	}
	
}