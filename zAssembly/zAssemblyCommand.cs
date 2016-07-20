using RMA.Rhino;
using RMA.OpenNURBS;
using System;
using System.Collections.Generic;

namespace zAssembly
{
    ///<summary>
    /// A Rhino.NET plug-in can contain as many MRhinoCommand derived classes as it wants.
    /// DO NOT create an instance of this class (this is the responsibility of Rhino.NET.)
    /// A command wizard can be found in visual studio when adding a new item to the project.
    /// </summary>
    public class zAssemblyCommand : RMA.Rhino.MRhinoCommand
    {
        ///<summary>
        /// Rhino tracks commands by their unique ID. Every command must have a unique id.
        /// The Guid created by the project wizard is unique. You can create more Guids using
        /// the "Create Guid" tool in the Tools menu.
        ///</summary>
        ///<returns>The id for this command</returns>
        public override System.Guid CommandUUID()
        {
            return new System.Guid("{eb7b59e0-a993-40a8-9e76-0ec9035da1b3}");
        }

        ///<returns>The command name as it appears on the Rhino command line</returns>
        public override string EnglishCommandName()
        {
            return "zAssembly";
        }

        ///<summary> This gets called when when the user runs this command.</summary>
        public override IRhinoCommand.result RunCommand(IRhinoCommandContext context)
        {   

            //get the parts to assemble
            MRhinoGetObject go = new MRhinoGetObject();
            go.SetCommandPrompt("Select brep");
            go.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.polysrf_object
                               | IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object);
            go.GetObjects(1, 0);
            int count = go.ObjectCount();         
            if (go.CommandResult() != IRhinoCommand.result.success)
                return go.CommandResult();

            
            // Define source plane - coordinate system for parts derived from here
            MRhinoView view = go.View();
            if (view == null)
            {
                view = RhUtil.RhinoApp().ActiveView();
                if (view == null)
                    return IRhinoCommand.result.failure;
            }
            OnPlane source_plane = new OnPlane(view.ActiveViewport().ConstructionPlane().m_plane);



            //Lists of parts formed in terms of class
            List<Part> parts = new List<Part>();

            List<IRhinoObjRef> objref = new List<IRhinoObjRef>();
            for (int i = 0; i < count; i++)
            {
                objref.Add(go.Object(i));
                IOnBrep brep = objref[i].Brep();

                Part p = new Part(brep, source_plane);
                parts.Add(p);

            }
            bool assemble = true;
            while (assemble)
            {
                //clear any selection
                context.m_doc.UnselectAll();
                context.m_doc.Redraw();

                //two breps
                MRhinoGetObject gb = new MRhinoGetObject();
                gb.SetCommandPrompt("Select 2 parts to assemble");
                gb.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.polysrf_object
                                   | IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object);
                gb.GetObjects(2, 2);
                
                IRhinoObjRef objref1 = gb.Object(0);
                IOnBrep brep1 = objref1.Brep();

                IRhinoObjRef objref2 = gb.Object(1);
                IOnBrep brep2 = objref2.Brep();

                Part part1= new Part(brep1, source_plane);
                Part part2= new Part(brep2,source_plane);

                for (int i = 0; i < count; i++)
                {
                    if (parts[i].brep.Equals(brep1))
                    {
                        part1 = parts[i];
                    }

                    if (parts[i].brep.Equals(brep2))
                    {
                        part2 = parts[i];
                    }
                }

                
                //clear selection
                context.m_doc.UnselectAll();
                context.m_doc.Redraw();

                
                //take choice
                MRhinoGetNumber choice = new MRhinoGetNumber();
                choice.SetCommandPrompt("Enter 1. Fit, 2. Touch, 3. Allign");
                choice.GetNumber();
                if (choice.CommandResult() != IRhinoCommand.result.success)
                {
                    return choice.CommandResult();
                }
                int ch = (int) choice.Number();

                
                
                switch (ch)
                {
                    case 1:                     
                                             
                        Fit fit;
                        fit = new Fit(part1, part2);

                        OnBrep brep_pin = OnUtil.ON_BrepCylinder(fit.pin_cyl, true, true);
                        OnBrep brep_hole = OnUtil.ON_BrepCylinder(fit.hole_cyl, true, true);
                        
                        if (brep_pin != null)
                        {
                            MRhinoBrepObject cylinder_object = new MRhinoBrepObject();
                            cylinder_object.SetBrep(brep_pin);

                            if (context.m_doc.AddObject(cylinder_object))
                                context.m_doc.Redraw();
                        }

                        if (brep_hole != null)
                        {
                            MRhinoBrepObject cylinder_object1 = new MRhinoBrepObject();
                            cylinder_object1.SetBrep(brep_hole);
                            if (context.m_doc.AddObject(cylinder_object1))
                                context.m_doc.Redraw();
                        }
  
                        //boolean addition
                        BooleanAdd unionResult;
                        unionResult = new BooleanAdd(part1.brep, brep_pin);
                        
                        //context.m_doc.AddBrepObject(part1.brep);
                        //context.m_doc.DeleteObject();

                        part1.brep = unionResult.union;
                        part1.brep.Transform(fit.xform_t);
                        part1.brep.Transform(fit.xform_r);

                        context.m_doc.AddBrepObject(part1.brep);
                        context.m_doc.Redraw();
                        
                        BooleanSubtract subResult;
                        subResult = new BooleanSubtract(part2.brep, brep_hole);
                        //context.m_doc.AddBrepObject(part1.brep);
                        //context.m_doc.Redraw();
                        part2.brep = subResult.sub;                      
                        break;

                    case 2:
                        Touch touch;
                        touch = new Touch(part1, part2);

                        part1.brep.Transform(touch.xform_t);
                        part1.brep.Transform(touch.xform_r);

                        context.m_doc.AddBrepObject(part1.brep);
                        context.m_doc.Redraw();
                        break;
                    
                    case 3:
                        Align align;
                        align = new Align(part1, part2);

                        part1.brep.Transform(align.xform_t);
                        part1.brep.Transform(align.xform_r);

                        context.m_doc.AddBrepObject(part1.brep);
                        context.m_doc.Redraw();
                        break;

                        
                        

                }

            }


            return IRhinoCommand.result.success;

        }
    }
}

