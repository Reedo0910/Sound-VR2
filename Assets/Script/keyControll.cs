using UnityEngine;
using System.Collections;

public class keyControll : MonoBehaviour {
	public Animator Anim_door;
	public Animator Anim_fridge;
	public Animator Anim_pan;
	public Animator Anim_pot;
	public Animator Anim_fan;
	public OVRPlayerController hmd;
	// Use this for initialization
	public GameObject playerObject;
	public GameObject fridge;
	Vector3 playerPos;
	void Start () {
		//hmd.SetSkipMouseRotation(true);
	}

	/*********************
	Operation
	1 - Fridge
	2 - Pan
	3 - Pot
	4 - Fan
	0 - Door to outside
	*********************/

	void Update() {
		
		//Debug.Log (playerObject.transform.position+"::"+fridge.transform.position);
		//Debug.Log(Mathf.Abs(fridge.transform.position.z-playerObject.transform.position.z));


		if(Input.GetKeyDown(KeyCode.Alpha1)){
			Anim_fridge.Play("Fridge");
		}
		if(Input.GetKeyDown(KeyCode.Alpha2)){
			Anim_pan.Play("Pan");
		}
		if(Input.GetKeyDown(KeyCode.Alpha3)){
			Anim_pot.Play("Pot");
		}
		if(Input.GetKeyDown(KeyCode.Alpha4)){
			Anim_fan.Play("Fan");
		}
		if(Input.GetKeyDown(KeyCode.Alpha0)){
			Anim_door.Play("Door");
		}
	}


}
