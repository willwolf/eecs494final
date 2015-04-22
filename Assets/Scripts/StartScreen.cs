using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartScreen : MonoBehaviour {

	public Text playText;
	float flashTimer;

	// Use this for initialization
	void Start () {
		flashTimer = Time.time + 1;
	}
	
	// Update is called once per frame
	void Update () {
		if(Time.time > flashTimer){
			playText.enabled = !playText.enabled;
			flashTimer = Time.time + 1;
		}

		if(Input.GetKeyDown(KeyCode.P)){
			Application.LoadLevel(0);
		}
	}
}
