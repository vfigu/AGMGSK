#region Using Statements
using System;
using System.IO;  // needed for trace()'s fout
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace AGMGSKv9 {
    class APath {
        private Stage stage;
        private int spacing;
        private Object3D agentObject;
        private List<Model3D> treasures;

        public APath(Stage theStage, Object3D agent, List<Model3D> objList) {
            stage = theStage;
            spacing = theStage.Spacing;
            agentObject = agent;
            treasures = objList;
        }

        // creates NavNode for agent's current location
        // not needed afterall
        public NavNode Start {
            get {
                NavNode node = new NavNode(new Vector3(agentObject.X,
                    stage.Terrain.surfaceHeight((int)agentObject.X / spacing,
                    (int)agentObject.Z / spacing), agentObject.Z),
                    NavNode.NavNodeEnum.WAYPOINT);
                return node;
            }
        }

        // returns the NavNode of the treasure that is closest to the agent 
        public NavNode ClosestNode(float radius)
        {
            NavNode node;
            Object3D closest = ClosestObject(radius);
            if (closest == null)
                return null;

            node = new NavNode(new Vector3(closest.X,
                stage.Terrain.surfaceHeight((int)closest.X / spacing, 
                (int)closest.Y / spacing), closest.Z),
                NavNode.NavNodeEnum.WAYPOINT);
            return node;
        }
        public Object3D ClosestObject(float radius)
        {
            Object3D closest = null;
            int X = (int)agentObject.X / spacing;
            int Z = (int)agentObject.Z / spacing;
            int x, z;

            double A = 0;
            double B = 0;
            double C = 0;
            double smallest = -1;

            for (int i = 0; i < treasures.Count; i++)
            {
                x = (int)treasures[i].Instance[0].X / spacing;
                z = (int)treasures[i].Instance[0].Z / spacing;
                A = Math.Pow(x - X, 2);
                B = Math.Pow(z - Z, 2);
                C = Math.Sqrt(A + B);

                if (C < smallest || smallest < 0) {
                    smallest = C;
                    closest = treasures[i].Instance[0];
                }
            }

            if (smallest * spacing > radius && radius > 0)
                return null;
            return closest;
        }

        // helper functions
        public bool Exists(List<NavNode> Set, NavNode target) {
            for (int i = 0; i < Set.Count; i++)
                if (Set[i].Translation == target.Translation)
                    return true;
            return false;
        }
        public bool Invalid(NavNode target) {
            if(target.Translation.X > stage.Range)
                return true;
            else if (target.Translation.Z > stage.Range)
                return true;
            if (target.Translation.X < 1)
                return true;
            else if (target.Translation.Z < 1)
                return true;
            return false;
        }

        // Wikipedia pseudo code implementation
        public Path createPath(NavNode target) {
            //stage.Terrain.Multiplier = 0;

            Path aPath = null;
            Vector3 pos = new Vector3((int)agentObject.Translation.X / spacing,
                (int)agentObject.Translation.Y, (int)agentObject.Translation.Z / spacing);
            NavNode start = new NavNode(pos);

            pos = new Vector3((int)target.Translation.X / spacing,
                (int)target.Translation.Y, (int)target.Translation.Z / spacing);
            NavNode goal = new NavNode(pos);
            NavNode current = start;
            int tentative_gScore;

            // The set of acceptable moves: up, down, left, right, diagonals
            int[,] add = { { 0, 1}, { 0,-1}, { 1, 0}, {-1, 0},
                            { 1, 1}, {-1,-1}, { 1,-1}, {-1, 1} };

            // The set of nodes already evaluated
            List<NavNode> closedSet = new List<NavNode>();

            // The set of currently discovered nodes that are not evaluated yet.
            // Initially, only the start node is known.
            List<NavNode> openSet = new List<NavNode>();
            openSet.Add(start);

            // The  set of neighbors on the current node
            List<NavNode> neighborSet = new List<NavNode>();

            // For each node, which node it can most efficiently be reached from.
            // If a node can be reached from many nodes, cameFrom will eventually contain the
            // most efficient previous step.
            NavNode[,] cameFrom = new NavNode[stage.Range + 1, stage.Range + 1]; //an empty map

            // For each node, the cost of getting from the start node to that node.
            int[,] gScore = new int[stage.Range + 1, stage.Range + 1];  // map with default value of Infinity
            for(int i = 0; i < stage.Range + 1; i++)
                for(int j = 0; j < stage.Range + 1; j++)
                    gScore[i,j] = int.MaxValue;

            // The cost of going from start to start is zero.
            gScore[(int)start.Translation.X, (int)start.Translation.Z] = 0;

            // For each node, the total cost of getting from the start node to the goal
            // by passing by that node. That value is partly known, partly heuristic.
            int[,] fScore = new int[stage.Range + 1, stage.Range + 1];  // map with default value of Infinity
            for(int i = 0; i < stage.Range + 1; i++)
                for(int j = 0; j < stage.Range + 1; j++)
                    fScore[i,j] = int.MaxValue;

            // For the first node, that value is completely heuristic.
            fScore[(int)start.Translation.X, (int)start.Translation.Z] = start.Heuristic(goal);


            while(openSet.Count > 0) {
                current = openSet[0];
                for(int i = 1; i < openSet.Count; i++) {
                    if(fScore[(int)current.Translation.X, (int)current.Translation.Z] 
                        > fScore[(int)openSet[i].Translation.X, (int)openSet[i].Translation.Z])
                        current = openSet[i];
                } // current:= the node in openSet having the lowest fScore[] value
                if(current.Translation == goal.Translation) {
                    aPath = reconstructPath(cameFrom, current);
                    return aPath;
                }

                openSet.Remove(current);
                closedSet.Add(current);


                //Finds neighbors of current node
                neighborSet.Clear();
                for(int k = 0; k < 8; k++) {
                    pos = new Vector3((int)current.Translation.X + add[k, 0], (int)goal.Translation.Y, 
                        (int)current.Translation.Z + add[k, 1] );
                    Object3D obj3d = agentObject.CollidedWith(pos*spacing);
                    NavNode neighbor = new NavNode(pos);

                    if (Invalid(neighbor))
                        continue;
                    if (Exists(closedSet, neighbor))
                        continue;

                    neighborSet.Add(neighbor);
                    if (obj3d != null) {
                        // If the neighbor is an obstacle then skip
                        if (stage.SameType(obj3d.Name, "wall") || stage.SameType(obj3d.Name, "temple")) {
                            neighborSet.Remove(neighbor);
                            if (Exists(closedSet, neighbor))
                                continue;
                            closedSet.Add(neighbor);

                            // removes neighbors of a wall to decrease chance of collision
                            //for (int j = 0; j < 8; j++)
                            //{
                            //    pos = new Vector3((int)current.Translation.X + add[j, 0], (int)goal.Translation.Y,
                            //        (int)current.Translation.Z + add[j, 1]);
                            //    obj3d = agentObject.CollidedWith(pos * spacing);
                            //    neighbor = new NavNode(pos);

                            //    if (Exists(closedSet, neighbor))
                            //        continue;
                            //    closedSet.Add(neighbor);
                            //}
                        }

                        // finishes earlier
                        //else if (stage.SameType(obj3d.Name, "treasure")) {
                        //    if (neighbor.Translation == goal.Translation) {
                        //        aPath = reconstructPath(cameFrom, current);
                        //        return aPath;
                        //    }
                        //}
                    }
                }
                
                foreach (NavNode neighbor in neighborSet) {
                    if(Exists(openSet, neighbor))
                        continue;
                    openSet.Add(neighbor); // Discover a new node

                    // The distance from start to a neighbor
                    // the "dist_between" function may vary as per the solution requirements.
                    tentative_gScore = gScore[(int)current.Translation.X, (int)current.Translation.Z]
                        + current.DistanceBetween(neighbor);


                    if(tentative_gScore >= gScore[(int)neighbor.Translation.X, (int)neighbor.Translation.Z])
                        continue;		// This is not a better path.

                    // This path is the best until now. Record it!
                    cameFrom[(int)neighbor.Translation.X,(int)neighbor.Translation.Z] = current;
                    gScore[(int)neighbor.Translation.X,(int)neighbor.Translation.Z] = tentative_gScore;
                    fScore[(int)neighbor.Translation.X, (int)neighbor.Translation.Z] 
                        = gScore[(int)neighbor.Translation.X, (int)neighbor.Translation.Z] 
                        + neighbor.Heuristic(goal);
                }
            }
            return null;
        }
        
        public Path reconstructPath(NavNode[,] cameFrom, NavNode current) {
            List<NavNode> reversePath = new List<NavNode>();
            Vector3 pos = new Vector3((int)agentObject.Translation.X / spacing,
                (int)agentObject.Translation.Y, (int)agentObject.Translation.Z / spacing);
            NavNode start = new NavNode(pos);
            int x = (int)current.Translation.X;
            int z = (int)current.Translation.Z;

            while(cameFrom[x,z].Translation != start.Translation) {
                pos = cameFrom[x, z].Translation;
                reversePath.Add(new NavNode(pos * spacing));
                x = (int)pos.X;
                z = (int)pos.Z;
            }

            Path total_path = null;
            if(reversePath.Count < 1)
                total_path = new Path(stage, start, Path.PathType.SINGLE);
            else
                total_path = new Path(stage, reversePath[reversePath.Count - 1], Path.PathType.SINGLE);

            for (int i = reversePath.Count - 1; i >= 0; i--)
                total_path.Node.Add(reversePath[i]);

            return total_path;
        }
    }
}
