using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Control
{
    public class PatrolPath : MonoBehaviour
    {
        const float wayPontGizmoRadius = 0.3f;

        //This function is going to help us visualize our waypoints
        private void OnDrawGizmos() {
            //We are looping through so we can grab each waypoints in the patrol path.
            for(int i = 0; i < transform.childCount; i++)
            {
                //This will get our next index so we can draw lines form one to the next. 
                int j = GetNextIndex(i);
                //This turns each waypoint into a sphere.
                Gizmos.DrawSphere(GetWaypoint(i), wayPontGizmoRadius);
                //This draws the lines from the current waypoint to the next one. 
                Gizmos.DrawLine(GetWaypoint(i), GetWaypoint(j));
            }
        }

        public int GetNextIndex(int i)
        {
            //If we are at the last waypoint, return 0 so we can draw a line from the
            //last one to the first one. 
            if(i + 1 == transform.childCount)
            {
                return 0;
            }
            return i + 1;
        }

        public Vector3 GetWaypoint(int i)
        {
            return transform.GetChild(i).position;
        }
    }

}