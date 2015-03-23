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
	public float encumberPercent = 0.5f;

	public int curr_wood_resource = 0;
	public int wood_gather_val = 1;
	public static int MAX_WOOD_PER_TREE = 10;
	public int curr_stone_resource = 0;
	public int stone_gather_val = 1;
	public int MAX_RESOURCES = 30;
	public static int MAX_STONE_PER_ROCK = 10;
	public bool backpackFull = false;

	public float COLLECTION_COOLDOWN_TIME = 0.5f;
	private float collect_at_time;
	private bool stole_resorces;
	public float STEAL_COOLDOWN_TIME = 0.25f;
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

	public Material normMat;
	public Color hitColor;
	public Color normColor;

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

		normMat = this.renderer.material;
		normColor = this.renderer.material.color; 
		hitColor = Color.white;
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
			if (hasWeapon && currentWeapon is StealthScript) {
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

	void OnTriggerEnter(Collider coll) {
		//currently friendly fire is enabled
		if (coll.gameObject.layer == LayerMask.NameToLayer ("Weapon")) {
			//takeDamage(damage_amount, homeBase_GO);		
		}
		
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
			float encumbered = 1f;
			// max encumberance == 1 - enemy_base_speed_multiplier
			encumbered -= Mathf.Min(((1.0f * curr_wood_resource + curr_stone_resource) / MAX_RESOURCES), 
			                        enemy_base_speed_multiplier)*encumberPercent;
			return moveSpeed * encumbered ;
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
				    (item is StealthScript && currentWeapon is StealthScript) || 
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
		} else if (weapon is StealthScript) {
			hasWeapon = true;
			currentWeapon = weapon;
			currentWeaponIndex = 2;
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

	public  IEnumerator colorFlash(){

		print ("HitColor: " + renderer.material.color.ToString());		
		this.renderer.material = null;
		this.renderer.material.color = hitColor;
		yield return new WaitForSeconds(0.1f);
		this.renderer.material = normMat;
		this.renderer.material.color = normColor;   
		print ("NormColor: " + renderer.material.color.ToString());
	}

	public void takeDamage(int damage, GameObject enemy_base_GO){
		if(Time.time > vulnerable_at_time){
			health -= damage;
			//update health bar
			health_slider.value = health;
			vulnerable_at_time = Time.time + INVULNERABLE_TIME;
			Debug.Log("Player " + player_num + " health is " + health);
			StartCoroutine(colorFlash());
			/*
			print ("HitColor: " + renderer.material.color.ToString());		
			this.renderer.material = null;
			this.renderer.material.color = hitColor;
			yield return new WaitForSeconds(0.1f);
			this.renderer.material = normMat;
			this.renderer.material.color = normColor;   
			print ("NormColor: " + renderer.material.color.ToString());*/
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
				GameObject newArrow = Instantiate(arrow, transform.position + transform.forward, Quaternion.AngleAxis(90, transform.right)) as GameObject;
				newArrow.GetComponent<Arrow>().homeBase_GO = homeBase_GO;
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

			CollectResource(hitinfo.transform.gameObject, r.type);
		} 

		else if (IsInRange(out hitinfo, "DropPoint") && !shopOpen) {
			DropPoint drop = hitinfo.transform.GetComponent<DropPoint>();
			if (drop == null) {
				throw new UnassignedReferenceException("DropPoint layer object does not have a DropPoint script attached");
			}

			if (drop.playerBaseGO.GetInstanceID() == homeBase_GO.GetInstanceID()) {
				DropResource(drop);
			} else {
				StealResource(drop);
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

	void CollectResource(GameObject resource, ResourceType type){
		if(Time.time > collect_at_time && !backpackFull){
			if(type == ResourceType.stone){
				CollectStone(" is mining!");
				mining_stone.Play();
			} if(type == ResourceType.wood){
				CollectWood(" is chopping wood!");
				chopping_wood.Play();
			}
			decreaseResource(resource);
			collect_at_time = Time.time + COLLECTION_COOLDOWN_TIME;
			backpackFull = (curr_stone_resource + curr_wood_resource >= MAX_RESOURCES);
		}
	}

	void StealResource(DropPoint drop){
		if(Time.time > steal_at_time && !backpackFull){
			if(drop.resourceType == ResourceType.wood){
				if(gm.teamResources[drop.playerBaseGO.GetInstanceID()].wood == 0){
					updateMidScreenText("No resources to steal");
					return;
				}
				drop.StealResources(wood_gather_val);
				CollectWood(" is stealing wood!");
			} if(drop.resourceType == ResourceType.stone){
				if(gm.teamResources[drop.playerBaseGO.GetInstanceID()].stone == 0){
					updateMidScreenText("No resources to steal");
					return;
				}
				drop.StealResources(stone_gather_val);
				CollectStone(" is stealing stone!");
			}
			stealing_resources.Play();
			steal_at_time = Time.time + STEAL_COOLDOWN_TIME;
			backpackFull = (curr_stone_resource + curr_wood_resource >= MAX_RESOURCES);
		}
	}

	void DropResource(DropPoint drop){
		switch (drop.resourceType) {
		case ResourceType.stone:
			if(curr_stone_resource > 0) {
				drop.DepositResources(curr_stone_resource);
				curr_stone_resource = 0;
				updateStoneText();
				backpackFull = false;
			}
			break;
		case ResourceType.wood:
			if(curr_wood_resource > 0) {
				drop.DepositResources(curr_wood_resource);
				curr_wood_resource = 0;
				updateWoodText();
				backpackFull = false;
			}
			break;
		}
		dropping_resources.Play();
	}
	
	void CollectStone(string message){
		updateMidScreenText("Player " + Mathf.Ceil(player_num % 2.0f).ToString() + message);
		curr_stone_resource++;
		updateStoneText();
	}

	void CollectWood(string message){
		updateMidScreenText("Player " + Mathf.Ceil(player_num % 2.0f).ToString() + message);
		curr_wood_resource++;
		updateWoodText();
	}

	private void updateWoodText() {
		wood_text.text = "Carrying " + curr_wood_resource + " wood";
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
