﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using InControl;

public class MultiValueDictionary<Key, Value> : Dictionary<Key, List<Value>> {
	public void Add(Key key, Value value) {
		List<Value> values;
		if (!this.TryGetValue(key, out values)) {
			values = new List<Value>();
			this.Add(key, values);
		}
		values.Add(value);
	}
}

public enum CatapultPart {
	arm,
	legs,
	stone
}

public class CatapultTracker {
	public bool has_arm = false;
	public bool has_projectile = false;
	public bool has_legs = false;
}

public class GameManager : MonoBehaviour {
	public const bool USE_SCATTER = false;
	
	private Dictionary<string, string> teamNames = new Dictionary<string, string>() {
		{ "Player 1 Base", "Team 1" },
		{ "Player 2 Base", "Team 2" }
	};


	private const int MAX_LAYER = 31;

	private MultiValueDictionary<int, Text> teamTexts =  new MultiValueDictionary<int, Text>();
	private MultiValueDictionary<int, Text> opponentTexts = new MultiValueDictionary<int,Text>();
	private MultiValueDictionary<int, Text> enemyInBaseTexts = new MultiValueDictionary<int, Text>();
	private Dictionary<int, Material> teamMats = new Dictionary<int, Material>();
	public Dictionary<int, int> teamTrapLayer { get; private set; }

	public Dictionary<int, string> baseNames;
	public Dictionary<int, ResourceCount> teamResources;
	public Dictionary<int, CatapultTracker> teamCatapultStatus = new Dictionary<int, CatapultTracker>(); 
	public int winningWood = 500;
	public int winningStone = 500;


	public GameObject playerBase;
	public GameObject playerCanvasBase;

	public List<Material> mats;
	

	public static int winningTeam = -1;

	void addPlayer(GameObject base1, Rect viewport, int playerNum) {
		Vector3 pos = base1.transform.position;
		pos.x += playerNum % 3;

		GameObject player = Instantiate(playerBase, pos, new Quaternion()) as GameObject;
		var devices = InputManager.Devices;
		if (playerNum - 1 < devices.Count) {
			player.GetComponent<PlayerController>().device = devices[playerNum - 1];
		}

		Canvas canvas = (Instantiate(playerCanvasBase, new Vector3(), new Quaternion()) as GameObject).GetComponent<Canvas>();
		Camera cam = player.GetComponentInChildren<Camera>();
		cam.rect = viewport;
		canvas.worldCamera = player.GetComponentInChildren<Camera>();

		int trapLayer = teamTrapLayer[base1.GetInstanceID()];
		for (int i = MAX_LAYER - teamTrapLayer.Count + 1; i <= MAX_LAYER; i++) {
			if (i == trapLayer)
				continue;
			cam.cullingMask &= ~(1 << i);
		}

		PlayerController controller = player.GetComponent<PlayerController>();
		controller.player_num = playerNum;
		controller.homeBase_GO = base1;
		controller.canvas = canvas;

		teamTexts.Add(base1.GetInstanceID(), canvas.transform.FindChild("Team_Vals").GetComponent<Text>());
		opponentTexts.Add(base1.GetInstanceID(), canvas.transform.FindChild("Opponent_Vals").GetComponent<Text>());

		Text enemyWarning = canvas.transform.FindChild("Enemy_Warning").GetComponent<Text>();
		enemyWarning.enabled = false;
		enemyInBaseTexts.Add(base1.GetInstanceID(), enemyWarning);

		player.renderer.material = teamMats[base1.GetInstanceID()];
	}

	Rect getViewport(int numPlayers, int playerNum) {
		switch (numPlayers) {
		case 2:
			switch (playerNum) {
			case 1:
				return new Rect(0f, 0f, .5f, 1f);
			case 2:
				return new Rect(.5f, 0f, .5f, 1f);
			}
			break;
		case 4:
			switch (playerNum) {
			case 1:
				return new Rect(0f, .5f, .5f, .5f);
			case 2:
				return new Rect(.5f, .5f, .5f, .5f);
			case 3:
				return new Rect(0f, 0f, .5f, .5f);
			case 4:
				return new Rect(.5f, 0f, .5f, .5f);
			}
			break;
		}
		return new Rect(-1f, -1f, -1f, -1f);
	}

	// Use this for initialization
	void Start () {
		teamTrapLayer = new Dictionary<int, int>();

		teamResources = new Dictionary<int, ResourceCount>();

		baseNames = new Dictionary<int, string>();

		int trapLayer = MAX_LAYER;
		int teamNum = 0;
		int numPlayers = InputManager.Devices.Count >= 2 ? 4 : 2;
		foreach (KeyValuePair<string, string> pair in teamNames) {
			GameObject baseObj = GameObject.Find(pair.Key);
			baseNames.Add(baseObj.GetInstanceID(), pair.Value);
			teamResources.Add(baseObj.GetInstanceID(), new ResourceCount());
			teamMats.Add(baseObj.GetInstanceID(), mats[teamNum++]);
			teamCatapultStatus.Add(baseObj.GetInstanceID(), new CatapultTracker());
			teamTrapLayer.Add(baseObj.GetInstanceID(), trapLayer--);
		}

		int playerNum = 1;
		foreach (KeyValuePair<string, string> pair in teamNames) {
			GameObject baseObj = GameObject.Find(pair.Key);
			addPlayer(baseObj, getViewport(numPlayers, playerNum), playerNum);
			playerNum++;
		}

		if (numPlayers == 4) {
			// Add the remaining two players in the event of there are 4 players
			foreach (KeyValuePair<string, string> pair in teamNames) {
				GameObject baseObj = GameObject.Find(pair.Key);
				
				addPlayer(baseObj, getViewport(numPlayers, playerNum), playerNum);
				playerNum++;
			}
		}

		foreach (KeyValuePair<int, string> pair in baseNames) {
			updateTeamText(pair.Key);
		}
		updateAllOppTexts ();
		print ("winning wood: " + winningWood + " stone: " + winningStone);

	}

	void updateTeamText(int baseId) {
		foreach (Text text in teamTexts[baseId]) {
			ResourceCount counts = teamResources[baseId];

			text.text = baseNames[baseId] + " Wood: " + counts.wood + " Stone: " + counts.stone;
		}
	}

	void updateAllOppTexts(){
		foreach (KeyValuePair<int, string> pair in baseNames) {
			updateOppText (pair.Key);
		}
	}

	//this only works if there are 2 teams, but currently that's how the whole UI works as well
	void updateOppText(int baseId){
		foreach (Text text in opponentTexts[baseId]) {
		text.text = "";
			foreach (KeyValuePair<int,string> pair in baseNames) {
				if (baseId != pair.Key){
					ResourceCount counts = teamResources[pair.Key];
					text.text += baseNames[pair.Key] + " Wood: " + counts.wood + " Stone: " + counts.stone + "\n";
				}		
			}
		}
	}


	// Update is called once per frame
	void Update () {
//		foreach(KeyValuePair<int, ResourceCount> team in teamResources){
//			if(team.Value.wood >= winningWood && team.Value.stone >= winningStone){
//				print (team.Key + " team resources: " + team.Value.wood + " winning: " + winningWood);
//				winningTeam = team.Key;
//			}
//		}
		foreach(KeyValuePair<int, CatapultTracker> team in teamCatapultStatus) {
			if (team.Value.has_arm && team.Value.has_legs && team.Value.has_projectile) {
				winningTeam = team.Key;
				break;
			}
		}
		if (Time.timeScale == 0 && winningTeam != -1) {
			if(Input.GetKeyDown(KeyCode.R)){
				Application.LoadLevel(Application.loadedLevel);

			}		
		}

		//Testing purposes only
		if(Input.GetKeyDown(KeyCode.T)){
			foreach(KeyValuePair<int, ResourceCount> kvp in teamResources){
				kvp.Value.wood += 10;
				kvp.Value.stone += 10;
				updateTeamText(kvp.Key);
			}
			updateAllOppTexts();
		}
	}

	public void playerInBase(bool inBase, int baseId) {
		foreach (Text text in enemyInBaseTexts[baseId]) {
			text.enabled = inBase;
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
		updateAllOppTexts ();

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
			updateAllOppTexts();
		}
	}

	static Quaternion RandomQuaternion() {
		return new Quaternion(Random.Range(0f,360f),
		                      Random.Range(0f,360f),
		                      Random.Range(0f,360f),
		                      Random.Range(0f,360f));
	}
	public static void SpawnScatteredObject(GameObject scatteredObj, Vector3 origin) {
		Vector3 spawnSpot = Vector3.zero;
		// Should never push object below map
		spawnSpot = origin + (Vector3.up * Random.Range(2,3)) + (Vector3.right * Random.Range(-2,3)) + (Vector3.forward * Random.Range(-2,3));
		Instantiate(scatteredObj, spawnSpot, RandomQuaternion());
	}

	public ResourceCount GetTeamResourceInfo(int team_id) {
		return teamResources[team_id];
	}

	public void AddCatapultPart(int team_id, CatapultPart part) {
		switch (part) {
		case CatapultPart.arm:
			teamCatapultStatus[team_id].has_arm = true;

			break;
		case CatapultPart.legs:
			teamCatapultStatus[team_id].has_legs = true;
			break;
		case CatapultPart.stone:
			teamCatapultStatus[team_id].has_projectile = true;
			break;
		}
	} 
}
