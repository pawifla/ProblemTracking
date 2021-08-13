using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using WebLayouts.Data;
using Newtonsoft.Json;
using WebLayouts.App_Code;

namespace WebLayouts.Services
{
    /// <summary>
    /// Summary description for DataService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class DataService : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }
        [WebMethod]

        public List<DataTableClass> getData(string appNo)
        {
            DataAccessLayer dal = new DataAccessLayer();

            return dal.getData(appNo);
        }
        [WebMethod]
        public List<DataTableClass> GetFilteredQuery(string[] appNos, string[] basinNo, string meetingDate, string[] owners)
        {
            //logic for modularquery
            Console.WriteLine(appNos);
            DataAccessLayer dal = new DataAccessLayer();
            return dal.FilteredQuery(appNos, meetingDate, basinNo, owners);
            
        }
        [WebMethod]
        public bool InsertNoteRecord(string appNo, string seDecision, string historyNotes, string noteDate, string noteReviewer, string meetingNotes, List<string> ownerList, string criticalDate, string meetingDate)
        {
            //ownersWillNeedToChange
            DataAccessLayer dal = new DataAccessLayer();
            return dal.insertNoteRecord(appNo, seDecision, historyNotes, noteDate, noteReviewer, meetingNotes, ownerList, criticalDate, meetingDate);
        }
        [WebMethod]
        public bool UpdateNoteRecord(int id,string appNo, string seDecision, string historyNotes, string noteDate, string noteReviewer, string meetingNotes, List<string> ownerList, string criticalDate, string meetingDate)
        {
            //ownersWillNeedToChange
            DataAccessLayer dal = new DataAccessLayer();
            return dal.updateNoteRecord(id, appNo, seDecision, historyNotes, noteDate, noteReviewer, meetingNotes, ownerList, criticalDate, meetingDate);
        }
        [WebMethod]
        public List<string> getReferenceDates(string app)
        {
            DataAccessLayer dal = new DataAccessLayer();
            return dal.getReferenceDates(app);
        }
        [WebMethod]
        public bool RemoveOwners(int id, List<string> ownerList)
        {
            DataAccessLayer dal = new DataAccessLayer();
            return dal.removeOwners(id, ownerList);
        }
        [WebMethod]
        public bool deleteRecord(int id)
        {
            DataAccessLayer dal = new DataAccessLayer();
            return dal.deleteRecord(id);
        }
        [WebMethod]
        public List<DataTableClass> selectAllNotes()
        {
            DataAccessLayer dal = new DataAccessLayer();
            return dal.selectAllNotes();
        }
        [WebMethod]
        public string ADUserLoad()
        {
            DataAccessLayer dal = new DataAccessLayer();
            string user = dal.adUserLoad();
            return dal.adUserLoad();
        }
        [WebMethod]
        public List<string> ShowOwnerList(string app)
        {
            DataAccessLayer dal = new DataAccessLayer();
            return dal.getOwners(app);
        }
        [WebMethod]
        public List<OwnerInfo> ShowDetailOwners(int noteID)
        {
            DataAccessLayer dal = new DataAccessLayer();
            return dal.getDetailOwners(noteID);
        }
        [WebMethod]
        public List<OwnerInfo> ShowParentOwners(string app)
        {
            DataAccessLayer dal = new DataAccessLayer();
            return dal.getParentOwners(app);
        }
        [WebMethod]
        public List<DataTableClass> SelectNotesByOwner(string owner)
        {
            DataAccessLayer dal = new DataAccessLayer();
            return dal.selectNotesByOwner(owner);
        }
        [WebMethod]
        public DetailsRow PullEditRecord(int id)
        {
            DataAccessLayer dal = new DataAccessLayer();
            return dal.pullEditRecord(id);
        }
        [WebMethod]
        public List<SeDecision> GetSeDecisions()
        {
            DataAccessLayer dal = new DataAccessLayer();
            return dal.getSeDecisions();
        }
        [WebMethod]
        public string GetOriginalDueDate(string appNo)
        {
            DataAccessLayer dal = new DataAccessLayer();
            return dal.getOrigDueDate(appNo);
        }
    }
}
