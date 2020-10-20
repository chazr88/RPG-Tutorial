using UnityEngine;
using RPG.Movement;
using RPG.Core;
using RPG.Saving;
using RPG.Attributes;
using RPG.Stats;
using System.Collections.Generic;
using GameDevTV.Utils;
using System;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction, ISaveable, IModifierProvider
    {

        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] Transform rightHandTransform = null;
        [SerializeField] Transform leftHandTransform = null;
        [SerializeField] WeaponConfig defaultWeapon = null;



        //We set target to type Health so we have access to everything in the Health class. 
        Health target;
        //We set this to a high number so it will attack immediately when the attack is started.
        float timeSinceLastAttack = Mathf.Infinity;
        //Weapon config is the information about the weapon
        WeaponConfig currentWeaponConfig;
        //Weapon is the actual weapon they are holding.
        LazyValue<Weapon> currentWeapon;

        private void Awake() 
        {
            currentWeaponConfig = defaultWeapon;
            currentWeapon = new LazyValue<Weapon>(SetupDefaultWeapon);
        }

        private Weapon SetupDefaultWeapon()
        {
            return AttachWeapon(defaultWeapon);
        }

        private void Start() 
        {
            currentWeapon.ForceInit();
        }

        private void Update()
        {
            //Increments time since last attack every frame.
            timeSinceLastAttack += Time.deltaTime;

            //This allows us to skip this entire function if there is not target to prevent bugs.
            if(target == null) return;

            //When the target dies, stop attacking.
            if(target.IsDead()) return;

            //If we have a target and we are not in range, move to. Else stop.
            if (!GetIsInRange(target.transform))
            {
                GetComponent<Mover>().MoveTo(target.transform.position, 1f);
            }
            else
            {
                //When in range stop moving and attack.
                GetComponent<Mover>().Cancel();
                AttackBehaviour();
            }
        }

        //When this is called we want to spawn a weapon. The weapon is pulled from scriptable objects
        //and changes depending on what weapon we have equip.
        public void EquipWeapon(WeaponConfig weapon)
        {
            currentWeaponConfig = weapon;
            currentWeapon.value = AttachWeapon(weapon);
        }

        private Weapon AttachWeapon(WeaponConfig weapon)
        {
            Animator animator = GetComponent<Animator>();
            //This takes an animation that will be passed in a seralized field and uses it to override our current animation.
            //This now returns a weapon
            return weapon.Spawn(rightHandTransform, leftHandTransform, animator);
        }

        public Health GetTarget()
        {
            return target;
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

            float damage = GetComponent<BaseStats>().GetStat(Stat.Damage);
            //If weapon is a bow or projectile...

            //Make sure you have a weapon you can hold.. ex this dont need to run for fireball
            if(currentWeapon.value != null)
            {
                currentWeapon.value.OnHit();
            }

            if(currentWeaponConfig.HasProjectile())
            {
                currentWeaponConfig.LaunchProjectile(rightHandTransform, leftHandTransform, target, gameObject, damage);
            }
            else
            {
                //No not projectile...
                target.TakeDamage(gameObject, damage);
            }


        }

        void Shoot()
        {
            Hit();
        }

        private bool GetIsInRange(Transform targetTransform)
        {
            //Gets distance between 2 vectors to determine if we are in range. 
            return Vector3.Distance(transform.position, targetTransform.position) < currentWeaponConfig.GetRange();
        }

        public bool CanAttack(GameObject combatTarget)
        {  
            //If we have no target we cannot attack
            if(combatTarget == null) return false;
            if(!GetComponent<Mover>().CanMoveTo(combatTarget.transform.position) && 
                !GetIsInRange(combatTarget.transform)) 
            {
                return false;
            }
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
            //Cancels movement when action is cancelled. 
            GetComponent<Mover>().Cancel();
        }

        private void StopAttack()
        {
            GetComponent<Animator>().ResetTrigger("attack");
            GetComponent<Animator>().SetTrigger("stopAttack");
        }

        //This is an going to help us get dmg modifiers from weapons. 
        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            if(stat == Stat.Damage)
            {
                yield return currentWeaponConfig.GetDamage();
            }
        }

        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
            if(stat == Stat.Damage)
            {
                yield return currentWeaponConfig.GetPercentageBonus();
            }
        }

        //This captures the current weapon
        public object CaptureState()
        {
            return currentWeaponConfig.name;
        }

        //This takes the captured weapon, casts it to a state, goes into resources and grabs that weapon scriptable object
        //and based on the name returns us the correct weapon. We then equip that weapon. 
        public void RestoreState(object state)
        {
            string weaponName = (string)state;
            //Unity is smart enough to have scrits just for folders named Resources. This line is
            //looking in the resources folder for a weapon scriptable object and trying to find the defaultWeaponName
            //and the scriptable object belonging to that name it was given. 
            WeaponConfig weapon = UnityEngine.Resources.Load<WeaponConfig>(weaponName);
            EquipWeapon(weapon);
        }
    }
}