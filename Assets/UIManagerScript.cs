using UnityEngine;
using System.Collections;

public class UIManagerScript : MonoBehaviour {

	public void startGame(){
		Debug.Log ("startGame called");
		Application.LoadLevel("BeerPong");
	}
}
