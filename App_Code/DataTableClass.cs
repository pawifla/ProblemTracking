using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebLayouts.App_Code
{
    public class DataTableClass
    {
        //p for parent row
        public string pAppNo { get; set; }
        public string pBasin { get; set; }
        public string pSeDecision { get; set; }
        //decisions: Yes, No, Maybe
        public string pNumExtsGranted { get; set; }
        public string pNoteStatus { get; set; }
        //stati: No More Exts, Warning Letter, Req for Info, Watch
        
        public string pPocDueDate { get; set; }
        public string referenceDate { get; set; }
        //this will be a referenceDate to poc or pbu filed once we know.
        public string pCriticalDate { get; set; }

        public string pReviewer { get; set; }
        
        public string adReviewer { get; set; }
        //User Name from Active Directory: to be used for inserts.s
        
        public List<DetailsRow> details  { get; set; }

        public List<string> ownerList { get; set; }
        //possibly use this for the insert
        //this for when the user want to search by owner
        public List<SeDecision> decisionList { get; set; }
        public string pMOU { get; set; }
        public string pPriorityDate { get; set; }
        public string pMeetingDate { get; set; }
    }
    public class DetailsRow
    {
        //Notes List: ['ID', NoteText', 'SeDecision','NoteDate','Reviewer', 'NoteStatus']
        //The idea here is that only one of these will be related to a given note (one owner is the kicker)
        public string dNoteText { get; set; }

        public string dSeDecision { get; set; }

        public int dNoteID { get; set; }
      
        public string dNoteDate { get; set; }
        public string dReviewer { get; set; }
        public string dNoteStatus { get; set; }
        //Remove once dOwnerList is working
        public string dOwner { get; set; }
        public List<string> dOwnerList { get; set; }

        public string dAppNo { get; set; }

        public string dMeetingNotes { get; set; }
        public string dCriticalDate { get; set; }
        public string dMeetingDate { get; set; }
    }
    public class OwnerInfo
    {//this is used for child owner modal. displays div/duty/acres
        public string OwnerName { get; set; }
        public char OwnerType { get; set; }
        public float OwnersAcres { get; set; }
        public float OwnersDivRate { get; set; }
        public float OwnersDuty { get; set; }

    }
    public class SeDecision
    {
        public string decisionID { get; set; }
        public string decisionText { get; set; }
    }
}