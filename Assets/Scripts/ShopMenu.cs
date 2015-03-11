using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;


public class ShopMenu : MonoBehaviour {

	public List<ShopItem> items;
	GameManager manager = null;
	public GameObject menuButtonPrefab;
	public Transform contentPanel;

	// Use this for initialization
	void Start () {
		manager = GameObject.Find ("GameManager").GetComponent<GameManager>();
		ShopItemList temp = new ShopItemList();
		items = temp.items;
		print ("number of shop icons: " + items.Count); 
		populateList ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	bool MakePurchase(int team_id, int item_id) {
		ResourceCount team_count = manager.GetTeamResourceInfo(team_id);
		ShopItem item = items[item_id];
		if (CanPurchase(item, team_count)) {
			manager.RemoveResources(team_id, ResourceType.stone, item.stone_cost);
			manager.RemoveResources(team_id, ResourceType.wood, item.wood_cost);
			return true;
		} 
		return false;
	}
	
	bool CanPurchase(ShopItem item, ResourceCount resource_count) {
		return item.wood_cost <= resource_count.wood && item.stone_cost <= resource_count.stone;
	}

	public void populateList(){
		foreach (ShopItem i in items) {
			GameObject newButton = Instantiate (menuButtonPrefab) as GameObject;
			MenuButton button = newButton.GetComponent <MenuButton> ();
			button.nameLabel.text = i.name;
			button.icon.sprite = i.icon;
			button.woodLabel.text = "Wood: " + i.wood_cost.ToString();
			button.stoneLabel.text = "Stone: " + i.stone_cost.ToString();
			newButton.transform.SetParent (contentPanel);
		}
	}

}
