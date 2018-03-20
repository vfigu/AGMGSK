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
    class TreasureMaker
    {
        public int count = 1;
        public int MAX = 4;
        private string name;
        private List<Model3D> treasures = new List<Model3D>();
        private int[,] locations = { {500, 25}, {447, 453}, {150, 460}, {330, 320}, {10, 10} };

        public TreasureMaker(Stage theStage, string label, string meshFile, string altMeshFile = null,
            float boundingMultiplier = 1.05f, int nTreasures = 1)
        {
            name = label;

            if (nTreasures > 1 && nTreasures <= MAX) count = nTreasures;
            else if (nTreasures > MAX) count = nTreasures;
            
            // creates a Treasure (modded Model3D) for each number of treasures
            for(int i = 0; i < count; i++)
                treasures.Add(new Treasure(theStage, String.Format("{0}.{1}", label, treasures.Count),
                    meshFile, altMeshFile, locations[i,0], locations[i,1], boundingMultiplier));
        }

        public string Name
        {
            get { return name; }
        }

        public List<Model3D> Treasures
        {
            get { return treasures; }
            set { treasures = value; }
        }
    }
}
