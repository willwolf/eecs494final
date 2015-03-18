using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using InControl;

public class PlayerController : MonoBehaviour {

	public int RESPAWN_TIME = 20;
	private bool dead = false;
	private float respawn_at_time;
	public int startingHealth = 10;
	private int health;
	public int damage_amount = 2;
	public float INVULNERABLE_TIME = 2;
	private float vulnerable_at_time;
	public float HEALTH_REGEN_TIME = 5;
	private float regen_at_time;
	public float FIRE_RATE_TIME = 0.5f;
	private float next_fire_at_time;
	
	public int player_num  = 0;
	public InputDevice device = null;
	public float controller_sensitivity = 0.5f;
	public float jump_height = 5f;
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

	public float WOOD_COOLDOWN_TIME = 1.0f;
	private bool collected_wood = false;
	private float get_wood_at_time;

	public float STONE_COOLDOWN_TIME = 1.0f;
	private bool collected_stone = false;
	private float get_stone_at_time;

	public float STEAL_COOLDOWN_TIME = .5f;
	private bool stole_resorces = false;
	private float steal_at_time;

	public int MIDTEXT_COOLDOWN_TIME = 5;
	private bool showing = false;
	private float zero_at_time;

	public GameObject homeBase_GO;
	public Base homeBase { get; private set; }
	public bool inBase = false;
	public bool inEnemyBase = false;
	public int EnemyBaseId;

	public bool hasStealth = false;
	public bool stealthActive = false;
	private double stealthAmount = 1;

	
	private Text stone_text;
	private Text wood_text;
	private Text mid_screen_text;
	private Slider health_slider;

	public AudioSource mining_stone;
	public AudioSource chopping_wood;
	public AudioSource dropping_resources;
	public AudioSource stealing_resources;
	public AudioSource swinging_sword;

//	public GameObject sword;
	public bool hasWeapon = false;
	public int currentWeaponIndex = 0;
	public List<GameObject> weapons = new List<GameObject>();
	public WeaponItem currentWeapon = null;

	public GameManager gm;
	public Canvas canvas;

	public GameObject shop;
	public ShopMenu shopMenu;
	public bool shopOpen;
	public GameObject arrow;

	// Use this for initialization
	void Start () {
		Time.timeScale = 1;
		if (player_num == 0) {
			throw new UnassignedReferenceException("PlayerController::playerNum must be non-zero");
		}
		wood_text = canvas.transform.FindChild("Wood_Text").GetComponent<Text>();
		stone_text = canvas.transform.FindChild("Stone_Text").GetComponent<Text>();
		mid_screen_text = canvas.transform.FindChild("mid_screen_text").GetComponent<Text>();
		health_slider = canvas.transform.FindChild("Slider").GetComponent<Slider>();

		mid_screen_text.text = "";
		updateStoneText();
		updateWoodText();

		homeBase = homeBase_GO.GetComponent<Base>();
		shop = canvas.transform.FindChild ("Shop_Menu").gameObject;
		shopMenu = shop.GetComponent<ShopMenu>();

		shopOpen = false;
		shop.SetActive(false);

		health = startingHealth;
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();

		foreach (GameObject weapon in weapons) {
			weapon.SetActive(false);
		}

		if (device != null) {
			device.RightStickX.LowerDeadZone = controller_sensitivity;
			device.RightStickY.LowerDeadZone = controller_sensitivity;
			device.LeftStickX.LowerDeadZone = controller_sensitivity;
			device.LeftStickY.LowerDeadZone = controller_sensitivity;
		}
	}
	
	// Update is called once per frame
	void Update () {

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
		} else if(inBase) {
			if(Time.time > regen_at_time){
				health += 1;
				if(health > startingHealth) {
					health = startingHealth;
				}
				health_slider.value = health;
				regen_at_time = Time.time + HEALTH_REGEN_TIME;
			}
		} else {
			regen_at_time = Time.time + HEALTH_REGEN_TIME;
		}

		if (showing) {
			if (Time.time > zero_at_time) {
				mid_screen_text.text = "";
				
				showing = false;
			} 
		}

		//update timers
		if (collected_wood && (Time.time > get_wood_at_time)) {
			collected_wood = false;
		}

		if (collected_stone && (Time.time > get_stone_at_time)) {
			collected_stone = false;
		}

		if (stole_resorces && (Time.time > steal_at_time)) {
			stole_resorces = false;
		}

		if (curr_wood_resource + curr_stone_resource >= MAX_RESOURCES) {
			updateMidScreenText("Backpack Full");
		}



		if (!shopOpen) {
			Move();
		} else {
			CheckShopInputs();
		}

		foreach (Renderer renderer in this.GetComponentsInChildren<Renderer>()) {
			Color col = renderer.material.color;
			if (stealthActive) {
				col.a = .1f;
			} else {
				col.a = 1f;
			}
			renderer.material.color = col;
		}

		if (device != null) {
			if (device.Action1.IsPressed) {
				TakeAction();
			} if(device.RightTrigger.IsPressed && !shopOpen){
				Attack();
			}
		} else if (Input.GetButton("Action_" + Mathf.Ceil(player_num % 2.0f).ToString())) {
			if(!shopOpen){
				Attack();
			}
			TakeAction();
		}
		if (device != null) {
			if (device.DPadUp.WasPressed && inBase) {
				ToggleStore();
			}
		} else if (Input.GetButtonDown("Store_Open_" + Mathf.Ceil(player_num % 2.0f).ToString()) && inBase) {
			ToggleStore();
		}
	}

	void LateUpdate(){

	}

	void Move() {
		float rotate_input = 0,
			  forward_input = 0,
			  sidestep_input = 0,
			  jump_input = 0;
		if (device != null) {
			// Default to using controller inputs, if they are present otherwise use keyboard commands
			rotate_input = device.RightStickX;
			sidestep_input = device.LeftStickX;
			forward_input = device.LeftStickY;
			if (device.Action4.WasPressed) {
				// Check for being on the ground when the button is pressed
				if (Physics.Raycast(transform.position, Vector3.down, transform.collider.bounds.extents.y + 0.1f)) {
					jump_input = 1;
				}
			}
		} else {
			rotate_input = Input.GetAxis("Horizontal_" + Mathf.Ceil(player_num % 2.0f).ToString());
			forward_input = Input.GetAxis("Vertical_" + Mathf.Ceil(player_num % 2.0f).ToString());
		}
		
		transform.Rotate(Vector3.up, rotate_speed * Time.deltaTime * rotate_input);
		transform.localPosition += (CalculateMoveSpeed(transform.forward, forward_input) +
		                            CalculateMoveSpeed(transform.right, sidestep_input));
		Vector3 newVel = transform.rigidbody.velocity;
		newVel.y += jump_input * jump_height;
		transform.rigidbody.velocity = newVel;
	}

	Vector3 CalculateMoveSpeed(Vector3 direction, float input_data) {
		Vector3 moveSpeed = direction * walk_speed * input_data * Time.deltaTime;
		if (!inEnemyBase) {
			float encumbered = 1;
			// max encumberance == 1 - enemy_base_speed_multiplier
			encumbered -= Mathf.Min(((1.0f * curr_wood_resource + curr_stone_resource) / MAX_RESOURCES), 
			                        enemy_base_speed_multiplier);
			return moveSpeed * encumbered;
		} else {
			return moveSpeed * enemy_base_speed_multiplier;
		}
	}

	void CheckShopInputs() {
		if (device != null) {
			if (device.LeftStickY < 0) {
				shopMenu.ScrollDown();
			} else if (device.LeftStickY > 0) {
				shopMenu.ScrollUp();
			}
			if (device.Action1.WasPressed) {
				ShopItem item = shopMenu.GetCurrentItem();
				if ((item is SwordScript && currentWeapon is SwordScript) || 
				    (item is BowScript && currentWeapon is BowScript) || 
				    (item is WallScript && homeBase.HasWalls())) {
					return;
				}
				item = shopMenu.MakePurchase(homeBase_GO.GetInstanceID());
				HandlePurchase(item);
			}
		} else {
			float vertInput = Input.GetAxis("Vertical_" + (player_num % 2.0f).ToString());
			if (vertInput < 0) {
				shopMenu.ScrollDown();
			}else if (vertInput > 0) {
				shopMenu.ScrollUp();
			}
			if (Input.GetButtonDown("Action_" + (player_num % 2).ToString())) {
				ShopItem item = shopMenu.GetCurrentItem();
				if ((item is SwordScript && currentWeapon is SwordScript) || 
				    (item is BowScript && currentWeapon is BowScript) || 
				    (item is WallScript && homeBase.HasWalls())) {
					return;
				}
				item = shopMenu.MakePurchase(homeBase_GO.GetInstanceID());
				HandlePurchase(item);
			}
		}
	}

	void HandlePurchase(ShopItem item) {
		if (item) {
			if (item is WeaponItem) {
				HandleWeapon((WeaponItem)item);
			} else if (item is BaseUpgradeItem) {
				HandleBaseUpgrade((BaseUpgradeItem)item);
			}
		}
	}

	void HandleWeapon(WeaponItem weapon) {
		weapons[currentWeaponIndex].SetActive(false);
		if (weapon is SwordScript) {
			hasWeapon = true;
			currentWeapon = weapon;
			currentWeaponIndex = 0;
		} else if (weapon is BowScript) {
			hasWeapon = true;
			currentWeapon = weapon;
			currentWeaponIndex = 1;
		}
		weapons[currentWeaponIndex].SetActive(true);
		//set to visible
		foreach (MeshRenderer renderer in weapons[currentWeaponIndex].GetComponentsInChildren<MeshRenderer>()) {
			renderer.enabled = true;
		}
		foreach (Collider collider in weapons[currentWeaponIndex].GetComponentsInChildren<Collider>()) {
			collider.enabled = true;
		}


	}

	void HandleBaseUpgrade(BaseUpgradeItem upgrade) {
		if (upgrade is WallScript) {
			homeBase.TurnOnWalls();
		}
	}

	public void awakePlayer() {
		dead = false;
		health = startingHealth;
		health_slider.value = health;
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

	public void takeDamage(int damage, GameObject enemy_base_GO){
		if(Time.time > vulnerable_at_time){
			health -= damage;
			//update health bar
			health_slider.value = health;
			vulnerable_at_time = Time.time + INVULNERABLE_TIME;
			Debug.Log("Player " + player_num + " health is " + health);
		}

		if(health <= 0){
			killPlayer(enemy_base_GO);
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
		updateStoneText();
		updateWoodText();
		this.transform.position = homeBase_GO.transform.position;

		stealthActive = false;
		hasWeapon = false;
		currentWeapon = null;
		weapons[currentWeaponIndex].SetActive(false);
		hasStealth = false;

		dead = true;
		if (inEnemyBase) {
			gm.playerInBase(false, EnemyBaseId);
		}
		inEnemyBase = false;
		respawn_at_time = Time.time + RESPAWN_TIME;
	}

	void Attack(){
		if(currentWeapon is SwordScript){
			weapons[currentWeaponIndex].GetComponent<SwordScript>().Swing();
			swinging_sword.Play();
			RaycastHit hitinfo;
			if (IsInRange(out hitinfo, "Player") && !inEnemyBase){
				PlayerController other = hitinfo.transform.GetComponent<PlayerController>();
				if (other.homeBase_GO.GetInstanceID() != this.gameObject.GetInstanceID()) {
					other.takeDamage(damage_amount, homeBase_GO);
				}
			}
		}  else if (currentWeapon is BowScript){
			if(Time.time > next_fire_at_time){
				Arrow newArrow = Instantiate(arrow, transform.position + transform.forward, Quaternion.AngleAxis(90, transform.right)) as Arrow;
				next_fire_at_time = Time.time + FIRE_RATE_TIME;
			}

		}
	}

	void TakeAction() {
		RaycastHit hitinfo;
		if (IsInRange(out hitinfo, "Resource") && !shopOpen) {
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

		} 

		else if (IsInRange(out hitinfo, "DropPoint") && !shopOpen) {
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
					if (!stole_resorces && !backpackFull) {
						CollectStone(drop.StealResources(stone_gather_val), " is stealing stone...");
						stole_resorces = true;
						steal_at_time = Time.time + STEAL_COOLDOWN_TIME;
						stealing_resources.Play ();
					}
					break;
				case ResourceType.wood:
					if (!stole_resorces && !backpackFull) {
						CollectWood(drop.StealResources(wood_gather_val), " is stealing wood...");
						stole_resorces = true;
						steal_at_time = Time.time + STEAL_COOLDOWN_TIME;
						stealing_resources.Play ();
					}
					break;
				}
			}
		} 
	}

	void ToggleStore() {
		if(!shopOpen){
			shopOpen = true;
			shop.SetActive (true);
		}
		else{
			shopOpen = false;
			shop.SetActive(false);
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
		string chopNotification = "Player " + Mathf.Ceil(player_num % 2.0f).ToString() + " is chopping wood!";
		updateMidScreenText(chopNotification);
		CheckMaxWood(amount);
		if (wood_text == null) {
			throw new UnassignedReferenceException("wood_text for player " + Mathf.Ceil(player_num % 2.0f).ToString() + " is null");
		}
		wood_text.text = "Carrying " + curr_wood_resource + " wood";
		updateMidScreenText("Player " + Mathf.Ceil(player_num % 2.0f).ToString() + message);
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
//		print ("Amount of " + resource.ToString () + " left: " + 
//						resource.GetComponent<Resource> ().amountLeft);
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
		string mineNotification = "Player " + Mathf.Ceil(player_num % 2.0f).ToString() + " is mining!";
		updateMidScreenText(mineNotification);
		CheckMaxStone(amount);
		if (stone_text == null) {
			throw new UnassignedReferenceException("stone_text for player " + Mathf.Ceil(player_num % 2.0f).ToString() + " is null");
		}
		stone_text.text = "Carrying " + curr_stone_resource + " stone";
		updateMidScreenText("Player " + Mathf.Ceil(player_num % 2.0f).ToString() + message);
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
