using System.Text;
using RMA.Rhino;
using RMA.OpenNURBS;
using System;
using System.Collections.Generic;
namespace zAssembly
{
    class BooleanAdd
    {
        //two breps
        IOnBrep[] target;
        OnBrep[] add;
        public OnBrep union; 


        public BooleanAdd(OnBrep tar, OnBrep tul)
        {
            
            //size is the number of breps - not anything
            target = new IOnBrep[2];
            target[0] = tar;
            target[1] = tul;
            bool boo = true;
            Arrayint m = new Arrayint();
            bool succ = RhUtil.RhinoBooleanUnion(target, 0.01, ref boo, out add);
            union = add[0];


        }
    }
}
