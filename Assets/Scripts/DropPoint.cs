using UnityEngine;
using System.Collections;

public class DropPoint : MonoBehaviour {

	public GameManager gm = null;
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
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
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
		gm.AddResources(playerBaseGO.GetInstanceID(), resourceType, amount);
	}

	public int StealResources(int amount) {
		return gm.RemoveResources(playerBaseGO.GetInstanceID(), resourceType, amount);
	}
}
