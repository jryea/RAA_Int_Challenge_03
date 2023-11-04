#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

#endregion

namespace RAA_Int_Challenge_03
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Your code goes here
            View currentView = doc.ActiveView;

            List<Grid> horizGrids = GetGridsFromView(doc, currentView, false);
            List<Grid> vertGrids = GetGridsFromView(doc, currentView, true);

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create Grid Dimensions");

                CreateGridDimensions(doc, horizGrids, 3);
                CreateGridDimensions(doc, vertGrids, 3);

                t.Commit();
            }

                return Result.Succeeded;
        }

        private Dimension CreateGridDimensions(Document doc, List<Grid> grids, double offset)
        {
            Grid firstGrid = grids.First();
            Grid lastGrid = grids.Last();
            XYZ firstPt = firstGrid.Curve.GetEndPoint(1);
            XYZ lastPt = lastGrid.Curve.GetEndPoint(1); 
            
            ReferenceArray gridRefArray = new ReferenceArray();

            // Add offset based on grid orientation
            if (IsLineVertical(firstGrid.Curve) == true)
            {
                firstPt = new XYZ(firstPt.X, firstPt.Y - offset, firstPt.Z);
                lastPt = new XYZ(lastPt.X, lastPt.Y - offset, lastPt.Z);
            }
            else
            {
                firstPt = new XYZ(firstPt.X + offset, firstPt.Y, firstPt.Z);
                lastPt = new XYZ(lastPt.X + offset, lastPt.Y, lastPt.Z);
            }

            Line dimLine = Line.CreateBound(firstPt, lastPt);

            foreach (Grid grid in grids)
            {
                gridRefArray.Append(new Reference(grid));
            }

            Dimension returnDimension = doc.Create.NewDimension(doc.ActiveView, dimLine, gridRefArray);

            return returnDimension;
            
        }

        private List<Grid> GetGridsFromView(Document doc, View view, bool isVertical)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc, view.Id)
                                                 .OfCategory(BuiltInCategory.OST_Grids)
                                                 .WhereElementIsNotElementType();
                                                   
            List<Grid> horizGrids = new List<Grid>();
            List<Grid> vertGrids = new List<Grid>();

            foreach (Grid grid in collector) 
            {
                Curve gridCurve = grid.Curve;
                if (IsLineVertical(gridCurve) == true)
                    vertGrids.Add(grid);
                else
                    horizGrids.Add(grid);
            }

            // Sort lists
            horizGrids = horizGrids.OrderBy(g => g.Curve.GetEndPoint(0).Y).ToList();
            vertGrids = vertGrids.OrderBy(g => g.Curve.GetEndPoint(0).X).ToList();

            if (isVertical == true)
                return vertGrids;
            else
                return horizGrids;
        }

        private bool IsLineVertical(Curve curve)
        {
            XYZ p1 = curve.GetEndPoint(0);
            XYZ p2 = curve.GetEndPoint(1);
            if (Math.Abs(p1.X - p2.X) < Math.Abs(p1.Y - p2.Y))
                return true;
            else 
                return false;
        }


        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData1.Data;
        }
    }
}
