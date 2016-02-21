using UnityEngine;
using System.Collections;

public class BallMotionController : Singleton <BallMotionController> {

	public GameObject DummyTrailDirection;
	public GameObject Ball;

	void Awake () {

		DummyTrailDirection.SetActive (false);
	}

	private void RenderDummyTrail (Vector3 throwDirection) {

		if (!DummyTrailDirection.activeSelf) DummyTrailDirection.SetActive (true);
		DummyTrailDirection.transform.position = Ball.transform.position;
		DummyTrailDirection.transform.LookAt (throwDirection * 10000f);
		DummyTrailDirection.transform.position += throwDirection * DummyTrailDirection.transform.localScale.z / 2;
	}

	public void RenderTrail (float power, Vector3 throwDirection) {

		//TODO: Clear this once the trail renderer is available from Aaron
		RenderDummyTrail (throwDirection);

		//TODO: This method must render the trail of the ball based on power parameter, which ranges from 0 to 1
	}

	private void ClearDummyTrail () {

		DummyTrailDirection.SetActive (false);
	}

	//TODO: Clear ball trail
	public void ClearTrail () {

		//TODO: Clear this once the trail renderer is available from Aaron
		ClearDummyTrail ();
	}
}
