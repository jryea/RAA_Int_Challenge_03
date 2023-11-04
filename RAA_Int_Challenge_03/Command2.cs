#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Converters;

#endregion

namespace RAA_Int_Challenge_03
{
    [Transaction(TransactionMode.Manual)]
    public class Command2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Your code goes here

            // Get Rooms
            FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id)
                                                     .OfCategory(BuiltInCategory.OST_Rooms)
                                                     .WhereElementIsNotElementType();

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create Room Dimensions");

                int counter = 0;

                foreach (Room room in collector)
                {
                    // Create reference array and point list
                    ReferenceArray horizRefArray = new ReferenceArray();
                    ReferenceArray vertRefArray = new ReferenceArray();
                    List<XYZ> horizPoints = new List<XYZ>();
                    List<XYZ> vertPoints = new List<XYZ>();

                    // Set options
                    SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
                    options.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish;

                    //Get boundaries from room
                    List<BoundarySegment> segments = room.GetBoundarySegments(options).First().ToList();

                    //Loop through room boundaries
                    foreach (BoundarySegment segment in segments)
                    {
                        // Get Boundary geometry
                        Curve curve = segment.GetCurve();
                        XYZ point = curve.Evaluate(0.25, true);

                        //Get boundary wall
                        Element wall = doc.GetElement(segment.ElementId);

                        if (wall != null)
                        {

                            // Is line vertical?
                            if (IsLineVertical(curve) == false)
                            {
                                // Add to ref and point array
                                horizRefArray.Append(new Reference(wall));
                                horizPoints.Add(point);
                            }
                            else
                            {
                                vertRefArray.Append(new Reference(wall));   
                                vertPoints.Add(point);   
                            }
                        }
                    }

                    // Create dimension line
                    XYZ horizPoint1 = horizPoints.First();
                    XYZ horizPoint2 = horizPoints.Last();

                    XYZ vertPoint1 = vertPoints.First();
                    XYZ vertPoint2 = vertPoints.Last();

                    Line horizDimLine = Line.CreateBound(horizPoint1, new XYZ(horizPoint1.X, horizPoint2.Y, 0));
                    Line vertDimLine = Line.CreateBound(vertPoint1, new XYZ(vertPoint2.X, vertPoint1.Y, 0));

                    Dimension horizDim = doc.Create.NewDimension(doc.ActiveView, horizDimLine, horizRefArray);
                    Dimension vertDim = doc.Create.NewDimension(doc.ActiveView, vertDimLine, vertRefArray);

                    if (horizDim != null)
                        counter++;
                    if (vertDim != null)
                        counter++;
                }

                TaskDialog.Show("Dimensions Created", $"{counter} dimensions were placed!");

                t.Commit();
            }

            return Result.Succeeded;
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
            string buttonInternalName = "btnCommand2";
            string buttonTitle = "Button 2";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 2");

            return myButtonData1.Data;
        }
    }
}
