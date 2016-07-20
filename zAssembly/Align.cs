using System;
using System.Collections.Generic;
using System.Text;
using RMA.Rhino;
using RMA.OpenNURBS;
namespace zAssembly
{
    class Align
    {
        public Feature f_plane, f_plane1;
        public OnXform xform_t, xform_r;


        public Align(Part part1, Part part2)
        {
            //two breps            
            OnBrep brep = new OnBrep(part1.brep);
            OnBrep brep1 = new OnBrep(part2.brep);

            
            //Select the two faces to align
            MRhinoGetObject gf = new MRhinoGetObject();
            gf.SetCommandPrompt("Select two face");
            gf.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object);
            gf.GetObjects(2, 2);



            //Get the selected face
            IRhinoObjRef faceref = gf.Object(0);
            IOnBrepFace face = faceref.Face();
            IRhinoObjRef faceref1 = gf.Object(1);
            IOnBrepFace face1 = faceref1.Face();


            // Pick first point on the surface. Constrain picking to the face. get the closest on face
            MRhinoGetPoint gp = new MRhinoGetPoint();
            gp.SetCommandPrompt("Select points on surface1");
            gp.Constrain(face);
            gp.GetPoint();

            On3dPoint pt = gp.Point();

            //get normal at this point for the face
            double u = 0.0, v = 0.0;
            face.GetClosestPoint(pt, ref u, ref v);
            On3dVector n = face.NormalAt(u, v);
            On3dPoint p = face.PointAt(u, v);
            OnPlane plane = new OnPlane(p, n);

            // Pick second point on the surface. Constrain picking to the face.
            MRhinoGetPoint gp1 = new MRhinoGetPoint();
            gp1.SetCommandPrompt("Select points on surface2");
            gp1.Constrain(face1);
            gp1.GetPoint();

            On3dPoint pt1 = gp1.Point();
            double u1 = 0.0, v1 = 0.0;
            face1.GetClosestPoint(pt1, ref u1, ref v1);
            On3dVector n1 = face1.NormalAt(u1, v1);
            On3dPoint p1 = face1.PointAt(u1, v1);
            OnPlane plane1 = new OnPlane(p1, n1);


            // Unitize the input vectors
            n.Unitize();
            n1.Unitize();
            
            xform_t = new OnXform();
            xform_t.Translation(p1.x - p.x, p1.y - p.y, p1.z - p.z);
            
            On3dVector cross = OnUtil.ON_CrossProduct(n, n1);
            cross.Unitize();


            double dot = OnUtil.ON_DotProduct(n, n1);


            // Force the dot product of the two input vectors to 
            // fall within the domain for inverse cosine, which 
            // is -1 <= x <= 1. This will prevent runtime 
            // "domain error" math exceptions.
            dot = (dot < -1.0 ? -1.0 : (dot > 1.0 ? 1.0 : dot));
            double ang = System.Math.Acos(dot);
            //cross.Reverse();
            ang = System.Math.PI - ang;

            xform_r = new OnXform();
            xform_r.Rotation(ang, cross, p1);
            

            //forming feature and parts
            Feature f_plane_t = new Feature(part1, 2, null, null, plane, pt);
            f_plane1 = new Feature(part2, 2, f_plane_t, null, plane1, pt1);
            f_plane = new Feature(part1, 2, f_plane1, null, plane, pt);
            


        }


    }
}
