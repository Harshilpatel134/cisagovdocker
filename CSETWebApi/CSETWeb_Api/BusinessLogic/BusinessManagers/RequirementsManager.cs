//////////////////////////////// 
// 
//   Copyright 2019 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CSETWeb_Api.Models;
using DataLayerCore.Model;
using Nelibur.ObjectMapper;
using static CSETWeb_Api.BusinessLogic.ReportEngine.BasicReportData;
using CSETWeb_Api.BusinessLogic.Helpers;
using CSETWeb_Api.BusinessLogic.Models;
using CSETWeb_Api.BusinessLogic.BusinessManagers.Analysis;
using Microsoft.EntityFrameworkCore;

namespace CSETWeb_Api.BusinessManagers
{
    /// <summary>
    /// Manages Requirements.  This class is similar to the QuestionsManager
    /// but is encapsulated in its own component for clarity.
    /// </summary>
    public class RequirementsManager : QuestionRequirementManager
    {
        List<NEW_REQUIREMENT> Requirements;
        List<FullAnswer> Answers;
        Dictionary<int, MaturityMap> matLevels;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="assessmentId"></param>
        public RequirementsManager(int assessmentId) : base(assessmentId)
        {
            // Get the maturity level for all requirements
            var mm = new BusinessLogic.BusinessManagers.MaturityManager();
            matLevels = mm.GetRequirementMaturityLevels();
        }


        /// <summary>
        /// Builds a response containing Requirements that are related to the
        /// assessment's SAL level.
        /// </summary>
        public QuestionResponse GetRequirementsList()
        {
            using (var db = new CSET_Context())
            {
                RequirementsPass req = GetControls(db);
                return BuildResponse(req.Requirements.ToList(), req.Answers.ToList(), req.DomainAssessmentFactors.ToList());
            }
        }

        private RequirementsPass GetControls(CSET_Context db)
        {
            var q = from rs in db.REQUIREMENT_SETS
                    from s in db.SETS.Where(x => x.Set_Name == rs.Set_Name)
                    from r in db.NEW_REQUIREMENT.Where(x => x.Requirement_Id == rs.Requirement_Id)
                    from rl in db.REQUIREMENT_LEVELS.Where(x => x.Requirement_Id == r.Requirement_Id)
                    where _setNames.Contains(rs.Set_Name)
                        && rl.Standard_Level == _standardLevel
                    select new { r, rs, s };
            var results = q.Distinct()
                .OrderBy(x => x.s.Short_Name)
                .ThenBy(x => x.rs.Requirement_Sequence)
                .Select(x => new RequirementPlus { Requirement = x.r, SetShortName = x.s.Short_Name, SetName = x.s.Set_Name });

            var domains = (from d in db.FINANCIAL_DOMAINS
                join fg in db.FINANCIAL_GROUPS on d.DomainId equals fg.DomainId
                join af in db.FINANCIAL_ASSESSMENT_FACTORS on fg.AssessmentFactorId equals af.AssessmentFactorId
                select new DomainAssessmentFactor {DomainName = d.Domain, AssessmentFactorName = af.AssessmentFactor}).Distinct();



            // Get all REQUIREMENT answers for the assessment
            var answers = from a in db.ANSWER.Where(x => x.Assessment_Id == _assessmentId && x.Is_Requirement)
                          from b in db.VIEW_QUESTIONS_STATUS.Where(x => x.Answer_Id == a.Answer_Id).DefaultIfEmpty()
                          select new FullAnswer() { a = a, b = b };

            this.Requirements = q.Select(x => x.r).ToList();
            this.Answers = answers.ToList();

            return new RequirementsPass()
            {
                Requirements = results,
                Answers = answers, 
                DomainAssessmentFactors = domains
            };
        }


        /// <summary>
        /// Construct a response containing the applicable requirement questions with their answers.
        /// </summary>
        /// <param name="requirements"></param>
        /// <param name="answers"></param>
        /// <returns></returns>
        private QuestionResponse BuildResponse(List<RequirementPlus> requirements,
            List<FullAnswer> answers, List<DomainAssessmentFactor> domains)
        {
            List<QuestionGroup> groupList = new List<QuestionGroup>();
            QuestionGroup g = new QuestionGroup();
            QuestionSubCategory sg = new QuestionSubCategory();
            QuestionAnswer qa = new QuestionAnswer();

            string currentGroupHeading = string.Empty;
            string currentSubcategoryHeading = string.Empty;

            try
            {

                foreach (var dbRPlus in requirements)
                {

                    
                    var dbR = dbRPlus.Requirement;

                    // Make sure there are no leading or trailing spaces - it will affect the tree structure that is built
                    dbR.Standard_Category = dbR.Standard_Category==null?null: dbR.Standard_Category.Trim();
                    dbR.Standard_Sub_Category = dbR.Standard_Sub_Category==null?null: dbR.Standard_Sub_Category.Trim();

                    // If the Standard_Sub_Category is null (like CSC_V6), default it to the Standard_Category
                    if (dbR.Standard_Sub_Category == null)
                    {
                        dbR.Standard_Sub_Category = dbR.Standard_Category;
                    }


                    if (dbR.Standard_Category != currentGroupHeading)
                    {
                        g = new QuestionGroup()
                        {
                            GroupHeadingId = dbR.Question_Group_Heading_Id,
                            GroupHeadingText = dbR.Standard_Category,
                            StandardShortName = dbRPlus.SetShortName,

                        };

                        if (domains.Any(x => x.AssessmentFactorName == g.GroupHeadingText))
                        {
                            g.DomainName = domains.FirstOrDefault(x => x.AssessmentFactorName == g.GroupHeadingText)
                                .DomainName;
                        }

                        groupList.Add(g);

                        currentGroupHeading = g.GroupHeadingText;
                    }

                    // new subcategory
                    if (dbR.Standard_Sub_Category != currentSubcategoryHeading)
                    {
                        sg = new QuestionSubCategory()
                        {
                            SubCategoryId = 0,
                            SubCategoryHeadingText = dbR.Standard_Sub_Category,
                            GroupHeadingId = g.GroupHeadingId
                        };

                        g.SubCategories.Add(sg);

                        currentSubcategoryHeading = dbR.Standard_Sub_Category;
                    }
                    


                    FullAnswer answer = answers.Where(x => x.a.Question_Or_Requirement_Id == dbR.Requirement_Id).FirstOrDefault();

                        qa = new QuestionAnswer()
                        {
                            DisplayNumber = dbR.Requirement_Title,
                            QuestionId = dbR.Requirement_Id,
                            QuestionText = dbR.Requirement_Text.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace("\r", "<br/>"),
                            Answer = answer?.a.Answer_Text,
                            AltAnswerText = answer?.a.Alternate_Justification,
                            Comment = answer?.a.Comment,
                            Feedback = answer?.a.Feedback,
                            MarkForReview = answer?.a.Mark_For_Review ?? false,
                            Reviewed = answer?.a.Reviewed ?? false,
                            MaturityLevel = ReqMaturityLevel(dbR.Requirement_Id),
                            SetName = dbRPlus.SetName,
                            Is_Component = answer?.a.Is_Component ?? false,
                            Is_Requirement = answer?.a.Is_Requirement ?? false
                        };
                        if (answer != null)
                        {
                            TinyMapper.Map<VIEW_QUESTIONS_STATUS, QuestionAnswer>(answer.b, qa);
                        }

                        qa.ParmSubs = GetTokensForRequirement(qa.QuestionId, (answer != null) ? answer.a.Answer_Id : 0);

                        sg.Questions.Add(qa);
                   
                }

                QuestionResponse resp = new QuestionResponse
                {
                    QuestionGroups = groupList,
                    ApplicationMode = this.applicationMode
                };

                resp.QuestionCount = new QuestionsManager(this._assessmentId).NumberOfQuestions();
                resp.RequirementCount = this.NumberOfRequirements();

                // Get the overall risk level
                var acetDash = new ACETDashboardManager().LoadDashboard(this._assessmentId);
                resp.OverallIRP = acetDash.SumRiskLevel;
                if (acetDash.Override > 0)
                {
                    resp.OverallIRP = acetDash.Override;
                }

                BuildComponentsResponse(resp);
                return resp;
            }catch(Exception e)
            {
                throw e;
            }         
        }


        /// <summary>
        /// Returns the number of questions that are relevant for the selected standards 
        /// when in REQUIREMENTS mode.
        /// 
        /// TODO:  This query is a copy of the one above.  Find a way to have a single copy of the query
        /// that can be used for full queries or counts or whatever.
        /// </summary>
        /// <returns></returns>
        public int NumberOfRequirements()
        {
            using (var db = new CSET_Context())
            {
                var q = from rs in db.REQUIREMENT_SETS
                        from r in db.NEW_REQUIREMENT.Where(x => x.Requirement_Id == rs.Requirement_Id)
                        from rl in db.REQUIREMENT_LEVELS.Where(x => x.Requirement_Id == r.Requirement_Id)
                        where _setNames.Contains(rs.Set_Name)
                            && rl.Standard_Level == _standardLevel
                        select new { r, rs };

                return q.Distinct().Count();
            }
        }


        /// <summary>
        /// Returns a list of answer IDs that are currently 'active' on the
        /// Assessment, given its SAL level and selected Standards.
        /// 
        /// This piggy-backs on GetRequirementsList() so that we don't need to support
        /// multiple copies of the question and answer queries.
        /// </summary>
        /// <returns></returns>
        public List<int> GetActiveAnswerIds()
        {
            QuestionResponse resp = this.GetRequirementsList();

            List<int> relevantAnswerIds = this.Answers.Where(ans =>
                this.Requirements.Select(q => q.Requirement_Id).Contains(ans.a.Question_Or_Requirement_Id))
                .Select(x => x.a.Answer_Id)
                .ToList<int>();

            return relevantAnswerIds;
        }

        private void LoadParametersList(CSET_Context db)
        {
            parametersDictionary = (from p in db.PARAMETERS
                join r in db.PARAMETER_REQUIREMENTS on p.Parameter_ID equals r.Parameter_Id                
                select new { p , r })
                .GroupBy(x=> x.r.Requirement_Id)
                .ToDictionary(x=> x.Key, x=> x.Select(y=> y.p).ToList());


            parametersAssessmentList = (from pa in db.PARAMETER_ASSESSMENT
            join p in db.PARAMETERS on pa.Parameter_ID equals p.Parameter_ID
            where pa.Assessment_ID == _assessmentId
            select new ParameterAssessment() { p = p, pa = pa }).ToList();

            parametersAnswerDictionary =  (from p in db.PARAMETERS
            join pv in db.PARAMETER_VALUES on p.Parameter_ID equals pv.Parameter_Id            
            select new ParameterValues(){ p = p, pv=pv })
            .GroupBy(x=> x.pv.Answer_Id)
            .ToDictionary(x=> x.Key,x=> x.Select(y=> y).ToList());
        }

        private Dictionary<int, List<PARAMETERS>> parametersDictionary = null;
        private List<ParameterAssessment> parametersAssessmentList;
        private Dictionary<int, List<ParameterValues>> parametersAnswerDictionary;

        /// <summary>
        /// Pull any 'global' parameters for the requirement, overlaid with any 'local' parameters for the answer.
        /// </summary>
        /// <param name="reqId"></param>
        /// <param name="ansId"></param>
        /// <returns></returns>
        private List<ParameterToken> GetTokensForRequirement(int reqId, int ansId)
        {
            ParameterSubstitution ps = new ParameterSubstitution();

            using (var db = new CSET_Context())
            {
                // get the 'base' parameter values (parameter_name) for the requirement
                if (parametersDictionary == null)
                    LoadParametersList(db);
                List<PARAMETERS> qBaseLevel;
                if (parametersDictionary.TryGetValue(reqId, out qBaseLevel)) {

                foreach (var b in qBaseLevel)
                {
                        ps.Set(b.Parameter_ID, b.Parameter_Name, b.Parameter_Name, reqId, 0);
                    }
                }

                // overlay with any assessment-specific parameters for the requirement
                var qAssessLevel = parametersAssessmentList;

                foreach (var a in qAssessLevel)
                {
                    ps.Set(a.p.Parameter_ID, a.p.Parameter_Name, a.pa.Parameter_Value_Assessment, reqId, 0);
                }

                // overlay with any 'inline' values for the answer
                if (ansId != 0)
                {
                    List<ParameterValues> qLocal;
                    if (parametersAnswerDictionary.TryGetValue(ansId, out qLocal)){
	                    foreach (var local in qLocal.ToList())
	                    {
	                        ps.Set(local.p.Parameter_ID, local.p.Parameter_Name, local.pv.Parameter_Value, 0, local.pv.Answer_Id);
	                    }
                    }                
                }

                
                ps.Tokens =  ps.Tokens.OrderByDescending(x => x.Token.Length).ToList();

                return ps.Tokens;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="requirementID"></param>
        /// <returns></returns>
        private string ReqMaturityLevel(int requirementID)
        {
            if (matLevels.ContainsKey(requirementID))
            {
                return matLevels[requirementID].Acronym;
            }

            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<ParameterToken> GetDefaultParametersForAssessment()
        {
            ParameterSubstitution ps = new ParameterSubstitution();

            using (var db = new CSET_Context())
            {
                // Get the list of requirement IDs
                List<RequirementPlus> reqs = GetControls(db).Requirements.ToList();
                List<int> requirementIds = reqs.Select(r => r.Requirement.Requirement_Id).ToList();


                // get the 'base' parameter values (parameter_name) for the requirement
                var qBaseLevel = from p in db.PARAMETERS
                                 join r in db.PARAMETER_REQUIREMENTS on p.Parameter_ID equals r.Parameter_Id
                                 where requirementIds.Contains(r.Requirement_Id)
                                 select new { p, r };

                foreach (var b in qBaseLevel)
                {
                    ps.Set(b.p.Parameter_ID, b.p.Parameter_Name, b.p.Parameter_Name, b.r.Requirement_Id, 0);
                }

                // overlay with any assessment-specific parameters for the requirement
                var qAssessLevel = from pa in db.PARAMETER_ASSESSMENT
                                   join p in db.PARAMETERS on pa.Parameter_ID equals p.Parameter_ID
                                   join pr in db.PARAMETER_REQUIREMENTS on p.Parameter_ID equals pr.Parameter_Id
                                   where pa.Assessment_ID == _assessmentId
                                    && requirementIds.Contains(pr.Requirement_Id)
                                   select new { p, pa, pr };

                foreach (var a in qAssessLevel)
                {
                    ps.Set(a.p.Parameter_ID, a.p.Parameter_Name, a.pa.Parameter_Value_Assessment, a.pr.Requirement_Id, 0);
                }
            }

            return ps.Tokens;
        }


        /// <summary>
        /// Saves a new text value override in a PARAMETER_ASSESSMENT row.  
        /// </summary>
        /// <param name="requirementId"></param>
        /// <param name="parameterId"></param>
        /// <param name="answerId"></param>
        /// <param name="newText"></param>
        /// <returns></returns>
        public ParameterToken SaveAssessmentParameter(int parameterId, string newText)
        {
            using (var db = new CSET_Context())
            {
                // If an empty value is supplied, delete the PARAMETER_VALUES row.
                if (string.IsNullOrEmpty(newText))
                {
                    var g = db.PARAMETER_ASSESSMENT.Where(p => p.Parameter_ID == parameterId 
                            && p.Assessment_ID == this._assessmentId).FirstOrDefault();
                    if (g != null)
                    {
                        db.PARAMETER_ASSESSMENT.Remove(g);
                        db.SaveChanges();
                    }

                    AssessmentUtil.TouchAssessment(_assessmentId);

                    // build a partial return object just to inform the UI what the new value is
                    var baseParameter = db.PARAMETERS.Where(p => p.Parameter_ID == parameterId).First();
                    return new ParameterToken(baseParameter.Parameter_ID, "", baseParameter.Parameter_Name, 0, 0);
                }


                // Otherwise, insert or update the PARAMETER_ASSESSMENT record
                var pa = db.PARAMETER_ASSESSMENT.Where(p => p.Parameter_ID == parameterId
                        && p.Assessment_ID == this._assessmentId).FirstOrDefault();

                if (pa == null)
                {
                    pa = new PARAMETER_ASSESSMENT();
                }

                pa.Assessment_ID = this._assessmentId;
                pa.Parameter_ID = parameterId;
                pa.Parameter_Value_Assessment = newText;

                if (db.PARAMETER_ASSESSMENT.Find(pa.Parameter_ID, pa.Assessment_ID) == null)
                {
                    db.PARAMETER_ASSESSMENT.Add(pa);
                }
                else
                {
                    db.PARAMETER_ASSESSMENT.Update(pa);
                }
                db.SaveChanges();

                AssessmentUtil.TouchAssessment(_assessmentId);

                // build a partial return object just to inform the UI what the new value is
                return new ParameterToken(pa.Parameter_ID, "", pa.Parameter_Value_Assessment, 0, 0);
            }
        }


        /// <summary>
        /// Saves a new text value override in a PARAMETER_VALUES row.  
        /// Creates a new Answer if need be.
        /// If no text is provided, any PARAMETER_VALUES is deleted.
        /// </summary>
        /// <param name="requirementId"></param>
        /// <param name="parameterId"></param>
        /// <param name="answerId"></param>
        /// <param name="newText"></param>
        /// <returns></returns>
        public ParameterToken SaveAnswerParameter(int requirementId, int parameterId, int answerId, string newText)
        {
            // create an answer if there isn't one already
            if (answerId == 0)
            {
                Answer ans = new Answer()
                {
                    QuestionId = requirementId,
                    Is_Requirement = true,
                    MarkForReview = false,
                    QuestionNumber = 0,
                    AnswerText = "U"                    
                };
                answerId = StoreAnswer(ans);
            }

            using (var db = new CSET_Context())
            {
                // If an empty value is supplied, delete the PARAMETER_VALUES row.
                if (string.IsNullOrEmpty(newText))
                {
                    var g = db.PARAMETER_VALUES.Where(pv => pv.Parameter_Id == parameterId && pv.Answer_Id == answerId).FirstOrDefault();
                    if (g != null)
                    {
                        db.PARAMETER_VALUES.Remove(g);
                        db.SaveChanges();
                    }

                    AssessmentUtil.TouchAssessment(_assessmentId);

                    return this.GetTokensForRequirement(requirementId, answerId).Where(p => p.Id == parameterId).First();
                }


                // Otherwise, add or update the PARAMETER_VALUES row
                var dbParameterValues = db.PARAMETER_VALUES.Where(pv => pv.Parameter_Id == parameterId
                        && pv.Answer_Id == answerId).FirstOrDefault();

                if (dbParameterValues == null)
                {
                    dbParameterValues = new PARAMETER_VALUES();
                }

                dbParameterValues.Answer_Id = answerId;
                dbParameterValues.Parameter_Id = parameterId;
                dbParameterValues.Parameter_Is_Default = false;
                dbParameterValues.Parameter_Value = newText;


                if (db.PARAMETER_VALUES.Find(dbParameterValues.Answer_Id, dbParameterValues.Parameter_Id) == null)
                {
                    db.PARAMETER_VALUES.Add(dbParameterValues);
                }
                else
                {
                    db.PARAMETER_VALUES.Update(dbParameterValues);
                }
                db.SaveChanges();

                AssessmentUtil.TouchAssessment(_assessmentId);

                // Return the token that was just updated
                return this.GetTokensForRequirement(requirementId, answerId).Where(p => p.Id == parameterId).First();
            }
        }


        /// <summary>
        /// Returns Requirement text with Parameter substitutions applied.
        /// Also converts linefeed characters to HTML.
        /// </summary>
        /// <param name="requirementText"></param>
        /// <returns></returns>
        public string ResolveParameters(int reqId, int ansId, string requirementText)
        {
            List<ParameterToken> tokens = this.GetTokensForRequirement(reqId, ansId);
            foreach (ParameterToken t in tokens)
            {
                requirementText = requirementText.Replace(t.Token, t.Substitution);
            }

            requirementText = requirementText.Replace("\r\n", "<br/>").Replace("\r", "<br/>").Replace("\n", "<br/>");

            return requirementText;
        }

        public string RichTextParameters (int reqId, int ansId, string requirementText)
        {
            List<ParameterToken> tokens = this.GetTokensForRequirement(reqId, ansId);
            foreach (ParameterToken t in tokens)
            {
                requirementText = requirementText.Replace(t.Token, t.Substitution);
            }

            requirementText = requirementText.Replace("\r\n", "%0D%0A").Replace("\r", "%0D%0A").Replace("\n", "%0D%0A");

            return requirementText;

        }
    }


    internal class RequirementsPass
    {
        public IQueryable<RequirementPlus> Requirements { get; set; }
        public IQueryable<FullAnswer> Answers { get; set; }
        public IQueryable<DomainAssessmentFactor> DomainAssessmentFactors { get; set; }
    }


    /// <summary>
    /// A super class that contains a Requirment as well as it's set short name.
    /// This is done so that the short name can be included in the response for
    /// display purposes.
    /// </summary>
    internal class RequirementPlus
    {
        public NEW_REQUIREMENT Requirement;
        public string SetShortName;
        public string SetName;
    }

    internal class DomainAssessmentFactor
    {
        public string DomainName;
        public string AssessmentFactorName;
    }

    internal class ParameterAssessment
    {
        public PARAMETERS p { get; set; }
        public PARAMETER_ASSESSMENT pa { get; set; }
    }

    internal class ParameterValues
    {
        public PARAMETERS p { get; set; }
        public PARAMETER_VALUES pv { get; set; }
    }
}

