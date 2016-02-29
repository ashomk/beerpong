using UnityEngine;
using System.Collections;

public class TestGamePlay : MonoBehaviour {

	public GameObject beerPongGame;

	// Use this for initialization
	void Start () {
	
		if (beerPongGame != null) {

			BeerPong pong = beerPongGame.GetComponent <BeerPong> ();
			if (pong != null) {
		
				pong.ActivateGame ();
			}
		}
	}
}
