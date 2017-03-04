using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Army - A container and interface for all of the player's units
 * 
 */
public class Army : MonoBehaviour
{
    ////     OTHER VARS     ////

    // Which team does this army belong to
    public int TeamNumber;



    ////     PREFABS     ////
    // Building unit prefabs
    public GameObject avatarPrefab;
    public GameObject healingStationPrefab;
    public GameObject bunkerPrefab;

    // Norm unit prefabs
    public GameObject infantryPrefab;
    public GameObject armoredVehiclePrefab;



    ////     UNIT LISTS     ////
    List<Unit> armyUnits;
    public List<Unit> selectedUnits;


    // Use this for initialization
    void Start ()
    {
        // Get any units in the children of this Army object
        armyUnits = new List<Unit>();
        foreach (Unit u in GetComponentsInChildren<Unit>())
        {
            armyUnits.Add(u);
            u.TeamNumber = this.TeamNumber;
        }

        selectedUnits = new List<Unit>();

        // Spawn any additional units here
        // eg...
        //    AddUnit(avatarPrefab, this.transform);

    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public bool AddUnit(GameObject newUnitGO, Transform place)
    {
        // If the unit we're trying to add does not have the Unit component
        if (newUnitGO.GetComponent<Unit>() == null)
            return false;

        // Spawn the new unit
        Unit newUnit = Instantiate(newUnitGO, place).GetComponent<Unit>();

        // If spawning failed, return false
        if (newUnit == null)
            return false;


        // Set the team
        newUnit.TeamNumber = this.TeamNumber;

        // Add to the list
        armyUnits.Add(newUnit);

        return true;
    }

    // Tell the selected units to move to a specific location
    public bool MoveCommand(Transform moveTo)
    {
        // TODO: move the "center" of the units to a specific spot
        //       and have the units be equally spaced around that point

        // Only return false if all units fail to move
        bool returnVal = false;

        foreach (Unit u in selectedUnits)
        {
            if (u.MoveCommand(moveTo))
                returnVal = true;
        }

        return returnVal;
    }

    // Give the selected units a specific command
    public bool GiveCommand(string command)
    {
        // Only return false if all units fail to act
        bool returnVal = false;

        foreach (Unit u in selectedUnits)
        {
            if (u.GiveCommand(command))
                returnVal = true;
        }

        return returnVal;
    }
}
