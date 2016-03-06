﻿using UnityEngine;
using System.Collections;

public class PowerUpRing : MonoBehaviour {

	public delegate void HitRingEvent (PowerUpRing ring);
	public event HitRingEvent OnHitRing;

	public GameObject hoop;

	public float ringFrequency = 1.0f;
	public const float HIT_WAIT_TIME = 1.0f;
	public const float DISABLE_WAIT_TIME = 0.5f;

	private float visiblityToggleTime = 0;
	private float visibilityChangeTime = 0;

	public float lastHitTime = 0;

	private bool visibility = false;
	private Color baseColor;

	public Color onHitColor = Color.yellow;
	public Color onInvisibilityColor = new Color (0, 0, 0, 0);

	void Start () {

		transform.localPosition = (GameStateBehaviour.tableLocalScale.y + hoop.GetComponent<Renderer> ().bounds.size.y) * Vector3.up;
		transform.localRotation = transform.rotation;
		baseColor = hoop.GetComponent<Renderer> ().material.color;
		hoop.GetComponent<Renderer> ().material.color = onInvisibilityColor;
	}

	private void UpdateVisibility() {

		if (!visibility && (Time.time - visibilityChangeTime) < DISABLE_WAIT_TIME) {

			return;
		}

		foreach (Transform childTrans in transform) {

			childTrans.gameObject.SetActive (visibility);
		}

		GetComponent <Renderer> ().enabled = visibility;
		GetComponent <Collider> ().enabled = visibility;
	}

	void Update()
	{
		UpdateVisibility ();

		if (GetComponentInParent<BeerPong> ().isActive &&
		    Time.time - GetComponentInParent<BeerPong> ().activationTime > 30.0f) {

			if (visiblityToggleTime < Time.time) {

				visibility = !visibility;
				visiblityToggleTime = Time.time + Random.Range (5.0f, 15.0f);
				visibilityChangeTime = Time.time;

			}

			//Wait for HIT_WAIT_TIME, if necessary
			Vector3 targetLocalPosition = (GameStateBehaviour.tableLocalScale.y + hoop.GetComponent<Renderer> ().bounds.size.y) * Vector3.up;
			Color targetColor = hoop.GetComponent<Renderer> ().material.color;
			float colorSlerpParam = Time.deltaTime;
			float deltaSlerpFactor = 1f;
			if (visibility) {

				deltaSlerpFactor = 1f;
				targetColor = baseColor;
				colorSlerpParam = Mathf.Clamp01 ((Time.time - visibilityChangeTime) / DISABLE_WAIT_TIME);

				if (Time.time - lastHitTime <= HIT_WAIT_TIME) {
				
					colorSlerpParam = Time.deltaTime * 30;
					float colorChange = Mathf.Sin (Time.time * 30);
					targetColor = baseColor * colorChange + onHitColor * (1f - colorChange);

					deltaSlerpFactor = 0.1f;
				}

				targetLocalPosition = new Vector3 (0.7f * Mathf.Sin (Time.time * ringFrequency), 
				                                   GameStateBehaviour.tableLocalScale.y * 2f, 
				                                   0);
			} else {
			
				targetColor = onInvisibilityColor;
				colorSlerpParam = Mathf.Clamp01 ((Time.time - visibilityChangeTime) / DISABLE_WAIT_TIME);
			}

			float clampedDeltaTime = Mathf.Clamp01 (Time.deltaTime) * deltaSlerpFactor;
			transform.localPosition =   targetLocalPosition * clampedDeltaTime + 
										transform.localPosition * (1f - clampedDeltaTime);
			hoop.GetComponent<Renderer> ().material.color = targetColor * colorSlerpParam + 
															hoop.GetComponent<Renderer> ().material.color * (1f - colorSlerpParam);


		} else {

			visibility = false;
		}
	}

	void OnTriggerEnter(Collider other) {

		if(OnHitRing!=null)
			OnHitRing (this);

		lastHitTime = Time.time;
	}


	void OnTriggerExit(Collider other) {

	}

}