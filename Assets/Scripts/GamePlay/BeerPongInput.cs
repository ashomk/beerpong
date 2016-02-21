using UnityEngine;
using System.Collections;

public class BeerPongInput : Singleton <BeerPongInput> {

	// Use this for initialization
	void Start () {
	
	}

	private float dummyInputStartTime = 0;
	private const float DUMMY_INPUT_INTERVAL = 2;

	private void DummyInputUpdate () {

		if ((Input.touchCount == 0 || Input.GetTouch (0).position.x > Screen.width/3) && !Input.GetMouseButton(0)) {

			if (isTouchDown) {

				if (OnThrowEnd != null)
					OnThrowEnd ();
			}

			isTouchDown = false;
		
		} else {

			if (!isTouchDown) {

				dummyInputStartTime = Time.time;

				if (OnThrowStart != null)
					OnThrowStart ();
			
			} else if (OnThrowUpdate != null) {

				OnThrowUpdate ();
			}
			
			currentPower = Mathf.Clamp01 ((Time.time - dummyInputStartTime) / DUMMY_INPUT_INTERVAL);
			isTouchDown = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
		DummyInputUpdate ();
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


		gameObject.SetActive (visibility);
	}

	//This method must clear the slider to a state as though the interaction hasn't been started
	public void Reset () {
	}
}
