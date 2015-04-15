using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Base : MonoBehaviour {

	GameManager gm;
	public bool hasWalls = false;
	public bool hasCatapultArm {
		get; private set;
	}
	public bool hasCatapultLegs {
		get; private set;
	}
	public bool hasCatapultStone {
		get; private set;
	}
	public bool hasCatapultWeight {
		get; private set;
	}
	public GameObject catapult_GO;
	public Catapult catapult;

	void Start() {
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		foreach(Transform child in GetComponentsInChildren<Transform>()){
			if(child.tag == "Wall"){
				child.renderer.enabled = false;
				child.collider.enabled = false;
			}
		}
		hasCatapultArm = hasCatapultLegs = hasCatapultStone = hasCatapultWeight = false;
		catapult = catapult_GO.GetComponent<Catapult>();
		DisableCatapult();
	}

	void DisableCatapult() {
		foreach (Collider collider in catapult_GO.GetComponentsInChildren<Collider>()) {
			collider.enabled = false;
		}
		foreach (MeshRenderer renderer in catapult_GO.GetComponentsInChildren<MeshRenderer>()) {
			renderer.enabled = false;
		}
		foreach (Rigidbody rb in catapult_GO.GetComponentsInChildren<Rigidbody>()) {
			rb.isKinematic = false;
		}
	}

	void Update(){

	}

	public bool HasWalls(){
		return hasWalls;
	}

	public void TurnOnWalls(){
		foreach(Transform child in GetComponentsInChildren<Transform>()){
			if(child.tag == "Wall"){
				child.renderer.enabled = true;
				child.collider.enabled = true;
			}
		}
		hasWalls = true;
	}

	public void TurnOnCatapultArm() {
		catapult.EnableArm();
		hasCatapultArm = true;
	}
	public void TurnOnCatapultLegs() {
		catapult.EnableLegs();
		hasCatapultLegs = true;
	}
	public void TurnOnCatapultStone() {
		catapult.EnableStone();
		hasCatapultStone = true;
	}
	public void TurnOnCatapultWeight() {
		catapult.EnableWeight();
		hasCatapultWeight = true;
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
			return;

		PlayerController player = FindPlayerController(other);
		if (player == null) {
			return;
		}
		if (player.homeBase_GO.GetInstanceID() == this.gameObject.GetInstanceID()) {
			player.inBase = true;
		} else {
			player.inEnemyBase = true;
			player.EnemyBaseId = this.gameObject.GetInstanceID();
			gm.playerInBase(true, this.gameObject.GetInstanceID());
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
			return;
		
		PlayerController player = FindPlayerController(other);
		if (player == null) {
			return;
		}
		if (player.homeBase_GO.GetInstanceID() == this.gameObject.GetInstanceID()) {
			player.inBase = false;
		} else {
			player.inEnemyBase = false;
			gm.playerInBase(false, this.gameObject.GetInstanceID());
		}
	}

	PlayerController FindPlayerController(Collider other) {
		Transform other_GO = other.transform;
		PlayerController player = other_GO.GetComponent<PlayerController>();
		while (player == null && other_GO != null) {
			other_GO = other_GO.parent;
			if (other_GO) {
				player = other_GO.GetComponent<PlayerController>();
			}
		}
		return player;
	}
}
