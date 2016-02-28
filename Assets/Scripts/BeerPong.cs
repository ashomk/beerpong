using UnityEngine;
using System.Collections;

public class BeerPong : Singleton <BeerPong> {

	public GameObject GameElements;
	public GameStateBehaviour GamePlay;

	public Vector3 gravity = new Vector3 (0, -5f, 0);

	public enum PlayerID {
		
		First = 1,
		Second
	}

	private void Awake () {

		Physics.gravity = gravity;
	}

	public void ActivateGame () {

		if (!GameElements.activeSelf) {
		
			GameElements.SetActive (true);
		}

		GamePlay.OnPairingComplete ();
	}
}
