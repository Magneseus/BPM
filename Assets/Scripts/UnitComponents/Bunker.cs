using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bunker : MonoBehaviour
{
    // TODO: Allow for more than one unit to be stored
    public const int NumberOfSlots = 1;
    public float Range = 5.0f;
    public float DamageMultiplier = 1.5f;
    public float RangeMultiplier = 2.0f;
    public float RoFMultiplier = 1.0f;

    private List<GameObject> garrisonUnits;
    private Attack attackComponent;
    private Unit selfUnit;
    private int selfTeam;

	// Use this for initialization
	void Start ()
    {
        garrisonUnits = new List<GameObject>();
        attackComponent = null;

        selfUnit = this.GetComponent<Unit>();
        if (selfUnit != null)
        {
            selfTeam = selfUnit.TeamNumber;
        }

	    gameObject.tag = "Bunker";
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public bool AddGarrisonUnit(GameObject unit)
    {
        // If the garrison is full or if the object is not a Unit/GarrisonUnit
        // Or if we're not on the same team
        if (garrisonUnits.Count >= NumberOfSlots ||
            selfUnit == null ||
            unit.GetComponent<Unit>() == null ||
            unit.GetComponent<GarrisonUnit>() == null ||
            selfTeam != unit.GetComponent<Unit>().TeamNumber)
        {
            return false;
        }

        // If the Unit isn't in range
        if (Vector3.Distance(this.transform.position, unit.transform.position) > Range)
        {
            return false;
        }

        // Otherwise add the unit
        CopyAttackComponent(unit);
        unit.SetActive(false);
        garrisonUnits.Add(unit);

        return true;
    }

    public void RemoveGarrison()
    {
        if (selfUnit == null)
            return;

        // Remove our attack component if it exists
        if (attackComponent != null)
            Destroy(attackComponent);

        // Re-enable all units and give them a move order
        foreach (GameObject go in garrisonUnits)
        {
            go.SetActive(true);
        }

        // Clear the garrison
        garrisonUnits.Clear();
    }

    public void RemoveGarrison(Vector3 moveTo)
    {
        if (selfUnit == null)
            return;

        // Remove our attack component if it exists
        if (attackComponent != null)
        {
            Destroy(attackComponent);
            attackComponent = null;
        }

        // Re-enable all units and give them a move order
        foreach (GameObject go in garrisonUnits)
        {
            go.SetActive(true);

            Unit u = go.GetComponent<Unit>();
            if (u != null)
            {
                u.MoveCommand(moveTo);
            }
        }

        // Clear the garrison
        garrisonUnits.Clear();
    }

    // Creates an Attack Component for the bunker that will copy the values from
    // the Unit being added to it's garrison
    private void CopyAttackComponent(GameObject original)
    {
        // If the gameobject has an attack component
        Attack originalAttack = original.GetComponent<Attack>();
        if (originalAttack != null)
        {
            // Create the component and add it's abilities
            attackComponent = this.gameObject.AddComponent<Attack>();

            attackComponent.Damage = originalAttack.Damage * DamageMultiplier;
            attackComponent.Range = originalAttack.Range * RangeMultiplier;
            attackComponent.RateOfFire = originalAttack.RateOfFire * RoFMultiplier;
        }
    }

    public bool HasGarrisonedUnits()
    {
        return garrisonUnits.Any();
    }
}
