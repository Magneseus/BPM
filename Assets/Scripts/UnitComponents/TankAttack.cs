using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankAttack : Attack
{
    public GameObject TankShellPrefab;
    public Vector3 TankShellSpawnOffset;
    public float ForceOfShot;

    protected override void DealDamageToTarget(Unit _targetUnit)
    {
        // Spawn the tank shell and give it an initial momentum
        GameObject go = Instantiate(TankShellPrefab);

        // Set its proper variables
        Shell sh = go.GetComponent<Shell>();
        sh.SelfTeam = this.GetComponent<Unit>().TeamNumber;
        sh.Damage = this.Damage;
        sh.SelfBody = this.gameObject;

        // Transform to the spawn offset
        go.transform.position = this.transform.position + 
            this.transform.rotation * TankShellSpawnOffset;
        go.transform.rotation = this.transform.rotation;

        // Apply momentum
        Vector3 forceAdded = go.transform.forward * ForceOfShot;
        forceAdded -= Physics.gravity;
        go.GetComponent<Rigidbody>().AddForce(forceAdded);
    }
}
