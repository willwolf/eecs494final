using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using InControl;

public class IntroScene : MonoBehaviour {

    public Canvas can;

    public Sprite title1;
    public Sprite title2;
    public AudioSource themeSong;

    public Sprite poroi;
    public Sprite dudes;
    public Sprite conflict;
    public Sprite cata;
    public Dictionary<int, Image> comic;

    float flashTimer;
    public int flashCount = 0;
    public int clickCount = 0;
	public float nextTransition = 0;
	public float transitionInterval = 2f;
	public bool USE_TIMER = true;

	// Use this for initialization
	void Awake () {
		themeSong.Play();
		can = GameObject.Find("Canvas").GetComponent<Canvas>();
		flashTimer = Time.time + .5f;
		nextTransition = Time.time + transitionInterval;
	}
	
	// Update is called once per frame
	void Update () {

     

      if (clickCount == 0 )
      {
          if (Time.time > flashTimer)
          {
              if (flashCount % 2 == 0)
              {
                  can.GetComponent<Image>().sprite = title1;
              }

              else
              {
                  can.GetComponent<Image>().sprite = title2;
              } 
              flashTimer = Time.time + 1;
              ++flashCount;
          } 
        
		}
		if (USE_TIMER && clickCount != 0 && Time.time > nextTransition) {
			MakeTransition(clickCount);
			clickCount++;
			nextTransition = Time.time + transitionInterval;
		}

      //change this to action button or something
		if (Input.GetKeyDown(KeyCode.P)) {
			MakeTransition(clickCount);
			++clickCount;
			nextTransition = Time.time + transitionInterval;
		}
		foreach(var d in InputManager.Devices) {
			if (d.Action1.WasPressed) {
				MakeTransition(clickCount);
				++clickCount;
				nextTransition = Time.time + transitionInterval;  
			}
		}
	}

	public void MakeTransition(int clickCount) {
		switch (clickCount) { 
		case 0:
			can.GetComponent<Image>().sprite = poroi;
			break;
		case 1:
			can.GetComponent<Image>().sprite = dudes;
			break;
		case 2:
			can.GetComponent<Image>().sprite = conflict;
			break;
		case 3:
			can.GetComponent<Image>().sprite = cata;
			break;
		case 4:
			Application.LoadLevel("_scene_tutorial_2");
			break;
		}
	}
}
