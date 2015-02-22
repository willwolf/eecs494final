using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public int playerNum  = 0;
	public float rotateSpeed = 90f;
	public float walkSpeed = 8f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		float horizInput = Input.GetAxis("Horizontal_" + playerNum.ToString()),
			  vertInput = Input.GetAxis("Vertical_" + playerNum.ToString());

		transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime * horizInput);
		transform.localPosition += (transform.forward * walkSpeed * vertInput * Time.deltaTime);
	}
}
