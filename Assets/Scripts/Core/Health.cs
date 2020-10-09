using UnityEngine;

namespace RPG.Core
{
    public class Health : MonoBehaviour 
    {
        [SerializeField] float healthPoints = 100f;

        bool isDead = false;

        //This is a getter. We can call this function from anywhere to get isDead.
        //This book gets set to true in the die function. If someone dies. 
        public bool IsDead()
        {
            return isDead;
        }

        public void TakeDamage(float damage)
        {
            //As long as the health is higher than 0, decrement it by the amount of the damage taken.
            healthPoints = Mathf.Max(healthPoints - damage, 0);
            if(healthPoints == 0)
            {
                Die();
            }
        }

        private void Die()
        {
            //This is added to prevent the death animation from happening if the target is already dead.
            if(isDead) return;
            isDead = true;
            //Trigger death animation
            GetComponent<Animator>().SetTrigger("die");
            //Stops any actions that were in effect after something dies. 
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }
    }
}