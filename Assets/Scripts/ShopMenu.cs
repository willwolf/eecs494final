using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;


public class ShopMenu : MonoBehaviour {

	public bool lerpV = false;
	public int current_item = 0;
	public Vector3 target_point = Vector3.zero;
	public float content_y_offset = 0f;
	public float button_size = 0f;

	public ShopItemList shoplist = null;
	GameManager manager = null;
	public ScrollRect scroll;
	public GameObject menuButtonPrefab;
	public Transform contentPanel;
	public Transform scrollView;
	public List<GameObject> menuButtons = new List<GameObject>();

	// Use this for initialization
	void Awake () {
		GameObject gm = GameObject.Find("GameManager");
		manager = gm.GetComponent<GameManager>();
		shoplist = gm.GetComponent<ShopItemList>();
		scroll = scrollView.GetComponent<ScrollRect>();
		print ("number of shop icons: " + shoplist.items.Count); 
		button_size = menuButtonPrefab.GetComponent<LayoutElement>().minHeight;
		content_y_offset = contentPanel.localPosition.y;
		populateList ();
	}
	
	// Update is called once per frame
	void Update () {
		// Treat the current button like it has a mouse hovering over it
		var pointer = new PointerEventData(EventSystem.current);
		ExecuteEvents.Execute(menuButtons[current_item], pointer, ExecuteEvents.pointerEnterHandler);
		if (lerpV) {
			contentPanel.localPosition = Vector3.Lerp(contentPanel.localPosition, target_point, 75 * scroll.elasticity * Time.deltaTime);
			if (scroll.verticalNormalizedPosition < 0) {
				// Cant scroll any further, so stop
				scroll.verticalNormalizedPosition = 0;
				lerpV = false;
			} else if ((int)contentPanel.localPosition.y == (int)target_point.y) {
				// Reached the end point
				lerpV = false;
			}
		}
	}

	public void ScrollDown() {
		Scroll(1);
	}
	public void ScrollUp() {
		Scroll(-1);
	}
	void Scroll(int shift) { 
		// Remove the pointer event on the previous item
		var pointer = new PointerEventData(EventSystem.current);
		ExecuteEvents.Execute(menuButtons[current_item], pointer, ExecuteEvents.pointerExitHandler);

		// Shift to next item on list
		current_item += shift;
		if (current_item < 0) {
			current_item = 0;
		} else if (current_item >= menuButtons.Count) {
			current_item = menuButtons.Count - 1;
		}

		// Place content panel at the top of the current item
		target_point = new Vector3(contentPanel.localPosition.x, content_y_offset + current_item * button_size, contentPanel.localPosition.z);
		lerpV = true;
	}

	public bool MakePurchase(int team_id, int item_id) {
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
			menuButtons.Add (newButton);
		}
	}

}
