using UnityEngine;

namespace RPG.Core
{
    public class ActionScheduler : MonoBehaviour 
    {

        IAction currentAction;
        //This function will be passed a monobehavior ex.. mover or fighter.
        //It should not know which one is actually passed, it should just do the same thing regardless
        //of which one is passed. 
        public void StartAction(IAction action)
        {   
            //If current action is the same as the new action do nothing.
            if(currentAction == action) return;
            //Cancel action
            if(currentAction != null)
            {
                //This cancel gets called thru the interface IAction. This way we can prevent circular dependencies. 
                currentAction.Cancel();
            }
            //Set new action
            currentAction = action;
        }

        public void CancelCurrentAction()
        {
            StartAction(null);
        }
    }
}