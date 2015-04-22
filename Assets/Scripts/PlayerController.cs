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
	public int health;
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
	public float backPedalMult = 0.5f; // Higher value means faster backpedal
	public float minMoveSpeed = 5;
	public float maxMoveSpeed = 20;
	public float enemy_base_speed_multiplier = 0.5f;
	public float encumberPercent = 0.5f;
	public float wincumberPercent = 0.05f;

	public int curr_wood_resource = 0;
	public int wood_gather_val = 1;

	public int curr_stone_resource = 0;
	public int stone_gather_val = 1;
	public int MAX_RESOURCES = 30;

	public float COLLECTION_COOLDOWN_TIME = 0.5f;
	private float collect_at_time;
	private bool stole_resorces;
	public float STEAL_COOLDOWN_TIME = 0.25f;
	public float steal_at_time;


	public int MIDTEXT_COOLDOWN_TIME = 5;
	private bool showing = false;
	private float zero_at_time;

	public GameObject homeBase_GO;
	public Base homeBase { get; private set; }
	public bool inBase = false;
	public bool inEnemyBase = false;
	public int EnemyBaseId;

	private bool frozen = false;
	private float frozenUntil;
	private float stunTime = 1.5f;
	
	private Text stone_text;
	private Text wood_text;
	private Text mid_screen_text;
	private Slider health_slider;
	private Slider wood_slider;
	private Slider stone_slider;
	public Slider armor_slider;
	public Slider arrow_slider;
	private Text arrow_text;

	public AudioSource mining_stone;
	public AudioSource chopping_wood;
	public AudioSource dropping_resources;
	public AudioSource stealing_resources;
	public AudioSource swinging_sword;
	public AudioSource splat_sound;
	public AudioSource arrow_sound;
	public AudioSource purchasing_sound;

	public bool hasWeapon = false;
	public int currentWeaponIndex = 0;
	public GameObject armorPrefab = null;
	public GameObject armor = null;
	public ArmorScript playerArmor = null;
	public List<GameObject> weapons = new List<GameObject>();
	public WeaponItem currentWeapon = null;

	public Camera playerCam;
	public Vector3 camEulerStart;
	public float MAX_CAM_ANGLE_DELTA = 10f;
	public float cameraYAxisSpeed = 5f;
	public GameManager gm;
	public Canvas canvas;
	public Image cataArm;
	public Image cataStone;
	public Image cataBase;

	public Image oppCataArm;
	public Image oppCataStone;
	public Image oppCataBase;

	public GameObject shop;
	public ShopMenu shopMenu;
	public bool shopOpen;
	public GameObject arrow;
	public GameObject aim;
	private GameObject aimLine;
	public GameObject resourceBox;
	public GameObject trapGO;

	public GameObject stone_scatterObj;
	public GameObject wood_scatterObj;

	public List<GameObject> changeShaderObjects = new List<GameObject>();

	public Material normMat;
	public Color hitColor;
	public Color normColor;
  public Animator anim;
  public int swordAnimHash;

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
		wood_slider = canvas.transform.FindChild ("WoodSlide").GetComponent<Slider> ();
		stone_slider = canvas.transform.FindChild ("StoneSlide").GetComponent<Slider> ();
		wood_slider.value = curr_wood_resource;
		stone_slider.value = curr_stone_resource;
		armor_slider = canvas.transform.FindChild ("ArmorSlide").GetComponent<Slider> ();
		armor_slider.value = 0;
		playerCam = transform.FindChild("Cam").GetComponent<Camera>();
		camEulerStart = playerCam.transform.rotation.eulerAngles;

		arrow_slider = canvas.transform.FindChild ("Arrow Slider").GetComponent<Slider> ();
		arrow_text   = arrow_slider.transform.FindChild ("ArrowText").GetComponent<Text> ();

		mid_screen_text.text = "";
		updateStoneText();
		updateWoodText();

		
		cataBase = canvas.transform.FindChild ("Team_Catapult_Base_Icon").GetComponent<Image> ();
		cataArm = canvas.transform.FindChild ("Team_Catapult_Arm_Icon").GetComponent<Image> ();
		cataStone = canvas.transform.FindChild ("Team_Catapult_Stone_Icon").GetComponent<Image> ();
		cataBase.enabled = false;
		cataArm.enabled = false;
		cataStone.enabled = false;

		//Currently hardcoded assuming just 2 teams
		oppCataBase = canvas.transform.FindChild ("Opp_Catapult_Base_Icon").GetComponent<Image> ();
		oppCataArm = canvas.transform.FindChild ("Opp_Catapult_Arm_Icon").GetComponent<Image> ();
		oppCataStone = canvas.transform.FindChild ("Opp_Catapult_Stone_Icon").GetComponent<Image> ();
		oppCataBase.enabled = false;
		oppCataArm.enabled = false;
		oppCataStone.enabled = false;

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
    anim = this.GetComponent<Animator>();
    swordAnimHash = Animator.StringToHash("SwordSwing");
	}

	// Update is called once per frame
	void Update () {
		if (hasWon (GameManager.winningTeam)) {
			updateMidScreenText("You won!\nPress 'R' to Replay");
			//			Time.timeScale = 0;
			return;
		} else if(GameManager.winningTeam != -1) {
			updateMidScreenText("You lost!\nPress 'R' to Replay");
			//			Time.timeScale = 0;
			return;
		}

		if (!(currentWeapon is BowScript)) {
			arrow_slider.gameObject.SetActive(false);
		}

		if (frozen && Time.time > frozenUntil) {
			frozen = false;
            rigidbody.constraints &= ~RigidbodyConstraints.FreezePosition;
			if (freeze_countdown) {
				updateMidScreenText(freeze_expiration_msg);
			}
		}
		if (frozen && freeze_countdown) {
			updateMidScreenText(Mathf.Ceil(frozenUntil - Time.time).ToString("0"));
		}
		if (stunned && Time.time > stunned_until) {
			stunned = false;
		}

		if (player_num == 0) {
			throw new UnassignedReferenceException("PlayerController::playerNum must be non-zero");
		}

		if (dead) {
			if (Time.time > respawn_at_time || Input.GetKeyDown(KeyCode.T)) {
				mid_screen_text.text = "";
				awakePlayer();
			} else {
				mid_screen_text.text = "Respawn in " + Mathf.Floor(respawn_at_time - Time.time).ToString("0");
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

		foreach (Renderer renderer in this.GetComponentsInChildren<Renderer>()) {
			Color col = renderer.material.color;
			if (hasWeapon && currentWeapon is StealthScript) {
				col.a = .1f;
			} else {
				col.a = 1f;
			}
			renderer.material.color = col;
		}

		if (!frozen) {
			if (!shopOpen) {
				Move();
			} else {
				CheckShopInputs();
			}

			if(inBase && (curr_wood_resource > 0 || curr_stone_resource > 0 || GetComponentInChildren<ResourceBox>())) {
				foreach(DropPoint drop in homeBase_GO.GetComponentsInChildren<DropPoint>()){
					DepositResources(drop);
				}
			}

//			RaycastHit hitinfo;
//			if (IsInRange(out hitinfo, "DropPoint") && !shopOpen) {
//				DropPoint drop = hitinfo.transform.GetComponent<DropPoint>();
//				if (drop == null) {
//					throw new UnassignedReferenceException("DropPoint layer object does not have a DropPoint script attached");
//				}
//				
//				if (drop.playerBaseGO.GetInstanceID() == homeBase_GO.GetInstanceID()) {
//					DepositResources(drop);
//				}
//			}

			if (device != null) {
				if (device.Action1.IsPressed) {
					TakeAction();
				} if((device.RightTrigger.IsPressed || device.RightBumper.IsPressed) && !shopOpen && 
                    !GetComponentInChildren<Trap>() && !GetComponentInChildren<ResourceBox>()){
					Attack();
				} if((device.LeftTrigger.IsPressed || device.LeftBumper.IsPressed) && !shopOpen &&
                    !GetComponentInChildren<Trap>() && !GetComponentInChildren<ResourceBox>()){
					Aim();
				} else if(aimLine){
					Destroy(aimLine);
				} if(device.Action2.WasPressed){
					DropResourceBox();
				}
			} else if (Input.GetButton("Action_" + (player_num % 2).ToString())) {
                if (!shopOpen && (!GetComponentInChildren<Trap>() && !GetComponentInChildren<ResourceBox>())) {
					Attack();
				} else if(GetComponentInChildren<ResourceBox>()) {
					DropResourceBox();
				}
				TakeAction();
			}
			if (device != null) {
				if (inBase && (device.Action3.WasPressed || (shopOpen && device.Action2.WasPressed))) {
					ToggleStore();
				}
			} else if (Input.GetButtonDown("Store_Open_" + (player_num % 2).ToString()) && inBase) {
				ToggleStore();
			}
		}
	}

	void LateUpdate(){
		foreach(GameObject go in changeShaderObjects) {			
			if (currentWeapon is StealthScript) {
				go.renderer.material.shader = Shader.Find("Transparent/Diffuse");
			} else {
				go.renderer.material.shader = Shader.Find("Diffuse");
			}
		}
	}
		
	void OnTriggerEnter(Collider col) {
		if (col.gameObject.layer == LayerMask.NameToLayer("ScatteredObject")) {
			ScatteredObj s = col.transform.GetComponentInParent<ScatteredObj>();
			if (s.CanPickUp() && !IsPackFull()) {
				switch (s.type) {
				case ResourceType.stone:
					CollectStone(" picked up some stone!");
					break;
				case ResourceType.wood:
					CollectWood(" picked up some wood!");
					break;
				}
				s.PickUp();
			}
		}
	}

	void OnTriggerStay(Collider col) {
		OnTriggerEnter(col);
	}

	bool freeze_countdown = false;
	string freeze_expiration_msg;

	public void freeze(float duration, bool flash, bool show_countdown = false, string expiration_msg = "") {
		frozen = true;
		frozenUntil = Time.time + duration;
//		if(Time.time > vulnerable_at_time)
//			vulnerable_at_time = frozenUntil;
		if(flash) {
			StartCoroutine(colorFlash());
		}

		freeze_countdown = show_countdown;
		freeze_expiration_msg = expiration_msg;
	}

	public  IEnumerator stunFlash(){
		int index = 0;
		while(Time.time < stunned_until){
			this.renderer.material = null;
			if(index % 2 == 0){
				this.renderer.material.color = normColor;
				this.renderer.material = normMat;
			}
			else{
				this.renderer.material.color = hitColor;
			}
			++index;
			yield return new WaitForSeconds(.1f);  
		}
		this.renderer.material.color = normColor;
		this.renderer.material = normMat;
	}

	bool stunned = false;
	float stunned_until;
	public void stun(float duration, bool flash) {
		stunned = true;
		stunned_until = Time.time + duration;
		if (flash) {
			StartCoroutine(stunFlash());
		}
		
	}


	void Move() {
		float rotate_input = 0,
			  forward_input = 0,
			  sidestep_input = 0,
			  jump_input = 0,
			  cameraYAxis = 0;
		if (device != null) {
			// Default to using controller inputs, if they are present otherwise use keyboard commands
			rotate_input = device.RightStickX;
			cameraYAxis = device.RightStickY;
			sidestep_input = device.LeftStickX;
			forward_input = device.LeftStickY;

			Vector2 rightStick = new Vector2(rotate_input, cameraYAxis);
			rightStick.Normalize();
            //if (Mathf.Abs(rightStick.x) > Mathf.Abs(rightStick.y)) {
            //    cameraYAxis = 0;
            //}


			if (device.Action4.WasPressed) {
				// Check for being on the ground when the button is pressed
				if (Physics.Raycast(transform.position, Vector3.down, transform.collider.bounds.extents.y + 0.1f)) {
					jump_input = 1;
				}
			}
		} else {
			rotate_input = Input.GetAxis("Horizontal_" + (player_num % 2).ToString());
			forward_input = Input.GetAxis("Vertical_" + (player_num % 2).ToString());
		}
		
		transform.Rotate(Vector3.up, rotate_speed * Time.deltaTime * rotate_input);
		RotateCameraYAxis(cameraYAxis);
		if (!GameManager.PLAYER_VELOCITY) {
			transform.localPosition += (CalculateMoveSpeed(transform.forward, forward_input, forward_input < 0) +
		                            	CalculateMoveSpeed(transform.right, sidestep_input, false));
			rigidbody.velocity = new Vector3(0,rigidbody.velocity.y, 0);
		} else {
			Vector3 forwardChange = CalculateMoveSpeed(transform.forward, forward_input, forward_input < 0),
					sideStepChange = CalculateMoveSpeed(transform.right, sidestep_input, false);
			transform.rigidbody.velocity += (forwardChange + sideStepChange);
			AdjustVelocity(forward_input, sidestep_input);
		}
		
		Vector3 newVel = transform.rigidbody.velocity;
		newVel.y += jump_input * jump_height;
		transform.rigidbody.velocity = newVel;
	}
	
    void AdjustVelocity(float forwardInput, float sidestepInput) {
		if (!Mathf.Approximately(forwardInput, 0) || !Mathf.Approximately(sidestepInput, 0)) {
			if (rigidbody.velocity.magnitude < minMoveSpeed) {
				rigidbody.velocity = rigidbody.velocity.normalized * minMoveSpeed;
			} else if (forwardInput < 0 && rigidbody.velocity.magnitude > maxMoveSpeed * backPedalMult) {
				rigidbody.velocity = rigidbody.velocity.normalized * maxMoveSpeed * backPedalMult;
			} else if (rigidbody.velocity.magnitude > maxMoveSpeed) {
				rigidbody.velocity = rigidbody.velocity.normalized * maxMoveSpeed;
				if (forwardInput < 0) {
					rigidbody.velocity *= backPedalMult;
				}
			}
		} else {
			if (rigidbody.velocity.magnitude < minMoveSpeed) {
				rigidbody.velocity = Vector3.zero;
			}
		}
	}
	
    void RotateCameraYAxis(float yaxisInput) {
		Vector3 q = playerCam.transform.rotation.eulerAngles;
		if (yaxisInput > 0.6f) {
			q.x = Mathf.Lerp(q.x, camEulerStart.x + MAX_CAM_ANGLE_DELTA, Time.deltaTime * cameraYAxisSpeed);
        } else if (yaxisInput < -0.6f) {
			q.x = Mathf.Lerp (q.x, camEulerStart.x - MAX_CAM_ANGLE_DELTA, Time.deltaTime * cameraYAxisSpeed);
		} else {
			q.x = Mathf.Lerp(q.x, camEulerStart.x, Time.deltaTime * cameraYAxisSpeed);
		}
		playerCam.transform.rotation = Quaternion.Euler(q);
	}

	private float CalculateWinningEncumbered(){
		float percent = 0;
		if(homeBase.hasCatapultLegs) percent += wincumberPercent;
		if(homeBase.hasCatapultArm) percent += wincumberPercent;
		if(homeBase.hasCatapultStone) percent += wincumberPercent;
		return percent;
	}

	public float STUN_SLOWDOWN = .5f;

	Vector3 CalculateMoveSpeed(Vector3 direction, float input_data, bool isBackpedal) {
		Vector3 moveSpeed = direction * walk_speed * input_data * Time.deltaTime;
		if (isBackpedal) {
			moveSpeed *= backPedalMult;
		}

		if (stunned) {
			return moveSpeed * STUN_SLOWDOWN;
		} else if (!inEnemyBase) {
			float encumbered = 1f;
			float ecP = encumberPercent;
			if(GameManager.ENCUMBER_WINNERS){
				ecP += CalculateWinningEncumbered();
			}
			// max encumberance == 1 - enemy_base_speed_multiplier
			encumbered -= Mathf.Min(((1.0f * curr_wood_resource + curr_stone_resource) / MAX_RESOURCES), 
			                        enemy_base_speed_multiplier)*ecP;
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
				ShopItem item = shopMenu.MakePurchase(this, homeBase_GO.GetInstanceID());
				if(item){
					purchasing_sound.Play();
					// Close shop after making transaction
					ToggleStore();
				}
			}
		} else {
			float vertInput = Input.GetAxis("Vertical_" + (player_num % 2).ToString());
			if (vertInput < 0) {
				shopMenu.ScrollDown();
			}else if (vertInput > 0) {
				shopMenu.ScrollUp();
			}
			if (Input.GetButtonDown("Action_" + (player_num % 2).ToString())) {
				ShopItem item = shopMenu.MakePurchase(this, homeBase_GO.GetInstanceID());
				if(item){
					purchasing_sound.Play();
					// Close shop after making transaction
					ToggleStore();
				}
			}
		}
	}

	public void awakePlayer() {
		rigidbody.constraints &= ~RigidbodyConstraints.FreezePosition;
		dead = false;
		health = startingHealth;
		health_slider.value = health;
		vulnerable_at_time = Time.time + INVULNERABLE_TIME;
		foreach (Collider collider in GetComponentsInChildren<Collider>()) {
			collider.enabled = true;
		}
		foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>()) {
			renderer.enabled = true;
		}
		foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>()) {
			rb.isKinematic = false;
		}
    transform.position = gm.respawnPoints[player_num];
		transform.LookAt(GameObject.Find("CenterPoint").transform.position);
		transform.rotation = gm.LookAtCenter(transform);
		//StartCoroutine(colorFlash());
	}

	public  IEnumerator colorFlash(){
		int index = 0;
		while(Time.time < vulnerable_at_time){
			this.renderer.material = null;
			if(index % 2 == 0){
				this.renderer.material.color = normColor;
				this.renderer.material = normMat;
			}
			else{
				this.renderer.material.color = hitColor;
			}
			++index;
			yield return new WaitForSeconds(.1f);  
		}
		this.renderer.material.color = normColor;
		this.renderer.material = normMat;
	}

	public void takeDamage(int damage){
		if(Time.time > vulnerable_at_time){
			if (playerArmor != null) {
				playerArmor.TakeDamage(damage);
				armor_slider.value = playerArmor.armorHealth;
				if (playerArmor.IsDestroyed()) {
					Destroy(armor);
					armor = null;
					playerArmor = null;
				}
			} else {
				health -= damage;
				splat_sound.Play();
				//update health bar
				health_slider.value = health;
//				Debug.Log("Player " + player_num + " health is " + health);
			}
			vulnerable_at_time = Time.time + INVULNERABLE_TIME;
			if(frozen){
				frozen = false;
			} 
			StartCoroutine(colorFlash());
		}

		if(health <= 0){
			killPlayer();
		}
	}

	public void killPlayer() {
        Trap t = GetComponentInChildren<Trap>();
        if (t) {
            Destroy(t.gameObject);
        }
        // Remove trap so that resource box is dropped
		ScatterResources();
		DropResourceBox();

		foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>()) {
			renderer.enabled = false;
		}
		foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>()) {
			rb.isKinematic = true;
		}
		foreach (Collider collider in GetComponentsInChildren<Collider>()) {
			collider.enabled = false;
		}


		this.transform.position = homeBase_GO.transform.position + homeBase_GO.transform.up * 1;

		hasWeapon = false;
		currentWeapon = null;
		weapons[currentWeaponIndex].SetActive(false);
		if(aimLine) Destroy(aimLine);

		dead = true;
		if (inEnemyBase) {
			gm.playerInBase(false, EnemyBaseId);
		}
		inEnemyBase = false;
		respawn_at_time = Time.time + RESPAWN_TIME;
	}

	void DropResourceBox() {

		Vector3 direction = this.transform.forward;
		//pick layers to ignore
		LayerMask layerMask = ~( (1 << LayerMask.NameToLayer("Base"))
		                        + (1 << LayerMask.NameToLayer("Player")) 
		                       	+ (1 << LayerMask.NameToLayer("ScatteredObject"))
		                        + (1 << LayerMask.NameToLayer("ResourceBox"))
		                        + (1 << LayerMask.NameToLayer("Trap")));

		RaycastHit outInfo;
		while(Physics.Raycast(this.transform.position + this.transform.up, direction, out outInfo, 2.5f, layerMask)){
			if (outInfo.transform.gameObject.GetComponent<PlayerController>() == this) {
				break;
			}
			if(direction == this.transform.forward) {
				direction = this.transform.right;
			} else if (direction == this.transform.right){
				direction = -this.transform.forward;
			} else if (direction == -this.transform.forward) {
				direction = -this.transform.right;
			} else {
				direction = this.transform.forward;
				break;
			}
		}

		Vector3 drop_at_position = this.transform.position + direction * 1.5f;
		drop_at_position.y = 0.5f; //so it doesn't drop in the air, not the best solution

		Trap trap = this.GetComponentInChildren<Trap>();
        ResourceBox rbox = this.GetComponentInChildren<ResourceBox>();
		if (trap != null) {
			trap.transform.SetParent(null);
			trap.transform.position = drop_at_position;
			trap.init();
		} else if(rbox != null){
			rbox.transform.SetParent(null);
			rbox.transform.position = drop_at_position;
		} else if(curr_stone_resource + curr_wood_resource > 0){
			//drop resource box
			GameObject box_GO = Instantiate(resourceBox, drop_at_position, this.transform.rotation) as GameObject;
			rbox = box_GO.GetComponent<ResourceBox>();
			
			rbox.wood = curr_wood_resource;
			rbox.stone = curr_stone_resource;
			curr_wood_resource = 0;
			curr_stone_resource = 0;
			updateSliders();
			updateStoneText();
			updateWoodText();
		} 
	}

	void ScatterResources() {
		while (curr_stone_resource > 0 || curr_wood_resource > 0) {
			if (curr_stone_resource > 0) {
				GameManager.SpawnScatteredObject(stone_scatterObj, transform.position);
				curr_stone_resource--;
			}
			if (curr_wood_resource > 0) {
				GameManager.SpawnScatteredObject(wood_scatterObj, transform.position);
				curr_wood_resource--;
			}
		}
		updateSliders();
		updateStoneText();
		updateWoodText();
	}

	public int STARTING_ARROWS = 10;
	public int arrows;

	public void setNumArrows(int num) {
		arrows = num;
		arrow_slider.value = (float) arrows / STARTING_ARROWS;
		arrow_text.text = "Arrows: " + arrows.ToString();
	}

  public void setSwinging() { 
    
  }

	public void Attack(){
		if(currentWeapon is SwordScript){
			weapons[currentWeaponIndex].GetComponent<SwordScript>().Swing(swinging_sword);
            anim.SetTrigger(swordAnimHash);
            //anim.SetBool(Animator.StringToHash("swingBool"), true);
      
        


//			swinging_sword.Play();
			RaycastHit hitinfo;
		/*	if (IsInRange(out hitinfo, "Player")){
				PlayerController other = hitinfo.transform.GetComponent<PlayerController>();
				if (other.homeBase_GO.GetInstanceID() != this.homeBase_GO.GetInstanceID()) {
					other.takeDamage(damage_amount);
					other.stun(stunTime, true);
				}
			} else if (IsInRange(out hitinfo, "Enemy")){
				hitinfo.transform.GetComponent<EnemyScript>().takeDamage(damage_amount);
			}*/ 
		}  else if (currentWeapon is BowScript){
			if(Time.time > next_fire_at_time && arrows > 0){
				arrow_sound.Play();
				GameObject newArrow = Instantiate(arrow, transform.position + transform.forward * 3.5f + transform.up * 0.2f,
				                                  Quaternion.AngleAxis(90, transform.right)) as GameObject;
//				Time.timeScale = 0;
				newArrow.GetComponent<Arrow>().homebase_GO = homeBase_GO;

				setNumArrows(arrows - 1);
				next_fire_at_time = Time.time + FIRE_RATE_TIME;
			}
		}
	}

	void TakeAction() {
		RaycastHit hitinfo;
        ResourceBox rbox = GetComponentInChildren<ResourceBox>();
        Trap trap = GetComponentInChildren<Trap>();
		if (trap || rbox) {
			return;
		}
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

			if (drop.playerBaseGO.GetInstanceID() != homeBase_GO.GetInstanceID()) {
				StealResource(drop);
			}
		} else if(IsInRange(out hitinfo, "ResourceBox") && !shopOpen){
			//pick up resource box
			if (hitinfo.transform.gameObject.layer != LayerMask.NameToLayer("Player")) {
				hitinfo.transform.position = this.transform.position + this.transform.up * 0.5f + this.transform.forward;
				hitinfo.transform.SetParent(this.transform);
			}
		}
	}

	public void Aim(){
		if (currentWeapon is BowScript) {
			if (!aimLine) {
				aimLine = Instantiate(aim, transform.position + transform.forward * 5, Quaternion.AngleAxis(90, transform.right)) as GameObject;
				aimLine.layer = gm.teamTrapLayer[homeBase.gameObject.GetInstanceID()];
			}
			aimLine.GetComponent<LineRenderer>().SetPosition(0, transform.position);
			aimLine.GetComponent<LineRenderer>().SetPosition(1, transform.position + transform.forward * 60);
		}
	}
	
	void ToggleStore() {
		if(!shopOpen){
			shopOpen = true;
			shop.SetActive (true);
			shop.GetComponent<ShopMenu>().OpenShop(this, homeBase_GO.GetInstanceID());
		}
		else{
			shopOpen = false;
			shop.SetActive(false);
		}
	}

	public bool IsPackFull() {
		if (curr_wood_resource + curr_stone_resource >= MAX_RESOURCES) {
			updateMidScreenText("Backpack Full");
		}
		return curr_stone_resource + curr_wood_resource >= MAX_RESOURCES; 
	}

	void CollectResource(GameObject resource, ResourceType type){
		if(Time.time > collect_at_time) {
			if(type == ResourceType.stone){
				if (!GameManager.USE_SCATTER)
					CollectStone(" is mining!");
				mining_stone.Play();
			} if(type == ResourceType.wood){
				if (!GameManager.USE_SCATTER)
					CollectWood(" is chopping wood!");
				chopping_wood.Play();
			}
			decreaseResource(resource);
			collect_at_time = Time.time + COLLECTION_COOLDOWN_TIME;
		}
	}

	void StealResource(DropPoint drop){
		if(Time.time > steal_at_time && !IsPackFull()) {
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
		}
	}

	void DepositResources(DropPoint drop){
		ResourceBox rbox = this.GetComponentInChildren<ResourceBox>();
		switch (drop.resourceType) {
		case ResourceType.stone:
			if(curr_stone_resource > 0) {
				drop.DepositResources(curr_stone_resource);
				curr_stone_resource = 0;
				updateSliders();
				updateStoneText();
				dropping_resources.Play();
			} if(rbox != null && rbox.stone > 0){
				drop.DepositResources(rbox.stone);
				rbox.stone = 0;
				if(rbox.stone + rbox.wood == 0){
					Destroy(rbox.gameObject);
				}
				dropping_resources.Play();
			}
			break;
		case ResourceType.wood:
			if(curr_wood_resource > 0) {
				drop.DepositResources(curr_wood_resource);
				curr_wood_resource = 0;
				updateWoodText();
				updateSliders();
				dropping_resources.Play();
			} if(rbox != null && rbox.wood > 0){
				drop.DepositResources(rbox.wood);
				rbox.wood = 0;
				if(rbox.stone + rbox.wood == 0){
					Destroy(rbox.gameObject);
				}
				dropping_resources.Play();
			}
			break;
		}
	}

	void updateSliders(){
		stone_slider.value = curr_stone_resource;
		//this is because wood is underneath, must show past stone
		wood_slider.value = curr_wood_resource + curr_stone_resource;
	}

	void CollectStone(string message){
//		updateMidScreenText("Player " + player_num.ToString() + message);
		curr_stone_resource++;
		updateSliders ();
		updateStoneText();
	}

	void CollectWood(string message){
//		updateMidScreenText("Player " + player_num.ToString() + message);
		curr_wood_resource++;
		updateSliders ();
		updateWoodText();
	}

	private void updateWoodText() {
		wood_text.text = "Wood: " + curr_wood_resource;
	}

	private void updateStoneText() {
		stone_text.text = "Stone: " + curr_stone_resource;
	}

	void decreaseResource(GameObject resource){
		resource.GetComponent<Resource>().Gather();
	}

	
	bool IsInRange(out RaycastHit hitinfo, string Layer) {
		int layerMask = LayerMask.GetMask(Layer);
//		foreach (RaycastHit hit in Physics.SphereCastAll(transform.position, 1.5f, this.transform.forward, 3f, layerMask)) {
		foreach (RaycastHit hit in Physics.SphereCastAll(transform.position-transform.forward*1.5f, 1.5f, this.transform.forward, 4f, layerMask)) {
//			if(Vector3.Angle(transform.forward, hit.transform.position-transform.position)<30) {
			//if (hit.transform.position < this.transform.position) {
			if(true) {
				hitinfo = hit;
				return true;
			}
		}
		hitinfo = new RaycastHit();
		return false;
	}

	public void updateMidScreenText(string newText){
		showing = true;
		zero_at_time = Time.time + MIDTEXT_COOLDOWN_TIME;
	
		mid_screen_text.text = newText;

	}

	public bool hasWon(int baseID){
		return homeBase_GO.GetInstanceID() == baseID;
	}

	public bool isDead(){
		return dead;
	}
}
