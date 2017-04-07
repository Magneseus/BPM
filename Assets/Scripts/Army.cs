using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utils;
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

    public float DeployCooldown = 5.0f;
    public float EnemyDeployCooldown = 10.0f;


    ////     PREFABS     ////
    // Building unit prefabs
    public GameObject avatarPrefab;
    public GameObject healingStationPrefab;
    public GameObject bunkerPrefab;

    // Norm unit prefabs
    public GameObject infantryPrefab;
    public GameObject armoredVehiclePrefab;

    private GameObject LeaderAvatar;


    ////     UNIT LISTS     ////
    List<Unit> armyUnits;
    public List<Unit> selectedUnits;

    private float currentDeployCoolDown;

    private VehicleDeploy vehicleDeployScript;


    // Use this for initialization
    void Start ()
    {
        avatarPrefab = Resources.Load("Avatar") as GameObject;
        infantryPrefab = Resources.Load("Infantry") as GameObject;
        armoredVehiclePrefab = Resources.Load("Tank") as GameObject;

        // Get any units in the children of this Army object
        armyUnits = new List<Unit>();
        foreach (Unit u in GetComponentsInChildren<Unit>())
        {
            armyUnits.Add(u);
            u.TeamNumber = this.TeamNumber;
        }

        selectedUnits = new List<Unit>();

        if (TeamNumber != 0)
        {
            var newTransform = new GameObject().transform;
            newTransform.position = new Vector3(8, 0, -8);
            AddUnit(avatarPrefab, newTransform, "EnemyAvatar");

            LeaderAvatar = GameObject.Find("EnemyAvatar");
            LeaderAvatar.transform.position = new Vector3(7, 0, -7);

            vehicleDeployScript = LeaderAvatar.GetComponent<VehicleDeploy>();
        }
        else
        {
            LeaderAvatar = GameObject.Find("Avatar");
            vehicleDeployScript = LeaderAvatar.GetComponent<VehicleDeploy>();
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (vehicleDeployScript != null)
        {
            if (vehicleDeployScript.IsDeployed())
            {
                currentDeployCoolDown = Mathf.Max(0.0f, currentDeployCoolDown - Time.deltaTime);
                // Deploy units
                if (currentDeployCoolDown <= 0.0f)
                {
                    // Make sure it's not spawning on top of another unit
                    var spawnTransform = new GameObject().transform;
                    spawnTransform.position = LeaderAvatar.transform.position + Vector3.back;
                    var isValidSpawnPosition = false;

                    do
                    {
                        var hitColliders = Physics.OverlapSphere(spawnTransform.position, 0.2f).Where(h => h.tag == "Infantry" || h.tag == "Tank" || h.tag == "VehicleDeploy").ToList();
                        if (hitColliders.Any())
                        {
                            var posRnd = new System.Random();
                            int posRndNumber = posRnd.Next(1, 4);
                            Vector3 displacementVector = new Vector3();
                            switch (posRndNumber)
                            {
                                case 1:
                                    displacementVector = new Vector3(0.1f, 0, 0);
                                    break;
                                case 2:
                                    displacementVector = new Vector3(-0.1f, 0, 0);
                                    break;
                                case 3:
                                    displacementVector = new Vector3(0, 0, 0.1f);
                                    break;
                                case 4:
                                    displacementVector = new Vector3(0, 0, -0.1f);
                                    break;
                            }

                            var hitGameObject = hitColliders.FirstOrDefault();
                            if (hitGameObject != null)
                            {
                                if (hitGameObject.tag == "Infantry")
                                    spawnTransform.position += displacementVector;
                                else if (hitGameObject.tag == "Tank")
                                    spawnTransform.position += displacementVector*3;
                            }
                        }
                        else
                        {
                            isValidSpawnPosition = true;
                        }
                    } while (!isValidSpawnPosition);


                    var rnd = new System.Random();
                    int randomNumber = rnd.Next(1, 10);
                    GameObject newGameObject;
                    if (randomNumber < 7)
                    {
                        newGameObject = AddUnit(infantryPrefab, spawnTransform);
                    }
                    else
                    {
                        newGameObject = AddUnit(armoredVehiclePrefab, spawnTransform);
                    }

                    newGameObject.transform.position = spawnTransform.position;

                    if (TeamNumber == 0)
                        currentDeployCoolDown = DeployCooldown;
                    else
                        currentDeployCoolDown = EnemyDeployCooldown;
                }
            }
        }

        #region Enemy Army AI
        if (TeamNumber != 0)
        {
            /* Avatar AI */
            var hitColliders = Physics.OverlapSphere(LeaderAvatar.transform.position, 4).Where(h => h.tag == "Infantry" || h.tag == "Tank").ToList();

            if (hitColliders.Any())
            {
                var numberOfEnemies = 0;
                foreach (var go in hitColliders)
                {
                    var unitScript = go.GetComponent<Unit>();
                    if (unitScript != null && unitScript.TeamNumber != TeamNumber)
                        numberOfEnemies++;
                }

                if (numberOfEnemies <= 8)
                {
                    // Deploy if there are fewer than 6 units
                    if (!vehicleDeployScript.IsDeployed())
                        vehicleDeployScript.ToggleDeploy();
                }
                else
                {
                    // If there are too many enemies, undeploy and retreat
                    if (vehicleDeployScript.IsDeployed())
                        vehicleDeployScript.ToggleDeploy();
                    RetreatUnitFromEnemy(LeaderAvatar, hitColliders.FirstOrDefault().gameObject);
                }
            }
            else
            {
                // If there are no units and it's not deployed, deploy it.
                // If it's already deployed, keep producing units
                if (!vehicleDeployScript.IsDeployed())
                    vehicleDeployScript.ToggleDeploy();
            }

            /* Attacking unit AI */
            if (armyUnits.Count > 5)
            {
                var allAttackUnits = armyUnits.Where(a => a.tag == "Infantry" || a.tag == "Tank");
                foreach (var unit in allAttackUnits)
                {
                    var playerAvatar = GameObject.Find("Avatar");
                    unit.MoveCommand(playerAvatar.transform.position, true);
                }
            }
        }
        #endregion
    }

    private void RetreatUnitFromEnemy(GameObject currentUnit, GameObject enemy)
    {
        var retreatDirection = Vector3.Normalize(currentUnit.transform.position + enemy.transform.position);
        var retreatTransform = new GameObject().transform;
        retreatTransform.position = currentUnit.transform.position + retreatDirection*2;

        var unitScript = currentUnit.GetComponent<Unit>();
        unitScript.MoveCommand(retreatTransform);
    }

    public GameObject AddUnit(GameObject newUnitGO, Transform place, string name = "")
    {
        // If the unit we're trying to add does not have the Unit component
        if (newUnitGO.GetComponent<Unit>() == null)
            return null;

        // Spawn the new unit
        var newGameObject = Instantiate(newUnitGO, place);
        Unit newUnit = newGameObject.GetComponent<Unit>();

        // If spawning failed, return false
        if (newUnit == null)
            return null;

        if (!String.IsNullOrEmpty(name))
        {
            newUnit.name = name;
        }

        // Set the team
        newUnit.TeamNumber = this.TeamNumber;

        // Add to the list
        armyUnits.Add(newUnit);

        return newGameObject;
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
    // Need to also give it either a game object OR a transform for the command
    public bool GiveCommand(UIUtils.CommandType command, GameObject go, Transform trans)
    {
        // If we were given no game object or transform, return false
        if (go == null && trans == null)
            return false;

        // Only return false if all units fail to act
        bool returnVal = false;

        foreach (Unit u in selectedUnits)
        {
            if (u.GiveCommand(command, go, trans))
                returnVal = true;
        }

        return returnVal;
    }
}
