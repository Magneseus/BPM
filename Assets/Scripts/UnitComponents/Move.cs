using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    private Unit selfUnit;
    private Vector3 forwardDir;
    private Plane xzPlane;

    public float Speed;
    public float TurnSpeed;
    public float GoalTolerance;

    public Transform TargetLocation;


    // Use this for initialization
    void Start ()
    {
        selfUnit = this.GetComponent<Unit>();
        forwardDir = this.transform.forward;
    }

    // Tell the unit to move to a specific location
    public bool MoveCommand(Transform moveTo)
    {
        // Maybe do some checks to see if it is reachable territory
        // if moveTo is not reachable, return false

        // If we are a Unit
        if (selfUnit != null)
        {
            TargetLocation = moveTo;
        }

        return false;
    }

    // Update is called once per frame
    void Update ()
    {
        // If we are a unit and we have a target to move to
		if (selfUnit != null && TargetLocation != null)
        {
            // TODO: Change this so that the vectors aren't projected onto the
            //       plane, so that we can have vertical movement as well

            // Get the new forward dir
            forwardDir = this.transform.forward;

            // Calculate move vector and project onto the movement plane
            Vector3 dist = TargetLocation.position - this.transform.position;
            Vector3 moveDir = Vector3.ProjectOnPlane(dist, Vector3.up).normalized;

            // Get the current forward vector and project on the movement plane
            Vector3 curDir = Vector3.ProjectOnPlane(forwardDir, Vector3.up).normalized;


            // TODO: Add proper path finding


            // Rotate towards the correct orientation
            float totalAng = Vector3.Angle(moveDir, curDir);
            float linearTurn = Mathf.Clamp(
                (TurnSpeed * Time.deltaTime) / totalAng, 
                0.0f, 
                1.0f);

            // Lerp between the two vectors
            Vector3 newDir = 
                Vector3.Slerp(curDir, moveDir, linearTurn).normalized;


            // Set the new orientation
            Quaternion newOrient = Quaternion.LookRotation(newDir, Vector3.up);


            // Generate the new position
            Vector3 newPos = this.transform.position + 
                (newDir * Speed * Time.deltaTime);


            this.transform.position = newPos;
            this.transform.rotation = newOrient;


            // End goal
            if (dist.sqrMagnitude < GoalTolerance * GoalTolerance)
            {
                TargetLocation = null;
            }
        }
	}
}
