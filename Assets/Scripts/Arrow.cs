using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {

	public float LIFE_TIME = 8;
	private float kill_at_time;
	public int damage_amount = 2;
	public float speed_multiplier = 20;
	public GameObject homebase_GO;

	// Use this for initialization
	void Start () {
		kill_at_time = Time.time + LIFE_TIME;
	}
	
	// Update is called once per frame
	void Update () {
		if(Time.time > kill_at_time){
			Destroy(this.gameObject);
		}
		transform.localPosition += transform.up * Time.deltaTime * speed_multiplier;
	}

	void OnCollisionEnter(Collision other){
		if(other.gameObject.layer == LayerMask.NameToLayer("Player")){
			Debug.Log("Hit!");
			PlayerController player = other.transform.GetComponent<PlayerController>();
			if(homebase_GO.GetInstanceID() != player.homeBase_GO.GetInstanceID())
				player.takeDamage(damage_amount);
		} else if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")){
			other.gameObject.GetComponent<EnemyScript>().takeDamage(damage_amount);
		} 
		Destroy(this.gameObject);
	}
}
