using UnityEngine;
using System.Collections;

public class Obstacle : MonoBehaviour {
	
	public float obstacleFrequency = 1.0f;
	public const float DISABLE_WAIT_TIME = 1f;
	public const float HIT_WAIT_TIME = 0.75f;
	
	private float visiblityToggleTime = 0;
	private float visibilityChangeTime = 0;
	
	public float lastHitTime = 0;
	private float randomActivaitionOffset;
	
	private bool visibility = false;
	private Color baseColor;
	
	public Color onHitColor = Color.yellow;
	public Color onInvisibilityColor = new Color (0, 0, 0, 0);
	
	void Start () {
		
		transform.localPosition = (GameStateBehaviour.tableLocalScale.y + GetComponent<Renderer> ().bounds.size.y) * Vector3.up;
		transform.localRotation = transform.rotation;
		baseColor = GetComponent<Renderer> ().material.color;
		GetComponent<Renderer> ().material.color = onInvisibilityColor;
		randomActivaitionOffset = Random.Range (0, 30);
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
		    Time.time - GetComponentInParent<BeerPong> ().activationTime > 60.0f + randomActivaitionOffset) {
			
			if (visiblityToggleTime < Time.time) {
				
				visibility = !visibility;
				visiblityToggleTime = Time.time + Random.Range (3.0f, 10.0f);
				visibilityChangeTime = Time.time;
				
			}
			
			//Wait for HIT_WAIT_TIME, if necessary
			Vector3 targetLocalPosition = (GameStateBehaviour.tableLocalScale.y + GetComponent<Renderer> ().bounds.size.y) * Vector3.up;
			Color targetColor = GetComponent<Renderer> ().material.color;
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
				
				targetLocalPosition = new Vector3 (-0.7f * Mathf.Sin (Time.time * obstacleFrequency * 1.2f + randomActivaitionOffset),
				                                   GameStateBehaviour.tableLocalScale.y * 2f + 0.9f * Mathf.Sin (Time.time * obstacleFrequency * 5f), 
				                                   0.4f * Mathf.Sin (Time.time * obstacleFrequency * 3f));
			} else {
				
				targetColor = onInvisibilityColor;
				colorSlerpParam = Mathf.Clamp01 ((Time.time - visibilityChangeTime) / DISABLE_WAIT_TIME);
			}
			
			float clampedDeltaTime = Mathf.Clamp01 (Time.deltaTime) * deltaSlerpFactor;
			transform.localPosition =   targetLocalPosition * clampedDeltaTime + 
				transform.localPosition * (1f - clampedDeltaTime);

			GetComponent<Renderer> ().material.color = targetColor * colorSlerpParam + 
				GetComponent<Renderer> ().material.color * (1f - colorSlerpParam);

			transform.Rotate (new Vector3(1,2,3).normalized * 50 * Time.deltaTime, Space.Self);
			
			
		} else {
			
			visibility = false;
		}
	}
}