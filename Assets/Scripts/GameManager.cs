using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	//baseID + resourceCount
	public Dictionary<int, ResourceCount> teamResources;
	public int winningWood = 500;
	public int winningStone = 500;

	public static int winningTeam = -1;


	// Use this for initialization
	void Start () {
		teamResources = new Dictionary<int, ResourceCount>();
		GameObject[] bases = GameObject.FindGameObjectsWithTag("Base");
		foreach(GameObject b in bases){

			teamResources.Add(b.GetInstanceID(), new ResourceCount());
		}
		print ("winning wood: " + winningWood + " stone: " + winningStone);
	
	}
	
	// Update is called once per frame
	void Update () {
		foreach(KeyValuePair<int, ResourceCount> team in teamResources){
			if(team.Value.wood >= winningWood && team.Value.stone >= winningStone){
				print (team.Key + " team resources: " + team.Value.wood + " winning: " + winningWood);
				winningTeam = team.Key;
			}
		}
		if (Time.timeScale == 0 && winningTeam != -1) {
			if(Input.GetKeyDown(KeyCode.R)){
				Application.LoadLevel(Application.loadedLevel);

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
