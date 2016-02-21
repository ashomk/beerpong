using UnityEngine;
using System.Collections;

public class DifficultyMeter : Singleton <DifficultyMeter> {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public float Drunkenness {

		get;
		private set;
	}

	//TODO: Implement this method to show an empty beerbottle
	public void Clear () {
	}
}
