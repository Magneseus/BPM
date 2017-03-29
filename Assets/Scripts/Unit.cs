using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utils;
using UnityEditor;
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
    private float MaxHealth;
    public GameObject SelectionCircle;
    public UnitSpecialAction CurrentAction;
    private bool hasExploded;
    private const float EnemyDetectionRadius = 6;

    ////        SCRIPT TYPES        ////
    private Attack attackScript;
    private Move moveScript;

    //private ... ...Script;

    public enum UnitSpecialAction
    {
        None,
        MoveToBunker
    }

    // Use this for initialization
    void Start ()
    {
        //get all scripts that are available
        attackScript = GetComponent<Attack>();
        moveScript = GetComponent<Move>();

        // Get the current health as the max health
        MaxHealth = Health;

        //... = GetComponent<...>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        #region Health checks
        if (gameObject.name.Contains("Tank"))
        {
            var childGameObject = transform.FindChild("light_ranged");
            if (childGameObject != null)
            {
                var ps = childGameObject.GetComponent<ParticleSystem>();
                var main = ps.main;
                // Damaged Animations
                if (Health < MaxHealth / 2)
                {
                    main.loop = true;
                    if (!ps.isPlaying)
                        ps.Play();
                }
                else
                {
                    ps.Stop();
                }

                if (Health <= 0)
                {
                    ps.Stop();
                }
            }
        }

        // TODO: Remove this and replace with proper death checking
        if (Health <= 0.0f)
        {
            // Only tank has death animation right now
            if (gameObject.name.Contains("Tank"))
            {
                
                var ps = GetComponent<ParticleSystem>();

                if (!ps.isPlaying && !hasExploded)
                {
                    var main = ps.main;
                    main.loop = false;
                    ps.Play();
                    hasExploded = true;
                }

                if (!ps.IsAlive())
                    Destroy(gameObject);

            }
            else
            {
                Destroy(gameObject);
            }
            moveScript.Speed = 0;
            moveScript.TurnSpeed = 0;
        }
        #endregion

        #region Enemy AI
        // TODO: Assuming player's team is always 0 atm
	    if (TeamNumber > 0)
	    {
	        if (!attackScript.HasTarget())
	        {
	            var hitColliders = Physics.OverlapSphere(transform.position, EnemyDetectionRadius);
	            if (hitColliders.Any())
	            {
	                GameObject closestTarget = null;
	                float smallestDistance = 99999;
	                foreach (var targetGameObject in hitColliders)
	                {
	                    var unitComponent = targetGameObject.gameObject.GetComponent<Unit>();
	                    if (unitComponent != null && unitComponent.TeamNumber != TeamNumber)
	                    {
	                        var distanceToTarget = Vector3.Distance(targetGameObject.transform.position, gameObject.transform.position);
	                        if (closestTarget == null || distanceToTarget < smallestDistance)
	                        {
	                            closestTarget = targetGameObject.gameObject;
	                            smallestDistance = distanceToTarget;
	                        }
	                    }
	                }

                    if (closestTarget != null)
	                    attackScript.AttackTarget(closestTarget);
	            }
	        }
	    }
        #endregion
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

    public bool MoveCommand(Vector3 moveTo)
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
            case UIUtils.CommandType.StopAttack:
                if (attackScript != null && gameObject != null)
                    attackScript.Stop();
                break;
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

    // Heals a unit
    public void DoHeal(float healDealt)
    {
        Health = Mathf.Min(MaxHealth, Health + healDealt);
    }

}
