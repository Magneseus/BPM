using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealTower : MonoBehaviour
{
	public Animator HealAnimator;
	public float HealAmount;
    public float HealRate;

    private Unit selfUnit;
    private int selfTeam;
    private List<Unit> unitsInArea;
    private float healTimer;

	private bool upBeatPrevious = true;

	// Use this for initialization
	void Start ()
    {
        selfUnit = this.GetComponent<Unit>();
        if (selfUnit != null)
        {
            selfTeam = selfUnit.TeamNumber;
        }

        unitsInArea = new List<Unit>();

	}
	
	// Update is called once per frame
	void Update ()
    {
        healTimer += Time.deltaTime;

		//if (healTimer >= HealRate) {		// tentatively commenting previous heal timing in case revert required
		if(StartHealPulse()){
			// Reset the timer
			healTimer = 0.0f;

			// Activate Animation
			HealAnimator.SetBool ("Activated", true);

			// Heal all units in our area
			foreach (Unit u in unitsInArea) {
				u.DoHeal (HealAmount);
			}
		} else if(healTimer >= HealRate/2) {
			HealAnimator.SetBool ("Activated", false);
		}
	}

	// Checks against music for heal pulse timing
	private bool StartHealPulse(){

		// When beat first changes to down beat trigger pulse and reset local bool
		if (upBeatPrevious && Rhythm.Instance ().IsOnDownBeat ()) {
			upBeatPrevious = false;
			return true;
		}

		// When beat first changes to up beat activate local bool to enable pulse on next down beat
		else if (!upBeatPrevious && Rhythm.Instance ().IsOnUpBeat ()) {
			upBeatPrevious = true;
		}
		return false;
	}

    // Add units if they enter the heal area and are friendly
    private void OnTriggerEnter(Collider other)
    {
        // If the entering collider is a Unit
        Unit u = other.GetComponent<Unit>();
        if (u != null)
        {
            // If we're on the same team
            if (u.TeamNumber == this.selfTeam)
            {
                // Add to our list of units
                unitsInArea.Add(u);
            }
        }
    }

    // Remove units if they exit the heal area
    private void OnTriggerExit(Collider other)
    {
        // If the entering collider is a Unit
        Unit u = other.GetComponent<Unit>();
        if (u != null)
        {
            // Remove from our list of units
            unitsInArea.Remove(u);
        }
    }
}
