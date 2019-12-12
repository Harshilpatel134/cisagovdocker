//////////////////////////////// 
// 
//   Copyright 2019 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using DataLayerCore.Model;
using Microsoft.EntityFrameworkCore;
using Nelibur.ObjectMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSETWeb_Api.Data.ControlData
{

    /**Design notes.  
     * Findings handles getting all the findings collection for the top level page
     * The individual finding deals with the contacts etc.
     */
    public class FindingViewModel
    {
        private CSET_Context context;

        private FINDING dbFinding;
        private Finding webFinding;
        private int finding_Id;

        /// <summary>
        ///  The passed in finding is the source, if the ID does not exist it will create it. 
        ///  this will also push the data to the database and retrieve any auto identity id's 
        /// </summary>
        /// <param name="f">source finding</param>
        /// <param name="context">the data context to work on</param>
        public FindingViewModel(Finding f, CSET_Context context)
        {
            //get all the contacts in this assessment
            //get all the contexts on this finding
            this.webFinding = f;

            if (f.IsFindingEmpty())
            {
                return;
            }

            this.context = context;

            this.dbFinding = context.FINDING
                .Include(x => x.FINDING_CONTACT)
                .Where(x => x.Answer_Id == f.Answer_Id && x.Finding_Id == f.Finding_Id)
                .FirstOrDefault();
            if (dbFinding == null)
            {
                var finding = new FINDING
                {
                    Answer_Id = f.Answer_Id,
                    Summary = f.Summary,
                    Impact = f.Impact,
                    Issue = f.Issue,
                    Recommendations = f.Recommendations,
                    Vulnerabilities = f.Vulnerabilities,
                    Resolution_Date = f.Resolution_Date
                };

                this.dbFinding = finding;
                context.FINDING.Add(finding);
            }

            TinyMapper.Map(f, this.dbFinding);

            int importid = (f.Importance_Id == null) ? 1 : (int)f.Importance_Id;
            this.dbFinding.Importance_ = context.IMPORTANCE.Where(x => x.Importance_Id == importid).FirstOrDefault();//note that 1 is the id of a low importance

            if (f.Finding_Contacts != null)
            {
                foreach (FindingContact fc in f.Finding_Contacts)
                {
                    if (fc.Selected)
                    {
                        FINDING_CONTACT tmpC = dbFinding.FINDING_CONTACT.Where(x => x.Assessment_Contact_Id == fc.Assessment_Contact_Id).FirstOrDefault();
                        if (tmpC == null)
                            dbFinding.FINDING_CONTACT.Add(new FINDING_CONTACT() { Assessment_Contact_Id = fc.Assessment_Contact_Id, Finding_Id = f.Finding_Id });
                    }
                    else
                    {
                        FINDING_CONTACT tmpC = dbFinding.FINDING_CONTACT.Where(x => x.Assessment_Contact_Id == fc.Assessment_Contact_Id).FirstOrDefault();
                        if (tmpC != null)
                            dbFinding.FINDING_CONTACT.Remove(tmpC);
                    }
                }
            }
        }

        /// <summary>
        /// Will not create a new assessment
        /// if you pass a non-existent finding then it will throw an exception
        /// </summary>
        /// <param name="finding_Id"></param>
        /// <param name="context"></param>
        public FindingViewModel(int finding_Id, CSET_Context context)
        {
            this.finding_Id = finding_Id;
            this.context = context;
            this.dbFinding = context.FINDING.Where(x=>  x.Finding_Id == finding_Id).FirstOrDefault();
            if (dbFinding == null)
            {
                throw new ApplicationException("Cannot find finding_id" + finding_Id);
            }                        
        }

        public void Delete()
        {
            dbFinding.FINDING_CONTACT.ToList().ForEach(s => context.FINDING_CONTACT.Remove(s));
            context.FINDING.Remove(dbFinding);
            context.SaveChanges();
        }

        public void Save()
        {
            // safety valve in case this was built without an answerid
            if (this.webFinding.Answer_Id == 0)
            {
                return;
            }
          


            if (this.webFinding.IsFindingEmpty())
                return;

            //var contacts = Contacts.Where(s => s.Id != new Guid()).Distinct().ToList();
            //finding.FINDING_CONTACT.Where(s => !contacts.Select(se => se.Id).Contains(s.Assessment_Contact_Id)).ToList().ForEach(t => finding.FINDING_CONTACT.Remove(t));
            //contacts.Where(s => !finding.FINDING_CONTACT.Select(se => se.Assessment_Contact_Id).Contains(s.Id)).ToList().ForEach(t => finding.FINDING_CONTACT.Add(new FINDING_CONTACT { Assessment_Contact_Id = t.Id, FINDING = finding }));
            //context.AssessmentContext.Entry(finding).State = EntityState.Modified;
            context.SaveChanges();
            webFinding.Finding_Id = dbFinding.Finding_Id;
        }
    }
}

