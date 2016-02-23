using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class BeerPongInput : Singleton <BeerPongInput>{

	public Slider slider;

	public void PointerUp () {

		if (isTouchDown) {

			if (OnThrowEnd != null) {

					OnThrowEnd();
			}

			currentPower = slider.value;
			isTouchDown = false;
		}
	}

	public void PointerDown(){

		if (!isTouchDown) {
			if (OnThrowStart != null) {

	   			OnThrowStart();
	   					
			}

			currentPower = slider.value;
			isTouchDown = true;
		}
	}

	public void onValueChanged () {

		if (isTouchDown) {

			if (OnThrowUpdate != null) {

	   			OnThrowUpdate();
			}
			currentPower = slider.value;
		}
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

		slider.gameObject.SetActive (visibility);
	}

	//This method must clear the slider to a state as though the interaction hasn't been started
	public void Reset () {

		slider.value = 0;
	}
}


