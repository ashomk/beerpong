using UnityEngine;
using System;
using System.Collections;


public class PhysicsSync : Photon.MonoBehaviour {

	private PowerUpRing powerUpRing;
	private Obstacle obstacle;

	private Vector3 nextLocalPosition;
	private Quaternion nextLocalRotation;

	private bool nextVisibility;
	private Color nextColor;

	private bool isMyPhotonView;

	private GameStateBehaviour _gamePlay;
	private GameStateBehaviour gamePlay {
	
		get {

			if (_gamePlay == null) {
			
				_gamePlay = FindObjectOfType<GameStateBehaviour> ();
			}

			return _gamePlay;
		}
	}

	void Start () {

		powerUpRing = GetComponent<PowerUpRing> ();
		obstacle = GetComponent<Obstacle> ();

		isMyPhotonView = GetComponent<PhotonView> ().isMine;

		nextLocalPosition = transform.localPosition;
		nextLocalRotation = transform.localRotation;
	}

	void FixedUpdate() {

		Rigidbody rb = GetComponent<Rigidbody> ();
		if (rb != null && !isMyPhotonView) {
			
			Destroy (rb);
		}

		if (gamePlay != null) {

			if (transform.parent != gamePlay.transform) {
		
				transform.parent = gamePlay.transform;
			}
		}

		if (!isMyPhotonView) {

			transform.localPosition = Vector3.Lerp (nextLocalPosition, transform.localPosition, 0.9f);
			transform.localRotation = Quaternion.Slerp (nextLocalRotation, transform.localRotation, 0.9f);

			if (powerUpRing != null) {

				powerUpRing.UpdateColor (Color.Lerp (nextColor, powerUpRing.currentColor, 0.5f));
				powerUpRing.visibility = nextVisibility;
				powerUpRing.UpdateVisibility ();

			} else if (obstacle != null) {
				
				obstacle.UpdateColor (Color.Lerp (nextColor, obstacle.currentColor, 0.5f));
				obstacle.visibility = nextVisibility;
				obstacle.UpdateVisibility ();
			}
		}
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (gamePlay !=null) {
			if (stream.isWriting) {

				if (!gamePlay.isMyTurn) {
					
					transform.localPosition = Vector3.down * 50f;
				}

				stream.SendNext (transform.localPosition);
				stream.SendNext (transform.localRotation);

				if (powerUpRing != null) {

					Color color = powerUpRing.currentColor;
					Quaternion colorQuat = new Quaternion(color.r, color.g, color.b, color.a);
					stream.SendNext (colorQuat);
					stream.SendNext (powerUpRing.visibility ? 1f : 0f);

				} else if (obstacle != null) {
					
					Color color = obstacle.currentColor;
					Quaternion colorQuat = new Quaternion(color.r, color.g, color.b, color.a);
					stream.SendNext (colorQuat);
					stream.SendNext (obstacle.visibility ? 1f : 0f);
				}

			} else if (stream.isReading) {

				nextLocalPosition = (Vector3) stream.ReceiveNext ();
				nextLocalRotation = (Quaternion) stream.ReceiveNext ();

				if (powerUpRing != null || obstacle != null) {

					Quaternion colorQuat = (Quaternion) stream.ReceiveNext ();
					nextColor = new Color (colorQuat.x, colorQuat.y, colorQuat.z, colorQuat.w);
					nextVisibility = ((float)stream.ReceiveNext ()) > 0.5f;
				}
			}
		}
	}
}
