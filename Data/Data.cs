using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebLayouts.Data
{
    public class Data
    {
        public string permitNumber { get; set; }
        public string ownerName { get; set; }

        public string[] notesArr { get; set; }
        public string noteStatus { get; set; }
        public DateTime date { get; set; }
    }
}