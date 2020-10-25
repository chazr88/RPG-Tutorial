using UnityEngine;
using RPG.Attributes;
using RPG.Control;

namespace RPG.Combat
{
    // This is added so when we add a combat target, it automatically adds a health component. 
    // We do this because so many scripts depend on health for the combat target. 
    [RequireComponent(typeof(Health))]
    public class CombatTarget : MonoBehaviour, IRaycastable
    {
        public CursorType GetCursorType()
        {
            return CursorType.Combat;
        }

        public bool HandleRaycast(PlayerController callingController)
        {
            if (!enabled) return false;
            //This way our loop will then return the live enemy as the next target.
            if(!callingController.GetComponent<Fighter>().CanAttack(gameObject))
            {
                return false;
            }
            
            if(Input.GetMouseButtonDown(0))
            {
                callingController.GetComponent<Fighter>().Attack(gameObject);
            }
            return true;
        }
    }
}