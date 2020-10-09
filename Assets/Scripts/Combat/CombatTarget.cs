using UnityEngine;
using RPG.Core;

namespace RPG.Combat
{
    // This is added so when we add a combat target, it automatically adds a health component. 
    // We do this because so many scripts depend on health for the combat target. 
    [RequireComponent(typeof(Health))]
    public class CombatTarget : MonoBehaviour 
    {
    
    }
}