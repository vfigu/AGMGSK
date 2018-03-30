/*  
    Copyright (C) 2017 G. Michael Barnes
 
    The file NPAgent.cs is part of AGMGSKv9 a port and update of AGXNASKv8 from
    MonoGames 3.5 to MonoGames 3.6  

    AGMGSKv9 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


#region Using Statements
using System;
using System.IO;  // needed for trace()'s fout
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace AGMGSKv9 {

/// <summary>
/// A non-playing character that moves.  Override the inherited Update(GameTime)
/// to implement a movement (strategy?) algorithm.
/// Distribution NPAgent moves along an "exploration" path that is created by the
/// from int[,] pathNode array.  The exploration path is traversed in a reverse path loop.
/// Paths can also be specified in text files of Vector3 values, see alternate
/// Path class constructors.
/// 
/// 1/20/2016 last changed
/// </summary>
public class NPAgent : Agent {
    private bool pathFinding = true;
    private bool reset = false;
    private KeyboardState oldKeyboardState;
    private NavNode nextGoal;
    private NavNode Goal;
    private NavNode resume;
        private Path path;
    private Path treasurePath;
    public int spacing;
    private int snapDistance = 1;  // this should be a function of step and stepSize
	// If using makePath(int[,]) set WayPoint (x, z) vertex positions in the following array
	private int[,] pathNode = { {505, 490}, {500, 500}, {490, 505},  // bottom, right
								{435, 505}, {425, 500}, {420, 490},  // bottom, middle
								{420, 450}, {425, 440}, {435, 435},  // middle, middle
                                {490, 435}, {500, 430}, {505, 420},  // middle, right
								{505, 105}, {500,  95}, {490,  90},  // top, right
                                {110,  90}, {100,  95}, { 95, 105},  // top, left
								{ 95, 480}, {100, 490}, {110, 495},  // bottom, left
								{495, 480} };						 // loop return
    private APath aPath;


   /// <summary>
   /// Create a NPC. 
   /// AGXNASK distribution has npAgent move following a Path.
   /// </summary>
   /// <param name="theStage"> the world</param>
   /// <param name="label"> name of </param>
   /// <param name="pos"> initial position </param>
   /// <param name="orientAxis"> initial rotation axis</param>
   /// <param name="radians"> initial rotation</param>
   /// <param name="meshFile"> Direct X *.x Model in Contents directory </param>

   public NPAgent(Stage theStage, string label, Vector3 pos, Vector3 orientAxis, 
      float radians, string meshFile, string objOfInterest = null, List<Model3D> objList = null,
      float boundingMultiplier = 1.05f)
      : base(theStage, label, pos, orientAxis, radians, meshFile, objOfInterest, objList, boundingMultiplier)
      {  // change names for on-screen display of current camera
      first.Name =  "npFirst";
      follow.Name = "npFollow";
      above.Name =  "npAbove";
      IsCollidable = true;
      if(IsCollidable) stage.Collidable.Add(agentObject);
      spacing = theStage.Spacing;
            
      aPath = new APath(theStage, agentObject, objList);
      // path is built to work on specific terrain, make from int[x,z] array pathNode
      path = new Path(stage, pathNode, Path.PathType.LOOP); // continuous search path

      //treasurePath = new Path(stage, objList, Path.PathType.LOOP); // continuous search path

            stage.Components.Add(path);
      //stage.Components.Add(treasurePath);
      nextGoal = path.NextNode;  // get first path goal
      //agentObject.turnToFace(nextGoal.Translation);  // orient towards the first path goal
		// set snapDistance to be a little larger than step * stepSize
		snapDistance = (int) (1.5 * (agentObject.Step * agentObject.StepSize));
      }   

   /// <summary>
   /// Simple path following.  If within "snap distance" of a the nextGoal (a NavNode) 
   /// move to the NavNode, get a new nextGoal, turnToFace() that goal.  Otherwise 
   /// continue making steps towards the nextGoal.
   /// </summary>
   public override void Update(GameTime gameTime) {
        KeyboardState keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.N) && !oldKeyboardState.IsKeyDown(Keys.N) && !pathFinding) { 
            // toggles path finding on
            pathFinding = true;
            System.Diagnostics.Debug.WriteLine("N: on");
            nextGoal = path.CurrentNode;
        }
        else if (keyboardState.IsKeyDown(Keys.N) && !oldKeyboardState.IsKeyDown(Keys.N) && pathFinding) { 
            // toggles path finding off
            pathFinding = false;
            reset = false;
            System.Diagnostics.Debug.WriteLine("N: off");
            Goal = aPath.Closest; // searches for closest treasure and creates Goal Node
            treasurePath = aPath.createPath(Goal); // creates A* Path for Goal node
            nextGoal = treasurePath.NextNode;
            resume = new NavNode(agentObject.Translation);
        }
        
        if(pathFinding || this.TreasureList.Count <= 0){
            nextGoal = path.CurrentNode;
            agentObject.turnToFace(nextGoal.Translation);  // adjust to face nextGoal every move
		    //agentObject.turnTowards(nextGoal.Translation);
		    // See if at or close to nextGoal, distance measured in 2D xz plane
		    float distance = Vector3.Distance(
			    new Vector3(nextGoal.Translation.X, 0, nextGoal.Translation.Z),
			    new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z));
		    stage.setInfo(15, stage.agentLocation(this));
            stage.setInfo(16,
			    string.Format("          nextGoal ({0:f0}, {1:f0}, {2:f0})  distance to next goal = {3,5:f2})", 
				nextGoal.Translation.X/stage.Spacing, nextGoal.Translation.Y, nextGoal.Translation.Z/stage.Spacing, distance) );
            if (distance  <= snapDistance)  {  
                // snap to nextGoal and orient toward the new nextGoal 
                nextGoal = path.NextNode;
                //agentObject.turnTowards(nextGoal.Translation);
            }
        }
        else{ // if there are still more treasures NPAgent follows A* Path
            agentObject.turnToFace(nextGoal.Translation);  // adjust to face nextGoal every move
            //agentObject.turnTowards(nextGoal.Translation);
            // See if at or close to nextGoal, distance measured in 2D xz plane
            float distance = Vector3.Distance(
                new Vector3(nextGoal.Translation.X, 0, nextGoal.Translation.Z),
                new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z));
            stage.setInfo(15, stage.agentLocation(this));
            stage.setInfo(16,
                string.Format("          nextGoal ({0:f0}, {1:f0}, {2:f0})  distance to next goal = {3,5:f2})",
                nextGoal.Translation.X / stage.Spacing, nextGoal.Translation.Y, nextGoal.Translation.Z / stage.Spacing, distance));

            if (distance <= snapDistance)
            {
                // snap to nextGoal and orient toward the new nextGoal 
                //nextGoal = path.NextNode;
                nextGoal = treasurePath.NextNode;
                // agentObject.turnToFace(nextGoal.Translation);
            }

            if(!reset && Goal.DistanceBetween(agentObject.Translation, spacing) < 2) {
                // quick fix for occasionly getting stuck on walls
                // if it gets close enough to tag the treaure it turns back
                // it creates an A* Path to the previous pathfinding node 
                Goal = resume;
                treasurePath = aPath.createPath(Goal);
                reset = true;
            }
            else if(reset && Goal.DistanceBetween(agentObject.Translation, spacing) < 2) {
                // if the NPAgent returns to previous path node it returns pathfinding mode
                reset = false;
                pathFinding = true;
                nextGoal = path.CurrentNode;
            }
        }
            
        oldKeyboardState = keyboardState;
        
        base.Update(gameTime);  // Agent's Update();
      }
   } 
}
