using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public Dictionary<int, ResourceCount> teamResources;

	// Use this for initialization
	void Start () {
		teamResources = new Dictionary<int, ResourceCount>();
		GameObject[] bases = GameObject.FindGameObjectsWithTag("Base");
		foreach(GameObject b in bases){
			teamResources.Add(b.GetInstanceID(), new ResourceCount());
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void UpdateResources(int baseId, ResourceType t, int amount) {
		switch (t) {
		case ResourceType.stone:
			teamResources[baseId].stone += amount;
			break;
		case ResourceType.wood:
			teamResources[baseId].wood += amount;
			break;
		}
	}
}
