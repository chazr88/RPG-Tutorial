using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RPG.Core;
using GameDevTV.Saving;
using RPG.Attributes;

namespace RPG.Movement
{
    //Adding IAction here allows the IAction file to know this class is being passed to it.
    public class Mover : MonoBehaviour, IAction, ISaveable
    {
        //Adds transform target as a field
        [SerializeField] Transform target;
        [SerializeField] float maxSpeed = 6f;
        [SerializeField] float maxNavPathLength = 40f;

        
        NavMeshAgent navMeshAgent;
        Health health;

        private void Awake() {
            navMeshAgent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
        }

        void Update()
        {
            //If something dies, this disables their navMeshAgent to prevent you from bumping into them. 
            navMeshAgent.enabled = !health.IsDead();
            updateAnimator();
        }


        //When this function is called it cancels combat and moves to dest.
        //This way we have a way to click off our target and cancel combat.
        //We added the speed fraction so our enemies will have a different patrol speed. 
        public void StartMoveAction(Vector3 destination, float speedFraction)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            MoveTo(destination, speedFraction);
        }

        //This function is to help us limit movement to far away places that may cause issues. Via movement or combat.
        public bool CanMoveTo(Vector3 destination)
        {
            //Here we set up the new path. We need to give this an object so the CalculatePath can modify that obj.
            NavMeshPath path = new NavMeshPath();
            //This checks the path and we can get the return of complete path, partial path, or invalid path.
            bool hasPath = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
            if(!hasPath) return false;
            //If hasPath returned a value but its not a complete path return false.
            if(path.status != NavMeshPathStatus.PathComplete) return false;
            if(GetPathLength(path) > maxNavPathLength) return false;

            return true;
        }

        //Here we are setting the var destination as the vector 3. This info will be passed to this function from
        //a controller file. Ex a player controller. We currently have this set up in the player controller as 
        //the point where we are clicking out mouse. It saves that click, saves it as the dest, and passes it here for our
        //nav mesh agent to move that way. 
        public void MoveTo(Vector3 destination, float speedFraction)
        {
            //Use the passed in destination to move the current NavMeshAgent to that destination.
            navMeshAgent.destination = destination;
            navMeshAgent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
            navMeshAgent.isStopped = false;
        }

        //Stop navMeshAgent from walking
        public void Cancel()
        {
            navMeshAgent.isStopped = true;
        }

        //The purpose of this is to get the global vel and convert that to local vel. We do this because we do not care where our character is in the world,
        //no matter where our character is, we will be able to tell it if its moving in a forward direction or not, and how how fast.
        private void updateAnimator()
        {
            //Gets global velocity from NavMeshAgent ans stores it in a variable with the type of vector3.
            Vector3 velocity = navMeshAgent.velocity;
            //Takes global velocity we set and transforms it to a local variable we can use.
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            //Get speed from the forward direction
            float speed = localVelocity.z;
            //Here we use that speed and pass it into the animator.
            //This will match our speed with the correct animation.
            GetComponent<Animator>().SetFloat("forwardSpeed", speed);
        }

        //This just gets us the length of the path from our navMeshAgent to where we clicked. 
        private float GetPathLength(NavMeshPath path)
        {
            float total = 0;
            if(path.corners.Length < 2) return total;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                total += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }

            return total;
        }

        //Part of the ISavable interface
        public object CaptureState()
        {
            //Just grabs our current position. 
            return new SerializableVector3(transform.position);
        }

        //Part of the ISavable interface
        public void RestoreState(object state)
        {
            SerializableVector3 position = (SerializableVector3)state;
            //To prevent our NMA from causing issues when we are changing the position, we disable it here
            //and re enable it after the position set. 
            GetComponent<NavMeshAgent>().enabled = false;
            transform.position = position.ToVector();
            GetComponent<NavMeshAgent>().enabled = true;
        }
    }  
}
