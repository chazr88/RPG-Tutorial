namespace RPG.Core
{
    //An interface is like a contract between a caller and an implementer. 
    //For our purpose the Movement and Combat are the callers, and the Scheduler is the implementer. 
    //Basically we have this cancel function in the callers. When it is called, we can now determine who is doing the calling.
    //This allows us to differentiate between the callers. 
    public interface IAction 
    {
        void Cancel();
    }
}