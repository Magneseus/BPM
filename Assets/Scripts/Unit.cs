using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utils;
//using UnityEditor;
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

    private Vector3 TargetToLookAt;

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

        TargetToLookAt = new Vector3(float.MaxValue, 0, 0);
	}
	
	// Update is called once per frame
	void Update ()
    {
        #region Attack Moving

	    if (moveScript != null && moveScript.GetIsAttackMove())
	    {
	        LockOnClosestEnemyUnit();
	    }
        #endregion

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
            LockOnClosestEnemyUnit();
        }
        #endregion
    }

    void FixedUpdate()
    {
        if (TargetToLookAt.x != float.MaxValue)
        {
            TurnUnit();
            TargetToLookAt.x = float.MaxValue;
        }
    }

    // Tell the unit to move to a specific location
    public bool MoveCommand(Transform moveTo, bool isAttackMove = false)
    {
        // If we don't have a move script then return false
        if (moveScript == null)
            return false;

        // Otherwise try and move
        return moveScript.MoveCommand(moveTo, isAttackMove);
    }

    public bool MoveCommand(Vector3 moveTo, bool isAttackMove = false)
    {
        // If we don't have a move script then return false
        if (moveScript == null)
            return false;

        // Otherwise try and move
        return moveScript.MoveCommand(moveTo, isAttackMove);
    }

    // Stop movement
    public void MoveStop()
    {
        // If we don't have a move script then do nothing
        if (moveScript == null)
            return;

        // Otherwise, stop movement
        moveScript.Stop();
    }

    // Are we moving?
    public bool IsMoveStopped()
    {
        // If we don't have a move script then return true, we aren't moving
        if (moveScript == null)
            return true;

        // Otherwise check if we're stopped
        return moveScript.IsStopped();
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
                {
                    moveScript.StopAttackMove();
                    return attackScript.AttackTarget(go);
                }
                else
                    return false;
            case UIUtils.CommandType.StopAttack:
                if (attackScript != null && gameObject != null)
                {
                    attackScript.Stop();
                    moveScript.StopAttackMove();
                }
                else
                    return false;
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

    private void LockOnClosestEnemyUnit()
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


    public void TurnToLookAt(Vector3 _TargetToLookAt)
    {
        TargetToLookAt = _TargetToLookAt;
    }

    // Turns a unit
    private void TurnUnit()
    {
        // Get the new forward dir
        Vector3 forwardDir = this.transform.forward;

        // Calculate move vector and project onto the movement plane
        Vector3 dist = TargetToLookAt - this.transform.position;

        // TODO: Make movement a non-2d operation
        dist.y = 0.0f;

        Vector3 moveDir = Vector3.ProjectOnPlane(dist, Vector3.up).normalized;

        // Get the current forward vector and project on the movement plane
        Vector3 curDir = Vector3.ProjectOnPlane(forwardDir, Vector3.up).normalized;


        float TurnSpeed = 1000.0f;
        if (moveScript != null)
            TurnSpeed = moveScript.TurnSpeed;

        // Rotate towards the correct orientation
        float totalAng = Vector3.Angle(dist, forwardDir);
        float turnspd = TurnSpeed * Time.deltaTime;
        if (GetComponentInParent<Unit>().TeamNumber == 0 && Rhythm.Instance().IsOnDownBeat())
        {
            turnspd *= Rhythm.Instance().GetMoveMultiplier();
        }
        float linearTurn = Mathf.Clamp(
                                        turnspd / totalAng,
                                        0.0f,
                                        1.0f);

        // Lerp between the two vectors
        Vector3 newDir =
            Vector3.Slerp(forwardDir, dist, linearTurn).normalized;


        // Set the new orientation
        Quaternion newOrient = Quaternion.LookRotation(newDir, Vector3.up);
        this.transform.rotation = newOrient;
    }

}
