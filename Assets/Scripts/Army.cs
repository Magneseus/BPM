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

    public float DeployCooldown = 10.0f;
    public float EnemyDeployCooldown = 2f;


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
            LeaderAvatar.transform.position = new Vector3(9, 0, -9);

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
	    foreach (var unit in armyUnits)
	    {
	        var index = armyUnits.IndexOf(unit);
	        if (unit == null)
	            armyUnits.RemoveAt(index);
	    }

        if (vehicleDeployScript != null && LeaderAvatar != null)
        {
            if (vehicleDeployScript.IsDeployed())
            {
                currentDeployCoolDown = Mathf.Max(0.0f, currentDeployCoolDown - Time.deltaTime);
                // Deploy units
                if (currentDeployCoolDown <= 0.0f)
                {
                    // Make sure it's not spawning on top of another unit
                    var spawnTransform = new GameObject().transform;
                    var spawnDisplacement = new Vector3(-1.5f, 0, 1.5f);
                    spawnTransform.position = LeaderAvatar.transform.position + spawnDisplacement;
                    var isValidSpawnPosition = false;

                    int numOfLoops = 0;
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
                        numOfLoops++;
                    } while (!isValidSpawnPosition && numOfLoops < 5);


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
        if (TeamNumber != 0 && LeaderAvatar != null)
        {
            #region Avatar AI
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
            #endregion

            #region Attacking Unit AI
            var allAttackUnits = armyUnits.Where(a => a.name != "EnemyAvatar").ToList();
            if (allAttackUnits.Any())
            {
                var firstUnit = allAttackUnits.FirstOrDefault(u => u != null);

                if (firstUnit != null)
                {
                    var playerUnitHitColliders =
                        Physics.OverlapSphere(firstUnit.transform.position, 7)
                            .Where(h => h.tag == "Infantry" || h.tag == "Tank")
                            .ToList();


                    // If the Player has less than 1.2 times the AI's units currently visible to the first unit in the AI's army
                    // Also only attack when the AI has at least 5 units, so the player can't get rushed
                    if ((float) playerUnitHitColliders.Count/armyUnits.Count < 1.3 && armyUnits.Count > 5)
                    {
                        var attackingUnits = allAttackUnits.Take(allAttackUnits.Count()/2);
                        foreach (var unit in attackingUnits)
                        {
                            var playerAvatar = GameObject.Find("Avatar");
                            if (playerAvatar != null)
                                unit.MoveCommand(playerAvatar.transform.position, true);
                        }
                    }
                    // If there are no enemy units nearby, form a ball
                    else if (playerUnitHitColliders.Count == 0)
                    {
                        var attackingUnits = allAttackUnits.Take(allAttackUnits.Count()/2).ToList();
                        foreach (var unit in attackingUnits.Skip(1))
                        {
                            unit.MoveCommand(attackingUnits.FirstOrDefault().transform.position);
                        }
                    }
                    else
                    {
                        foreach (var unit in allAttackUnits)
                        {
                            unit.MoveCommand(LeaderAvatar.transform.position);
                        }
                    }
                }
            }

            #endregion
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

    public bool IsAvatarAlive()
    {
        return LeaderAvatar != null;
    }
}
