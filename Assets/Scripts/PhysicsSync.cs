using UnityEngine;
using System;
using System.Collections;


public class PhysicsSync : Photon.MonoBehaviour {

	private PowerUpRing powerUpRing;
	private Obstacle obstacle;
	private bool isBall;

	private Vector3 nextLocalPosition;
	private Quaternion nextLocalRotation;

	private bool nextVisibility;
	private Color nextColor;

	private Vector3 currentThrowDirection = Vector3.forward;
	private Vector3 nextThrowDirection;
	private float currentTrailRenderPower;
	private float nextTrailRenderPower;

	private bool isMyPhotonView;

	private static float THROW_POWER_THRESHOLD = 0.025f;

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
		isBall = GetComponent<Ball> () != null;

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
			
			} else if (isBall) {
			
				currentThrowDirection = Vector3.Lerp (nextThrowDirection, currentThrowDirection, 0.75f);

				if (nextTrailRenderPower > THROW_POWER_THRESHOLD && gamePlay != null) {

					currentTrailRenderPower = nextTrailRenderPower * 0.25f + currentTrailRenderPower * 0.75f;
					Vector3 initialVelocity = currentTrailRenderPower * currentThrowDirection.normalized * GameStateBehaviour.MAX_VELOCITY;
					float targetY = transform.position.y - gamePlay.BoardwalkPong.transform.position.y;
					BallMotionController.Instance.RenderTrail (initialVelocity, transform.position, targetY);
				
				} else {

					BallMotionController.Instance.ClearTrail ();
					currentTrailRenderPower = 0;
				}
			}
		}
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (gamePlay !=null) {
			if (stream.isWriting) {

				if (!gamePlay.isMyTurn) {
					
					transform.localPosition = Vector3.down * GameStateBehaviour.OBJECT_DEFAULT_POSITION;
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

				} else if (isBall) {
					
					Vector3 localThrowDirection = Vector3.zero;
					float isThrowing = (gamePlay != null && 
					                    GameStateBehaviour.States.RenderTrail == (GameStateBehaviour.States)gamePlay.GetState()) ? 1f : 0f;

					if (gamePlay != null) {
						
						localThrowDirection = gamePlay.transform.InverseTransformDirection (gamePlay.throwDirection);
					}

					stream.SendNext (isThrowing * BeerPongInput.Instance.currentPower);
					stream.SendNext (localThrowDirection);
				}

			} else if (stream.isReading) {

				nextLocalPosition = (Vector3) stream.ReceiveNext ();
				nextLocalRotation = (Quaternion) stream.ReceiveNext ();

				if (powerUpRing != null || obstacle != null) {

					Quaternion colorQuat = (Quaternion) stream.ReceiveNext ();
					nextColor = new Color (colorQuat.x, colorQuat.y, colorQuat.z, colorQuat.w);
					nextVisibility = ((float)stream.ReceiveNext ()) > 0.5f;
				
				} else if (isBall) {
				
					float throwPower = (float)stream.ReceiveNext ();
					bool isThrowing = throwPower > THROW_POWER_THRESHOLD;
					Vector3 localThrowDirection = (Vector3) stream.ReceiveNext ();

					if (isThrowing && gamePlay != null) {

						nextThrowDirection = gamePlay.transform.TransformDirection (localThrowDirection);
					}

					nextTrailRenderPower = throwPower;
				}
			}
		}
	}
}
