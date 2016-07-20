using System.Text;
using RMA.Rhino;
using RMA.OpenNURBS;
using System;
using System.Collections.Generic;
namespace zAssembly
{
    class BooleanSubtract
    {
        //two breps
        IOnBrep[] target, tool;
        OnBrep[] subtract;
        public OnBrep sub;


        public BooleanSubtract(OnBrep tar, OnBrep tul)
        {
            
            //size is the number of breps - not anything
            target = new IOnBrep[1];
            target[0] = tar;
            tool = new IOnBrep[1];
            tool[0] = tul;
            bool boo1 = true;
            Arrayint m = new Arrayint();

            //can implement check for manifoldness
            bool succ = RhUtil.RhinoBooleanDifference(target, tool, 0.1, ref boo1, out subtract, ref m);
            sub = subtract[0];
        }
    }
}
