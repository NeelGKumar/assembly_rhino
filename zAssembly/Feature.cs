using System;
using System.Collections.Generic;
using System.Text;
using RMA.Rhino;
using RMA.OpenNURBS;


namespace zAssembly
{
    class Feature
    {
        Part on;
        int type_id;
        //OnXform location;
        Feature matedTo;
        OnCylinder cylinder;
        //OnBrep feature;
        OnPlane plane;
        On3dPoint origin;

        public Feature(Part P, int id, Feature mate, OnCylinder cyl, OnPlane pl, On3dPoint pt)
        {
            on = P;
            type_id = id;
            //location = loc;
            matedTo = mate;
            cylinder = cyl;
            plane = pl;
            origin = pt;
        }
    }
}
