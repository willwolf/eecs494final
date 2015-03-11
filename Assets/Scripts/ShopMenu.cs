using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;


public class ShopMenu : MonoBehaviour {

	public ShopItemList shoplist = null;
	GameManager manager = null;
	public GameObject menuButtonPrefab;
	public Transform contentPanel;

	// Use this for initialization
	void Awake () {
		GameObject gm = GameObject.Find("GameManager");
		manager = gm.GetComponent<GameManager>();
		shoplist = gm.GetComponent<ShopItemList>();
		print ("number of shop icons: " + shoplist.items.Count); 
		populateList ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	bool MakePurchase(int team_id, int item_id) {
		ResourceCount team_count = manager.GetTeamResourceInfo(team_id);
		ShopItem item = shoplist.items[item_id].GetComponent<ShopItem>();
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
		foreach (GameObject go in shoplist.items) {
			ShopItem i = go.GetComponent<ShopItem>();
			GameObject newButton = Instantiate (menuButtonPrefab) as GameObject;
			MenuButton button = newButton.GetComponent <MenuButton> ();
			button.nameLabel.text = i.item_name;
			button.icon.sprite = i.icon;
			button.woodLabel.text = "Wood: " + i.wood_cost.ToString();
			button.stoneLabel.text = "Stone: " + i.stone_cost.ToString();
			newButton.transform.SetParent (contentPanel);
		}
	}

}
