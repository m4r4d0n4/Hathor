using cgimin.engine.object3d;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.cgimin.engine.postprocessing
{
    public class BasePostProcessing
    {        
        protected int Program;

        public static BaseObject3D fullscreenQuad = null;
        public BasePostProcessing()
        {

            if (fullscreenQuad == null)
            {
                fullscreenQuad = new BaseObject3D();
                fullscreenQuad.AddTriangle(new Vector3(1, -1, 0), new Vector3(-1, -1, 0), new Vector3(1, 1, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(1, 1));
                fullscreenQuad.AddTriangle(new Vector3(1, 1, 0), new Vector3(-1, -1, 0), new Vector3(-1, 1, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 1));
                fullscreenQuad.CreateVAO();
            }

        }
    }
}
