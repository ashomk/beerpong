using UnityEngine;
using System.Collections;

public class PowerUpRing : MonoBehaviour {

	public delegate void HitRingEvent (PowerUpRing ring);
	public event HitRingEvent OnHitRing;

	public GameObject hoop;

	public float ringFrequency = 1.0f;
	public const float HIT_WAIT_TIME = 1.0f;
	public const float DISABLE_WAIT_TIME = 0.5f;

	public enum Type
	{
		ROCKET,
		SHOTGUN
	}

	public Type ringType = Type.ROCKET;

	private float visiblityToggleTime = 0;
	private float visibilityChangeTime = 0;

	public float lastHitTime = 0;

	public bool visibility = false;
	public Color rocketRingColor = Color.green;
	public Color shotGunRingColor = Color.red;

	private Renderer hoopRenderer;
	private Renderer thisRenderer;
	private Collider thisCollider;

	private Color baseColor {
		
		get {
			
			return (ringType == Type.ROCKET) ? rocketRingColor : shotGunRingColor;
		}
	}
	
	public Color onHitColor = Color.yellow;
	public Color onInvisibilityColor = new Color (0, 0, 0, 0);

	private float offSetTime = 20;

	public bool isMyPhotonView = false;
	private GameStateBehaviour gamePlay;
	private BeerPong beerPong;
	
	void Start () {
		
		beerPong = FindObjectOfType<BeerPong> ();
		gamePlay = FindObjectOfType<GameStateBehaviour> ();
		hoopRenderer = hoop.GetComponent<Renderer> ();
		thisRenderer = GetComponent <Renderer> ();
		thisCollider = GetComponent <Collider> ();
		hoopRenderer.material.color = onInvisibilityColor;
		offSetTime += Random.Range (0, 20f);
	}

	public void UpdateVisibility() {

		if (!visibility && (Time.time - visibilityChangeTime) < DISABLE_WAIT_TIME) {

			return;
		}

		foreach (Transform childTrans in transform) {

			childTrans.gameObject.SetActive (visibility);
		}

		thisRenderer.enabled = visibility;
		thisCollider.enabled = visibility;
	}

	public Color currentColor {
		
		get {
			
			return hoopRenderer.material.color;
		}
	}
	
	public void UpdateColor (Color color) {

		hoopRenderer.material.color = color;

	}

	void Update()
	{
		if (!isMyPhotonView) {

			return;
		}

		transform.localRotation = Quaternion.Euler (0, 90, 90);

		if (!gamePlay.isMyTurn) {

			visibility = false;
			UpdateVisibility ();
			return;
		}

		UpdateVisibility ();

		float lastStartTime = Mathf.Max (beerPong.activationTime,
		                                 gamePlay.gameStartTime);
		if (beerPong.isActive &&
		    Time.time - lastStartTime > offSetTime) {

			if (visiblityToggleTime < Time.time) {

				visibility = !visibility;
				visiblityToggleTime = Time.time + Random.Range (5.0f, 15.0f);
				visibilityChangeTime = Time.time;

			}

			//Wait for HIT_WAIT_TIME, if necessary
			Vector3 targetLocalPosition = (GameStateBehaviour.tableLocalScale.y + hoopRenderer.bounds.size.y) * Vector3.up;
			Color targetColor = hoopRenderer.material.color;
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

				targetLocalPosition = new Vector3 (0.7f * Mathf.Sin (Time.time * ringFrequency + offSetTime), 
				                                   GameStateBehaviour.tableLocalScale.y * 2f, 
				                                   0);

			} else {
			
				targetColor = onInvisibilityColor;
				colorSlerpParam = Mathf.Clamp01 ((Time.time - visibilityChangeTime) / DISABLE_WAIT_TIME);
			}

			float clampedDeltaTime = Mathf.Clamp01 (Time.deltaTime) * deltaSlerpFactor;
			transform.localPosition =   targetLocalPosition * clampedDeltaTime + 
										transform.localPosition * (1f - clampedDeltaTime);
			UpdateColor (targetColor * colorSlerpParam + 
			             hoopRenderer.material.color * (1f - colorSlerpParam));


		} else {

			visibility = false;
		}
	}

	void OnTriggerEnter(Collider other) {

		if(OnHitRing!=null)
			OnHitRing (this);

		lastHitTime = Time.time;
	}
}