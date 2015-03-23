using UnityEngine;
using System.Collections;

public class Catapult : MonoBehaviour {

	public GameObject legs;
	public GameObject arm;
	public GameObject stone;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void EnableObject(GameObject obj) {
		foreach(Collider c in obj.GetComponentsInChildren<Collider>()) {
			c.enabled = true;
		}
		foreach(MeshRenderer mr in obj.GetComponentsInChildren<MeshRenderer>()) {
			mr.enabled = true;
		}
	}

	public void EnableArm() {
		EnableObject(arm);
	}
	public void EnableLegs() {
		EnableObject(legs);
	}
	public void EnableStone() {
		EnableObject(stone);
	}
}
