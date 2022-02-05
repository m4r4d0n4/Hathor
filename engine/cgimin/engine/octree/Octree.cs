using cgimin.engine.camera;
using cgimin.engine.helpers;
using OpenTK;
using System;
using System.Collections.Generic;

namespace cgimin.engine.octree
{
    public class Octree
    {
        private static int drawCountStatistic;

        private static int maxIterationDepth = 5;

        internal List<Octree> children;

        internal Vector3 bMin;
        internal Vector3 bMax;

        internal Vector3 mid;
        internal float midRadius;

        internal List<OctreeEntity> enteties;

        private static List<OctreeEntity> transparentEntities;

        private int iteration;

        public Octree(Vector3 boundsMin, Vector3 boundsMax, int iterationDepth = 1)
        {
            iteration = iterationDepth;

            bMin = boundsMin;
            bMax = boundsMax;

            mid = (bMin + bMax) / 2;
            midRadius = (mid - bMax).Length;

            children = new List<Octree>();
            for (int i = 0; i < 8; i++) children.Add(null);
        }


        public void AddEntity(OctreeEntity entity)
        {

            if (iteration == 1)
            {
                if (enteties == null) enteties = new List<OctreeEntity>();
                enteties.Add(entity);
            }

            // extracting position from object transform matrix
            Vector3 pos = new Vector3(entity.Transform.M41, entity.Transform.M42, entity.Transform.M43);
            float radius = entity.Object3d.radius;

            if (Helpers.SphereAARectangleIntersect(pos, radius, bMin, bMax))
            {

                if (iteration == maxIterationDepth)
                {
                    // only add entity when at max iteration depth
                    if (enteties == null) enteties = new List<OctreeEntity>();
                    enteties.Add(entity);
                }
                else
                {
                    for (int i = 0; i < 8; i++)
                    {
                        // i is the index of the sub-bounding box
                        Vector3 dif = (bMax - bMin) / 2;
                        Vector3 bMinSub = bMin;
                        Vector3 bMaxSub = (bMin + bMax) / 2;

                        if (i % 2 == 1) { bMinSub.X += dif.X; bMaxSub.X += dif.X; }
                        if ((i / 2) % 2 == 1) { bMinSub.Y += dif.Y; bMaxSub.Y += dif.Y; }
                        if (i >= 4) { bMinSub.Z += dif.Z; bMaxSub.Z += dif.Z; }

                        // if object is inside the sub-boundary..
                        if (Helpers.SphereAARectangleIntersect(pos, radius, bMinSub, bMaxSub))
                        {
                            // create a new octree for it and add
                            if (children[i] == null) children[i] = new Octree(bMinSub, bMaxSub, iteration + 1);
                            children[i].AddEntity(entity);
                        }
                    }
                }
            }
        }


        public void Draw()
        {
            if (iteration == 1)
            {
                drawCountStatistic = 0;
                int len = enteties.Count;
                for (int i = 0; i < len; i++) enteties[i].drawn = false;

                transparentEntities = new List<OctreeEntity>();
            }

            if (iteration == maxIterationDepth)
            {
                int len = enteties.Count;
                for (int i = 0; i < len; i++)
                {
                    if (enteties[i].drawn == false)
                    {

                        if (enteties[i].Material.isTransparent)
                        {
                            transparentEntities.Add(enteties[i]);
                        }
                        else
                        {
                            enteties[i].Object3d.Transformation = enteties[i].Transform;
                            enteties[i].Material.DrawWithSettings(enteties[i].Object3d, enteties[i].MaterialSetting);
                        }

                        enteties[i].drawn = true;
                        drawCountStatistic++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    if (children[i] != null && Camera.SphereIsInFrustum(children[i].mid, children[i].midRadius))
                    {
                        children[i].Draw();
                    }
                }
            }

            if (iteration == 1)
            {
                Console.WriteLine(drawCountStatistic);
            }

        }


        public static void DrawTransparentEntities()
        {
            foreach (OctreeEntity entety in transparentEntities)
            {
                entety.distanceToCam = (new Vector3(entety.Transform.M41, entety.Transform.M42, entety.Transform.M43) - Camera.Position).Length;
            }

            transparentEntities.Sort((x, y) => y.distanceToCam.CompareTo(x.distanceToCam));

            foreach (OctreeEntity entety in transparentEntities)
            {
                entety.Object3d.Transformation = entety.Transform;
                entety.Material.DrawWithSettings(entety.Object3d, entety.MaterialSetting);
            }
        }



    }
}
