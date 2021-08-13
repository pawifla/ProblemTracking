using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

using Newtonsoft.Json;
using System.Data;
using System.DirectoryServices.AccountManagement;
using System.Globalization;

namespace WebLayouts.App_Code
{
    public class DataAccessLayer
    {
        string connString = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;

        SqlConnection conn;

        public DataAccessLayer()
        {
            conn = new SqlConnection(connString);
        }
        public bool insertNoteRecord(string appNo, string seDecision, string historyNotes, string noteDate, string noteReviewer, string meetingNotes, List<string> ownerList, string criticalDate, string meetingDate)
        {//this is CreateNote modal
            //for null meetingDate
            string strMeetingDate = (meetingDate == "") ? "NULL)": $"Convert(datetime,'{meetingDate}'))";
            conn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            
            cmd.CommandText = $"Insert Into exts_notes_tracking (app, se_decision, note_date, reviewer, meeting_notes, history_notes, critical_date, meeting_date)" +
                                $" Values ('{appNo}', '{seDecision}', Convert(datetime,'{noteDate}'),'{noteReviewer}', '{meetingNotes}', " +
                               $" '{historyNotes}', Convert(datetime,'{criticalDate}'), "+
                               strMeetingDate;
            int i = cmd.ExecuteNonQuery();
            cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            int o = 0;//to check if worked
            foreach (string owner in ownerList)
            {
                cmd.CommandText = $"Insert Into exts_notes_owners (owner_name, app, note_ID)" +
                $"Values('{owner}','{appNo}', (Select Max(ID) from exts_notes_tracking where app = '{appNo}'))";
                o = cmd.ExecuteNonQuery();
            };

            conn.Close();
            //if rows affected is zero then false, else true
            return (i == 0 || o == 0) ? false : true;
        }

        public bool updateNoteRecord(int id, string appNo, string seDecision, string historyNotes, string noteDate, string noteReviewer, string meetingNotes, List<string> ownerList, string criticalDate, string meetingDate)
        {//this is UpdateNote
            string strMeetingDate = (meetingDate == "") ? "meeting_date = NULL ":  $"meeting_date = '{meetingDate}' ";
            conn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            //need to insert OWNERS TO NOTE OWNERS
            cmd.CommandText = $"Update exts_notes_tracking " +
                                $"Set app = '{appNo}', se_decision = '{seDecision}', note_date = '{noteDate}', reviewer = '{noteReviewer}', " +
                                $"meeting_notes = '{meetingNotes}', history_notes = '{historyNotes}' , critical_date = '{criticalDate}', "+
                                strMeetingDate +
                                $"Where id = {id} " +
                                $"Delete  From exts_notes_owners where note_ID = {id}";
            //delete old owners here and then fix the insert below
            int i = cmd.ExecuteNonQuery();
            cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            int o = 0;//to check if worked
            foreach (string owner in ownerList)
            {
                //check if owner already exists, if not then insert
                cmd.CommandText = $"INSERT INTO exts_notes_owners (note_ID, owner_name, app) VALUES ({id},'{owner}', '{appNo}') ";
                o = cmd.ExecuteNonQuery();
            };

            conn.Close();
            //if rows affected is zero then false, else true
            return (i == 0 || o == 0) ? false : true;
        }

        public bool removeOwners(int id, List<string> ownerList)
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            int i = 0;
            foreach (string owner in ownerList)
            {
                cmd.CommandText = $"IF EXISTS(SELECT * FROM exts_notes_owners where owner_name like '%{owner}%' and note_ID = {id}) " +
                                    $"BEGIN " +
                                    $"Delete from exts_notes_owners WHERE owner_name like '%{owner}%' and note_ID = {id} " +
                                    $"END";
                i = cmd.ExecuteNonQuery();
            }
            conn.Close();
            return (i == 0) ? false : true;
        }
        public bool deleteRecord(int id)
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"Delete from exts_notes_tracking where id = {id}";
            int i = cmd.ExecuteNonQuery();
            return (i == 0) ? false : true;

        }

        public string adUserLoad()
        {
            //To load the user on pageload
            System.Security.Principal.IPrincipal User;
            User = HttpContext.Current.User;

            string[] arrUser = User.Identity.Name.Split('\\');
            string fullname = null;
            string Role1 = @"DCNR\NDWR-Staff";
            TextInfo ti = new CultureInfo("en-US", false).TextInfo;

            //put in next line when published to test
            string strUser;
            try
            {
                strUser = arrUser[1];
            }
            catch (IndexOutOfRangeException e)
            {
                strUser = null;
            }

            //Grab User Name from AD
            DataTableClass parentRow = new DataTableClass();

             using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
             {
                 using (UserPrincipal user = UserPrincipal.FindByIdentity(context, User.Identity.Name))
                 {
                     if (user != null)
                     {
                         parentRow.adReviewer = user.DisplayName;
                     }
                     else
                     {
                         parentRow.adReviewer = strUser;
                     }
                 }
             }
             return parentRow.adReviewer;
        }
        public List<DataTableClass> FilteredData = new List<DataTableClass>();
        public List<DataTableClass> FilteredQuery(string[] permitNums, string meetingDate, string[] basinNo, string[] owners )
        {
            DataTableClass parentRow = new DataTableClass();
            string strBasinNos = "";
            for(int i = 0; i < basinNo.Length; i++)
            {
                strBasinNos += basinNo[i];
                if(i != permitNums.Count() - 1)
                {
                    strBasinNos += "','";
                }
            }
            string strOwners = "";
            if(permitNums.Length == 0 && basinNo.Length == 0 && owners.Length != 0)
            {
                for(int i = 0; i < owners.Length; i++)
                {
                    strOwners += owners[i];
                    // if(i != permitNums.Count() - 1)
                    // {
                    //     strOwners += "','";
                    // }
                    //if there are owners, then return the appNos associated
                    permitNums = getAppsForOwners(strOwners);
                }
            }
            string strPermitNos = "";
            for(int i = 0; i < permitNums.Length; i++)
            {
                strPermitNos += permitNums[i];
                if(i != permitNums.Count() - 1)
                {
                    strPermitNos += "','";
                }
            }
            //this is currently broken. only works for searching for owners only but pulls too many parents so bad
            string cmdStr = "SELECT Distinct e.app, m.basin, (Select SE_Decision FROM exts_seDecision_lut where decision_ID = e.se_decision) as se_decision, " +
                                "DateDiff(year, e.critical_date, GETDATE()) as years_since, " +
                                "e.meeting_notes, m.poc_due_dt as poc_due_dt, e.reviewer, e.critical_date as c_date, " +
                                "m.prior_dt as PriorityDate, (Select use_name from use_lut where m.mou = mou) as mannerOfUse, " +
                                " ISNULL((Select Max(meeting_date) from exts_notes_tracking where app = m.app),'') as meetingDate " +
                                "FROM exts_notes_tracking AS e " +
                                "Join main_src AS m ON m.app = e.app " +
                                                                "WHERE " + 
                                "e.note_date = (SELECT MAX(note_date) FROM exts_notes_tracking WHERE app = m.app)";
           cmdStr += (strPermitNos != null && strPermitNos != "" ) ? $" and m.app IN ('{strPermitNos}') " : "";
           cmdStr += (meetingDate != "") ? $" and (Select Max(meeting_date) from exts_notes_tracking where app = m.app)= Convert(datetime, '{meetingDate}') ": "";
           cmdStr += (strBasinNos != null && strBasinNos != "") ? $" and m.basin IN ('{strBasinNos}') " : "";
           cmdStr += (strBasinNos != null && strBasinNos != "") ? $" and m.basin IN ('{strBasinNos}') " : "";

            conn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = cmdStr;

           SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read()) {
                        parentRow = new DataTableClass();
                        parentRow.adReviewer = adUserLoad();
                        parentRow.pAppNo = rdr[0].ToString();
                        parentRow.pBasin = rdr[1].ToString();
                        parentRow.pMeetingDate = Convert.ToDateTime(rdr["meetingDate"]).ToShortDateString();
                        parentRow.pMeetingDate = parentRow.pMeetingDate == "1/1/1900" ? "" : parentRow.pMeetingDate;
                        parentRow.pMOU = rdr["mannerOfUse"].ToString();
                        parentRow.pPriorityDate = Convert.ToDateTime(rdr["PriorityDate"]).ToShortDateString();
                        parentRow.pSeDecision = rdr[2].ToString();
                        parentRow.pNumExtsGranted = rdr["years_since"].ToString();
                        parentRow.pNoteStatus = rdr[4].ToString();
                        if (String.IsNullOrEmpty(rdr["c_date"].ToString()))
                            if (rdr["poc_due_dt"] == null)
                                parentRow.pCriticalDate = "";
                            else
                                parentRow.pCriticalDate = Convert.ToDateTime(rdr["poc_due_dt"]).ToShortDateString();
                        else
                            parentRow.pCriticalDate = Convert.ToDateTime(rdr["c_date"]).ToShortDateString();
                        
                        parentRow.pReviewer = rdr[6].ToString();
                        FilteredData.Add(parentRow);
                    }
                    rdr.Close();
                    //by here we get all the parent rows.

                    for (int i = 0; i < FilteredData.Count; i++)
                    {
                        insertFilteredDetails(FilteredData[i].pAppNo);
                    }
                 conn.Close();
           return FilteredData;
        }

        public string[] getAppsForOwners (string owner)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            conn.Open();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"SELECT distinct m.app "+
                            "FROM exts_notes_tracking AS e " +
                            "Join main_src AS m ON m.app = e.app  " +
                            "Left outer join exts_notes_owners AS o on m.app = o.app " +
                            $"Where o.owner_name like '%{owner}%' " +
                            "group by m.app";
            SqlDataReader rdr = cmd.ExecuteReader();
            List<string> appList = new List<string>();
            while (rdr.Read())
            {
                appList.Add(rdr[0].ToString());
            }
            rdr.Close();
            conn.Close();
            string[] appNos = appList.ToArray();
            return appNos;

        }

        public void insertFilteredDetails(string id)
        {
            int index = this.FilteredData.FindIndex(p => p.pAppNo == id);
            SqlCommand cmd = new SqlCommand();
          
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"Select history_notes, (Select SE_Decision FROM exts_seDecision_lut where decision_ID = e.se_decision) as seDecision, e.meeting_date, "+
                            $"note_date, reviewer, meeting_notes, ID, owner_name, ISNULL(critical_date,'') as critical_date" +
                            $" From exts_notes_tracking as e Where app = '{id}'"+
                            $" Order By ISNULL(meeting_date, note_date) desc";

            SqlDataReader rdr2 = cmd.ExecuteReader();

            FilteredData[index].details = new List<DetailsRow>();
            while (rdr2.Read())
            {
                FilteredData[index].details.Add(new DetailsRow()
                {
                    dNoteText = rdr2["history_notes"].ToString(),
                    dSeDecision = rdr2["seDecision"].ToString(),
                    dNoteDate = Convert.ToDateTime(rdr2["note_date"]).ToShortDateString(),
                    dMeetingDate = !(rdr2["meeting_date"] is DBNull) ? Convert.ToDateTime(rdr2["meeting_date"]).ToShortDateString(): "",
                    dReviewer = rdr2["reviewer"].ToString(),
                    dNoteStatus = rdr2["meeting_notes"].ToString(),
                    dNoteID = Convert.ToInt32(rdr2["ID"]),
                    dCriticalDate = Convert.ToDateTime(rdr2["critical_date"]).ToShortDateString()
                });
            }
          rdr2.Close();
           
        }

        public List<DataTableClass> getData(string appNo)
        {
            //Retrieves data based on appNo
            List<DataTableClass> TableData = new List<DataTableClass>();
            DataTableClass parentRow = new DataTableClass();

            parentRow.adReviewer = adUserLoad();
            //need to load AD User somewhere in here

            conn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;

            //Grabs parent row information
            cmd.CommandText = $"Select Top(1) m.app as app, m.basin as basin, (Select SE_Decision FROM exts_seDecision_lut where decision_ID = p.se_decision) as seDecision,"+
                                $" m.poc_due_dt as poc_due_date, p.reviewer as reviewer, " +
                                $"ISNULL(p.critical_date,'') as critical_date, "+
                                "DateDiff(year, p.critical_date, GETDATE()) as years_since, " +
                                "m.prior_dt as PriorityDate, (Select use_name from use_lut where m.mou = mou) as mannerOfUse "+ 
                                $"From main_src as m " +
                                $"Inner Join exts_notes_tracking as p ON m.app = p.app " +
                                $"Where m.app = '{appNo}'" +
                                $"Order by p.meeting_date desc";

            // reinsert this if needed in the future. (not useful for creation) and m.app_status in ('PER', 'CER')

            SqlDataReader rdr = cmd.ExecuteReader();
            
            while (rdr.Read())
            {
                parentRow.pAppNo = rdr["app"].ToString();
                parentRow.pBasin = rdr["basin"].ToString();
                parentRow.pMOU = rdr["mannerOfUse"].ToString();
                parentRow.pPriorityDate = Convert.ToDateTime(rdr["PriorityDate"]).ToShortDateString();
                parentRow.pSeDecision = rdr["seDecision"].ToString();
                parentRow.pNumExtsGranted = rdr["years_since"].ToString();
               if (String.IsNullOrEmpty(rdr["critical_date"].ToString()))
                    if (rdr["poc_due_dt"] == null)
                        parentRow.pCriticalDate = "";
                    else
                        parentRow.pCriticalDate = Convert.ToDateTime(rdr["poc_due_dt"]).ToShortDateString();
                else
                    parentRow.pCriticalDate = Convert.ToDateTime(rdr["critical_date"]).ToShortDateString();
                
                parentRow.pReviewer = rdr["reviewer"].ToString();
            }
            rdr.Close();
            //Gets list of related apps
            cmd.CommandText = $"Select history_notes, (Select SE_Decision FROM exts_seDecision_lut where decision_ID = e.se_decision) as seDecision, note_date, reviewer, meeting_notes, ID, owner_name, ISNULL(critical_date, '') as critical_date" +
                $" From exts_notes_tracking as e Where app = '{appNo}' " +
                "Order by e.meeting_date desc";
            
            SqlDataReader rdr2 = cmd.ExecuteReader();

            parentRow.details = new List<DetailsRow>();
            while (rdr2.Read())
            {
                parentRow.details.Add(new DetailsRow() {
                    dNoteText = rdr2["history_notes"].ToString(),
                    dSeDecision = rdr2["seDecision"].ToString(),
                    dNoteDate = Convert.ToDateTime(rdr2["note_date"]).ToShortDateString(),
                    dReviewer = rdr2["reviewer"].ToString(),
                    //this will need to be owenr list
                    dOwner = rdr2["owner_name"].ToString(),
                    dNoteStatus = rdr2["meeting_notes"].ToString(),
                    dNoteID = Convert.ToInt32(rdr2["ID"]),
                    dCriticalDate = Convert.ToDateTime(rdr2["critical_date"]).ToShortDateString()

                });
            }
            TableData.Add(parentRow);
            conn.Close();
            return TableData;
            //Will have to grab owners eventually
        }


        List<DataTableClass> TableData = new List<DataTableClass>();
        public List<DataTableClass> selectAllNotes()
        {//not used keep closed and get rid of
            
            DataTableClass parentRow = new DataTableClass();
            //select all note records.
            parentRow.adReviewer = adUserLoad();
            conn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"SELECT Distinct e.app, m.basin, (Select SE_Decision FROM exts_seDecision_lut where decision_ID = e.se_decision) as se_decision, " +
                            "DateDiff(year, e.critical_date, GETDATE()) as years_since, " +
                            "e.meeting_notes, m.poc_due_dt as poc_due_dt, e.reviewer, e.critical_date as c_date, " +
                            "m.prior_dt as PriorityDate, (Select use_name from use_lut where m.mou = mou) as mannerOfUse, "+ 
                            "ISNULL((Select Max(meeting_date) from exts_notes_tracking where app = m.app), '') as meetingDate "  +
                            "FROM exts_notes_tracking AS e " +
                            "Join main_src AS m " +
                            "ON m.app = e.app " +
                            "WHERE e.note_date = (SELECT MAX(note_date) FROM exts_notes_tracking WHERE app = m.app)";
            SqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read()) {
                parentRow = new DataTableClass();
                parentRow.adReviewer = adUserLoad();
                parentRow.pAppNo = rdr[0].ToString();
                parentRow.pBasin = rdr[1].ToString();
                parentRow.pMeetingDate = Convert.ToDateTime(rdr["meetingDate"]).ToShortDateString();
                parentRow.pMOU = rdr["mannerOfUse"].ToString();
                parentRow.pPriorityDate = Convert.ToDateTime(rdr["PriorityDate"]).ToShortDateString();
                parentRow.pSeDecision = rdr[2].ToString();
                parentRow.pNumExtsGranted = rdr["years_since"].ToString();
                parentRow.pNoteStatus = rdr[4].ToString();
                if (String.IsNullOrEmpty(rdr["c_date"].ToString()))
                    if (rdr["poc_due_dt"] == null)
                        parentRow.pCriticalDate = "";
                    else
                        parentRow.pCriticalDate = Convert.ToDateTime(rdr["poc_due_dt"]).ToShortDateString();
                else
                    parentRow.pCriticalDate = Convert.ToDateTime(rdr["c_date"]).ToShortDateString();
                
                parentRow.pReviewer = rdr[6].ToString();
                TableData.Add(parentRow);
            }
            rdr.Close();
            //by here we get all the parent rows.

            for (int i = 0; i < TableData.Count; i++)
            {
                insertDetails(TableData[i].pAppNo);
            }
         conn.Close();
            return TableData;
        }
        //inserts details for selectAllNotes
        public void insertDetails(string id)
        {
            int index = this.TableData.FindIndex(p => p.pAppNo == id);
            SqlCommand cmd = new SqlCommand();
          
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"Select history_notes, (Select SE_Decision FROM exts_seDecision_lut where decision_ID = e.se_decision) as seDecision, ISNULL(e.meeting_date,'') as meetingDate, "+
                            $"note_date, reviewer, meeting_notes, ID, owner_name, ISNULL(critical_date,'') as critical_date" +
                            $" From exts_notes_tracking as e Where app = '{id}'"+
                            $" Order By meeting_date desc";

            SqlDataReader rdr2 = cmd.ExecuteReader();

            TableData[index].details = new List<DetailsRow>();
            while (rdr2.Read())
            {
                TableData[index].details.Add(new DetailsRow()
                {
                    //string tempDate = Convert.ToDateTime(rdr2["meetingDate"]).ToShortDateString()
                    dNoteText = rdr2["history_notes"].ToString(),
                    dSeDecision = rdr2["seDecision"].ToString(),
                    dNoteDate = Convert.ToDateTime(rdr2["note_date"]).ToShortDateString(),
                    dMeetingDate = Convert.ToDateTime(rdr2["meetingDate"]).ToShortDateString(),
                    dReviewer = rdr2["reviewer"].ToString(),
                    dNoteStatus = rdr2["meeting_notes"].ToString(),
                    dNoteID = Convert.ToInt32(rdr2["ID"]),
                    dCriticalDate = Convert.ToDateTime(rdr2["critical_date"]).ToShortDateString()
                });
            }
          rdr2.Close();
        }
        public List<string> getOwners(string app)
        {//this will serve an owner list for a dialog.
            DataTableClass dtc = new DataTableClass();
            List<string> owners = new List<string>();
            SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"Select distinct owner_name from owner WHERE app = '{app}' and owner_type in ('B', 'C')";
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                string owner = rdr["owner_name"].ToString();
                owners.Add(owner);
            }
            dtc.ownerList = owners;
            rdr.Close();
            conn.Close();
            return dtc.ownerList;
        }
        public List<OwnerInfo> getDetailOwners(int noteID)
        {//serves ownerList for dialog.
            //this will now have div rate and duty, acres from main
            DataTableClass dtc = new DataTableClass();
            List<OwnerInfo> owners = new List<OwnerInfo>();
            SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"  select distinct owner_duty, owner_acre, owner_div_rate, noteOwners.owner_name as OwnerName, owner_type, noteOwners.ID from exts_notes_owners "+ 
                                  "join exts_notes_tracking on exts_notes_owners.note_ID = exts_notes_tracking.ID "+
                                  "join exts_notes_owners as noteOwners on exts_notes_tracking.ID = noteOwners.note_ID "+
                                  "join dbo.owner  as o on exts_notes_tracking.app = o.app "+
                                  $"where noteOwners.note_ID = '{noteID}' AND o.owner_name = noteOwners.owner_name";
            SqlDataReader rdr3 = cmd.ExecuteReader();
            while (rdr3.Read())
            {
                OwnerInfo o = new OwnerInfo();
                o.OwnerName = rdr3["OwnerName"].ToString();
                o.OwnerType = Convert.ToChar(rdr3["owner_type"]);
                o.OwnersDuty = Convert.ToSingle(rdr3["owner_duty"]);
                o.OwnersDivRate = Convert.ToSingle(rdr3["owner_div_rate"]);
                o.OwnersAcres = Convert.ToSingle(rdr3["owner_acre"]);
                owners.Add(o);

            }
            rdr3.Close();
            conn.Close();
            return owners;
        }
        public List<OwnerInfo> getParentOwners(string app)
        {//serves ownerList for dialog.
            //this will now have div rate and duty, acres from main
            DataTableClass dtc = new DataTableClass();
            List<OwnerInfo> owners = new List<OwnerInfo>();
            SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"  select distinct owner_duty, owner_acre, owner_div_rate, noteOwners.owner_name as OwnerName, owner_type from exts_notes_owners "+ 
                                  "join exts_notes_tracking on exts_notes_owners.note_ID = exts_notes_tracking.ID "+
                                  "join exts_notes_owners as noteOwners on exts_notes_tracking.ID = noteOwners.note_ID "+
                                  "join dbo.owner  as o on exts_notes_tracking.app = o.app "+
                                  $"where exts_notes_tracking.app = '{app}' AND o.owner_name = noteOwners.owner_name";
            SqlDataReader rdr3 = cmd.ExecuteReader();
            while (rdr3.Read())
            {
                OwnerInfo o = new OwnerInfo();
                o.OwnerName = rdr3["OwnerName"].ToString();
                o.OwnerType = Convert.ToChar(rdr3["owner_type"]);
                o.OwnersDuty = Convert.ToSingle(rdr3["owner_duty"]);
                o.OwnersDivRate = Convert.ToSingle(rdr3["owner_div_rate"]);
                o.OwnersAcres = Convert.ToSingle(rdr3["owner_acre"]);
                owners.Add(o);

            }
            rdr3.Close();
            conn.Close();
            return owners;
        }
        //this one will be used to search by owner
        public List<DataTableClass> selectNotesByOwner(string ownerName)
        {//not used get rid of
            DataTableClass parentRow = new DataTableClass();
            //select all note records.
            parentRow.adReviewer = adUserLoad();
            conn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"Select Distinct n.app, m.basin, (Select SE_Decision FROM exts_seDecision_lut where decision_ID = n.se_decision) as se_decision, " +
                            $"DateDiff(year, n.critical_date, GETDATE()) as years_since, " +
                            $"n.meeting_notes, m.poc_due_dt as poc_due_dt, n.reviewer as reviewer, ISNull(n.critical_date, '') as critical_date, " +
                             "m.prior_dt as PriorityDate, (Select use_name from use_lut where m.mou = mou) as mannerOfUse "+ 
                            $"From main_src as m " +
                            $"join exts_notes_tracking as n on m.app = n.app " +
                            $"join exts_notes_owners as e on n.ID = e.note_ID " +
                            $"Where e.owner_name like '%{ownerName}%' " +
                            $"AND n.note_date = (Select MAX(note_date) FROM exts_notes_tracking Where app = n.app)";
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                parentRow = new DataTableClass();
                parentRow.adReviewer = adUserLoad();
                parentRow.pReviewer = rdr["reviewer"].ToString();
                parentRow.pAppNo = rdr[0].ToString();
                parentRow.pBasin= rdr[1].ToString();
                parentRow.pMOU = rdr["mannerOfUse"].ToString();
                parentRow.pPriorityDate = Convert.ToDateTime(rdr["PriorityDate"]).ToShortDateString();
                parentRow.pSeDecision = rdr[2].ToString();
                parentRow.pNumExtsGranted = rdr[3].ToString();
                parentRow.pNoteStatus = rdr[4].ToString();
                if (rdr["critical_date"] == null)
                    if (rdr["poc_due_dt"] == null)
                        parentRow.pCriticalDate = "";
                    else
                        parentRow.pCriticalDate = Convert.ToDateTime(rdr["poc_due_dt"]).ToShortDateString();
                else
                    parentRow.pCriticalDate = Convert.ToDateTime(rdr["critical_date"]).ToShortDateString();
                TableData.Add(parentRow);
            }
            rdr.Close();
            for (int i = 0; i < TableData.Count; i++)
            {
                insertOwnerDetails(TableData[i].pAppNo, ownerName);
            }
            
            
            conn.Close();
            //TODO: First create a search by owner option here, then create a search by both option below. may need to refactor this hoe. see you in 2 narutos and an emmett walk.
            return TableData;
        }

        public void insertOwnerDetails(string id, string owner)
        {
            int index = this.TableData.FindIndex(p => p.pAppNo == id);
            SqlCommand cmd = new SqlCommand();

            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"Select history_notes, (Select SE_Decision FROM exts_seDecision_lut where decision_ID = n.se_decision) as se_decision, note_date, ISNULL(meeting_date,'') as meeting_date, reviewer, meeting_notes, n.ID, e.owner_name, critical_date " +
                            $" From exts_notes_tracking as n "+
                            $"JOIN exts_notes_owners as e on n.ID = e.note_ID "+
                            $"Where e.owner_name like '%{owner}%' AND app = '{id}' " +
                            $" Order By note_date desc";

            SqlDataReader rdr2 = cmd.ExecuteReader();

            TableData[index].details = new List<DetailsRow>();
            while (rdr2.Read())
            {
                TableData[index].details.Add(new DetailsRow()
                {
                    dNoteText = rdr2["history_notes"].ToString(),
                    dSeDecision = rdr2["se_decision"].ToString(),
                    dOwner = rdr2["owner_name"].ToString(),
                    dNoteDate = Convert.ToDateTime(rdr2["note_date"]).ToShortDateString(),
                    dMeetingDate = Convert.ToDateTime(rdr2["meeting_date"]).ToShortDateString(),
                    dReviewer = rdr2["reviewer"].ToString(),
                    dNoteStatus = rdr2["meeting_notes"].ToString(),
                    dNoteID = Convert.ToInt32(rdr2["ID"]),
                    dCriticalDate = Convert.ToDateTime(rdr2["critical_date"]).ToShortDateString()
                });
            }
            rdr2.Close();
        }
        public DetailsRow pullEditRecord(int id)
        {
            DetailsRow noteRecord = new DetailsRow();
            conn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"Select ID, app, (Select decision_ID FROM exts_seDecision_lut where decision_ID = e.se_decision) as se_decision, "+
                                $"meeting_notes, ISNULL(critical_date, '') as critical_date, meeting_date, history_notes, reviewer from exts_notes_tracking as e WHERE ID = {id}";
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                noteRecord.dNoteID = Convert.ToInt32(rdr["ID"]);
                noteRecord.dAppNo = rdr["app"].ToString();
                //beware that this se is the decision id not the text
                noteRecord.dReviewer = rdr["reviewer"].ToString();
                noteRecord.dSeDecision = rdr["se_decision"].ToString();
                noteRecord.dMeetingNotes = rdr["meeting_notes"].ToString();
                noteRecord.dNoteText = rdr["history_notes"].ToString();
                noteRecord.dCriticalDate = Convert.ToDateTime(rdr["critical_date"]).ToShortDateString();
                noteRecord.dMeetingDate = (rdr["meeting_date"] == System.DBNull.Value)? "": Convert.ToDateTime(rdr["meeting_date"]).ToShortDateString();
            }
            rdr.Close();
            cmd.CommandText = $"Select owner_name FROM exts_notes_owners WHERE note_ID = {noteRecord.dNoteID}";
            SqlDataReader rdr2 = cmd.ExecuteReader();
            noteRecord.dOwnerList = new List<string>();
            while (rdr2.Read())
            {
                string owner = rdr2["owner_name"].ToString();
                noteRecord.dOwnerList.Add(owner);
            }
            //noteRecord.dOwnerList owners;
            rdr2.Close();
            conn.Close();
            return noteRecord;
        }
        public List<SeDecision> getSeDecisions()
        {
            List<SeDecision> decisionList = new List<SeDecision>();
            DataTableClass dtc = new DataTableClass();
            
            SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"Select decision_ID, SE_Decision FROM exts_seDecision_lut";
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {SeDecision decisions = new SeDecision();
                decisions.decisionID =rdr["decision_ID"].ToString();
                decisions.decisionText = rdr["SE_Decision"].ToString();
                decisionList.Add(decisions);
            }
            dtc.decisionList = decisionList;
            rdr.Close();
            conn.Close();
            return decisionList;
        }
        public List<string> getReferenceDates(string app)
        {//call in create new modal for reference date
            List<string> refDates = new List<string>();
            SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"Select  ISNULL(pbu_filed_dt, '') as pbu_filed_dt, ISNULL(pbu_due_dt, '') as pbu_due_dt, "+
                $"ISNULL(poc_filed_dt, '') as poc_filed_dt ,ISNULL(poc_due_dt, '') as poc_due_dt from main_src where app = '{app}'";
            SqlDataReader rdr = cmd.ExecuteReader();
            string refDate = "";
            while (rdr.Read())
            {
                for(int i = 0; i< 4; i++)
                {
                    refDate = Convert.ToDateTime(rdr[i]).ToShortDateString();
                    refDates.Add(refDate);
                }
            }
            rdr.Close();
            conn.Close();
            return refDates;    
        }             
        public string getOrigDueDate(string app)
        {
            string OrigDueDate="";
            SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"Select MAX(ISNULL(critical_date, '')) as critical_date from exts_notes_tracking where app = '{app}'";
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                OrigDueDate = Convert.ToDateTime(rdr["critical_date"]).ToShortDateString();
            }
            rdr.Close();
            conn.Close();
            return OrigDueDate;
        }
    }
}
