using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public Dictionary<int, ResourceCount> teamResources;

	// Use this for initialization
	void Start () {
		GameObject[] bases = GameObject.FindGameObjectsWithTag("Base");
		foreach(GameObject b in bases){
			teamResources.Add(b.GetInstanceID(), new ResourceCount());
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
