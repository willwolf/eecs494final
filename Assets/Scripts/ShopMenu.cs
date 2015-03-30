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
	public const float scroll_interval = 0.15f;
	public float allow_scroll_at = 0;

	public ShopItemList shoplist = null;
	GameManager manager = null;
	public ScrollRect scroll;
	public GameObject menuButtonPrefab;
	public Transform contentPanel;
	public Transform scrollView;
	public List<GameObject> menuButtons = new List<GameObject>();
	public List<Button> buttonList = new List<Button>();

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
		if (current_item >= 0 && current_item < menuButtons.Count) {
			var pointer = new PointerEventData(EventSystem.current);
			ExecuteEvents.Execute(menuButtons[current_item], pointer, ExecuteEvents.pointerEnterHandler);
		}
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
		FindNextAvailableDown();
	}
	public void ScrollUp() {
		Scroll(-1);
		FindNextAvailableUp();
	}
	void Scroll(int shift) { 
		if (Time.time < allow_scroll_at) {
			return;
		}
		allow_scroll_at = Time.time + scroll_interval;
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
	 
	void FindNextAvailableDown() {
		FindNextAvailable(1);
	}
	void FindNextAvailableUp() {
		FindNextAvailable(-1);
	}
	void FindNextAvailable(int adjust) {
		if (current_item < 0) {
			current_item = 0;
		} else if (current_item >= menuButtons.Count) {
			current_item = menuButtons.Count - 1;
		}

		Button b = buttonList[current_item];
		int tempCur = current_item;
		while (!b.interactable && tempCur >= 0 && tempCur < menuButtons.Count) {
			tempCur += adjust;
			if (tempCur < menuButtons.Count && tempCur >= 0) {
				b = buttonList[tempCur];
			}
		}

		if (tempCur != current_item && (tempCur < menuButtons.Count && tempCur >= 0)) {
			current_item = tempCur;
			// Remove the pointer event on the previous item
			var pointer = new PointerEventData(EventSystem.current);
			ExecuteEvents.Execute(menuButtons[current_item], pointer, ExecuteEvents.pointerExitHandler);

			// Scroll to next item
			target_point = new Vector3(contentPanel.localPosition.x, content_y_offset + current_item * button_size, contentPanel.localPosition.z);
			lerpV = true;
		}
	}

	public ShopItem GetCurrentItem() {
		return shoplist.items[current_item].GetComponent<ShopItem>();
	}

	public ShopItem MakePurchase(PlayerController pc, int team_id) {
		ResourceCount team_count = manager.GetTeamResourceInfo(team_id);
		ShopItem item = shoplist.items[current_item].GetComponent<ShopItem>();
		if (CanPurchase(pc, team_id, item, team_count)) {
			manager.RemoveResources(team_id, ResourceType.stone, item.stone_cost);
			manager.RemoveResources(team_id, ResourceType.wood, item.wood_cost);
			item.MakePurchase(pc, team_id, manager);
			manager.UpdateTeamShops(team_id);
			UpdateShop(pc, team_id);
			FindNextAvailableUp();
			return item;
		} 
		return null;
	}
	
	bool CanPurchase(PlayerController pc, int teamId, ShopItem item, ResourceCount resource_count) {
		return item.wood_cost <= resource_count.wood && item.stone_cost <= resource_count.stone &&
			   item.CanPurchase(pc, teamId, manager);
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
			buttonList.Add(newButton.GetComponent<Button>());
		}
	}

	public void OpenShop(PlayerController pc, int team_id) {
		UpdateShop(pc, team_id);
	}

	public void UpdateShop(PlayerController pc, int team_id) {
		ResourceCount team_count = manager.GetTeamResourceInfo(team_id);
		for (int i = 0; i < shoplist.items.Count; i++) {
			GameObject go = shoplist.items[i];
			ShopItem item = go.GetComponent<ShopItem>();
			
			Button b = buttonList[i];
			b.interactable = false;
			if (CanPurchase(pc, team_id, item, team_count)) {
				b.interactable = true;
			}
		}
	}
}
