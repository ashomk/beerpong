using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class BeerPongInput : Singleton <BeerPongInput>{

	public Slider slider;

	public void Awake(){
		
	}

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

	public void setSliderPosition(){

		slider.transform.position = Camera.main.WorldToScreenPoint(GameObject.Find("Ball").transform.position);
		Vector3 yPos = slider.transform.position;
		yPos.y = slider.transform.position.y - 240;
		slider.transform.position = yPos;

		//change slider direction
		slider.SetDirection (Slider.Direction.TopToBottom, false);

		//set width and height
		RectTransform rt = slider.GetComponent (typeof (RectTransform)) as RectTransform;
		rt.sizeDelta = new Vector2 (540, 750);

//		Image fillImage = slider.transform.FindChild ("Fill Area/Fill").GetComponent<Image> ();
//		fillImage.material.color = new Color (0, 0, 0, 0.4f);
//		slider.image = fillImage;
//
//		Image fillImage1 = slider.transform.FindChild ("Handle Slide Area/Handle").GetComponent<Image> ();
//
		Color fillColor = slider.image.color;
		fillColor.a = 0f;
		slider.image.color = fillColor;


		Image fill = slider.transform.FindChild ("Fill Area/Fill").GetComponent<Image> ();
		if (fill != null)
		{
			fill.sprite = Resources.Load<Sprite>("BeARPong_Slider_01");
			fill.color = new Color (255, 164, 0, 0.1f);
		}



		Image fill1 = slider.transform.FindChild ("Background").GetComponent<Image> ();
		if (fill1 != null)
		{
			fill1.sprite = Resources.Load<Sprite>("BeARPong_Slider_01");
			fill1.color = new Color (255, 164, 0, 0f);

		}




//		SetVisible (false);




	}

	public void setSliderInitialState(float value){
		Image fill1 = slider.transform.FindChild ("Background").GetComponent<Image> ();
		if (fill1 != null)
		{
			fill1.sprite = Resources.Load<Sprite>("BeARPong_Slider_01");
			fill1.color = new Color (255, 164, 0, value);

		}

//		Image fill = slider.transform.FindChild ("Handle Slide Area/Handle").GetComponent<Image> ();
//		if (fill != null)
//		{
//			fill.color = new Color (255, 164, 0, value);
//		}
//
//		Color fillColor = slider.image.color;
//		fillColor.a = 0f;
//		slider.image.color = fillColor;
	}
		
}


