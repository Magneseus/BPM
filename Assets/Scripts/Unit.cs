using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Utils;
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

    public int TeamNumber;
    public float Health;
    public GameObject SelectionCircle;


    ////        SCRIPT TYPES        ////
    private Attack attackScript;
    private Move moveScript;

    //private ... ...Script;

    // Use this for initialization
    void Start ()
    {
        //get all scripts that are available
        attackScript = GetComponent<Attack>();
        moveScript = GetComponent<Move>();

        //... = GetComponent<...>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        // TODO: Remove this and replace with proper death checking
        if (Health <= 0.0f)
        {
            Destroy(this.gameObject);
        }

    }

    // Tell the unit to move to a specific location
    public bool MoveCommand(Transform moveTo)
    {
        // If we don't have a move script then return false
        if (moveScript == null)
            return false;

        // Otherwise try and move
        return moveScript.MoveCommand(moveTo);
    }

    // Give this unit a command (eg. "attack", "ability1", etc)
    // Need to also give it either a game object OR a transform for the command
    public bool GiveCommand(UIUtils.CommandType command, GameObject go, Transform trans)
    {
        // If given neither GameObject or Transform, return false
        if (go == null && trans == null)
            return false;

        switch (command)
        {
            // Attack needs a GameObject
            case UIUtils.CommandType.Attack:
                if (attackScript != null && gameObject != null)
                    return attackScript.AttackTarget(go);
                else
                    return false;
            //case "coolability1":
            //    
            //    break;
        }

        return false;
    }

    // Deals damage to a Unit
    public void DoDamage(float damageDealt)
    {
        Health = Mathf.Max(0.0f, Health - damageDealt);

        // TODO: Check for death and proceed accordingly?
    }
}
