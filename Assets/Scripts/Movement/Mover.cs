using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RPG.Core;

namespace RPG.Movement
{
    //Adding IAction here allows the IAction file to know this class is being passed to it. 
    public class Mover : MonoBehaviour, IAction
    {
        //Adds transform target as a field
        [SerializeField] Transform target;
        
        NavMeshAgent navMeshAgent;
        Health health;

        private void Start() {
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
        public void StartMoveAction(Vector3 destination)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            MoveTo(destination);
        }

        //Here we are setting the var destination as the vector 3. This info will be passed to this function from
        //a controller file. Ex a player controller. We currently have this set up in the player controller as 
        //the point where we are clicking out mouse. It saves that click, saves it as the dest, and passes it here for our
        //nav mesh agent to move that way. 
        public void MoveTo(Vector3 destination)
        {
            //Use the passed in destination to move the current NavMeshAgent to that destination.
            navMeshAgent.destination = destination;
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
    }  
}
