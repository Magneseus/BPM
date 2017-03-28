using UnityEngine;
using System.Collections;

public class Rhythm : MonoBehaviour {

	private BasicTimer songTimer;
	public float beatTime;
	public float startOffset;
	private bool upBeat = false;
	private static float damageMultiplier = 1.5f;
	private static float moveMultiplier = 1.3f;

	private static Rhythm monolith;

	public static Rhythm Instance(){
		if (monolith == null) {
			monolith = FindObjectOfType<Rhythm> ();

			if (monolith == null) {
				monolith = new Rhythm ();
			}
		}
		return monolith;
	}

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

	public bool IsOnDownBeat(){
		return !upBeat;
	}

	public float GetBeatTimer(){
		return songTimer.CurrentCount ();
	}

	public float GetDamageMultiplier(){
		return damageMultiplier;
	}

	public float GetMoveMultiplier(){
		return moveMultiplier;
	}
}
