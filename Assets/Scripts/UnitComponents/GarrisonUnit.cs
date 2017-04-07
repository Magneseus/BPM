using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarrisonUnit : MonoBehaviour
{
    private Unit selfUnit;
    private int selfTeam;

    private Move moveScript;
    private Bunker targetBunker;

    private IEnumerator runToBunkerCoroutine;

	// Use this for initialization
	void Start ()
    {
        selfUnit = this.GetComponent<Unit>();
        if (selfUnit != null)
        {
            selfTeam = selfUnit.TeamNumber;
        }

        moveScript = this.GetComponent<Move>();
        targetBunker = null;
        runToBunkerCoroutine = null;
	}

    IEnumerator goToBunker()
    {
        // Send a move command to the bunker
        if (moveScript != null)
        {
            moveScript.MoveCommand(targetBunker.transform.position, false);
        }
        // Otherwise just return
        else
        {
            yield break;
        }

        // While we're still moving to the target location
        while (targetBunker != null && moveScript.TargetLocation == targetBunker.transform.position)
        {
            // Check if we're in range
            if (Vector3.Distance(this.transform.position, 
                targetBunker.transform.position) <= targetBunker.Range)
            {
                targetBunker.AddGarrisonUnit(this.gameObject);
                targetBunker = null;
                yield break;
            }

            yield return new WaitForSeconds(0.1f);
        }

        targetBunker = null;
        yield break;
    }

    public bool EnterBunker(GameObject bunker)
    {
        Bunker b = bunker.GetComponent<Bunker>();
        if (b == null)
            return false;

        return EnterBunker(b);
    }

    public bool EnterBunker(Bunker bunker)
    {
        // If we're not on the same team
        if (bunker.gameObject.GetComponent<Unit>().TeamNumber != selfTeam)
            return false;

        // Start running to the bunker if we're not in range
        if (Vector3.Distance(this.transform.position, bunker.transform.position) > bunker.Range)
        {
            targetBunker = bunker;

            runToBunkerCoroutine = goToBunker();
            StartCoroutine(runToBunkerCoroutine);
        }
        // Otherwise just enter the bunker
        else
        {
            return bunker.AddGarrisonUnit(this.gameObject);
        }

        return true;
    }
	
	// Update is called once per frame
	void Update ()
    {

    }
}
