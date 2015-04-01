using UnityEngine;
using System.Collections;

public class Trap : ShopItem {

	public GameObject owner_base;

	public int FREEZE_DURATION = 10;

	private GameManager gm;

	bool initilized = false;

	// Use this for initialization
	void Start () {


	}

	public void init() {
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		
		if (owner_base == null) {
			throw new System.Exception("base not set");
		}
		
		gameObject.layer = gm.teamTrapLayer[owner_base.GetInstanceID()];
		this.transform.localScale = new Vector3(3f, .5f, 3f);
	}

	public override bool CanPurchase(PlayerController p, int teamId, GameManager gm) {
		return !p.hasBox && p.GetComponentInChildren<Trap>() == null;
	}

	public override void MakePurchase(PlayerController p, int teamId, GameManager gm) {
		Vector3 drop_at_position = p.transform.position - p.transform.up * 0.5f + p.transform.forward * 1.5f;
		GameObject t = Instantiate(p.trapGO, drop_at_position, p.transform.rotation) as GameObject;
		t.transform.parent = p.transform;
		
		Trap trap = t.GetComponent<Trap>();
		trap.owner_base = p.homeBase_GO;
		p.hasBox = true;
	}

	bool activated = false;
	float frozen_until;
	
	// Update is called once per frame
	void Update () {
		if (activated) {
			gameObject.layer = 0;

			if (Time.time > frozen_until) {
				Destroy(this.gameObject);
			}
		}

	}

	void OnTriggerEnter(Collider other) {
		PlayerController player = other.gameObject.GetComponent<PlayerController>();
		if (player == null) 
			return;

		if (player.homeBase_GO.GetInstanceID() != owner_base.GetInstanceID()) {
			player.freeze(FREEZE_DURATION);
			activated = true;
			frozen_until = Time.time + FREEZE_DURATION;
		}
	}
}
