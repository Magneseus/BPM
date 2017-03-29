using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    private Unit selfUnit;
    private int selfTeam;
    private IEnumerator attackCoroutine;
    private bool CR_Running;
	private Animator unitAnimator;

    public GameObject Target;
    private Unit TargetUnit;
    public float Range;
    public float Damage;
    public float RateOfFire;
    public bool DoChaseTarget;


    // Use this for initialization
    void Start ()
    {
        selfUnit = this.GetComponent<Unit>();
		unitAnimator = this.GetComponent<Animator> ();
        if (selfUnit != null)
            selfTeam = selfUnit.TeamNumber;

    }

    // Attacking a GameObject (must contain a Unit)
    public bool AttackTarget(GameObject _target)
    {
        // If the game object has a Unit Component, we can attack
        Unit _targetUnit = _target.GetComponent<Unit>();

        // Try to attack
        if (_targetUnit != null)
            return AttackTarget(_targetUnit);

        // Otherwise, it's not a Unit
        return false;
    }

    // Attacking a Unit directly
    public bool AttackTarget(Unit _targetUnit)
    {
        // If we cannot attack this target for some reason, return false
        if (_targetUnit.TeamNumber == this.selfTeam)
        {
            Target = null;
            TargetUnit = null;
            return false;
        }

        // Set the Target and TargetUnit and start the coroutine
        Target = _targetUnit.gameObject;
        TargetUnit = _targetUnit;

        attackCoroutine = AttackMove();
        StartCoroutine(attackCoroutine);

        return false;
    }

    IEnumerator AttackMove()
    {
        CR_Running = true;

        // While the target is active 
        // AND 
        // (we can chase the target OR it's in our range)
        while (Target != null &&
            (DoChaseTarget || 
            (this.transform.position -Target.transform.position).sqrMagnitude
                <= Range))
        {
            ///////               MOVE               ///////
            // If we're not in range, then move towards the unit
            Vector3 dist = Target.transform.position - this.transform.position;
            if (dist.sqrMagnitude > Range * Range)
            {
				// deactivate attack animation if active
				unitAnimator.SetBool("Attacking", false);

                // If we can't chase then cancel
                if (!DoChaseTarget)
                    break;

                // If we are a unit but cannot can move to the target, cancel
                if (selfUnit != null && !selfUnit.MoveCommand(Target.transform))
                {
                    break;
                }
            }
            ///////               ATTACK               ///////
            else
            {
                // Deal damage to the unit, modified by ryhthm if applicable
				float dmg = Damage;
				if (GetComponentInParent<Unit> ().TeamNumber == 0 && Rhythm.Instance ().IsOnUpBeat ()) {
					dmg *= Rhythm.Instance ().GetDamageMultiplier ();
				}

				TargetUnit.DoDamage(dmg);

                // trigger attack animation
				unitAnimator.SetBool("Attacking", true);
            }


            // Wait for our RoF to reset
            yield return new WaitForSeconds(RateOfFire);
        }

        // We're done attacking
        Target = null;
        TargetUnit = null;
        CR_Running = false;
    }

    // Stops attacking and removes target
    public void Stop()
    {
        // Stop the attack Coroutine
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        CR_Running = false;

        // Stop attacking the target
        Target = null;
        TargetUnit = null;

        unitAnimator.SetBool("Attacking", false);
    }

    public bool HasTarget()
    {
        return Target == null;
    }
	
	// Update is called once per frame
	void Update ()
    {
        // Start the coroutine if it isn't already and we have a target
		if (Target != null && CR_Running == false)
        {
            AttackTarget(Target);
        }
	}
}
