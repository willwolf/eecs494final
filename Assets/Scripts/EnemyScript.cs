using UnityEngine;
using System.Collections;

public class EnemyScript : MonoBehaviour {

	public float radius = 10f;
	private GameObject target;
	public int health = 4;
	public AudioSource splat_sound;
	public AudioSource cow_sound;
	public AudioSource gallop_sound;
	private float flash_timer;
	public float speed_multiplier = 20f;

	public Material normMat;
	public Color hitColor;
	public Color normColor;
	public Color alertColor;

	// Use this for initialization
	void Start () {
		normMat = this.renderer.material;
		normColor = this.renderer.material.color; 
		hitColor = Color.white;
		alertColor = Color.red;
	}
	
	// Update is called once per frame
	void Update () {
		if(!target){
			Collider[] hitColliders = Physics.OverlapSphere(transform.position, 
			                                                radius, 
			                                                1 << LayerMask.NameToLayer("Player"));
			if(hitColliders.Length > 0) {
				target = hitColliders[0].gameObject;
				flash_timer = Time.time + 1f;
				cow_sound.Play();
				gallop_sound.Play();
				StartCoroutine(colorFlash(normColor, alertColor));
			}
		} else {
			Vector3 direction = target.transform.position - this.transform.position;
			if(direction.magnitude > radius){
				stopChasing();
			} else {
				transform.LookAt(target.transform);
				transform.localPosition += direction.normalized * Time.deltaTime * speed_multiplier;
			}
		}

	}

	void OnCollisionEnter(Collision other){
		if(other.gameObject.layer == LayerMask.NameToLayer("Player")){
			PlayerController pc = other.gameObject.GetComponent<PlayerController>();
			pc.takeDamage(1);
			if(pc.isDead()){
				stopChasing();
			}
		}
	}

	void OnCollisionStay(Collision other){
		OnCollisionEnter(other);
	}

	private void stopChasing(){
		target = null;
		gallop_sound.Stop();
	}

	public void takeDamage(int damage){
		flash_timer = Time.time + 1f;
		health -= damage;
		splat_sound.Play();
		StartCoroutine(colorFlash(normColor, hitColor));
		
		if(health <= 0){
			Destroy(this.gameObject);
		}
	}

	public  IEnumerator colorFlash(Color normalColor, Color otherColor){
		int index = 0;
		while(Time.time < flash_timer){
			this.renderer.material = null;
			if(index % 2 == 0){
				this.renderer.material.color = normalColor;
				this.renderer.material = normMat;
			}
			else{
				this.renderer.material.color = otherColor;
			}
			++index;
			yield return new WaitForSeconds(.1f);  
		}
		this.renderer.material.color = normalColor;
		this.renderer.material = normMat;
	}
	
}
