using System;
using System.IO;
using System.Diagnostics;
using OpenTK;
using System.Collections.Generic;
using System.Globalization;

namespace cgimin.engine.object3d
{
    public class ObjLoaderObject3D : BaseObject3D
    {

        public ObjLoaderObject3D(String filePath, float scale = 1.0f, bool doAverageTangets = false) : base()
        {

            List<Vector3> v = new List<Vector3>();
            List<Vector2> vt = new List<Vector2>();
            List<Vector3> vn = new List<Vector3>();

            var input = File.ReadLines(filePath);

            foreach (string line in input)
            {
                string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0)
                {
                    if (parts[0] == "v") v.Add(new Vector3(float.Parse(parts[1], CultureInfo.InvariantCulture) * scale, float.Parse(parts[2], CultureInfo.InvariantCulture) * scale, float.Parse(parts[3], CultureInfo.InvariantCulture) * scale));
                    if (parts[0] == "vt") vt.Add(new Vector2(float.Parse(parts[1], CultureInfo.InvariantCulture), 1.0f - float.Parse(parts[2], CultureInfo.InvariantCulture)));
                    if (parts[0] == "vn") vn.Add(new Vector3(float.Parse(parts[1], CultureInfo.InvariantCulture), float.Parse(parts[2], CultureInfo.InvariantCulture), float.Parse(parts[3], CultureInfo.InvariantCulture)));

                    if (parts[0] == "f")
                    {
                        string[] triIndicesV1 = parts[1].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] triIndicesV2 = parts[2].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] triIndicesV3 = parts[3].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                        AddTriangle(v[Convert.ToInt32(triIndicesV1[0]) - 1], v[Convert.ToInt32(triIndicesV2[0]) - 1], v[Convert.ToInt32(triIndicesV3[0]) - 1],
                                    vn[Convert.ToInt32(triIndicesV1[2]) - 1], vn[Convert.ToInt32(triIndicesV2[2]) - 1], vn[Convert.ToInt32(triIndicesV3[2]) - 1],
                                    vt[Convert.ToInt32(triIndicesV1[1]) - 1], vt[Convert.ToInt32(triIndicesV2[1]) - 1], vt[Convert.ToInt32(triIndicesV3[1]) - 1]);

                    }
                }
            }

            if (doAverageTangets == true) AverageTangents();

            CreateVAO();

        }


    }
}
