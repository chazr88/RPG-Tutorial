using UnityEngine;
using RPG.Movement;
using RPG.Combat;
using RPG.Attributes;
using System;
using UnityEngine.EventSystems;
using UnityEngine.AI;

namespace RPG.Control
{
    public class PlayerController : MonoBehaviour 
    {
        Health health;


        //This is a struct. It allows us to make a var that can hold related data of various types. 
        [System.Serializable]
        struct CursorMapping
        {
            public CursorType type;
            public Texture2D texture;
            public Vector2 hotspot;
        }

        [SerializeField] CursorMapping[] cursorMappings = null;
        [SerializeField] float maxNavMeshProjectionDistance = 1f;
        [SerializeField] float raycastRadius = 1f;

        bool isDraggingUI = false;

        private void Awake() 
        {
            health = GetComponent<Health>();
        }
        
        private void Update()
        {
            if(InteractWithUI()) return;
            //This returns true if the player is dead. It prevents any actions from taking place. 
            if(health.IsDead())
            {
                SetCursor(CursorType.None);
                return;
            }

            if(InteractWithComponent()) return;
            //If we click somewhere that is not a combat target, we will move there if we are able. 
            if(InteractWithMovement()) return;
            //If we click somewhere that is not a combat target, and we cannot go there, this happens.
            //print("Nothing to do.");
            SetCursor(CursorType.None);
        }

        private bool InteractWithUI()
        {
            if(Input.GetMouseButtonUp(0))
            {
                //When you let go of the mouse set back to false. 
                isDraggingUI = false;
            }
            //Is this over a game object that is a piece of UI
            if(EventSystem.current.IsPointerOverGameObject())
            {
                //This is to prevent movement when draggin in the UI
                if(Input.GetMouseButtonDown(0))
                {
                    isDraggingUI = true;
                }
                SetCursor(CursorType.UI);
                return true;
            }
            if(isDraggingUI)
            {
                return true;
            }
            return false;
        }
        
        private bool InteractWithComponent()
        {
            //Go thru all components
            RaycastHit[] hits = RaycastAllSorted();
            foreach (RaycastHit hit in hits)
            {
                //We store all those components in an array
                IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();
                foreach(IRaycastable raycastable in raycastables)
                {
                    //Let me know if somethign in the array can handle a raycast
                    if(raycastable.HandleRaycast(this))
                    {
                        SetCursor(raycastable.GetCursorType());
                        return true;
                    }
                }
            }
            return false;
        }

        RaycastHit[] RaycastAllSorted()
        {
            //Get all hits
            RaycastHit[] hits = Physics.SphereCastAll(GetMouseRay(), raycastRadius);
            //Sort by distance
            //Build array distances
            //We are building the array to be the same length as the hits array.
            float[] distances = new float[hits.Length];
            for (int i = 0; i < hits.Length; i++)
            {
                //Here we are populating the distances array with the distances from the hits array.
                //With this assignment, since our raycast is going thru all the objects, it technically stores them based
                //on distance. This allows the distances array to have sorted distances of closer to further. 
                distances[i] = hits[i].distance;
            }
            //Sort the hits.
            //This sort works by taking evaluating the first array, then changing the second to make it match.
            //This has no return value so it rearranges the hits in place. 
            Array.Sort(distances, hits);
            //Return
            return hits;
        }

        private bool InteractWithMovement()
        {
            Vector3 target;
            //Raycasting to navmesh. If we can go there we will.
            bool hasHit = RaycastNavMesh(out target);
            if (hasHit)
            {
                if(!GetComponent<Mover>().CanMoveTo(target)) return false;

                if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
                {   
                    //Sets the destination to our raycast point. (Point where ray collided with terrain)
                    //We moved this script and added the GetComponent<mover> so we could access this component since we are in a diff file. 
                    GetComponent<Mover>().StartMoveAction(target, 1f);
                }
                SetCursor(CursorType.Movement);
                return true;
            }
            return false;
        }

        //This function will be used to see if where our cursor is, has NavMesh or not.
        private bool RaycastNavMesh(out Vector3 target)
        {   
            //This is being set as a blank vector3. We do this incase we return false because
            //we still have to return an out.
            target = new Vector3();
            //Raycast to terrain.
            RaycastHit hit;
            //Telling hit to store our information from ray. When this happens Raycast is a bool that will be set to True
            bool hasHit = Physics.Raycast(GetMouseRay(), out hit);
            if(!hasHit) return false;
            //Find nearest navmesh point.
            NavMeshHit navMeshHit;
            bool hasCastToNavMesh = NavMesh.SamplePosition(
                hit.point, out navMeshHit, maxNavMeshProjectionDistance, NavMesh.AllAreas);
            if(!hasCastToNavMesh) return false;

            //Return true if found.
            target = navMeshHit.position;

            return true;
        }

        //NOTE this SetCursor is our function. The Cursor.SetCursor is a Unity function.
        private void SetCursor(CursorType type)
        {
            //This will return with the mapping type from the function below. 
            //Ex movement.
            CursorMapping mapping = GetCursorMapping(type);
            Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
        }

        private CursorMapping GetCursorMapping(CursorType type)
        {
            foreach(CursorMapping mapping in cursorMappings)
            {
                if(mapping.type == type)
                {
                    return mapping;
                }
            }
            return cursorMappings[0];
        }

        private static Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }
    }
}