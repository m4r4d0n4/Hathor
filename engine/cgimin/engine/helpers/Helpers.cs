using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cgimin.engine.helpers
{
    class Helpers
    {

        public static bool SphereAARectangleIntersect(Vector3 sphereCenter, float sphereRadius, Vector3 rectMin, Vector3 rectMax)
        {
            if (sphereCenter.X + sphereRadius < rectMin.X) return false;
            if (sphereCenter.Y + sphereRadius < rectMin.Y) return false;
            if (sphereCenter.Z + sphereRadius < rectMin.Z) return false;

            if (sphereCenter.X - sphereRadius > rectMax.X) return false;
            if (sphereCenter.Y - sphereRadius > rectMax.Y) return false;
            if (sphereCenter.Z - sphereRadius > rectMax.Z) return false;

            // unfinished

            return true;
        }


    }
}
