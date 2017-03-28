using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessingFX : MonoBehaviour {

	public Material screenMat;

	void Awake(){
		screenMat.SetVector("_CameraAngle", transform.rotation.eulerAngles);
		screenMat.SetVector ("_ScreenSize", new Vector4(Screen.width, Screen.height, 0, 0));
	}

	void Update(){
		int upbeat;
		if (Rhythm.Instance ().IsOnUpBeat ()) {
			upbeat = 1;
		} else {
			upbeat = 0;
		}

		screenMat.SetInt ("_UpBeat", upbeat);
		screenMat.SetFloat ("_BeatTime", Rhythm.Instance ().GetBeatTimer ());
	}

	void OnRenderImage(RenderTexture src, RenderTexture dest){
		Graphics.Blit (src, dest, screenMat);
	}
}
