using UnityEngine;
using System.Collections.Generic;

public class BallMotionController : Singleton <BallMotionController> {

	public GameObject DummyTrailDirection;
	public GameObject Ball;

	public float time_limit = 0.7f;
	public float time_interval = 0.02f;
	
	//LineRenderer
	private LineRenderer lineRenderer;

	public class MotionData {
		public List <Vector3> pathLocalPositions;
		public List <Vector3> speeds;
		
		public MotionData () {
			
			pathLocalPositions = new List<Vector3> ();
			speeds = new List<Vector3> ();
		}
	};

	void Awake () {

		InitializeLineRenderer ();
		lineRenderer.enabled = false;
		//DummyTrailDirection.SetActive (false);
	}

	public MotionData GenerateMotionData (Vector3 u) {
		MotionData motiondata = new MotionData();
		Vector3 gravityVector = Physics.gravity;
		// 10 data points in each list
		for(float t = 0; t < time_limit; t += time_interval) {
			Vector3 velocity = u + gravityVector * t;
			Vector3 distance = u * t + 0.5f * gravityVector * t * t;
			motiondata.pathLocalPositions.Add (distance);
			motiondata.speeds.Add (velocity);
		}
		return motiondata;
	}
	
	public void InitializeLineRenderer () {
		
		// add lineRenderer component
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		// set material
		lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		// set color, width
		lineRenderer.SetColors(Color.yellow, Color.red);
		lineRenderer.SetWidth(0.007f, 0.007f);
	}
	
	private void RenderDummyTrail (Vector3 throwDirection) {

		if (!DummyTrailDirection.activeSelf) DummyTrailDirection.SetActive (true);
		DummyTrailDirection.transform.position = Ball.transform.position;
		DummyTrailDirection.transform.LookAt (throwDirection * 10000f);
		DummyTrailDirection.transform.position += throwDirection * DummyTrailDirection.transform.localScale.z / 2;
	}

	void DrawTrajectory (Vector3 initialVelocity, Vector3 offsetPosition) {

		if (!lineRenderer.enabled) {

			lineRenderer.enabled = true;
		}

		// retrieve lists from the other class
		MotionData motiondata2 = GenerateMotionData ( initialVelocity );
		lineRenderer.SetVertexCount (motiondata2.pathLocalPositions.Count);
		
		for (int j = 0; j < motiondata2.pathLocalPositions.Count; j ++) {
			lineRenderer.SetPosition (j, motiondata2.pathLocalPositions[j] + offsetPosition);
		}
	}

	public void RenderTrail (Vector3 initialVelocity, Vector3 offsetPosition) {

		DrawTrajectory (initialVelocity, offsetPosition);

		//TODO: Clear this once the trail renderer is available from Aaron
		//RenderDummyTrail (throwDirection);
	}

	private void ClearDummyTrail () {

		DummyTrailDirection.SetActive (false);
	}

	//TODO: Clear ball trail
	public void ClearTrail () {

		lineRenderer.enabled = false;

		//TODO: Clear this once the trail renderer is available from Aaron
		//ClearDummyTrail ();
	}
}
