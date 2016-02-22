using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BeerPongInput : Singleton <BeerPongInput> {

	public GameObject UISlider;

	private void InputUpdate () {

		if (Input.touchCount == 0 && !Input.GetMouseButton(0)) {

			Touch touch = Input.GetTouch(0);

			if (isTouchDown) {

				if(touch.phase == TouchPhase.Ended)
	            {
					if (OnThrowEnd != null) {

						OnThrowEnd();
					}

					currentPower = UISlider.GetComponent <Slider> ().value;
	            }

				isTouchDown = false;
			
			} else {

				if (!isTouchDown) {

					if(touch.phase == TouchPhase.Began) {

						if (OnThrowStart != null) {
							
							OnThrowStart();
						}

		                isTouchDown = true;

		            } else  if(touch.phase == TouchPhase.Moved){

						if (OnThrowUpdate != null) {
							
							OnThrowUpdate();
						}

						currentPower = UISlider.GetComponent <Slider> ().value;
						isTouchDown = true;
        			}
				}
			}
		}
	}

	//The dummy has been updated to incorporate the UISlider @arpanbadeka has introduced
	private void DummyInputUpdate () {

		if ((Input.touchCount == 0 || Input.GetTouch (0).position.x > Screen.width/3) && !Input.GetMouseButton(0)) {

			if (isTouchDown) {

				if (OnThrowEnd != null)
					OnThrowEnd ();
			}

			isTouchDown = false;
		
		} else {

			if (!isTouchDown) {

				if (OnThrowStart != null)
					OnThrowStart ();
			
			} else if (OnThrowUpdate != null) {

				OnThrowUpdate ();
			}
			
			currentPower = UISlider.GetComponent <Slider> ().value;
			isTouchDown = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
		//TODO: Call InputUpdate once touch events are listened from the slider
		//InputUpdate ();
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

		UISlider.SetActive (visibility);
	}

	//This method must clear the slider to a state as though the interaction hasn't been started
	public void Reset () {

		UISlider.GetComponent<Slider> ().value = 0;
	}
}
