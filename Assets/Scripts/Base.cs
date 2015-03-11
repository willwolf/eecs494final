using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Base : MonoBehaviour {

	GameManager gm;
	private bool hasWalls = false;

	void Start() {
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		foreach(Transform child in GetComponentsInChildren<Transform>()){
			if(child.tag == "Wall"){
				child.renderer.enabled = false;
				child.collider.enabled = false;
			}
		}
	}
	
	void Update(){

	}

	public void HasWalls(){
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
	
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
			return;

		PlayerController player = other.GetComponent<PlayerController>();
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
		
		PlayerController player = other.GetComponent<PlayerController>();
		if (player.homeBase_GO.GetInstanceID() == this.gameObject.GetInstanceID()) {
			player.inBase = false;
		} else {
			player.inEnemyBase = false;
			gm.playerInBase(false, this.gameObject.GetInstanceID());
		}
	}
}
