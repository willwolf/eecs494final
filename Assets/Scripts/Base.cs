using UnityEngine;
using System.Collections;

public class Base : MonoBehaviour {

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
			return;

		PlayerController player = other.GetComponent<PlayerController>();
		if (player.homeBase_GO.GetInstanceID() == this.gameObject.GetInstanceID()) {
			player.inBase = true;
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
			return;
		
		PlayerController player = other.GetComponent<PlayerController>();
		if (player.homeBase_GO.GetInstanceID() == this.gameObject.GetInstanceID()) {
			player.inBase = false;
		}
	}
}
