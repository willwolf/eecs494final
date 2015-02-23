using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

	public int RESPAWN_TIME = 20;
	private bool dead = false;
	private float respawn_at_time;
	
	public int player_num  = 0;
	public float rotate_speed = 90f;
	public float walk_speed = 8f;

	public int curr_wood_resource = 0;
	public int wood_gather_val = 5;
	public int curr_stone_resource = 0;
	public int stone_gather_val = 5;
	public int MAX_RESOURCES = 50;

	public int WOOD_COOLDOWN_TIME = 20;
	private bool collected_wood = false;
	private float get_wood_at_time;

	public int STONE_COOLDOWN_TIME = 20;
	private bool collected_stone = false;
	private float get_stone_at_time;

	public GameObject homeBase_GO;
	public Base homeBase { get; private set; }
	public bool inBase = false;
	
	private Text stone_text;
	private Text wood_text;
	private Text mid_screen_text;

	// Use this for initialization
	void Start () {
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
	}
	
	// Update is called once per frame
	void Update () {
		if (player_num == 0) {
			throw new UnassignedReferenceException("PlayerController::playerNum must be non-zero");
		}

		if (dead) {
			if (Time.time > respawn_at_time) {
				mid_screen_text.text = "";
				awakePlayer();
			} else {
				mid_screen_text.text = "Respawn in " + Mathf.Floor(Time.time - respawn_at_time).ToString("0") + " seconds";
				return;
			}
		}

		if (collected_wood && (Time.time > get_wood_at_time)) {
			collected_wood = false;
			print ("Player " + player_num.ToString() + " may collect wood again!");
		}

		if (collected_stone && (Time.time > get_stone_at_time)) {
			collected_stone = false;
			print ("Player " + player_num.ToString() + " may collect stone again!");
		}

		float horizInput = Input.GetAxis("Horizontal_" + player_num.ToString()),
			  vertInput = Input.GetAxis("Vertical_" + player_num.ToString());

		transform.Rotate(Vector3.up, rotate_speed * Time.deltaTime * horizInput);
		transform.localPosition += (transform.forward * walk_speed * vertInput * Time.deltaTime);

		if (Input.GetButtonDown("Action_" + player_num.ToString())) {
			TakeAction();
		}
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

	public void killPlayer() {
		foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>()) {
			renderer.enabled = false;
		}
		foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>()) {
			rb.isKinematic = true;
		}
		foreach (Collider collider in GetComponentsInChildren<Collider>()) {
			collider.enabled = false;
		}

		curr_wood_resource = curr_stone_resource = 0;
		this.transform.position = homeBase_GO.transform.position;

		dead = true;
		respawn_at_time = Time.time + RESPAWN_TIME;
	}

	void TakeAction() {
		print ("Player " + player_num.ToString() + " is taking an action!");

		RaycastHit hitinfo;
		if (IsInRange(out hitinfo, "Player")) {
			PlayerController other = hitinfo.transform.GetComponent<PlayerController>();

			if (other.homeBase_GO.GetInstanceID() != this.gameObject.GetInstanceID()) {
				Debug.Log("In range of enemy player");
				if (this.inBase) {
					other.killPlayer(); 
				}
			} else {
				Debug.Log("In range of friendly plaer");
			}

		} else if (IsInRange(out hitinfo, "Resource")) {
			print ("Player " + player_num.ToString() + " is in range!");
			Resource r = hitinfo.transform.GetComponent<Resource>();
			if (r == null) {
				throw new UnassignedReferenceException("Resource layer object does not have Resource script attached");
			}

			switch (r.type) {
			case ResourceType.stone:
				MineStone();
				break;
			case ResourceType.wood:
				ChopWood();
				break;
			}
		}
	}

	private void updateWoodText() {
		wood_text.text = "Carrying " + curr_wood_resource + " wood";
	}

	void ChopWood() {
		if(!collected_wood){
			string chopNotification = "Player " + player_num.ToString() + " is chopping wood!";
			updateMidScreenText(chopNotification);
			if (curr_wood_resource + wood_gather_val > MAX_RESOURCES) {
				string maxWood = "Player " + player_num.ToString () + " has max amount of wood!";
				updateMidScreenText(maxWood);
				curr_wood_resource = MAX_RESOURCES;
			} else {
				curr_wood_resource += wood_gather_val;
			}
			if (wood_text == null) {
				throw new UnassignedReferenceException("wood_text for player " + player_num.ToString() + " is null");
			}
			wood_text.text = "Carrying " + curr_wood_resource + " wood";
			collected_wood = true;
			get_wood_at_time = Time.time + WOOD_COOLDOWN_TIME;
			print ("Get wood at: " + get_wood_at_time);
		} else{
			updateMidScreenText("Player " + player_num.ToString() + " must wait " + 
			                    (get_wood_at_time - Time.time) + " seconds to collect wood again.");
		}
		updateWoodText();
	}

	private void updateStoneText() {
		stone_text.text = "Carrying " + curr_stone_resource + " stone";
	}

	void MineStone() {
		if(!collected_stone){
			print ("Player " + player_num.ToString() + " is mining!");
			if (curr_stone_resource + stone_gather_val > MAX_RESOURCES) {
				print ("Player " + player_num.ToString() + " has max amount of stone!");
				curr_stone_resource = MAX_RESOURCES;
			} else {
				curr_stone_resource += stone_gather_val;
			}
			if (stone_text == null) {
				throw new UnassignedReferenceException("stone_text for player " + player_num.ToString() + " is null");
			}
			stone_text.text = "Carrying " + curr_stone_resource + " stone";
			collected_stone = true;
			get_stone_at_time = Time.time + STONE_COOLDOWN_TIME;
			print ("Get wood at: " + get_wood_at_time);
		} else{
			print ("Player " + player_num.ToString() + " must wait " + 
			       (get_stone_at_time - Time.time) + " seconds to collect stone again.");
		}
		updateStoneText();
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
		int layerMask = LayerMask.GetMask(Layer); // only collide with Resource layer
		return Physics.Raycast(origin, transform.forward, out info, 1.5f, layerMask);
	}

	private void updateMidScreenText(string newText){
		stone_text.text = newText;
	}
}
