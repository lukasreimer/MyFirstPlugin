using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace MyFirstPlugin
{
    // Lesson 6: Working with Room Geometry
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ConsiderablyEnhancedPlaceGroupCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            try
            {
                Reference pickedref = null;

                Selection sel = uiapp.ActiveUIDocument.Selection;
                GroupPickFilter selFilter = new GroupPickFilter();
                pickedref = sel.PickObject(ObjectType.Element, selFilter, "Please select a group");
                Element elem = doc.GetElement(pickedref);
                Group group = elem as Group;

                // Get the group's center point
                XYZ origin = GetElementCenter(group);
                // Get the room that the picked group is located in
                Room room = GetRoomOfGroup(doc, origin);
                // Get the room's center point
                XYZ sourceCenter = GetRoomCenter(room);
                string coords = $"X = {sourceCenter.X}\nY = {sourceCenter.Y}\nZ = {sourceCenter.Z}";
                TaskDialog.Show("Source Room Center", coords);

                //XYZ point = sel.PickPoint("Please pick a point to place group");

                Transaction trans = new Transaction(doc);
                trans.Start("ConsiderablyEnhancedPlaceGroupCommand");
                XYZ groupLocation = sourceCenter + new XYZ(20, 0, 0);
                doc.Create.PlaceGroup(groupLocation, group.GroupType);
                trans.Commit();

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        public XYZ GetElementCenter(Element elem)
        {
            BoundingBoxXYZ bounding = elem.get_BoundingBox(null);
            XYZ center = (bounding.Max + bounding.Min) * 0.5;
            return center;
        }

        Room GetRoomOfGroup(Document doc, XYZ point)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Rooms);
            Room room = null;
            foreach (Element elem in collector)
            {
                room = elem as Room;  // may fail, return null
                if (room != null)
                {
                    if (room.IsPointInRoom(point))
                    {
                        break;  // out of foreach loop
                    }
                }
            }
            return room;
        }

        public XYZ GetRoomCenter(Room room)
        {
            XYZ boundCenter = GetElementCenter(room);
            LocationPoint locPt = (LocationPoint)room.Location;
            XYZ roomCenter = new XYZ(boundCenter.X, boundCenter.Y, locPt.Point.Z);
            return roomCenter;
        }
    }
}
