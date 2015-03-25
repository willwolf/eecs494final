using UnityEngine;
using System.Collections;

public class ResourceBox : MonoBehaviour {

	public int stone = 0;
	public int wood = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		foreach(TextMesh t in this.GetComponentsInChildren<TextMesh>()){
			Debug.Log(t.name);
			if(t.name == "WoodText"){
				t.text = wood.ToString();
			} if(t.name == "StoneText"){
				t.text = stone.ToString();
			}
		}
	}

}
