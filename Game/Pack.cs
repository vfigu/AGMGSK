/*  
    Copyright (C) 2017 G. Michael Barnes
 
    The file Pack.cs is part of AGMGSKv9 a port and update of AGXNASKv8 from
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
/// Pack represents a "flock" of MovableObject3D's Object3Ds.
/// Usually the "player" is the leader and is set in the Stage's LoadContent().
/// With no leader, determine a "virtual leader" from the flock's members.
/// Model3D's inherited List<Object3D> instance holds all members of the pack.
/// 
/// 2/1/2016 last changed
/// </summary>
public class Pack : MovableModel3D {   
   Object3D leader;
   int[] packZones = new int[4] { 6000, 12000, 18000, 24000 };

/// <summary>
/// Construct a pack with an Object3D leader
/// </summary>
/// <param name="theStage"> the scene </param>
/// <param name="label"> name of pack</param>
/// <param name="meshFile"> model of a pack instance</param>
/// <param name="xPos, zPos">  approximate position of the pack </param>
/// <param name="aLeader"> alpha dog can be used for flock center and alignment </param>
   public Pack(Stage theStage, string label, string meshFile, int nDogs, int xPos, int zPos, Object3D theLeader, float boundingMultiplier = 1.05f)
      : base(theStage, label, meshFile, boundingMultiplier) {
      isCollidable = true;
		random = new Random();
      leader = theLeader;
		int spacing = stage.Spacing;
		// initial vertex offset of dogs around (xPos, zPos)
		int [,] position = { {0, 0}, {10, -7}, {-20, -4}, {-10, 4}, {15, 7} };
		for( int i = 0; i < position.GetLength(0); i++) {
			int x = xPos + position[i, 0];
			int z = zPos + position[i, 1];

            //Unnecessary to have random sizes for cars
            //float scale = (float)(0.5 + random.NextDouble());
            float scale = 1f;
            addObject( new Vector3(x * spacing, stage.surfaceHeight(x, z), z * spacing),
						  new Vector3(0, 1, 0), 0.0f,
			              new Vector3(scale, scale, scale));
        }
      }

   /// <summary>
   /// Each pack member's orientation matrix will be updated.
   /// Distribution has pack of dogs moving randomly.  
   /// Supports leaderless and leader based "flocking" 
   /// </summary>      
   public override void Update(GameTime gameTime) {
        //Original implementation
        // if (leader == null) need to determine "virtual leader from members"
        float angle = 0.5f;
        foreach (Object3D obj in instance) {
                float distance = Vector3.Distance(
                    new Vector3(Leader.Translation.X, 0, Leader.Translation.Z),
                    new Vector3(obj.Translation.X, 0, obj.Translation.Z));
                //System.Diagnostics.Debug.WriteLine("Distance from Leader: " + distance);
                if (stage.PercentPack == 0)
                {
                    RandomWalk(obj, angle);
                }
                else if(stage.PercentPack == 33)
                {
                    PackLightly(obj, angle, distance);
                }
         //obj.Yaw = 0.0f;
         //// change direction 4 time a second  0.07 = 4/60
         //  if ( random.NextDouble() < 0.07) {
         //       if (random.NextDouble() < 0.5) obj.Yaw -= angle; // turn left
         //       else  obj.Yaw += angle; // turn right
         //  }
         //obj.updateMovableObject();
         //stage.setSurfaceHeight(obj);
         }
      base.Update(gameTime);  // MovableMesh's Update(); 
      }
     //end of original implementation

   public Object3D Leader {
      get { return leader; }
      set { leader = value; }}

   public void RandomWalk(Object3D obj,float angle){
      obj.Yaw = 0.0f;
      // change direction 4 time a second  0.07 = 4/60
        if (random.NextDouble() < 0.07){
             if (random.NextDouble() < 0.5) obj.Yaw -= angle; // turn left
             else obj.Yaw += angle; // turn right
      }
      obj.updateMovableObject();
      stage.setSurfaceHeight(obj);
      }
   public void PackLightly(Object3D obj, float angle, float distance){
            obj.Yaw = 0.0f;
            // change direction 4 time a second  0.07 = 4/60
            if (random.NextDouble() < 0.04)
            {
                if (distance >= 24000.0f)                                                         //Cohesion Range
                    obj.turnToFace(Leader.Translation);
                else if (distance < 24000.0f && distance >= 18000.0f) {                           //between alignment and coheasion Range
                    if (random.NextDouble() > 0.9)//10% split
                    {
                        if (random.NextDouble() < 0.6)
                        {//6% Coheasion
                            obj.turnToFace(Leader.Translation);
                            System.Diagnostics.Debug.WriteLine("Distance from Leader: " + distance + " 6 % Coheasion");
                        }
                        else { 
                            obj.Yaw = Leader.Yaw;//4% Alignment
                        System.Diagnostics.Debug.WriteLine("Distance from Leader: " + distance + " 4 % Align");
                        }
                    }
                    else//90% random
                    {
                        if (random.NextDouble() < 0.5) {
                        
                            obj.Yaw -= angle; // turn left
                            //System.Diagnostics.Debug.WriteLine("90%Random left");
                        }
                        else { 
                            obj.Yaw += angle; // turn right
                            //System.Diagnostics.Debug.WriteLine("90%Random Right");
                        }
                    }
                }
                else if (distance < 18000.0f && distance >= 12000.0f){                            //Alignment Range
                    if (random.NextDouble() < 0.3)
                    {
                        if (random.NextDouble() < 0.7)
                        {
                            obj.Yaw = Leader.Yaw;
                            System.Diagnostics.Debug.WriteLine("Distance from Leader: " + distance + " 15% Align" + Leader.Forward);
                        }
                        else
                        {
                            obj.turnToFace(Leader.Translation);
                            System.Diagnostics.Debug.WriteLine("Distance from Leader: " + distance + " 15 % Coheasion");
                        }
                    }
                    else
                    {
                        if (random.NextDouble() < 0.5)
                        {

                            obj.Yaw -= angle; // turn left
                            //System.Diagnostics.Debug.WriteLine("90%Random left");
                        }
                        else
                        {
                            obj.Yaw += angle; // turn right
                            //System.Diagnostics.Debug.WriteLine("90%Random Right");
                        }
                    }
                }
                else if (distance < 12000.0f && distance > 6000.0f)                             //Between Separation and Allignment
                {
                    if (random.NextDouble() < 0.3)//30% Separate
                    {
                        obj.turnToFace(Leader.Translation);
                        obj.Yaw += (float)Math.PI;
                        System.Diagnostics.Debug.WriteLine("Distance from Leader: " + distance + " 30% Separate");
                    }
                    else//70%
                    {
                        if (random.NextDouble() < 0.5)//35%Align
                        {
                            obj.Yaw = Leader.Yaw;
                            //obj.Yaw += (float)Math.PI;
                            System.Diagnostics.Debug.WriteLine("Distance from Leader: " + distance + " 25% Align" + Leader.Forward);

                        }
                        else//35%
                        {
                            if (random.NextDouble() < 0.5)//17%left
                            {

                                obj.Yaw -= angle; // turn left
                                                  //System.Diagnostics.Debug.WriteLine("90%Random left");
                            }
                            else//17% right
                            {
                                obj.Yaw += angle; // turn right
                                                  //System.Diagnostics.Debug.WriteLine("90%Random Right");
                            }
                        }
                    }
                }
                else if (distance <= 6000) {                                                      //Separation Range
                    obj.turnToFace(Leader.Translation);
                    obj.Yaw += (float)Math.PI;
                    System.Diagnostics.Debug.WriteLine("Distance from Leader: " + distance + " Separate");
                }
            }
            obj.updateMovableObject();
            stage.setSurfaceHeight(obj);
        }
    }

}
