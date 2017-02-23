using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Unit - An abstract representation of some unit type (building, infantry, etc)
 * 
 */
public class Unit : MonoBehaviour
{
    // Maybe we want some sort of collider like a sphere collider here?
    // Something to use to check for simple collisions/selections
    //
    // If so, add it as a component to the prefab and set a reference in Start
    // eg.  SphereCollider unitSphere; <--------------------- HERE
    //      unitSphere = GetComponent<SphereCollider>(); <--- START()

	// Use this for initialization
	void Start ()
    {

	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    // Tell the unit to move to a specific location
    // TO BE OVERRIDDEN IN THE GIVEN UNIT
    public virtual void MoveCommand(Transform moveTo) {}

    // Give this unit a command (eg. "attack", "ability1", etc)
    // TO BE OVERRIDDEN IN THE GIVEN UNIT
    public virtual void GiveCommand(string commandName) {}
}
