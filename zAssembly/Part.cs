using System;
using System.Collections.Generic;
using System.Text;
using RMA.Rhino;
using RMA.OpenNURBS;

namespace zAssembly
{
    class Part
    {


        public OnBrep brep;
        public OnPlane splane;

        public Part(IOnBrep mem, OnPlane plane)
        {
            brep = new OnBrep(mem);
            OnBrepVertex p = brep.m_V[0];
            //On3dVector v = p.point - OnUtil.On_origin;
            //cord = new OnXform(OnUtil.On_origin, OnUtil.On_xaxis, OnUtil.On_yaxis, OnUtil.On_zaxis);
            //cord.Translation(v);
            plane.SetOrigin(p.point);
            splane = plane;
        }

        public OnBrep getBrep()
        {
            return brep;
        }

        public OnPlane getPlane()
        {
            return splane;
        }


    }
}
