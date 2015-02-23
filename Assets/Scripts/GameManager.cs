using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public Dictionary<int, ResourceCount> teamResources;
	public int winningWood = 500;
	public int winningStone = 500;

	// Use this for initialization
	void Start () {
		GameObject[] bases = GameObject.FindGameObjectsWithTag("Base");
		foreach(GameObject b in bases){
			teamResources.Add(b.GetInstanceID(), new ResourceCount());
		}
	}
	
	// Update is called once per frame
	void Update () {
		foreach(KeyValuePair<int, ResourceCount> team in teamResources){
			if(team.Value.wood >= winningWood && team.Value.stone >= winningStone){
				Application.LoadLevel(1);
			}
		}
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
