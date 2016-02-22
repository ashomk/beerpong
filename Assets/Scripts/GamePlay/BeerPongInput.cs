using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BeerPongInput : Singleton <BeerPongInput> {

	public GameObject UISlider;
	// Use this for initialization
	void Start () {
	}


	private void InputUpdate () {

		if ((Input.touchCount == 0 || Input.GetTouch (0).position.x > Screen.width/3) && !Input.GetMouseButton(0)) {
			Touch touch = Input.GetTouch(0);
			if (isTouchDown) {

			if(touch.phase == TouchPhase.Ended)
            {
               OnThrowEnd();
			   currentPower = GameObject.Find("UISlider").GetComponent <Slider> ().value;
            }

			isTouchDown = false;
		
		} else {

			if (!isTouchDown) {
				if(touch.phase == TouchPhase.Began)
           			 {
                	OnThrowStart();
                isTouchDown = true;
            } else  if(touch.phase == TouchPhase.Moved){
            				OnThrowUpdate();
							currentPower = GameObject.Find("UISlider").GetComponent <Slider> ().value;
							isTouchDown = true;
            		}
		}
	}
}
}

	// Update is called once per frame
	void Update () {
	
		InputUpdate ();
	}

	//This value must return true if the user is touching any part of the slider (not necessarily the knob)
	public bool isTouchDown {

		get;
		private set;
	}

	public delegate void ThrowAction ();
	public event ThrowAction OnThrowStart;
	public event ThrowAction OnThrowUpdate;
	public event ThrowAction OnThrowEnd;

	public float currentPower {

		get;
		private set;
	}

	public void SetVisible (bool visibility) {

		//Do any beer pong logic here


		UISlider.SetActive (visibility);
	}

	//This method must clear the slider to a state as though the interaction hasn't been started
	public void Reset () {
	}
}
