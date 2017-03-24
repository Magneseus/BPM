using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    private Unit selfUnit;
    private Vector3 forwardDir;
    private Plane xzPlane;
	private Animator unitAnimator;

    public float Speed;
    public float TurnSpeed;
    public float GoalTolerance = 0.1f;
	public float AnimationStartMoveSpeed = 0.15f;
	public float AnimationStopMoveSpeed = 0.15f;

    public Vector3 TargetLocation = new Vector3(float.MaxValue, 0, 0);


    // Use this for initialization
    void Start ()
    {
        selfUnit = this.GetComponent<Unit>();
        forwardDir = this.transform.forward;
		unitAnimator = this.GetComponent<Animator> ();
    }

    // Tell the unit to move to a specific location
    public bool MoveCommand(Transform moveTo)
    {
        return MoveCommand(moveTo.position);
    }

    // Tell the unit to move to a specific location
    public bool MoveCommand(Vector3 moveTo)
    {
        // Maybe do some checks to see if it is reachable territory
        // if moveTo is not reachable, return false

        // If we are a Unit
        if (selfUnit != null)
        {
            TargetLocation = moveTo;
            return true;
        }

        return false;
    }

    // Stops the movement and removes target
    public void Stop()
    {
        TargetLocation = new Vector3(float.MaxValue, 0, 0);
    }

    // Update is called once per frame
    void FixedUpdate ()
    {

        // If we are a unit and we have a target to move to
		if (selfUnit != null && TargetLocation.x != float.MaxValue) {
			// TODO: Change this so that the vectors aren't projected onto the
			//       plane, so that we can have vertical movement as well

			// Get the new forward dir
			forwardDir = this.transform.forward;

			// Calculate move vector and project onto the movement plane
			Vector3 dist = TargetLocation - this.transform.position;

			// TODO: Make movement a non-2d operation
			dist.y = 0.0f;

			Vector3 moveDir = Vector3.ProjectOnPlane (dist, Vector3.up).normalized;

			// Get the current forward vector and project on the movement plane
			Vector3 curDir = Vector3.ProjectOnPlane (forwardDir, Vector3.up).normalized;


			// TODO: Add proper path finding


			// Rotate towards the correct orientation
			float totalAng = Vector3.Angle (moveDir, curDir);
			float linearTurn = Mathf.Clamp (
				                            (TurnSpeed * Time.deltaTime) / totalAng, 
				                            0.0f, 
				                            1.0f);

			// Lerp between the two vectors
			Vector3 newDir = 
				Vector3.Slerp (curDir, moveDir, linearTurn).normalized;


			// Set the new orientation
			Quaternion newOrient = Quaternion.LookRotation (newDir, Vector3.up);


			// Generate the new position
			Vector3 newPos = this.transform.position +
			                          (newDir * Speed * Time.deltaTime);


			// End goal
			if (dist.sqrMagnitude < GoalTolerance * GoalTolerance) {
				Stop ();
			}


			// Set the new values
			this.transform.position = newPos;
			this.transform.rotation = newOrient;

			// Update Animator Values
			unitAnimator.SetFloat ("Speed", Speed);
		} else {
			unitAnimator.SetFloat ("Speed", 0f);
		}
	}
}
