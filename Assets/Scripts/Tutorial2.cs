using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Tutorial2 : MonoBehaviour {

	private GameManager gm;
	public GameObject barriers;
	private int num_steps = 6;
	private int wall_drop = 13;
	private List<int> tutorialSteps = new List<int>();
	private List<float> waitTimes = new List<float>();
	private Dictionary<int, string> tutorialTexts = new Dictionary<int, string>();
	private float startTime;
	private float startGameTime;
	private bool wallDrop = false;
	private bool allDone = false;
	
	// Use this for initialization
	void Start () {
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		for(int i = 0; i < 4; i++){
			tutorialSteps.Add(0);
			waitTimes.Add(Time.time);
		}
		for(int i = 0; i <= num_steps; i++) {
			tutorialTexts[i] = i + ") " + getText(i);
		}
		startTime = Time.time + 4;
	}
	
	// Update is called once per frame
	void Update () {
		allDone = true;
		wallDrop = true;
		foreach(PlayerController pc in gm.allPlayers){
			if(getTask(pc)) {
				if(tutorialSteps[pc.player_num - 1] != 0){
					pc.updateMidScreenText("Task Complete!");
					pc.freeze(1, false);
				}
				if(tutorialSteps[pc.player_num - 1] < num_steps) {
					tutorialSteps[pc.player_num - 1]++;
				} else {
					pc.updateMidScreenText("Waiting for other players...");
					pc.freeze(1, false);
				}
				string nextText = tutorialTexts[tutorialSteps[pc.player_num - 1]];
				Text tut_text = pc.canvas.transform.Find("Tutorial_Text").gameObject.GetComponent<Text>();
				tut_text.text = nextText;
				int colorChoice = tutorialSteps[pc.player_num - 1] % 3;
				if(colorChoice == 0){
					tut_text.color = Color.blue;
				} else if(colorChoice == 1){
					tut_text.color = Color.red;
				} else {
					tut_text.color = Color.black;
				}
			}
			allDone = allDone && (tutorialSteps[pc.player_num - 1] >= num_steps);
			wallDrop = wallDrop && (tutorialSteps[pc.player_num - 1] >= wall_drop);
		} if(allDone) {
			if(Time.time > startGameTime){
				foreach(PlayerController pc in gm.allPlayers){
					string nextText = "Tutorial Complete! Press any button to start a game";
					pc.canvas.transform.Find("Tutorial_Text").gameObject.GetComponent<Text>().text = nextText;
					if(Input.anyKey || (pc.device != null && pc.device.AnyButton)){
						Application.LoadLevel("_scene_1");
					}
				}
			}
		} else {
			startGameTime = Time.time + 3;
		}
		
		//quit tutorial, play game
		if(Input.GetKeyDown(KeyCode.P)){
			Application.LoadLevel("_scene_1");
		}
	}
	
	bool getTask(PlayerController pc){
		int taskNum = tutorialSteps[pc.player_num - 1];
		switch(taskNum){
		case 0:
			return task0(pc);
		case 1:
			return task1(pc);
		case 2:
			return task2(pc);
		case 3:
			return task6(pc);
		case 4:
			return task8_1(pc);
		case 5:
			return task15_1(pc);
		case 6:
			return true;
		default:
			return false;
		}
	}
	
	string getText(int num){
		switch(num){
		case 0:
			return "Welcome to the Base Race Tutorial! Press P at any time to start the game";
		case 1:
			return "Collect 3 wood or 3 stone by pressing and holding A when close to the trees or rocks";
		case 2:
			return "Resources are automatically deposited when you enter your base.";
		case 3:
			return "You can toggle the shop menu when you are in your base by pressing X. Purchase a sword by pressing A";
		case 4:
			return "Use Right Trigger to attack";
		case 5:
			return "You can also steal resources from the enemy by pressing A when close to the boxes in their base";
		case 6:
			return "Good work! You are now ready to take on the enemy! First team to build their catapult wins!";
		default:
			return "";
		}
	}
	
	bool task0(PlayerController pc){
		//great job!
		pc.freeze(1, false);
		pc.canvas.transform.Find("Tutorial_Text").gameObject.GetComponent<Text>().text = getText(0);
		return Time.time > startTime;
	}
	
	bool task1(PlayerController pc){
		//collect 3 wood or stone
		return pc.curr_wood_resource >= 3 || pc.curr_stone_resource >= 3;
	}
	
	bool task2(PlayerController pc) {
		//deposit resources
		return pc.inBase;
	}

	bool task6(PlayerController pc){
		//purchase a sword
		return pc.GetComponentsInChildren<SwordScript>().Length > 0;
	}

	bool task8_1(PlayerController pc){
		//fire arrows
		if(pc.hasWeapon && (pc.device.RightTrigger.IsPressed || pc.device.RightBumper.IsPressed)){
			pc.Attack();
			return true;
		}
		waitTimes[pc.player_num - 1] = Time.time + 3;
		return false;
	}
	
	bool task15_1(PlayerController pc){
		pc.freeze(1, false);
//		pc.updateMidScreenText("Waiting for other players...");
		return Time.time > waitTimes[pc.player_num - 1];
	}

}
