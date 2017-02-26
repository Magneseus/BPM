using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Army - A container and interface for all of the player's units
 * 
 */
public class Army : MonoBehaviour
{
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
        armyUnits = new List<Unit>(GetComponentsInChildren<Unit>());
        selectedUnits = new List<Unit>();

        // Spawn any additional units here
        // eg...
        //    Unit newUnit = 
        //    Instantiate(avatarPrefab, this.transform).GetComponent<Unit>();
        //
        //    armyUnits.Add(newUnit);

    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    // Tell the selected units to move to a specific location
    public void MoveCommand(Transform moveTo)
    {
        // TODO: move the "center" of the units to a specific spot
        //       and have the units be equally spaced around that point

        foreach (Unit u in selectedUnits)
        {
            u.MoveCommand(moveTo);
        }
    }

    // Give the selected units a specific command
    public void GiveCommand(string command)
    {
        foreach (Unit u in selectedUnits)
        {
            u.GiveCommand(command);
        }
    }
}
