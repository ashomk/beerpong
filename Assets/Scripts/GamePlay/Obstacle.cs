using UnityEngine;
using System.Collections;

public class Obstacle : MonoBehaviour {
	
	public float obstacleFrequency = 1.0f;
	public const float DISABLE_WAIT_TIME = 1f;
	public const float HIT_WAIT_TIME = 0.75f;

	public float visiblityToggleTime = 0;
	private float visibilityChangeTime = -100f;
	
	public float lastHitTime = 0;
	private float randomActivaitionOffset;
	private float randomSpeedOffset;

	public bool visibility = false;
	private Color baseColor;
	
	public Color onHitColor = Color.yellow;
	public Color onInvisibilityColor = new Color (0, 0, 0, 0);

	public Renderer obstacleRenderer;
	public Collider obstacleCollider;
	
	public bool isMyPhotonView = false;
	private GameStateBehaviour gamePlay;
	private BeerPong beerPong;

	Shader particleShader = null;
	Shader defaultShader = null;
	
	void Awake () {

		beerPong = FindObjectOfType<BeerPong> ();
		gamePlay = FindObjectOfType<GameStateBehaviour> ();

		particleShader = Shader.Find ("Particles/Additive");
		defaultShader = obstacleRenderer.material.shader;
		baseColor = currentColor;
		currentColor = onInvisibilityColor;

		randomActivaitionOffset = Random.Range (0, 30);
		randomSpeedOffset = Random.Range (0.5f, 1f);

		StartCoroutine(ResetTransition());
	}
	
	IEnumerator ResetTransition()
	{
		yield return new WaitForSeconds(0.5f);
		var animator = gameObject.GetComponent<Animator>();
		animator.SetBool("changeTransition", false);
	}

	public void UpdateVisibility() {
		
		if (!visibility && (Time.time - visibilityChangeTime) < DISABLE_WAIT_TIME) {
			
			return;
		}
		
		foreach (Transform childTrans in transform) {
			
			childTrans.gameObject.SetActive (visibility);
		}

		if (visibility &&
			!obstacleRenderer.enabled) {
		
			// get a reference to the animator on this gameObject
			var animator = gameObject.GetComponent<Animator>();
			animator.SetBool("isHit", false);
			animator.Rebind ();

			float hue = Random.value / 3f - 1f / 6f;
			if (hue < 0)
				hue += 1f;
			baseColor = Utils.HSVToRGB (hue, 1, 1);
		}
		
		obstacleRenderer.enabled = visibility;
		obstacleCollider.enabled = visibility;
	}

	public Color currentColor {
	
		get {

			return (obstacleRenderer.material.shader == defaultShader) ? obstacleRenderer.material.color : obstacleRenderer.material.GetColor ("_TintColor");
		}

		set {

			obstacleRenderer.material.shader = value.a < 0.98f ? particleShader : defaultShader;

			if (obstacleRenderer.material.shader == defaultShader) {

				obstacleRenderer.material.color = value;
			
			} else {
			
				obstacleRenderer.material.SetColor ("_TintColor", value);
			}
		}
	}

	void Update()
	{
		if (!isMyPhotonView) {

			return;
		}

		transform.LookAt (Camera.main.transform, Vector3.up);

		if (!gamePlay.isMyTurn && isMyPhotonView) {
			
			MakeVanish ();
			UpdateVisibility ();
			return;
			
		} else if (!isMyPhotonView) {
			
			return;
		}

		UpdateVisibility ();
		
		float lastStartTime = Mathf.Max (beerPong.activationTime,
		                                 gamePlay.gameStartTime);
		if (beerPong.isActive &&
		    Time.time - lastStartTime > 60.0f + randomActivaitionOffset) {
			
			if (visiblityToggleTime < Time.time) {
				
				visibility = !visibility;
				visiblityToggleTime = Time.time + Random.Range (3.0f, 10.0f);
				visibilityChangeTime = Time.time;
			}
			
			//Wait for HIT_WAIT_TIME, if necessary
			Vector3 targetLocalPosition = (GameStateBehaviour.tableLocalScale.y + obstacleRenderer.bounds.size.y) * Vector3.up;
			Color targetColor = currentColor;
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
				
				targetLocalPosition = new Vector3 (-0.7f * Mathf.Sin (Time.time * obstacleFrequency * 1.2f * randomSpeedOffset + randomActivaitionOffset),
				                                   GameStateBehaviour.tableLocalScale.y * 2f + 0.9f * Mathf.Sin (Time.time * obstacleFrequency * 5f), 
				                                   0.4f * Mathf.Sin (Time.time * obstacleFrequency * 3f));
			} else {
				
				targetColor = onInvisibilityColor;
				colorSlerpParam = Mathf.Clamp01 ((Time.time - visibilityChangeTime) / DISABLE_WAIT_TIME);
			}
			
			float clampedDeltaTime = Mathf.Clamp01 (Time.deltaTime) * deltaSlerpFactor;
			transform.localPosition = targetLocalPosition * clampedDeltaTime + 
									  transform.localPosition * (1f - clampedDeltaTime);

			currentColor =	targetColor * colorSlerpParam + 
						 	currentColor * (1f - colorSlerpParam);

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