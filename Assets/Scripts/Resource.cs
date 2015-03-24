using UnityEngine;
using System.Collections;

public enum ResourceType {
	none,
	wood,
	stone
}

public class Resource : MonoBehaviour {
	
	public ResourceType type = ResourceType.none;
	public GameObject scatterObject;
	public int amountLeft = 0;

	// Use this for initialization
	void Awake () {
		if (type == ResourceType.wood) {
			amountLeft = PlayerController.MAX_WOOD_PER_TREE;	
		}
		else if(type == ResourceType.stone){
			amountLeft = PlayerController.MAX_STONE_PER_ROCK;
		}
	}
	
	// Update is called once per frame
	void Update () {
		Debug.DrawRay(transform.position, transform.forward, Color.red);
		Debug.DrawRay(transform.position, -transform.forward, Color.red);
		
		Debug.DrawRay(transform.position, transform.right, Color.blue);
		Debug.DrawRay(transform.position, -transform.right, Color.blue);

		Debug.DrawRay(transform.position, transform.up, Color.green);
		Debug.DrawRay(transform.position, -transform.up, Color.green);
	}

//	if (type == ResourceType.wood) {
//		spawnSpot = origin.position + (origin.up * Random.Range(2f, 3f)) + 
//			(origin.right * Random.Range(-2f, 3f) + (origin.forward * Random.Range(-2f, 3f)));
//	} else {
//		spawnSpot = transform.position + (origin.up * Random.Range(-2f, 3f)) + 
//			(transform.right * Random.Range(1f, 2.1f) + (transform.forward * Random.Range(-2f, 3f)));
//	}

	public void Gather() {
		amountLeft--;
		if (scatterObject) {
			GameManager.SpawnScatteredObject(scatterObject, transform.position);
		} else {
			print ("Resource has not scatterObject assigned!");
		}
		if (amountLeft == 0) {
			Destroy(this.gameObject);
		}
	}
}
