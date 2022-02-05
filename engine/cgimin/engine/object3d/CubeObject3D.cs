using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace cgimin.engine.object3d
{
    public class CubeObject3D : BaseObject3D
    {

        public CubeObject3D() : base()  
        {
           

            AddTriangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 1),
                        new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1));


            AddTriangle(new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 2, 0), new Vector3(1, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 1),
                        new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1));


            CreateVAO();

        }


    }
}
