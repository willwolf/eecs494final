using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using InControl;

public class PlayerController : MonoBehaviour {

	public int RESPAWN_TIME = 20;
	private bool dead = false;
	private float respawn_at_time;
	
	public int player_num  = 0;
	private InputDevice device = null;
	public float rotate_speed = 90f;
	public float walk_speed = 8f;
	public float enemy_base_speed_multiplier = 0.5f;

	public int curr_wood_resource = 0;
	public int wood_gather_val = 1;
	public static int MAX_WOOD_PER_TREE = 10;
	public int curr_stone_resource = 0;
	public int stone_gather_val = 1;
	public int MAX_RESOURCES = 30;
	public static int MAX_STONE_PER_ROCK = 10;
	public bool backpackFull = false;

	public float WOOD_COOLDOWN_TIME = 0.5f;
	private bool collected_wood = false;
	private float get_wood_at_time;

	public float STONE_COOLDOWN_TIME = 0.5f;
	private bool collected_stone = false;
	private float get_stone_at_time;

	public int MIDTEXT_COOLDOWN_TIME = 5;
	private bool showing = false;
	private float zero_at_time;

	public GameObject homeBase_GO;
	public Base homeBase { get; private set; }
	public bool inBase = false;
	public bool inEnemyBase = false;
	public bool hasSword = false;
	
	private Text stone_text;
	private Text wood_text;
	private Text mid_screen_text;

	public AudioSource mining_stone;
	public AudioSource chopping_wood;
	public AudioSource dropping_resources;
	public GameObject sword;

	// Use this for initialization
	void Start () {
		Time.timeScale = 1;
		if (player_num == 0) {
			throw new UnassignedReferenceException("PlayerController::playerNum must be non-zero");
		}
		wood_text = GameObject.Find ("Wood_Text_" + player_num.ToString()).GetComponent<Text>();
		stone_text = GameObject.Find("Stone_Text_" + player_num.ToString()).GetComponent<Text>();
		mid_screen_text = GameObject.Find("mid_screen_text_" + player_num.ToString()).GetComponent<Text>();

		mid_screen_text.text = "";
		updateStoneText();
		updateWoodText();

		homeBase = homeBase_GO.GetComponent<Base>();

		var devices = InputManager.Devices;
		if (devices == null) {
			return;
		}
		foreach (InputDevice d in devices) {
			if (d.Meta.Contains(player_num.ToString())) {
				device = d;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(!hasSword){
			foreach(Renderer child in sword.GetComponentsInChildren<Renderer>()){
				child.enabled = false;
			}

		} else {
			foreach(Renderer child in sword.GetComponentsInChildren<Renderer>()){
				child.enabled = true;
			}
		}

		if (player_num == 0) {
			throw new UnassignedReferenceException("PlayerController::playerNum must be non-zero");
		}
		if (hasWon (GameManager.winningTeam)) {
			updateMidScreenText("You won!\nPress 'R' to Replay");
			Time.timeScale = 0;
		}

		if (dead) {
			if (Time.time > respawn_at_time) {
				mid_screen_text.text = "";
				awakePlayer();
			} else {
				mid_screen_text.text = "Respawn in " + Mathf.Floor(respawn_at_time - Time.time).ToString("0") + " seconds";
				return;
			}
		}

		if (showing) {
			if (Time.time > zero_at_time) {
				mid_screen_text.text = "";
				
				showing = false;
			} 
		}

		if (collected_wood && (Time.time > get_wood_at_time)) {
			collected_wood = false;
		}

		if (collected_stone && (Time.time > get_stone_at_time)) {
			collected_stone = false;
		}

		if (curr_wood_resource + curr_stone_resource >= MAX_RESOURCES) {
			updateMidScreenText("Backpack Full");
		}

		Move();

		if (Input.GetButton("Action_" + player_num.ToString()) || 
		    (device != null && device.Action1.IsPressed)) {
			TakeAction();
		}
	}

	void Move() {
		float rotate_input = 0,
			  forward_input = 0,
			  sidestep_input = 0;
		if (device != null) {
			// Default to using controller inputs, if they are present otherwise use keyboard commands
			rotate_input = device.RightStickX ? device.RightStickX : Input.GetAxis("Horizontal_" + player_num.ToString());
			sidestep_input = device.LeftStickX;
			forward_input = device.LeftStickY ? device.LeftStickY : Input.GetAxis("Vertical_" + player_num.ToString());
		} else {
			rotate_input = Input.GetAxis("Horizontal_" + player_num.ToString());
			forward_input = Input.GetAxis("Vertical_" + player_num.ToString());
		}
		if(inEnemyBase){
			rotate_input *= enemy_base_speed_multiplier;
			forward_input *= enemy_base_speed_multiplier;
			sidestep_input *= enemy_base_speed_multiplier;
		}
		
		transform.Rotate(Vector3.up, rotate_speed * Time.deltaTime * rotate_input);
		transform.localPosition += ((transform.forward * walk_speed * forward_input * Time.deltaTime) +
		                            (transform.right * walk_speed * sidestep_input * Time.deltaTime));
	}

	public void awakePlayer() {
		foreach (Collider collider in GetComponentsInChildren<Collider>()) {
			collider.enabled = true;
		}
		foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>()) {
			renderer.enabled = true;
		}
		foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>()) {
			rb.isKinematic = false;
		}
	}

	public void killPlayer(GameObject enemy_base_GO) {
		foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>()) {
			renderer.enabled = false;
		}
		foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>()) {
			rb.isKinematic = true;
		}
		foreach (Collider collider in GetComponentsInChildren<Collider>()) {
			collider.enabled = false;
		}

		// Drop all resources in enemy's base upon death
		GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		gm.AddResources(enemy_base_GO.GetInstanceID(), ResourceType.stone, curr_stone_resource);
		gm.AddResources(enemy_base_GO.GetInstanceID(), ResourceType.wood, curr_wood_resource);


		curr_wood_resource = curr_stone_resource = 0;
		this.transform.position = homeBase_GO.transform.position;

		dead = true;
		inEnemyBase = false;
		respawn_at_time = Time.time + RESPAWN_TIME;
	}

	void TakeAction() {
		RaycastHit hitinfo;
		if (IsInRange(out hitinfo, "Player")) {
			PlayerController other = hitinfo.transform.GetComponent<PlayerController>();

			if (other.homeBase_GO.GetInstanceID() != this.gameObject.GetInstanceID()) {
				Debug.Log("In range of enemy player");
				if (this.inBase) {
					other.killPlayer(homeBase_GO); 
				}
			} else {
				Debug.Log("In range of friendly plaer");
			}

		} else if (IsInRange(out hitinfo, "Resource")) {
			Resource r = hitinfo.transform.GetComponent<Resource>();
			if (r == null) {
				throw new UnassignedReferenceException("Resource layer object does not have Resource script attached");
			}

			switch (r.type) {
			case ResourceType.stone:
				MineStone(hitinfo.transform.gameObject);
				break;
			case ResourceType.wood:
				ChopWood(hitinfo.transform.gameObject);
				break;
			
			}

		} else if (IsInRange(out hitinfo, "DropPoint")) {
			DropPoint drop = hitinfo.transform.GetComponent<DropPoint>();
			if (drop == null) {
				throw new UnassignedReferenceException("DropPoint layer object does not have a DropPoint script attached");
			}

			if (drop.playerBaseGO.GetInstanceID() == homeBase_GO.GetInstanceID()) {
				switch (drop.resourceType) {
				case ResourceType.stone:
					if(curr_stone_resource > 0)dropping_resources.Play();
					drop.DepositResources(curr_stone_resource);
					curr_stone_resource = 0;
					updateStoneText();
					backpackFull = false;
					break;
				case ResourceType.wood:
					if(curr_wood_resource > 0)dropping_resources.Play();
					drop.DepositResources(curr_wood_resource);
					curr_wood_resource = 0;
					updateWoodText();
					backpackFull = false;
					break;
				}
			} else {
				switch (drop.resourceType) {
				case ResourceType.stone:
					if (!collected_stone && !backpackFull) {
						CollectStone(drop.StealResources(stone_gather_val), " is stealing stone...");
					}
					break;
				case ResourceType.wood:
					if (!collected_wood && !backpackFull) {
						CollectWood(drop.StealResources(wood_gather_val), " is stealing wood...");
					}
					break;
				}
			}
		}
	}

	private void updateWoodText() {
		wood_text.text = "Carrying " + curr_wood_resource + " wood";
	}

	void ChopWood(GameObject wood) {
		if(!collected_wood && !backpackFull){
			CollectWood(wood_gather_val, " is chopping wood...");
			if(chopping_wood) chopping_wood.Play();
			decreaseResource(wood);
		}
		updateWoodText();
	}

	void CollectWood(int amount, string message) {
		string chopNotification = "Player " + player_num.ToString() + " is chopping wood!";
		updateMidScreenText(chopNotification);
		CheckMaxWood(amount);
		if (wood_text == null) {
			throw new UnassignedReferenceException("wood_text for player " + player_num.ToString() + " is null");
		}
		wood_text.text = "Carrying " + curr_wood_resource + " wood";
		updateMidScreenText("Player " + player_num.ToString() + message);
		collected_wood = true;
		get_wood_at_time = Time.time + WOOD_COOLDOWN_TIME;
	}

	void CheckMaxWood(int amount) {
		if (curr_wood_resource + curr_stone_resource + amount > MAX_RESOURCES) {
			string maxWood = "Player " + player_num.ToString () + " has max amount of wood!";
			updateMidScreenText(maxWood);
			curr_wood_resource = MAX_RESOURCES - curr_stone_resource;
			backpackFull = true;
		} else {
			curr_wood_resource += amount;
		}
	}
	
	private void updateStoneText() {
		stone_text.text = "Carrying " + curr_stone_resource + " stone";
	}

	void decreaseResource(GameObject resource){
		print ("Amount of " + resource.ToString () + " left: " + 
						resource.GetComponent<Resource> ().amountLeft);
		resource.GetComponent<Resource>().amountLeft--; //dec first to not get off by 1 error
		if(resource.GetComponent<Resource>().amountLeft == 0){
			Destroy(resource);
		}
	}

	void MineStone(GameObject stone) {
		if(!collected_stone && !backpackFull){
			CollectStone(stone_gather_val, " is mining stone...");
			if(mining_stone) mining_stone.Play();
			decreaseResource (stone);
		}

		updateStoneText();
	}

	void CollectStone(int amount, string message) {
		string mineNotification = "Player " + player_num.ToString() + " is mining!";
		updateMidScreenText(mineNotification);
		CheckMaxStone(amount);
		if (stone_text == null) {
			throw new UnassignedReferenceException("stone_text for player " + player_num.ToString() + " is null");
		}
		stone_text.text = "Carrying " + curr_stone_resource + " stone";
		updateMidScreenText("Player " + player_num.ToString() + message);
		collected_stone = true;
		get_stone_at_time = Time.time + STONE_COOLDOWN_TIME;
	}

	void CheckMaxStone(int amount) {
		if (curr_stone_resource + curr_wood_resource + amount > MAX_RESOURCES) {
			string maxStone = "Player " + player_num.ToString () + " has max amount of stone!";
			updateMidScreenText(maxStone);
			curr_stone_resource = MAX_RESOURCES - curr_wood_resource;
			backpackFull = true;
		} else {
			curr_stone_resource += amount;
		}
	}
	
	bool IsInRange(out RaycastHit hitinfo, string Layer) {
		Vector3 halfWidth = transform.right / 2f;
		float halfHeight = transform.lossyScale.y / 2f;
		Vector3 center, leftCenter, rightCenter, footPos, footLeft, footRight;
		
		// Initialize center positions
		center = leftCenter = rightCenter = transform.position;
		leftCenter -= halfWidth;
		rightCenter += halfWidth;
		
		// Initialze bottom positions
		footPos = transform.position;
		footPos.y -= halfHeight;
		footLeft = footRight = footPos;
		footLeft -= halfWidth;
		footRight += halfWidth;
		
		return (CastActionRay(center, Layer, out hitinfo) || CastActionRay(footPos, Layer, out hitinfo) ||
		        CastActionRay(leftCenter, Layer, out hitinfo) || CastActionRay(footLeft, Layer, out hitinfo) ||
		        CastActionRay(rightCenter, Layer, out hitinfo) || CastActionRay(footRight, Layer, out hitinfo));
	}

	bool CastActionRay(Vector3 origin, string Layer, out RaycastHit info) {
		int layerMask = LayerMask.GetMask(Layer); // only collide with Layer specified
		return Physics.Raycast(origin, transform.forward, out info, 1.5f, layerMask);
	}

	private void updateMidScreenText(string newText){
		showing = true;
		zero_at_time = Time.time + MIDTEXT_COOLDOWN_TIME;
	
		mid_screen_text.text = newText;

	}

	public bool hasWon(int baseID){
		return homeBase_GO.GetInstanceID() == baseID;
	}
}
