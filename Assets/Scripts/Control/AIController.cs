using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using RPG.Attributes;
using GameDevTV.Utils;
using System;

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] float chaseDistance = 5f;
        [SerializeField] float suspicionTime = 5f;
        [SerializeField] float aggroCooldownTime = 5f;
        [SerializeField] PatrolPath patrolPath;
        //Distance they can be from waypoint before they are considered close enough.
        [SerializeField] float waypointTolerance = 1;
        [SerializeField] float waypointDwellTime = 3f;
        [Range(0,1)]
        //We are setting patrol speed at a fraction to make it easier to adjuster max speed and the fraction that 
        //will be the patrol speed. 
        [SerializeField] float patrolSpeedFraction = 0.2f;
        [SerializeField] float shoutDistance = 5f;

        Fighter fighter; 
        Health health;
        GameObject player;
        Mover mover;  

        LazyValue<Vector3> guardPosition;
        float timeSinceLastSawPlayer = Mathf.Infinity;
        float timeSinceArrivedAtWaypoint = Mathf.Infinity;
        float timeSinceAggro = Mathf.Infinity;
        int currentWaypointIndex = 0;

        private void Awake() 
        {
            fighter = GetComponent<Fighter>();
            health = GetComponent<Health>();
            mover = GetComponent<Mover>();
            //We set the tag on the player as "Player".
            //We will use this so the AI can recognize the player.
            player = GameObject.FindWithTag("Player");

            guardPosition = new LazyValue<Vector3>(GetGuardPosition);
        }

        private Vector3 GetGuardPosition()
        {
            return transform.position;
        }
            
        private void Start() 
        {
            guardPosition.ForceInit();
        }

        private void Update()
        {
            if (health.IsDead()) return;

            if (IsAggro() && fighter.CanAttack(player))
            { 
                AttackBehaviour();
            }
            else if (timeSinceLastSawPlayer < suspicionTime)
            {
                //When you walk out of range of the AI they cancel their action and stand there. 
                SuspicionBehaviour();
            }
            else
            {
                //Once the IA exits suspicion it will walk back to its starting position.
                PatrolBehaviour();
            }

            UpdateTimers();
        }

        //This is called from the take damage unity event in the enemy prefab. 
        //This resets the agro time which will trigger IsAggro in the update method.
        public void Aggo()
        {
            timeSinceAggro = 0;
        }

        private void UpdateTimers()
        {
            timeSinceLastSawPlayer += Time.deltaTime;
            timeSinceArrivedAtWaypoint += Time.deltaTime;
            timeSinceAggro += Time.deltaTime;
        }

        private void PatrolBehaviour()
        {
            Vector3 nextPosition = guardPosition.value;

            if(patrolPath != null)
            {
                //If we are close to our current waypoint, cycle waypoints. 
                if(AtWaypoint())
                {
                    timeSinceArrivedAtWaypoint = 0;
                    CycleWaypoint();
                }
                nextPosition = GetCurrentWaypoint();
            }
            //The AI will have a timer starting at infinity. That will trigger this to trigger causing him to go to the
            //next WP. When he gets there it resets his TSAAW to 0 causing him to stand there until it is greater than WPDT
            if(timeSinceArrivedAtWaypoint > waypointDwellTime)
            {
                //Here is where the patrol speed actually changes
                mover.StartMoveAction(nextPosition, patrolSpeedFraction);
            }

        }

        //Returns the current waypoint so we can help our AI know where to be. 
        private Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypoint(currentWaypointIndex);
        }
        
        //Once we reach the current waypoint this will help us know what the next one is. This happens on a loop. 
        private void CycleWaypoint()
        {
            currentWaypointIndex = patrolPath.GetNextIndex(currentWaypointIndex);
        }

        //Returns bool letting us know if we are close to our current waypoint. 
        private bool AtWaypoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
            return distanceToWaypoint < waypointTolerance;
        }

        private void SuspicionBehaviour()
        {
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        private void AttackBehaviour()
        {
            timeSinceLastSawPlayer = 0;
            fighter.Attack(player);

            AggroNearbyEnemies();
        }

        private void AggroNearbyEnemies()
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, shoutDistance, Vector3.up, 0);
            foreach (RaycastHit hit in hits)
            {
                AIController ai = (hit.collider.GetComponent<AIController>());
                if(ai == null) continue;

                ai.Aggo();

            }
        }

        private bool IsAggro()
        {
            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
            return distanceToPlayer < chaseDistance || timeSinceAggro < aggroCooldownTime;
        }

        // //Called by unity
        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.blue;
            //Draws a sphere around the enemy to show their aggro range. 
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }

}