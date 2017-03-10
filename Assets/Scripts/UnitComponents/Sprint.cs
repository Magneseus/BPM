using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sprint : MonoBehaviour
{
    public float SpeedMultiplier = 1.5f;
    public float TurnSpeedMultiplier = 0.75f;
    public float Length = 1.0f;
    public float Cooldown = 5.0f;

    private bool Active = false;
    private float sprintTimer;
    private float cooldownTimer;
    private Move moveScript;

    // Use these to return to normal after sprint is finished
    private float baseMoveSpeed;
    private float baseTurnSpeed;

	// Use this for initialization
	void Start ()
    {
        moveScript = this.GetComponent<Move>();

        // If we do have an attached Move script, get the default values
        if (moveScript != null)
        {
            baseMoveSpeed = moveScript.Speed;
            baseTurnSpeed = moveScript.TurnSpeed;
        }

        sprintTimer = 0.0f;
        cooldownTimer = 0.0f;
	}
	
	// Update is called once per frame
	void Update ()
    {
        // Sprinting
		if (Active)
        {
            // Ability timer
            sprintTimer = Mathf.Max(0.0f, sprintTimer - Time.deltaTime);

            // Sprint finished
            if (sprintTimer == 0.0f)
            {
                // Return Move speeds to normal
                moveScript.Speed = baseMoveSpeed;
                moveScript.TurnSpeed = baseTurnSpeed;

                // Disable ability and activate cooldown
                Active = false;
                cooldownTimer = Cooldown;
            }
        }

        // Cooldown
        if (cooldownTimer > 0.0f)
        {
            // Reduce the cooldown timer to 0
            cooldownTimer = Mathf.Max(0.0f, cooldownTimer - Time.deltaTime);
        }
	}

    public bool ActivateSprint()
    {
        // If we cannot activate the sprint
        if (Active || cooldownTimer != 0.0f)
            return false;

        // Change move speed values
        moveScript.Speed = moveScript.Speed * SpeedMultiplier;
        moveScript.TurnSpeed = moveScript.TurnSpeed * TurnSpeedMultiplier;

        // Activate ability and start timer
        Active = true;
        sprintTimer = Length;

        return true;
    }

    public bool IsSprintActive()
    {
        return Active;
    }
}
