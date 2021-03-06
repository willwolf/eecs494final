﻿using UnityEngine;
using System.Collections;

public enum ResourceType {
	none,
	wood,
	stone
}

public class Resource : MonoBehaviour {
	
	public ResourceType type = ResourceType.none;
	public GameObject scatterObject;
	public int amountLeft = 0;

	// Use this for initialization
	void Awake () {
		if (type == ResourceType.wood) {
			amountLeft = GameManager.MAX_WOOD_PER_TREE;	
		}
		else if(type == ResourceType.stone){
			amountLeft = GameManager.MAX_STONE_PER_ROCK;
		}
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void Gather() {
		amountLeft--;
		if (GameManager.USE_SCATTER) {
			if (scatterObject) {
				GameManager.SpawnScatteredObject(scatterObject, transform.position);
			} else {
				print ("Resource has not scatterObject assigned!");
			}
		}
		if (amountLeft == 0) {
			Destroy(this.gameObject);
		}
	}
}
