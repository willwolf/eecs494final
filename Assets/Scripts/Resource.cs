using UnityEngine;
using System.Collections;

public enum ResourceType {
	none,
	wood,
	stone
}

public class Resource : MonoBehaviour {
	
	public ResourceType type = ResourceType.none;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
