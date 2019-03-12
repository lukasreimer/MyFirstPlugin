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
    // Lesson 5: Simple Selection of a Group
    public class GroupPickFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_IOSModelGroups);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class EnhancedPlaceGroupCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get application and documnet objects
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;  // could be null if no project is open!  

            try
            {
                // Define a reference Object to accept the pick result
                Reference pickedref = null;

                // Pick a group
                Selection sel = uiapp.ActiveUIDocument.Selection;
                GroupPickFilter selFilter = new GroupPickFilter();
                pickedref = sel.PickObject(ObjectType.Element, selFilter, "Please select a group");  // could be terminated by the user!, user could select non group objects
                Element elem = doc.GetElement(pickedref);
                Group group = elem as Group;  // could be null if type cast fails (elem is not a group)!

                // Pick point
                XYZ point = sel.PickPoint("Please pick a point to place group");  // could be terminated by the user!
                
                // Place the group
                Transaction trans = new Transaction(doc);
                trans.Start("EnhancedPlaceGroupCommand");
                doc.Create.PlaceGroup(point, group.GroupType);
                trans.Commit();
                
                // Return the result
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)  // if the user right clicks or presses ESC button
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
