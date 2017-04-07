﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Move : MonoBehaviour
{
    private Unit selfUnit;
    private Vector3 forwardDir;
    private Plane xzPlane;
	private Animator unitAnimator;
    private bool IsAttackMove;
    private NavMeshAgent unitNav;

    public float Acceleration;
    public float Speed;
    public float TurnSpeed;
    public float GoalTolerance = 0.1f;
	public float AnimationStartMoveSpeed = 0.15f;
	public float AnimationStopMoveSpeed = 0.15f;

    public Vector3 TargetLocation = new Vector3(float.MaxValue, 0, 0);
    private NavMeshPath TargetPath;


    // Use this for initialization
    void Start ()
    {
        selfUnit = this.GetComponent<Unit>();
        forwardDir = this.transform.forward;
		unitAnimator = this.GetComponent<Animator> ();
        unitNav = this.GetComponent<NavMeshAgent>();
        TargetPath = null;

        // Tell the unit nav to not move on it's own
        unitNav.updatePosition = false;

        // Set the unit nav movement vars
        unitNav.speed = Speed;
        unitNav.acceleration = Acceleration;
        unitNav.angularSpeed = TurnSpeed;
        unitNav.stoppingDistance = GoalTolerance;
    }

    // Tell the unit to move to a specific location
    public bool MoveCommand(Transform moveTo, bool isAttackMove)
    {
        return MoveCommand(moveTo.position, isAttackMove);
    }

    // Tell the unit to move to a specific location
    public bool MoveCommand(Vector3 moveTo, bool isAttackMove)
    {
        // Check the NavMesh to see if it's a valid position
        NavMeshHit nvh;
        if (NavMesh.SamplePosition(moveTo, out nvh, 1.0f, NavMesh.AllAreas))
        {
            // New Path
            NavMeshPath newPath = new NavMeshPath();

            // Generate a path, if one exists
            if (unitNav.CalculatePath(nvh.position, newPath))
            {
                if (newPath.status == NavMeshPathStatus.PathComplete)
                {
                    // Something?
                    IsAttackMove = isAttackMove;

                    // Set the goal
                    TargetLocation = moveTo;

                    // Set the path
                    TargetPath = newPath;
                    unitNav.SetPath(TargetPath);

                    return true;
                }
            }
        }

        return false;
    }

    // Stops the movement and removes target
    public void Stop()
    {
        TargetLocation = new Vector3(float.MaxValue, 0, 0);
        if (unitNav != null)
            unitNav.ResetPath();
        TargetPath = null;
    }

    // Are we moving?
    public bool IsStopped()
    {
        if (!unitNav.pathPending)
        {
            if (unitNav.remainingDistance <= unitNav.stoppingDistance)
            {
                if (!unitNav.hasPath || unitNav.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }

        return TargetLocation.x == float.MaxValue || TargetPath == null;
    }

    #region PurePathfindingMovement
    void UpdateOld()
    {
        float turnspd = TurnSpeed;
        float spd = Speed;

        // Update speeds based on rhythm
        if (GetComponentInParent<Unit>().TeamNumber == 0 && Rhythm.Instance().IsOnDownBeat())
        {
            turnspd *= Rhythm.Instance().GetMoveMultiplier();
        }
        if (GetComponentInParent<Unit>().TeamNumber == 0 && Rhythm.Instance().IsOnDownBeat())
        {
            spd *= Rhythm.Instance().GetMoveMultiplier();
        }

        unitNav.speed = spd;
        unitNav.angularSpeed = turnspd;

        // Update Animator Values
        if (IsStopped())
        {
            unitAnimator.SetFloat("Speed", 0);
        }
        else
        {
            unitAnimator.SetFloat("Speed", spd);
        }
    }
    #endregion

    #region OldMovement
    // Update is called once per frame
    void FixedUpdate ()
    {
        // If we are a unit and we have a target to move to
		if (selfUnit != null && !this.IsStopped())
        {
            // Get the position we need to go to
            Vector3 targ = unitNav.steeringTarget;

			// Get the new forward dir
			forwardDir = this.transform.forward;

			// Calculate move vector
			Vector3 dist = TargetLocation - this.transform.position;
            Vector3 moveDir = (targ - this.transform.position).normalized;

			// Get the current forward vector 
			Vector3 curDir = forwardDir.normalized;


			// Rotate towards the correct orientation
			float totalAng = Vector3.Angle (moveDir, curDir);
			float turnspd = TurnSpeed * Time.deltaTime;
			if (GetComponentInParent<Unit> ().TeamNumber == 0 && Rhythm.Instance ().IsOnDownBeat ()) {
				turnspd *= Rhythm.Instance ().GetMoveMultiplier ();
			}
			float linearTurn = Mathf.Clamp (
											turnspd/ totalAng, 
				                            0.0f, 
				                            1.0f);

			// Lerp between the two vectors
			Vector3 newDir = 
				Vector3.Slerp (curDir, moveDir, linearTurn).normalized;


			// Set the new orientation
			Quaternion newOrient = Quaternion.LookRotation (newDir, Vector3.up);


			// Check if multiplier from music
			float spd = Speed;
			if (GetComponentInParent<Unit> ().TeamNumber == 0 && Rhythm.Instance ().IsOnDownBeat ()) {
				spd *= Rhythm.Instance ().GetMoveMultiplier ();
			}

			// Generate the new position
			Vector3 newPos = this.transform.position +
											(newDir * spd * Time.deltaTime);


			// End goal
			if (dist.sqrMagnitude < GoalTolerance * GoalTolerance)
            {
				Stop ();
			}


			// Set the new values
			this.transform.position = newPos;
            unitNav.nextPosition = newPos;

            //this.transform.rotation = newOrient;

			// Update Animator Values
			unitAnimator.SetFloat ("Speed", spd);
		} else {
			unitAnimator.SetFloat ("Speed", 0f);
		}
	}

    #endregion
    public void StopAttackMove()
    {
        IsAttackMove = false;
    }

    public bool GetIsAttackMove()
    {
        return IsAttackMove;
    }
}
