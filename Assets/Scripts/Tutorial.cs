using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Tutorial : MonoBehaviour {

	private GameManager gm;
	public GameObject barriers;
	private int num_steps = 16;
	private int wall_drop = 13;
	private List<int> tutorialSteps = new List<int>();
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
				tutorialSteps[pc.player_num - 1]++;
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
			Debug.Log(pc.GetComponentsInChildren<StealthScript>().Length);
		}
		if(wallDrop){
			Destroy(barriers);
		} if(allDone) {
			foreach(PlayerController pc in gm.allPlayers){
				string nextText = "Tutorial Complete! Press any button to start a game";
				pc.canvas.transform.Find("Tutorial_Text").gameObject.GetComponent<Text>().text = nextText;
				if(pc.device.AnyButton && Time.time > startGameTime){
					Application.LoadLevel(0);
				}
			}
		} else {
			startGameTime = Time.time + 5;
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
			return task3(pc);
		case 4:
			return task4(pc);
		case 5:
			return task5(pc);
		case 6:
			return task6(pc);
		case 7:
			return task7(pc);
		case 8:
			return task8(pc);
		case 9:
			return task9(pc);
		case 10:
			return task10(pc);
		case 11:
			return task11(pc);
		case 12:
			return task12(pc);
		case 13:
			return task13(pc);
		case 14:
			return task14(pc);
		case 15:
			return task15(pc);
		case 16:
			return true;
		default:
			return false;
		}
	}

	string getText(int num){
		switch(num){
		case 0:
			return "Welcome to Base Race! This tutorial will teach you how to play";
		case 1:
			return "Collect 3 wood or 3 stone by pressing A when close to the trees or rocks";
		case 2:
			return "Resources are automatically deposited when you enter your base.";
		case 3:
			return "Collect a full backpack. The more resources in your backpack, the slower you go!";
		case 4:
			return "Luckily, you can carry resources in your hands as well. Hit B to drop a box and A to pick it up.";
		case 5:
			return "Boxes do not slow you down, but prevent weapon use. Drop boxes by pressing B, or deposit by walking into your base.";
		case 6:
			return "You can toggle the shop menu when you are in your base by pressing X. Purchase a sword by pressing A";
		case 7:
			return "You can use Right Trigger to swing the sword. Next, purchase a bow";
		case 8:
			return "Use Right Trigger to fire arrows";
		case 9:
			return "Use Left Trigger to show the aim line";
		case 10:
			return "Traps can be very useful for slowing down your enemy. Purchase a trap from the store";
		case 11:
			return "You can place traps by pressing B. Your enemy can't see your trap once you place it down";
		case 12:
			return "Walls help protect your base and the resources behind them. Purchase walls for your base";
		case 13:
			return "You can also purchase stealth, which makes you invisible, but doesn't allow you to hold a weapon";
		case 14:
			return "Armor can help you survive longer. Purchase armor in the store";
		case 15:
			return "Stealth can help you steal resources. You can steal by pressing the A button on the enemies storages";
		case 16:
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
		if(pc.curr_wood_resource >= 3 || pc.curr_stone_resource >= 3) {
			return true;
		}
		return false;
	}

	bool task2(PlayerController pc) {
		//deposit resources
		//this is not fool proof, they could drop a box and it would still finish
		return (pc.curr_wood_resource == 0 && pc.curr_stone_resource == 0 
		   && (gm.teamResources[pc.homeBase_GO.GetInstanceID()].stone >= 3 || gm.teamResources[pc.homeBase_GO.GetInstanceID()].wood >= 3));
	}

	bool task3(PlayerController pc){
		//collect a full backpack
		return pc.IsPackFull();
	}

	bool task4(PlayerController pc){
		//drop a box of resources and pick it up
		return pc.GetComponentsInChildren<ResourceBox>().Length > 0;
	}

	bool task5(PlayerController pc){
		//drop or deposit a box
		return pc.GetComponentsInChildren<ResourceBox>().Length == 0;
	}

	bool task6(PlayerController pc){
		//purchase a sword
		return pc.GetComponentsInChildren<SwordScript>().Length > 0;
	}

	bool task7(PlayerController pc){
		//purchase a bow
		return pc.GetComponentsInChildren<BowScript>().Length > 0;
	}

	bool task8(PlayerController pc){
		//fire arrows
		if(pc.currentWeapon is BowScript && pc.device.RightTrigger.IsPressed){
			pc.Attack();
			return true;
		}
		return false;
	}

	bool task9(PlayerController pc){
		//show aim line
		if(pc.currentWeapon is BowScript && pc.device.LeftTrigger.IsPressed){
			pc.Aim();
			return true;
		}
		return false;
	}

	bool task10(PlayerController pc){
		//purchase a trap
		return pc.GetComponentsInChildren<Trap>().Length > 0;
	}

	bool task11(PlayerController pc){
		//place a trap
		return pc.GetComponentsInChildren<Trap>().Length == 0;
	}

	bool task12(PlayerController pc){
		//purchase walls (if applicable)
		return gm.teamBases[pc.homeBase_GO.GetInstanceID()].hasWalls;
	}

	bool task13(PlayerController pc){
		//purchase stealth
		return pc.currentWeapon is StealthScript;
	}

	bool task14(PlayerController pc){
		//purchase armor
		return pc.GetComponentsInChildren<ArmorScript>().Length > 0;
	}

	bool task15(PlayerController pc){
		//steal resources
		return pc.steal_at_time > Time.time;
	}




}
