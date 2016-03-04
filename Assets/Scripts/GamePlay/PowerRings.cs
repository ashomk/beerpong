using UnityEngine;
using System.Collections;

public class PowerRings : MonoBehaviour {

	public delegate void HitRingEvent ();
	public event HitRingEvent OnHitRing;

	public float ringSpeed = 1.0f;

	private float ringVisiblityToggleTime = 0;

	private bool visibility = false;

	bool didBallHit
	{
		get {

			return true;
		}

	}

	public void SetVisibility(bool visibility) {

		foreach (Transform childTrans in transform) {

			childTrans.gameObject.SetActive (visibility);
		}

		GetComponent <Renderer> ().enabled = visibility;
		GetComponent <Collider> ().enabled = visibility;
	}

	void Update()
	{
		
		if (Time.time > 30.0f) {

			if (ringVisiblityToggleTime < Time.time) {

				visibility = !visibility;
				SetVisibility (visibility);
				ringVisiblityToggleTime = Time.time + Random.Range (5.0f, 10.0f);

			}

			gameObject.transform.localPosition = new Vector3 (0.5f * Mathf.Sin (Time.time * ringSpeed), GameStateBehaviour.tableLocalScale.y * 2f, 0);

		} else {

			SetVisibility (false);
		}
	}

	void OnTriggerEnter(Collider other) {

		if(OnHitRing!=null)
			OnHitRing ();
	}


	void OnTriggerExit(Collider other) {

	}

}