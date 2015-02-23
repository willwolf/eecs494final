using UnityEngine;
using System.Collections;

public class DropPoint : MonoBehaviour {
	
	public GameObject playerBaseGO = null;
	public ResourceType resourceType = ResourceType.none;
	public int currentResourceCount = 0;

	// Use this for initialization
	void Start () {
		if (playerBaseGO == null) {
			throw new UnassignedReferenceException("DropPoint::player_base_GO is uninitialized");
		}
		if (resourceType == ResourceType.none) {
			throw new UnassignedReferenceException("DropPoint::resourceType is uninitialized");
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (playerBaseGO == null) {
			throw new UnassignedReferenceException("DropPoint::player_base_GO is uninitialized");
		}
		if (resourceType == ResourceType.none) {
			throw new UnassignedReferenceException("DropPoint::resourceType is uninitialized");
		}
	}

	public void DepositResources(int amount) {
		currentResourceCount += amount;
	}

	public int StealResources() {
		return 0;
	}
}
