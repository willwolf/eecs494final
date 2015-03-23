using UnityEngine;
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

public class GameManager : MonoBehaviour {

	
	private Dictionary<string, string> teamNames = new Dictionary<string, string>() {
		{ "Player 1 Base", "Team 1" },
		{ "Player 2 Base", "Team 2" }
	};

	private MultiValueDictionary<int, Text> teamTexts =  new MultiValueDictionary<int, Text>();
	private MultiValueDictionary<int, Text> enemyInBaseTexts = new MultiValueDictionary<int, Text>();
	private Dictionary<int, Material> teamMats = new Dictionary<int, Material>();

	public Dictionary<int, string> baseNames;
	public Dictionary<int, ResourceCount> teamResources;
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
		Debug.Log(cam.rect);
		canvas.worldCamera = player.GetComponentInChildren<Camera>();

		PlayerController controller = player.GetComponent<PlayerController>();
		controller.player_num = playerNum;
		controller.homeBase_GO = base1;
		controller.canvas = canvas;

		teamTexts.Add(base1.GetInstanceID(), canvas.transform.FindChild("Team_Vals").GetComponent<Text>());

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

		teamResources = new Dictionary<int, ResourceCount>();

		baseNames = new Dictionary<int, string>();

		int teamNum = 0;
		int playerNum = 1;
		int numPlayers = InputManager.Devices.Count >= 2 ? 4 : 2;
		foreach (KeyValuePair<string, string> pair in teamNames) {
			GameObject baseObj = GameObject.Find(pair.Key);
			baseNames.Add(baseObj.GetInstanceID(), pair.Value);
			teamResources.Add(baseObj.GetInstanceID(), new ResourceCount());
			teamMats.Add(baseObj.GetInstanceID(), mats[teamNum++]);

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
		print ("winning wood: " + winningWood + " stone: " + winningStone);

	}

	void updateTeamText(int baseId) {
		foreach (Text text in teamTexts[baseId]) {
			ResourceCount counts = teamResources[baseId];

			text.text = baseNames[baseId] + " Wood: " + counts.wood + " Stone: " + counts.stone;
		}
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

		//Testing purposes only
		if(Input.GetKeyDown(KeyCode.T)){
			foreach(KeyValuePair<int, ResourceCount> kvp in teamResources){
				kvp.Value.wood += 10;
				kvp.Value.stone += 10;
				updateTeamText(kvp.Key);
			}
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

	public ResourceCount GetTeamResourceInfo(int team_id) {
		return teamResources[team_id];
	}
}
