using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CatapultStoneScript : BaseUpgradeItem {

	public GameObject target;
	private GameManager gm;
	private bool rotate = true;
	public bool fired = false;
	public GameObject fire;
	public int num_fires;
	public float radius_min = 5;
	public float radius_max = 20;
	private Vector3 vel;
	public AudioSource catapult_sound;
	public AudioSource explosion_sound;
	public AudioSource flame_sound;

	// Use this for initialization
	void Start () {
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		
		Vector3 direction = (target.transform.position - this.transform.position);
		float distance = direction.magnitude;
		direction = direction.normalized;
		float velocity = Mathf.Sqrt(distance * Physics.gravity.magnitude);
//		vel = new Vector3(velocity * direction.x, 0.46f * velocity, velocity * direction.z);
		vel = new Vector3(0.69f * velocity * direction.x, 0.7f * velocity, 0.69f * velocity * direction.z);
		this.rigidbody.useGravity = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(fired){
			if(rotate){
				transform.Rotate(Time.deltaTime, 0, 0.5f);
			}
			
			foreach(PlayerController pc in gm.allPlayers){
				Camera cam = pc.GetComponentInChildren<Camera>();
				cam.transform.position = this.transform.position 
					+ this.transform.right * 20 
						+ this.transform.up * 5 
						+ this.transform.forward * 40;
				cam.transform.LookAt(this.transform);
			}
		}
	}
	
	override
	public bool CanPurchase(PlayerController p, int teamId, GameManager gm) {
		if (!GameManager.FORCE_CATAPULT_ORDER)
			return !gm.teamCatapultStatus[teamId].has_projectile;
		else
			return gm.teamCatapultStatus[teamId].has_arm && !gm.teamCatapultStatus[teamId].has_projectile;
	}

	override public void MakePurchase(PlayerController p, int teamId, GameManager gm) {
		gm.AddCatapultPart(teamId, CatapultPart.stone);
	}

	void OnCollisionEnter(Collision other){
		if(!fired) return;
		rigidbody.freezeRotation = true;
		rigidbody.velocity = Vector3.zero;
		rotate = false;
		this.collider.enabled = false;
		this.rigidbody.useGravity = false;
		this.transform.position = new Vector3(this.transform.position.x, other.transform.position.y + 0.5f, this.transform.position.z);

		Vector3 pointPos;
		for(int i = 0; i < num_fires;i++){
			//randomize the radius fo where the fire spawns
			float radiusX = Random.Range(radius_min, radius_max);
			float radiusZ = Random.Range(radius_min, radius_max);
			
			//multiply 'i' by '1.0f' to ensure the result is a fraction
			float pointNum = (i*1.0f)/num_fires;
			
			//angle along the unit circle for placing points
			float angle = pointNum*Mathf.PI*2;
			
			float x = Mathf.Sin (angle)*radiusX;
			float z = Mathf.Cos (angle)*radiusZ;
			
			//position for the point prefab
			pointPos = new Vector3(x, 0, z) + this.transform.position;
			pointPos.y = 0;
			
			//place the prefab at given position
			Instantiate (fire, pointPos, Quaternion.identity);
		}
		flame_sound.Play();
	}

	public void Fire(){
		fired = true;
		this.transform.SetParent(null);
		this.rigidbody.velocity = vel;
		rigidbody.useGravity = true;
		catapult_sound.Play();
		explosion_sound.Play();
	}
}
