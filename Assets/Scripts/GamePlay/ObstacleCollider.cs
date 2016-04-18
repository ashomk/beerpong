using UnityEngine;
using System.Collections;

public class ObstacleCollider : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter(Collision collision) {
		Debug.Log ("HIT THE DEMON!");

		// get a reference to the animator on this gameObject
		var animator = gameObject.transform.parent.gameObject.GetComponent<Animator>();

		//animator.SetTrigger("tossTrigger");
		animator.SetBool("isHit", true);

	}
}
