using UnityEngine;
using RPG.Movement;
using RPG.Core;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction
    {

        [SerializeField] float weaponRange = 2f;
        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] float weaponDamage = 5f;

        //We set target to type Health so we have access to everything in the Health class. 
        Health target;

        //We set this to a high number so it will attack immediately when the attack is started.
        float timeSinceLastAttack = Mathf.Infinity;

        private void Update()
        {
            //Increments time since last attack every frame.
            timeSinceLastAttack += Time.deltaTime;

            //This allows us to skip this entire function if there is not target to prevent bugs.
            if(target == null) return;

            //When the target dies, stop attacking.
            if(target.IsDead()) return;

            //If we have a target and we are not in range, move to. Else stop.
            if (!GetIsInRange())
            {
                GetComponent<Mover>().MoveTo(target.transform.position);
            }
            else
            {
                //When in range stop moving and attack.
                GetComponent<Mover>().Cancel();
                AttackBehaviour();
            }
        }

        private void AttackBehaviour()
        {
            //This forces our player to turn and look at the enemy if hes facing the wrong way.
            transform.LookAt(target.transform);
            //Here we are checking to see if enough time has passed to call attack.
            if(timeSinceLastAttack > timeBetweenAttacks)
            {
                TriggerAttack();
                //Reset time since last attack.
                timeSinceLastAttack = 0;
            }

        }

        private void TriggerAttack()
        {
            //This was added to ensure the stop attack trigger does not get stuck on. 
            //This will force it to be consumed when we go to attack.
            GetComponent<Animator>().ResetTrigger("stopAttack");
            //This calls our attack trigger from the animator.
            //This triggers the hit event below this function.
            GetComponent<Animator>().SetTrigger("attack");
        }

        //Animation event. Called in animator not code
        void Hit()
        {
            if(target == null) return;
            target.TakeDamage(weaponDamage);
        }

        private bool GetIsInRange()
        {
            //Gets distance between 2 vectors to determine if we are in range. 
            return Vector3.Distance(transform.position, target.transform.position) < weaponRange;
        }

        public bool CanAttack(GameObject combatTarget)
        {
            //If we have no target we cannot attack
            if(combatTarget == null) {return false;}
            //Gets health of current target we want to test
            Health targetToTest = combatTarget.GetComponent<Health>();
            //If we have a target and they are not dead, return true. 
            return targetToTest != null && !targetToTest.IsDead();
        }

        //Sets combat target to our target. This is called from another script by clicking on the combat target. Once called
        //it sets our target. When our target is set and we are not in range, the Update function forces our navMeshAgent
        //to walk into range of our target. 
        public void Attack(GameObject combatTarget)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            target = combatTarget.GetComponent<Health>();
        }

        public void Cancel()
        {
            StopAttack();
            target = null;
        }

        private void StopAttack()
        {
            GetComponent<Animator>().ResetTrigger("attack");
            GetComponent<Animator>().SetTrigger("stopAttack");
        }
    }
}