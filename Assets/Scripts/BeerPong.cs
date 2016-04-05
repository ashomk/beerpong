using UnityEngine;
using System.Collections;

public class BeerPong : MonoBehaviour {

	public GameObject GameElements;
	public GameObject Canvas;
	public GameStateBehaviour GamePlay;

	public delegate void ActivateGamePlayEvent();

	//This event is called when game play is active 
	public event ActivateGamePlayEvent ActivateGamePlay;


	public Vector3 gravity = new Vector3 (0, -5f, 0);

	public bool isActive {

		get;
		private set;
	}

	public float activationTime {
	
		get;
		private set;
	}

	public enum PlayerID {
		
		First = 1,
		Second
	}

	private void Awake () {

		Physics.gravity = gravity;
		Canvas.transform.parent = null;
	}

	public void ActivateGame () {

		if (!GameElements.activeSelf) {
		
			GameElements.SetActive (true);
		}


		ActivateGamePlay ();

		activationTime = Time.time;
		isActive = true;
	}

	private void OnDestroy () {
	
		Destroy (Canvas);
	}
}
