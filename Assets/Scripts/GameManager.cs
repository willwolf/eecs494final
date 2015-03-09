using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	
	private Dictionary<string, string> teamNames = new Dictionary<string, string>() {
		{ "Player 1 Base", "Team 1" },
		{ "Player 2 Base", "Team 2" }
	};

	private Dictionary<int, Text> teamTexts =  new Dictionary<int, Text>();
	private Dictionary<int, Text> enemyInBaseTexts = new Dictionary<int, Text>();

	public Dictionary<int, string> baseNames;
	public Dictionary<int, ResourceCount> teamResources;
	public int winningWood = 500;
	public int winningStone = 500;
	

	public static int winningTeam = -1;


	// Use this for initialization
	void Start () {
		teamResources = new Dictionary<int, ResourceCount>();

		baseNames = new Dictionary<int, string>();

		foreach (KeyValuePair<string, string> pair in teamNames) {
			GameObject baseObj = GameObject.Find(pair.Key);
			baseNames.Add(baseObj.GetInstanceID(), pair.Value);
			teamResources.Add(baseObj.GetInstanceID(), new ResourceCount());
			teamTexts.Add(baseObj.GetInstanceID(), GameObject.Find(pair.Value + "_vals").GetComponent<Text>());
			enemyInBaseTexts.Add(baseObj.GetInstanceID(), GameObject.Find(pair.Value + "_BaseWarning").GetComponent<Text>());
		}
		print ("winning wood: " + winningWood + " stone: " + winningStone);

		foreach (KeyValuePair<int, Text> pair in enemyInBaseTexts) {
			pair.Value.enabled = false;
		}
	
	}

	void updateTeamText(int baseId) {
		Text text = teamTexts[baseId];
		ResourceCount counts = teamResources[baseId];

		text.text = baseNames[baseId] + " Wood: " + counts.wood + " Stone: " + counts.stone;
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

	public void playerInBase(bool inBase, int baseId) {
		enemyInBaseTexts[baseId].enabled = inBase;
	}

	public void AddResources(int baseId, ResourceType t, int amount) {
		switch (t) {
		case ResourceType.stone:
			teamResources[baseId].stone += amount;
			break;
		case ResourceType.wood:
			teamResources[baseId].wood += amount;
			break;
		}
		updateTeamText(baseId);
	}


	public int RemoveResources(int baseId, ResourceType t, int amount) {
		try {
			switch (t) {
			case ResourceType.stone:
				if (teamResources[baseId].stone >= amount) {
					teamResources[baseId].stone -= amount;
					return amount;
				} else {
					int temp = teamResources[baseId].stone;
					teamResources[baseId].stone = 0;
					return temp;
				}
			case ResourceType.wood:
				if (teamResources[baseId].wood >= amount) {
					teamResources[baseId].wood -= amount;
					return amount;
				} else {
					int temp = teamResources[baseId].wood;
					teamResources[baseId].wood = 0;
					return temp;
				}
			}
			return 0;
		} finally {
			updateTeamText(baseId);
		}
	}
}
