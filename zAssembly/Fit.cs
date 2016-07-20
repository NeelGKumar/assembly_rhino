using System;
using System.Collections.Generic;
using System.Text;
using RMA.Rhino;
using RMA.OpenNURBS;

namespace zAssembly
{
    class Fit
    {

        public Feature pin, hole;
        public OnCylinder pin_cyl, hole_cyl;
        public OnXform xform_t, xform_r;

        public Fit(Part part1, Part part2)
        {

            //two breps            
            OnBrep brep = new OnBrep(part1.brep);
            OnBrep brep1 = new OnBrep(part2.brep);

            //first face
            //Select a surface
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





            //get radius and height
            MRhinoGetNumber gn = new MRhinoGetNumber();
            gn.SetCommandPrompt("radius");
            gn.GetNumber();


            double r = gn.Number();

            MRhinoGetNumber gh = new MRhinoGetNumber();
            gh.SetCommandPrompt("height");
            gh.GetNumber();

            double h = gh.Number();


            //create a cylinder
            OnPlane plane = new OnPlane(p, n);
            OnCircle circle = new OnCircle(plane, r);
            pin_cyl = new OnCylinder(circle, h);






            // Pick second point on the surface. Constrain picking to the face.
            MRhinoGetPoint gp1 = new MRhinoGetPoint();
            gp1.SetCommandPrompt("Select points on surface2");
            gp1.Constrain(face1);
            gp1.GetPoint();

            On3dPoint pt1 = gp1.Point();
            double u1 = 0.0, v1 = 0.0;
            face1.GetClosestPoint(pt1, ref u1, ref v1);
            On3dVector n1 = face1.NormalAt(u1, v1);
            n1.Reverse();
            On3dPoint p1 = face1.PointAt(u1, v1);


            //create a cylinder
            OnPlane plane1 = new OnPlane(p1, n1);
            OnCircle circle1 = new OnCircle(plane1, r);
            hole_cyl = new OnCylinder(circle1, h);




            // Unitize the input vectors
            n.Unitize();
            n1.Unitize();
            On3dVector nh = new On3dVector(h * n.x, h * n.y, h * n.z);
            On3dPoint ph = p + nh;


            On3dVector n1h = new On3dVector(h * n1.x, h * n1.y, h * n1.z);
            On3dPoint p1h = p1 + n1h;


            xform_t = new OnXform();
            xform_t.Translation(p1h.x - ph.x, p1h.y - ph.y, p1h.z - ph.z);
            //OnBrep b = new OnBrep(brep);
            //OnBrep b1 = new OnBrep(brep_cyl);


            //b.Transform(xform);
            //b1.Transform(xform);

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
            xform_r.Rotation(ang, cross, p1h);
            //b.Transform(xform1);
            //b1.Transform(xform1);


            //forming feature and parts
            pin = new Feature(part1, 1, null, pin_cyl, plane, pt);
            hole = new Feature(part2, 1, pin, hole_cyl, plane1, pt1);
            Feature pin_t = new Feature(part1, 1, hole, pin_cyl, plane, pt);
            pin = pin_t;



        }



    }
}
