using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleDeploy : MonoBehaviour
{
    public float DeployTime = 1.0f;
    public float DamageMultiplier = 1.5f;
    public float RangeMultiplier = 2.0f;
    public float RoFMultiplier = 1.0f;

    private bool deployed;
    private float deployTimer;

    private Move moveScript;
    private float baseMoveSpeed;
    private float baseTurnSpeed;

    private Attack attackScript;
    private float baseAttackDamage;
    private float baseAttackRange;
    private float baseAttackRoF;

	// Use this for initialization
	void Start ()
    {
        // Get the move script and base values
        moveScript = this.GetComponent<Move>();
        if (moveScript != null)
        {
            baseMoveSpeed = moveScript.Speed;
            baseTurnSpeed = moveScript.TurnSpeed;
        }

        // Get the attack script and base values
        attackScript = this.GetComponent<Attack>();
        if (attackScript != null)
        {
            baseAttackDamage = attackScript.Damage;
            baseAttackRange = attackScript.Range;
            baseAttackRoF = attackScript.RateOfFire;
        }

        // Default to undeployed
        deployed = false;
        deployTimer = 0.0f;
	    gameObject.tag = "VehicleDeploy";
    }
	
	// Update is called once per frame
	void Update ()
    {
		// If we're switching
        if (deployTimer != 0.0f)
        {
            // Count down
            deployTimer = Mathf.Max(0.0f, deployTimer - Time.deltaTime);

            // If we've completed the switch
            if (deployTimer == 0.0f)
            {
                // Deployed
                if (deployed)
                {
                    attackScript.Damage = baseAttackDamage * DamageMultiplier;
                    attackScript.Range = baseAttackRange * RangeMultiplier;
                    attackScript.RateOfFire = baseAttackRoF * RoFMultiplier;

                    moveScript.Speed = 0.0f;
                    moveScript.TurnSpeed = 0.0f;
                }
                // Normal
                else
                {
                    attackScript.Damage = baseAttackDamage;
                    attackScript.Range = baseAttackRange;
                    attackScript.RateOfFire = baseAttackRoF;

                    moveScript.Speed = baseMoveSpeed;
                    moveScript.TurnSpeed = baseTurnSpeed;
                }
            }
        }
	}

    public bool ToggleDeploy()
    {
        // If we're already switching we can't switch again
        if (deployTimer != 0.0f)
            return false;

        // Switch
        deployed = !deployed;

        // Disable movement and attack while deploying/undeploying
        moveScript.Stop();
        attackScript.Stop();
        attackScript.RateOfFire = 0.0f;
        moveScript.Speed = 0.0f;
        moveScript.TurnSpeed = 0.0f;

        // Start timer
        deployTimer = DeployTime;

        return true;
    }

    // Is it currently deployed?
    public bool IsDeployed()
    {
        return deployed;
    }
}
