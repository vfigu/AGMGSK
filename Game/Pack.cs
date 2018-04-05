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
   //Array to hold pack zone values. DLP
   float[] packZones = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };
   
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
    //Based on stage.PercentPack adjust packing boundries. DLP
    public void SetPackBoundries (int setPack)
    {
        switch(setPack)
        {
            case 33:
                SeparationUpper = 6000.0f;
                AlignLower = 12000.0f;
                AlignUpper = 18000.0f;
                CoheasionLower = 24000.0f;
                break;
            case 66:
                SeparationUpper = 4500.0f;
                AlignLower = 9000.0f;
                AlignUpper = 13500.0f;
                CoheasionLower = 18000.0f;
                break;
            case 99:
                SeparationUpper = 3000.0f;
                AlignLower = 6000.0f;
                AlignUpper = 9000.0f;
                CoheasionLower = 12000.0f;
                break;
            default:
                break;
        }
    }

   /// <summary>
   /// Each pack member's orientation matrix will be updated.
   /// Distribution has pack of dogs moving randomly.  
   /// Supports leaderless and leader based "flocking" 
   /// </summary>      
   public override void Update(GameTime gameTime) {
        // if (leader == null) need to determine "virtual leader from members"
        //Adjust turning angle to .5 radians ~ 28 degrees. DLP
        float angle = 0.5f;
        //Call to assign packing boundries based on PercentPack. DLP
        SetPackBoundries(stage.PercentPack);
        foreach (Object3D obj in instance) {
            //Calculate distance from this pack member to the leader. DLP
            float distance = Vector3.Distance(
                new Vector3(Leader.Translation.X, 0, Leader.Translation.Z),
                new Vector3(obj.Translation.X, 0, obj.Translation.Z));
            //Based on assigned packing percentage, call the packing function or wander. DLP
            if (stage.PercentPack == 0)
                RandomWalk(obj, angle);
            else
                PackAroundLeader(obj, angle, distance);
        }
        base.Update(gameTime);  // MovableMesh's Update(); 
   }
   
   //Turned original implementation into a function. DLP
   public void RandomWalk(Object3D obj,float angle){
       obj.Yaw = 0.0f;
       // change direction 4 time a second  0.07 = 4/60
       if (random.NextDouble() < 0.07){
           if (random.NextDouble() < 0.5)
               obj.Yaw -= angle; // turn left
           else
               obj.Yaw += angle; // turn right
       }
       obj.updateMovableObject();
       stage.setSurfaceHeight(obj);
   }
   
   public void PackAroundLeader(Object3D obj, float angle, float distance){
        obj.Yaw = 0.0f;
        // change direction 4 time a second  0.07 = 4/60
        if (random.NextDouble() < 0.04)
        {
            //Cohesion Range. DLP
            if (distance >= CoheasionLower)
                obj.turnToFace(Leader.Translation);
            //Between alignment and cohesion ranges. DLP
            else if (distance < CoheasionLower && distance >= AlignUpper) {
                if (random.NextDouble() > 0.9)
                //6% Coheasion 4% Alignment.
                {
                    if (random.NextDouble() < 0.6)
                        obj.turnToFace(Leader.Translation);
                    else
                        obj.Yaw = Leader.Yaw;
                }
                //90% Random.
                else
                {
                    if (random.NextDouble() < 0.5)
                        obj.Yaw -= angle;
                    else
                        obj.Yaw += angle;
                }
            }
            //Alignment Range. DLP
            else if (distance < AlignUpper && distance >= AlignLower){
                //21% Allocation to Alignment and 9% Cohesion.
                if (random.NextDouble() < 0.3)
                {
                    if (random.NextDouble() < 0.7)
                        obj.Yaw = Leader.Yaw;
                    else
                        obj.turnToFace(Leader.Translation);
                }
                //70% Random
                else
                {
                    if (random.NextDouble() < 0.5)
                        obj.Yaw -= angle;
                    else
                        obj.Yaw += angle;
                }
            }
            //Between separation and alignment. DLP
            else if (distance < AlignLower && distance > SeparationUpper)
            {
                //30% Separation.
                if (random.NextDouble() < 0.3)
                {
                    obj.turnToFace(Leader.Translation);
                    obj.Yaw += (float)Math.PI;
                }
                //35% Alignment 35% Random.
                else
                {
                    if (random.NextDouble() < 0.5)
                        obj.Yaw = Leader.Yaw;
                    else
                    {
                        if (random.NextDouble() < 0.5)
                            obj.Yaw -= angle;
                        else
                            obj.Yaw += angle;
                    }
                }
            }
            //Separation Range. DLP
            else if (distance <= SeparationUpper)
            {
                obj.turnToFace(Leader.Translation);
                obj.Yaw += (float)Math.PI;
            }
        }
        obj.updateMovableObject();
        stage.setSurfaceHeight(obj);
        }
        //Accessors for pack zone boundaries. DLP
        public Object3D Leader
        {
            get { return leader; }
            set { leader = value; }
        }

        public float CoheasionLower
        {
            get { return packZones[3]; }
            set { packZones[3] = value; }
        }

        public float AlignUpper
        {
            get { return packZones[2]; }
            set { packZones[2] = value; }
        }

        public float AlignLower
        {
            get { return packZones[1]; }
            set { packZones[1] = value; }
        }

        public float SeparationUpper
        {
            get { return packZones[0]; }
            set { packZones[0] = value; }
        }
    }
}
