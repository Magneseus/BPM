using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealTower : MonoBehaviour
{
    public float HealAmount;
    public float HealRate;

    private Unit selfUnit;
    private int selfTeam;
    private List<Unit> unitsInArea;
    private float healTimer;

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

        if (healTimer >= HealRate)
        {
            // Reset the timer
            healTimer = 0.0f;

            // Heal all units in our area
            foreach (Unit u in unitsInArea)
            {
                u.DoHeal(HealAmount);
            }
        }
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
