using UnityEngine;
using System.Collections.Generic;

public class BallMotionController : Singleton <BallMotionController> {

	public GameObject Ball;

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
	}

	private MotionData GenerateGravitationalMotionData (Vector3 u, float targetY) {

		MotionData motiondata = new MotionData();
		Vector3 gravityVector = Physics.gravity;

		//Target is at a lower height
		targetY *= -1;

		float discriminant = Mathf.Sqrt (u.y * u.y + 2f * gravityVector.y * targetY);
		float timeLimit = (-u.y - discriminant) / gravityVector.y;

		for(float t = 0; t < timeLimit; t += time_interval) {
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
	
	void DrawTrajectory (Vector3 initialVelocity, Vector3 offsetPosition, float targetY) {

		if (!lineRenderer.enabled) {

			lineRenderer.enabled = true;
		}

		// retrieve lists from the other class
		MotionData motiondata2 = GenerateGravitationalMotionData (initialVelocity, targetY);
		lineRenderer.SetVertexCount (motiondata2.pathLocalPositions.Count);
		
		for (int j = 0; j < motiondata2.pathLocalPositions.Count; j ++) {
			lineRenderer.SetPosition (j, motiondata2.pathLocalPositions[j] + offsetPosition);
		}
	}

	public void RenderTrail (Vector3 initialVelocity, Vector3 offsetPosition, float targetY) {

		DrawTrajectory (initialVelocity, offsetPosition, targetY);

		//TODO: Clear this once the trail renderer is available from Aaron
		//RenderDummyTrail (throwDirection);
	}

	public void ClearTrail () {

		lineRenderer.enabled = false;
	}
}
