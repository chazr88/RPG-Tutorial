using UnityEngine;
using RPG.Movement;
using System;
using RPG.Combat;
using RPG.Core;

namespace RPG.Control
{
    public class PlayerController : MonoBehaviour 
    {
        Health health;

        private void Start() {
            health = GetComponent<Health>();
        }

        private void Update()
        {
            //This returns true if the player is dead. It prevents any actions from taking place. 
            if(health.IsDead()) return;
            //If we click on a combat target, this statement returns true and we enter combat. 
            //This also causes us to skip InteractWithMovement. 
            if(InteractWithCombat()) return;
            //If we click somewhere that is not a combat target, we will move there if we are able. 
            if(InteractWithMovement()) return;
            //If we click somewhere that is not a combat target, and we cannot go there, this happens.
            print("Nothing to do.");
        }

        private bool InteractWithCombat()
        {
            //Raycast returns a list of all hit results
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());
            foreach (RaycastHit hit in hits)
            {
                //If the CombatTarget gets hit, save it in target var
                CombatTarget target = hit.transform.GetComponent<CombatTarget>();
                if(target == null) continue;

                //This if statement will allow our loop to continue skipping all the lies below and looping, 
                //in case our hit is on a dead enemy blocking a live enemy.
                //This way our loop will then return the live enemy as the next target.
                if(!GetComponent<Fighter>().CanAttack(target.gameObject))
                {
                    continue;
                }
                
                if(Input.GetMouseButtonDown(0))
                {
                    GetComponent<Fighter>().Attack(target.gameObject);
                }
                //We are returning outside the if so we can recognize combat even if we are not clicking. 
                return true;
            }
            //No targets found
            return false;
        }

        private bool InteractWithMovement()
        {
            RaycastHit hit;
            //Telling hit to store our information from ray. When this happens Raycast is a bool that will be set to True
            bool hasHit = Physics.Raycast(GetMouseRay(), out hit);
            if (hasHit)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
                {   
                    //Sets the destination to our raycast point. (Point where ray collided with terrain)
                    //We moved this script and added the GetComponent<mover> so we could access this component since we are in a diff file. 
                    GetComponent<Mover>().StartMoveAction(hit.point);
                }
                return true;
            }
            return false;
        }

        private static Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }
    }
}