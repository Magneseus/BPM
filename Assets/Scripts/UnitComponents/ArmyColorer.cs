using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ArmyColorer : MonoBehaviour {

	public GameObject rendererAttachTo;
	public Material enemyMat;

	void Awake(){
		// if owner is enemy and it has swappable textures, do so on awake
		if (GetComponentInParent<Unit> ().TeamNumber != 0 && rendererAttachTo != null) {
			rendererAttachTo.GetComponent<Renderer>().material = enemyMat;
		}
	}
}
