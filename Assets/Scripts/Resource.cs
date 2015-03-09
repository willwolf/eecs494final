using UnityEngine;
using System.Collections;

public enum ResourceType {
	none,
	wood,
	stone
}

public class Resource : MonoBehaviour {
	
	public ResourceType type = ResourceType.none;
	public int amountLeft = 0;

	// Use this for initialization
	void Awake () {
		if (type == ResourceType.wood) {
			amountLeft = PlayerController.MAX_WOOD_PER_TREE;	
		}
		else if(type == ResourceType.stone){
			amountLeft = PlayerController.MAX_STONE_PER_ROCK;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
