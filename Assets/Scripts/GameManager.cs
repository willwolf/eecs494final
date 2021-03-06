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
	stone,
	weight
}

public class CatapultTracker {
	public bool has_arm = false;
	public bool has_projectile = false;
	public bool has_weight = false;
	public bool has_legs = false;
}

public class GameManager : MonoBehaviour {
	public float CATAPULT_ALERT_TIME = 2f;
	public const bool USE_SCATTER = true;
	public const bool FORCE_CATAPULT_ORDER = true;
	public const bool ENCUMBER_WINNERS = true;
	public static int MAX_WOOD_PER_TREE = 10;
	public static int MAX_STONE_PER_ROCK = 10;
	public bool TUTORIAL = false;
	public static bool PLAYER_VELOCITY = false;
	
	private Dictionary<string, string> teamNames = new Dictionary<string, string>() {
		{ "Player 1 Base", "Team 1" },
		{ "Player 2 Base", "Team 2" }
	};
	private GameObject centerPoint;


	private const int MAX_LAYER = 31;

	private MultiValueDictionary<int, Text> teamTexts =  new MultiValueDictionary<int, Text>();
	private MultiValueDictionary<int, Text> opponentTexts = new MultiValueDictionary<int,Text>();
	private MultiValueDictionary<int, Text> enemyInBaseTexts = new MultiValueDictionary<int, Text>();
	public List<PlayerController> allPlayers = new List<PlayerController>();
  	public Dictionary<int, Vector3> respawnPoints = new Dictionary<int, Vector3>();
	private Dictionary<int, Material> teamMats = new Dictionary<int, Material>();
	public Dictionary<int, int> teamTrapLayer { get; private set; }
	public Dictionary<int, List<ShopMenu>> playerShops = new Dictionary<int, List<ShopMenu>>();

	public Dictionary<int, Base> teamBases = new Dictionary<int, Base>();
	public Dictionary<int, string> baseNames;
	public Dictionary<int, ResourceCount> teamResources;
	public Dictionary<int, CatapultTracker> teamCatapultStatus = new Dictionary<int, CatapultTracker>(); 
	
	public int INITIAL_FREEZE_DUR = 5;


	public GameObject playerBase;
	public GameObject playerCanvasBase;

	public List<Material> mats;
	

	public static int winningTeam = -1;

	public Quaternion LookAtCenter(Transform currentPos) {
		currentPos.LookAt(centerPoint.transform.position);
		Quaternion q = currentPos.rotation;
		q.x = q.z = 0;
		return q;
	} 

	void addPlayer(GameObject base1, Rect viewport, int playerNum, bool isLumberJack) {
		Vector3 pos = base1.transform.position;
		pos.x += playerNum % 3;

		GameObject player = Instantiate(playerBase, pos, new Quaternion()) as GameObject;

		// Force the player to look at the center
		player.transform.LookAt(centerPoint.transform.position);
		player.transform.rotation = LookAtCenter(player.transform);
		if (isLumberJack) {
			player.transform.FindChild("MinerHatObj").FindChild("MinerHat").gameObject.SetActive(false);
			player.transform.FindChild("MinerHatObj").FindChild("Beanie").gameObject.SetActive(true);
		} else {
			player.transform.FindChild("MinerHatObj").FindChild("MinerHat").gameObject.SetActive(true);
			player.transform.FindChild("MinerHatObj").FindChild("Beanie").gameObject.SetActive(false);
		}
    	respawnPoints.Add(playerNum, pos);

		var devices = InputManager.Devices;
		if (playerNum - 1 < devices.Count) {
			player.GetComponent<PlayerController>().device = devices[playerNum - 1];
		}

		Canvas canvas = (Instantiate(playerCanvasBase, new Vector3(), new Quaternion()) as GameObject).GetComponent<Canvas>();
		// Enable canvas scaler after instantiate to get buttons to scale
		canvas.GetComponent<CanvasScaler>().enabled = true;
		Camera cam = player.GetComponentInChildren<Camera>();
		cam.rect = viewport;
		canvas.worldCamera = player.GetComponentInChildren<Camera>();
		playerShops[base1.GetInstanceID()].Add(canvas.GetComponentInChildren<ShopMenu>());

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
		Slider health_slider = canvas.GetComponentInChildren<Slider>();
		health_slider.maxValue = health_slider.value = controller.startingHealth;
		if(!TUTORIAL){
			canvas.transform.FindChild("Tutorial").gameObject.SetActive(false);
			canvas.transform.FindChild("Tutorial_Text").gameObject.SetActive(false);
		}

		Text teamText = canvas.transform.FindChild ("Team_Vals").GetComponent<Text> ();
		teamText.color = teamMats [base1.GetInstanceID ()].color;
		teamTexts.Add(base1.GetInstanceID(), teamText);




		opponentTexts.Add(base1.GetInstanceID(), canvas.transform.FindChild("Opponent_Vals").GetComponent<Text>());
		allPlayers.Add (controller);

		Text enemyWarning = canvas.transform.FindChild("Enemy_Warning").GetComponent<Text>();
		enemyWarning.enabled = false;
		enemyInBaseTexts.Add(base1.GetInstanceID(), enemyWarning);

		player.renderer.material = teamMats[base1.GetInstanceID()];
    player.transform.FindChild("Chest").gameObject.renderer.material = teamMats[base1.GetInstanceID()];

    
		controller.freeze(INITIAL_FREEZE_DUR, false, true, "GO!");
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
		centerPoint = GameObject.Find("CenterPoint");
		teamTrapLayer = new Dictionary<int, int>();
		winningTeam = -1;

		teamResources = new Dictionary<int, ResourceCount>();

		baseNames = new Dictionary<int, string>();

		int trapLayer = MAX_LAYER;
		int teamNum = 0;
		int numPlayers = InputManager.Devices.Count >= 2 ? 4 : 2;
		foreach (KeyValuePair<string, string> pair in teamNames) {
			GameObject baseObj = GameObject.Find(pair.Key);
			teamBases.Add (baseObj.GetInstanceID(), baseObj.GetComponent<Base>());
			playerShops.Add(baseObj.GetInstanceID(), new List<ShopMenu>());
			baseNames.Add(baseObj.GetInstanceID(), pair.Value);
			teamResources.Add(baseObj.GetInstanceID(), new ResourceCount());
			teamMats.Add(baseObj.GetInstanceID(), mats[teamNum++]);
			teamCatapultStatus.Add(baseObj.GetInstanceID(), new CatapultTracker());
			teamTrapLayer.Add(baseObj.GetInstanceID(), trapLayer--);
		}

		int playerNum = 1;
		while(playerNum < numPlayers) {
			foreach (KeyValuePair<string, string> pair in teamNames) {
				GameObject baseObj = GameObject.Find(pair.Key);
				addPlayer(baseObj, getViewport(numPlayers, playerNum), playerNum, pair.Value == "Team 1");
				playerNum++;
			}
		}

		foreach (KeyValuePair<int, string> pair in baseNames) {
			updateTeamText(pair.Key);
		}
		updateAllOppTexts ();
		//updateAllOppCataIcons ();
		UpdateAllCatapultIcons();

	}

	void updateTeamText(int baseId) {
		foreach (Text text in teamTexts[baseId]) {
			ResourceCount counts = teamResources[baseId];

			text.text = "Wood: " + counts.wood + 
				" Stone: " + counts.stone;// + catapultText(baseId);
		}
	}

	void updateAllOppTexts(){
		foreach (KeyValuePair<int, string> pair in baseNames) {
			updateOppText (pair.Key);
		}
	}

	void UpdateAllCatapultIcons() {
		foreach (KeyValuePair<int, string> pair in baseNames) {
			UpdateTeamCatapultIcons (pair.Key);
			updateOppCataIcons(pair.Key);
		}
	}

	void UpdateTeamCatapultIcons(int teamId) {
		foreach (PlayerController p in allPlayers) {
			if (p.homeBase_GO.GetInstanceID() == teamId) {
				if (teamCatapultStatus [teamId].has_arm) {
					p.cataArm.enabled = true;
				}
				if(teamCatapultStatus[teamId].has_legs){
					p.cataBase.enabled = true;
				}
				if(teamCatapultStatus[teamId].has_projectile){
					p.cataStone.enabled = true;
				}
				if (teamCatapultStatus[teamId].has_weight) {
					p.cataWeight.enabled = true;
				}
			}
		}
	}

	void updateAllOppCataIcons(){
		foreach (KeyValuePair<int, string> pair in baseNames) {
				updateOppCataIcons(pair.Key);	
		}
	}

	void updateOppCataIcons(int oppId){
		foreach (PlayerController p in allPlayers){
			if(p.homeBase_GO.GetInstanceID() != oppId){
				print ("oppid: " + oppId + " arm " + teamCatapultStatus [oppId].has_arm +
				       " base " + teamCatapultStatus[oppId].has_legs + " stone " + teamCatapultStatus[oppId].has_projectile);
				if (teamCatapultStatus [oppId].has_arm) {
					p.oppCataArm.enabled = true;
				}
				if(teamCatapultStatus[oppId].has_legs){
					p.oppCataBase.enabled = true;
				}
				if(teamCatapultStatus[oppId].has_projectile){
					p.oppCataStone.enabled = true;
				}
				if (teamCatapultStatus[oppId].has_weight) {
					p.oppCataWeight.enabled = true;
				}
			}
		}
	}

	void updateOppText(int baseId){
		foreach (Text text in opponentTexts[baseId]) {
		text.text = "";
			foreach (KeyValuePair<int,string> pair in baseNames) {
				if (baseId != pair.Key){
					ResourceCount counts = teamResources[pair.Key];
					text.color = teamMats[pair.Key].color;
					text.text += "Wood: " + counts.wood + 
						" Stone: " + counts.stone;// + catapultText(pair.Key) + "\n";
				}		
			}
		}
	}

	public void CatapultPurchaseAlert(int teamId) {
		foreach (PlayerController p in allPlayers) {
			if (p.homeBase_GO.GetInstanceID() == teamId) {
				p.updateMidScreenText("Purchased catapult part!", CATAPULT_ALERT_TIME);
			} else {
				p.updateMidScreenText("Enemy purchased catapult part!", CATAPULT_ALERT_TIME);
			}
		}
	}

	public void UpdateTeamShops(int baseId) {
		foreach (ShopMenu s in playerShops[baseId]) {
			foreach (PlayerController p in allPlayers) {
				if (p.homeBase_GO.GetInstanceID() == baseId) {
					s.UpdateShop(p, baseId);
				}
			}
		}
	}

	string catapultText(int baseId){
		string cataText = "";
		if(teamCatapultStatus [baseId].has_arm ||
			teamCatapultStatus[baseId].has_legs ||
		teamCatapultStatus[baseId].has_projectile){
			cataText = " Obtained Catapult Parts: ";
			if (teamCatapultStatus [baseId].has_arm) {
				cataText += "Arm ";
			}
			if(teamCatapultStatus[baseId].has_legs){
				cataText += "Base ";
			}
			if(teamCatapultStatus[baseId].has_projectile){
				cataText += "Stone";
			}
		}
		return cataText;
	}

	// Update is called once per frame
	void Update () {
		foreach(KeyValuePair<int, CatapultTracker> team in teamCatapultStatus) {
			if (team.Value.has_arm && team.Value.has_legs && team.Value.has_projectile) {
				winningTeam = team.Key;
				if(!teamBases[team.Key].catapult.stone.GetComponent<CatapultStoneScript>().fired){
					teamBases[team.Key].catapult.stone.GetComponent<CatapultStoneScript>().Fire();
				}
				break;
			}
		}
		if (winningTeam != -1) {
			if(Input.GetKeyDown(KeyCode.R)){
				Application.LoadLevel(Application.loadedLevel);
			} else if (Input.GetKeyDown(KeyCode.Q)){
				Application.LoadLevel(0);
			}		
		}

		//Testing purposes only
		if(Input.GetKeyDown(KeyCode.T)){
			foreach(KeyValuePair<int, ResourceCount> kvp in teamResources){
				AddResources(kvp.Key, ResourceType.stone, 10);
				AddResources(kvp.Key, ResourceType.wood, 10);
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
		updateAllOppTexts ();
		UpdateTeamShops(baseId);

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
			UpdateTeamShops(baseId);
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
		try{
			switch (part) {
			case CatapultPart.arm:
				teamCatapultStatus[team_id].has_arm = true;
				teamBases[team_id].TurnOnCatapultArm();
				break;
			case CatapultPart.legs:
				teamCatapultStatus[team_id].has_legs = true;
				teamBases[team_id].TurnOnCatapultLegs();
				break;
			case CatapultPart.stone:
				teamCatapultStatus[team_id].has_projectile = true;
				teamBases[team_id].TurnOnCatapultStone();
				break;
			case CatapultPart.weight:
				teamCatapultStatus[team_id].has_weight = true;
				teamBases[team_id].TurnOnCatapultWeight();
				break;
			}
		} finally {
			updateTeamText(team_id);
			CatapultPurchaseAlert(team_id);
			updateAllOppTexts();
			UpdateAllCatapultIcons();
		}
	} 
}
