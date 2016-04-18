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

	public Renderer obstacleRenderer;
	public Collider obstacleCollider;
	
	void Start () {
		
		transform.localPosition = (GameStateBehaviour.tableLocalScale.y + obstacleRenderer.bounds.size.y) * Vector3.up;
		transform.localRotation = transform.rotation;
		baseColor = obstacleRenderer.material.color;
		obstacleRenderer.material.color = onInvisibilityColor;
		randomActivaitionOffset = Random.Range (0, 30);

	}
		
	
	private void UpdateVisibility() {
		
		if (!visibility && (Time.time - visibilityChangeTime) < DISABLE_WAIT_TIME) {
			
			return;
		}
		
		foreach (Transform childTrans in transform) {
			
			childTrans.gameObject.SetActive (visibility);
		}
		
		obstacleRenderer.enabled = visibility;
		obstacleCollider.enabled = visibility;
	}
	
	void Update()
	{
		UpdateVisibility ();
		
		if (GetComponentInParent<BeerPong> ().isActive &&
		    //DOOT//Time.time - GetComponentInParent<BeerPong> ().activationTime > 60.0f + randomActivaitionOffset) {
			Time.time - GetComponentInParent<BeerPong> ().activationTime > 0) {

			if (visiblityToggleTime < Time.time) {
				
				visibility = !visibility;
				visiblityToggleTime = Time.time + Random.Range (3.0f, 10.0f);
				visibilityChangeTime = Time.time;

				if (visibility) {
					// get a reference to the animator on this gameObject
					var animator = gameObject.GetComponent<Animator>();
					animator.SetBool("isHit", false);
					animator.Rebind ();
				}
				
			}
			
			//Wait for HIT_WAIT_TIME, if necessary
			Vector3 targetLocalPosition = (GameStateBehaviour.tableLocalScale.y + obstacleRenderer.bounds.size.y) * Vector3.up;
			Color targetColor = obstacleRenderer.material.color;
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

			obstacleRenderer.material.color = targetColor * colorSlerpParam + 
				obstacleRenderer.material.color * (1f - colorSlerpParam);

			//transform.Rotate (new Vector3(1,2,3).normalized * 50 * Time.deltaTime, Space.Self);
			
			
		} else {
			
			visibility = false;
		}
	}

	public void MakeVanish(){

		visibility = false;
		visiblityToggleTime = Time.time + Random.Range (3.0f, 10.0f);
		visibilityChangeTime = Time.time;

	}
}