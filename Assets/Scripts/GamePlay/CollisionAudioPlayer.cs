using UnityEngine;
using System.Collections;

public class CollisionAudioPlayer : MonoBehaviour {

	private BeerPong _beerPongGame;
	private BeerPong beerPongGame {

		get {

			if (_beerPongGame == null) {

				_beerPongGame = GetComponentInParent<BeerPong> ();
			}

			return _beerPongGame;
		}
	}

	[Range (0, 1)]
	public float volumeFactor = 1f;

	void OnTriggerEnter(Collider other) {

		if (beerPongGame == null || transform.position.y < beerPongGame.transform.position.y) {
			
			return;
		}
		
		if (other.GetComponent<Ball> () != null) {

			OnCollisionEnter (null);
		}
	}

	void OnCollisionEnter(Collision col) {

		if (beerPongGame == null || transform.position.y < beerPongGame.transform.position.y) {
			
			return;
		}
		
		if (col != null && col.collider.GetComponent<Ball> () == null) {
		
			return;
		}

		AudioSource audio = GetComponent<AudioSource> ();

		if (audio.isPlaying) {
			
			audio.Stop ();
		}

		float velocity = 5f;
		if (col != null) {

			velocity = col.relativeVelocity.magnitude;
		}

		if (velocity > 0) {
			
			audio.volume = Mathf.Clamp01 (volumeFactor * velocity / 5f);
			audio.Play ();
		}
	}
}
