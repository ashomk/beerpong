using UnityEngine;
using System.Collections;

public class ProjectileShooter : MonoBehaviour {
	GameObject prefab;
	private bool didPressDown = false;
	public GameObject ball;
	Vector3 shootingDirection = Vector3.forward;
//	private double lastInterval;
	public Transform target;
	// Use this for initialization
	void Start () {
//		lastInterval = Time.realtimeSinceStartup;
	}
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetMouseButtonDown (0)) {
			didPressDown = !didPressDown;
			shootingDirection = Camera.main.transform.forward;
		}
			
		if (didPressDown) {
			ball.transform.position += shootingDirection * 0.5f * Time.deltaTime;
		}
	}
}
//		float timeNow = Time.realtimeSinceStartup;
//		countdown -= Time.deltaTime;
//		if ((timeNow - lastInterval)  >= 2.0f) {
//			ball.transform.position.x = 0;
//			ball.transform.position.z = 0;
//			lastInterval = timeNow;
//			timeNow = Time.realtimeSinceStartup;
//		} else {
//			shooting ();
//		}
//		if (Input.GetMouseButtonDown (0)) {
//						didPressDown = !didPressDown;
//						shootingDirection = Camera.main.transform.forward;
//					}
//			
//					if (didPressDown) {
//			float step=  0.5f * Time.deltaTime;
//			ball.transform.position = Vector3.MoveTowards(ball.transform.position,target.position , step);			
//		}
//				}
//		}
//	}
//	void shooting(){
//		if (Input.GetMouseButtonDown (0)) {
//			didPressDown = !didPressDown;
//			shootingDirection = Camera.main.transform.forward;
//		}
//
//		if (didPressDown) {
//			ball.transform.position += shootingDirection * 0.5f * Time.deltaTime;
//		}
//	}
//}
