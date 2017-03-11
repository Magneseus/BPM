using UnityEngine;
using System.Collections;

public class Rhythm : MonoBehaviour {

	private BasicTimer songTimer;
	public float beatTime;
	public float startOffset;
	private bool upBeat = false;

	void Awake(){
		songTimer = BasicTimer.SetupTimer (gameObject, beatTime, true, true, startOffset);
	}

	void Update(){
		if (songTimer.HasCycled ()) {
			upBeat = !upBeat;
		}
		//Debug.Log ("upBeat = " + upBeat);
	}

	public bool IsOnUpBeat(){
		return upBeat;
	}
}
