using UnityEngine;
using System.Collections;

public class ObstacleMeshController : MonoBehaviour {

	private GameStateBehaviour _gamePlay;
	private GameStateBehaviour gamePlay {

		get {

			if (_gamePlay == null) {

				_gamePlay = FindObjectOfType<GameStateBehaviour> ();
			}

			return _gamePlay;
		}
	}
		
	void OnCollisionEnter(Collision collision) {

		if (collision.gameObject.GetComponent<Ball> () == null ||
			gamePlay == null || !gamePlay.isMyTurn) {
		
			return;
		}

		Debug.Log ("Hit the demon!");

		// get a reference to the animator on this gameObject
		var animator = transform.parent.GetComponent<Animator>();

		//animator.SetTrigger("tossTrigger");
		animator.SetBool("isHit", true);

		StartCoroutine(MakeVanish());
	}

	IEnumerator MakeVanish()
	{
		yield return new WaitForSeconds(1.5f);
		transform.parent.GetComponent<Obstacle> ().MakeVanish ();
	}
}
