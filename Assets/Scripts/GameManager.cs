using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
	
	private Dictionary<string, string> teamNames = new Dictionary<string, string>() {
		{ "Player 1 Base", "Team 1" },
		{ "Player 2 Base", "Team 2" }
	};

	private Dictionary<int, Text> teamTexts =  new Dictionary<int, Text>();

	public Dictionary<int, string> baseNames;
	public Dictionary<int, ResourceCount> teamResources;
	public int winningWood = 500;
	public int winningStone = 500;
	

	// Use this for initialization
	void Start () {
		teamResources = new Dictionary<int, ResourceCount>();
		baseNames = new Dictionary<int, string>();

		foreach (KeyValuePair<string, string> pair in teamNames) {
			GameObject baseObj = GameObject.Find(pair.Key);
			baseNames.Add(baseObj.GetInstanceID(), pair.Value);
			teamResources.Add(baseObj.GetInstanceID(), new ResourceCount());
			teamTexts.Add(baseObj.GetInstanceID(), GameObject.Find(pair.Value + "_vals").GetComponent<Text>());
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
				Application.LoadLevel(1);
			}
		}
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
