using UnityEngine;
using System.Collections;
using System.Collections.Generic;

struct Item {
	public int wood_cost;
	public int stone_cost;
	public string name;
}

public class ShopMenu : MonoBehaviour {
	
	GameManager manager = null;
	List<Item> items;
	
	// Use this for initialization
	void Start () {
		manager = GameObject.Find ("GameManager").GetComponent<GameManager>();
		items = new List<Item>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	bool MakePurchase(int team_id, int item_id) {
		ResourceCount team_count = manager.GetTeamResourceInfo(team_id);
		Item item = items[item_id];
		if (CanPurchase(item, team_count)) {
			manager.RemoveResources(team_id, ResourceType.stone, item.stone_cost);
			manager.RemoveResources(team_id, ResourceType.wood, item.wood_cost);
			return true;
		} 
		return false;
	}
	
	bool CanPurchase(Item item, ResourceCount resource_count) {
		return item.wood_cost <= resource_count.wood && item.stone_cost <= resource_count.stone;
	}
}
