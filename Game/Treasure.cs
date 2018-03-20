#region Using Statements
using System;
using System.IO;  // needed for trace()'s fout
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace AGMGSKv9
{
    class Treasure : Model3D
    {
        public string altMesh;

        public Treasure(Stage theStage, string label, string meshFile, string altMeshFile = null, 
            int xPos = 447, int zPos = 453, float boundingMultiplier = 1.05f)
            : base(theStage, label, meshFile, boundingMultiplier) {
            altMesh = altMeshFile;

            //Unnecessary to have random sizes for cars
            //float scale = (float)(0.5 + random.NextDouble());
            float scale = 1.5f;

            isCollidable = true;
            //random = new Random();
            int spacing = theStage.Spacing;
            addObject(new Vector3(xPos * spacing, theStage.surfaceHeight(xPos, zPos), zPos * spacing),
                              new Vector3(0, 1, 0), 0.0f,
                              new Vector3(scale, scale, scale));
        }

        public override void removeObject(Object3D obj3d)
        {
            obj3d.model.AlterMesh(altMesh);
            this.removeCollidable(obj3d);
            //this.removeInstance(obj3d);
        }
        /// <summary>
        /// Each pack member's orientation matrix will be updated.
        /// Distribution has pack of dogs moving randomly.  
        /// Supports leaderless and leader based "flocking" 
        /// </summary>      
    }
}