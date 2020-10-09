using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RPG.Core
{
    public class FollowCamera : MonoBehaviour
        {
        //Create a filed we can use inside of the editor for the follow camers.
        //We use transform so we can know the position of the item.
        [SerializeField] Transform target;

        //We made this late update to ensure the player moves before the camera does.
        void LateUpdate()
        {
            //We want the position of our follow camers to be the position of the target we set(player)
            transform.position = target.position;
        }
    }

}