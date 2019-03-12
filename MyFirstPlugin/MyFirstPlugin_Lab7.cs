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
    // Lesseon 7: Final Plugin
    public class RoomPickFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            return (element.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Rooms));
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MostEnhancedPlaceGroupCommand : IExternalCommand
    {
        // Main method for executing the command
        // =====================================
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // setup the main handles for the application and the active document
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            try  // business logic
            {
                // Ask the user to select a group
                Selection sel = uiapp.ActiveUIDocument.Selection;
                GroupPickFilter selFilter = new GroupPickFilter();
                Reference pickedref = sel.PickObject(ObjectType.Element, selFilter, "Please select a group");
                Element elem = doc.GetElement(pickedref);
                Group group = elem as Group;  // should be safe after using the GroupPickFilter on the Selection

                // Calculate the center location of the room the group is located in
                XYZ origin = GetElementCenter(group);
                Room room = GetRoomOfGroup(doc, origin);
                XYZ sourceCenter = GetRoomCenter(room);

                // Ask the user to pick target rooms
                RoomPickFilter roomPickFilter = new RoomPickFilter();
                IList<Reference> rooms = sel.PickObjects(ObjectType.Element, roomPickFilter, "Select target rooms for duplicating the group");

                // Place groups of the previously selected types in the selected target rooms
                Transaction trans = new Transaction(doc);
                trans.Start("MostEnhancedPlaceGroupCommand");
                PlaceFurnitureInRooms(doc, rooms, sourceCenter, group.GroupType, origin);
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

        // Private helper methods implementing core parts of the business logic
        // ====================================================================
        private XYZ GetElementCenter(Element elem)
        {
            BoundingBoxXYZ bounding = elem.get_BoundingBox(null);
            XYZ center = (bounding.Max + bounding.Min) * 0.5;
            return center;
        }

        private Room GetRoomOfGroup(Document doc, XYZ point)
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
                        break;  // out of the foreach loop
                    }
                }
            }
            return room;
        }

        private XYZ GetRoomCenter(Room room)
        {
            XYZ boundCenter = GetElementCenter(room);
            LocationPoint locPt = (LocationPoint)room.Location;
            XYZ roomCenter = new XYZ(boundCenter.X, boundCenter.Y, locPt.Point.Z);
            return roomCenter;
        }

        private void PlaceFurnitureInRooms(Document doc, IList<Reference> rooms, XYZ sourceCenter, GroupType groupType, XYZ groupOrigin)
        {
            XYZ offset = groupOrigin - sourceCenter;  // 3d offset
            XYZ offsetXY = new XYZ(offset.X, offset.Y, 0);  // pure horizontal 2d offset
            foreach (Reference reference in rooms)
            {
                Room roomTarget = doc.GetElement(reference) as Room;
                if (roomTarget != null)
                {
                    XYZ roomCenter = GetRoomCenter(roomTarget);
                    Group group = doc.Create.PlaceGroup(roomCenter + offsetXY, groupType);
                }
            }
        }
    }
}
