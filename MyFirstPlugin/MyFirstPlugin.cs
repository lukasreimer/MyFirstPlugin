using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace MyFirstPlugin
{
    // [TransactionAttribute(TransactionMode.Manual)]  // name can be simplified
    // [RegenerationAttribute(RegenerationOption.Manual)]  // name can be simplified
    [Transaction(TransactionMode.Manual)]  // http://www.revitapidocs.com/2018/84254a1f-7bba-885a-ce65-e68fc238fddb.htm
    [Regeneration(RegenerationOption.Manual)]  // http://www.revitapidocs.com/2018/26239bbb-d639-d306-cc43-cc2ec975b822.htm
    public class PlaceGroupCommand : IExternalCommand  // http://www.revitapidocs.com/2018/ad99887e-db50-bf8f-e4e6-2fb86082b5fb.htm
    {  // alternative IExternalApplication: http://www.revitapidocs.com/2018/ad99887e-db50-bf8f-e4e6-2fb86082b5fb.htm
        public Result Execute(
            ExternalCommandData commandData,  // http://www.revitapidocs.com/2018/e9aab085-720f-b924-3ace-1f3c33d95d44.htm
            ref string message,
            ElementSet elements)  // http://www.revitapidocs.com/2018/48b47759-c441-ded2-5d8c-5c541c3eab01.htm
        {
            //Get application and documnet objects
            UIApplication uiapp = commandData.Application;  // http://www.revitapidocs.com/2018/51ca80e2-3e5f-7dd2-9d95-f210950c72ae.htm
            Document doc = uiapp.ActiveUIDocument.Document;  // http://www.revitapidocs.com/2018/db03274b-a107-aa32-9034-f3e0df4bb1ec.htm

            //Define a reference Object to accept the pick result
            Reference pickedref = null;  // http://www.revitapidocs.com/2018/d28155ae-817b-1f31-9c3f-c9c6a28acc0d.htm

            //Pick a group
            Selection sel = uiapp.ActiveUIDocument.Selection;  // http://www.revitapidocs.com/2018/31b73d46-7d67-5dbb-4dad-80aa597c9afc.htm
            pickedref = sel.PickObject(ObjectType.Element, "Please select a group");  // http://www.revitapidocs.com/2018/0315fd62-b533-1817-2f2d-d9ebd4bc8e33.htm
            Element elem = doc.GetElement(pickedref);  // http://www.revitapidocs.com/2018/eb16114f-69ea-f4de-0d0d-f7388b105a16.htm
            Group group = elem as Group;  // http://www.revitapidocs.com/2018/ca54af3c-52d8-0aed-cd22-440ec2584b89.htm

            //Pick point
            XYZ point = sel.PickPoint("Please pick a point to place group");  // http://www.revitapidocs.com/2018/c2fd995c-95c0-58fb-f5de-f3246cbc5600.htm

            //Place the group
            Transaction trans = new Transaction(doc);  // http://www.revitapidocs.com/2018/308ebf8d-d96d-4643-cd1d-34fffcea53fd.htm
            trans.Start("PlaceGroupCommand");  //
            doc.Create.PlaceGroup(point, group.GroupType);  //
            trans.Commit();  //

            return Result.Succeeded;  // http://www.revitapidocs.com/2018/e6cebb3c-0c3f-7dc4-2063-e5df0a00b2f5.htm
        }
    }

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

                XYZ point = sel.PickPoint("Please pick a point to place group");

                Transaction trans = new Transaction(doc);
                trans.Start("ConsiderablyEnhancedPlaceGroupCommand");
                doc.Create.PlaceGroup(point, group.GroupType);
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
    }
}
