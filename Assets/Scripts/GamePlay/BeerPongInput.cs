using UnityEngine;
using System.Collections;

public class BeerPongInput : Singleton <BeerPongInput> {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
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
