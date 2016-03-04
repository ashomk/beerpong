using UnityEngine;
using System.Collections;

public class BeerPongCup : MonoBehaviour {
	
	public int cupNumber {
		
		get;
		set;
	}
	
	public GameObject ball ;
	public delegate void HitCupEvent (int cupID);
	
	public event HitCupEvent OnHit;
	
	bool didBallHit
	{
		get {
			Bounds cupBounds = gameObject.GetComponentInChildren<Renderer> ().bounds;
			Vector3 cupTop = new Vector3 (cupBounds.center.x, cupBounds.max.y, cupBounds.center.z);
			Vector3 ballPosition = ball.transform.position;
			float ballRadius = ball.GetComponent<Renderer> ().bounds.extents.x;
			float cupRadius = cupBounds.extents.x;
			
			Vector3 cupTopView = new Vector3 (cupTop.x, 0, cupTop.z); 
			Vector3 ballTopView = new Vector3 (ballPosition.x, 0, ballPosition.z); 
			
			if (cupTop.y > ballPosition.y &&
			    ((cupTopView - ballTopView).magnitude < cupRadius - ballRadius)) {
				
				return true;
			}
			
			return false;
			
		}
		
	}

	public Vector3 top {
	
		get {

			Bounds cupBounds = gameObject.GetComponentInChildren<Renderer> ().bounds;
			return new Vector3 (cupBounds.center.x, cupBounds.max.y, cupBounds.center.z);
		}
	}
	
	void Update()
	{
		if (didBallHit && OnHit!=null) {
			
			OnHit (cupNumber);
		}
	}
}
