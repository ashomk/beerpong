using UnityEngine;
using System.Collections;

public class CollisionAudioPlayer : MonoBehaviour {

	[Range (0, 1)]
	public float volumeFactor = 1f;

	void OnTriggerEnter(Collider other) {

		OnCollisionEnter (null);
	}

	void OnCollisionEnter(Collision col) {
		
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
