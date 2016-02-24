using UnityEngine;
using System.Collections;

public class BeerPong : Singleton <BeerPong> {

	public GameObject GameElements;
	public GameStateBehaviour GamePlay;

	public enum PlayerID {
		
		First = 1,
		Second
	}

	public void ActivateGame () {

		if (!GameElements.activeSelf) {
		
			GameElements.SetActive (true);
		}

		GamePlay.OnPairingComplete ();
	}
}
