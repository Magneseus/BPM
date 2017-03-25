using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public int SelfTeam;
    public float Damage;
    public GameObject SelfBody;

    public void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;

        // Check that we're not colliding with our original body
        if (other.gameObject != SelfBody)
        {

            // Check if we collided with a unit
            Unit otherUnit = other.GetComponent<Unit>();

            // Check if the unit is on another team
            if (otherUnit != null && otherUnit.TeamNumber != SelfTeam)
            {
                // Deal damage
                otherUnit.DoDamage(Damage);
            }

            // TODO: Explosion stuff here!
            Destroy(this.gameObject);
        }
    }
}
