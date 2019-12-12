//////////////////////////////// 
// 
//   Copyright 2019 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using BusinessLogic.Helpers;
using CSET_Main.Data.ControlData;
using CSET_Main.Questions.InformationTabData;
using CSET_Main.Views.Questions.QuestionDetails;
using CSETWeb_Api.BusinessLogic.Helpers;
using CSETWeb_Api.Models;
using DataLayerCore.Manual;
using DataLayerCore.Model;
using Nelibur.ObjectMapper;
using Snickler.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace CSETWeb_Api.BusinessManagers
{
    /// <summary>
    /// 
    /// </summary>
    public class QuestionsManager : QuestionRequirementManager
    {
        // 
        List<QuestionPlusHeaders> questions;
        
        List<FullAnswer> Answers;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="assessmentId"></param>
        public QuestionsManager(int assessmentId) : base(assessmentId)
        {

        }


        /// <summary>
        /// Returns a list of Questions.
        /// We can find questions for a single group or for all groups (*).
        /// </summary>        
        public QuestionResponse GetQuestionList(string questionGroupName)
        {
            using (var db = new CSET_Context())
            {
                IQueryable<QuestionPlusHeaders> query = null;

                string assessSalLevel = db.STANDARD_SELECTION.Where(ss => ss.Assessment_Id == _assessmentId).Select(c => c.Selected_Sal_Level).FirstOrDefault();
                string assessSalLevelUniversal = db.UNIVERSAL_SAL_LEVEL.Where(x => x.Full_Name_Sal == assessSalLevel).Select(x => x.Universal_Sal_Level1).First();

                if (_setNames.Count == 1)
                {
                    // If a single standard is selected, do it this way
                    query = from q in db.NEW_QUESTION
                            from qs in db.NEW_QUESTION_SETS.Where(x => x.Question_Id == q.Question_Id)
                            from l in db.NEW_QUESTION_LEVELS.Where(x => qs.New_Question_Set_Id == x.New_Question_Set_Id)
                            from s in db.SETS.Where(x => x.Set_Name == qs.Set_Name && x.Set_Name == qs.Set_Name)
                            from usl in db.UNIVERSAL_SAL_LEVEL.Where(x => x.Full_Name_Sal == assessSalLevel)
                            from usch in db.UNIVERSAL_SUB_CATEGORY_HEADINGS.Where(x => x.Heading_Pair_Id == q.Heading_Pair_Id)
                            from qgh in db.QUESTION_GROUP_HEADING.Where(x => x.Question_Group_Heading_Id == usch.Question_Group_Heading_Id)
                            from usc in db.UNIVERSAL_SUB_CATEGORIES.Where(x => x.Universal_Sub_Category_Id == usch.Universal_Sub_Category_Id)
                            where _setNames.Contains(s.Set_Name)
                               && l.Universal_Sal_Level == usl.Universal_Sal_Level1

                            select new QuestionPlusHeaders()
                            {
                                QuestionId = q.Question_Id,
                                SimpleQuestion = q.Simple_Question,
                                QuestionGroupHeadingId = qgh.Question_Group_Heading_Id,
                                QuestionGroupHeading = qgh.Question_Group_Heading1,
                                UniversalSubCategoryId = usc.Universal_Sub_Category_Id,
                                UniversalSubCategory = usc.Universal_Sub_Category,
                                SubHeadingQuestionText = usch.Sub_Heading_Question_Description,
                                PairingId = usch.Heading_Pair_Id
                            };

                    // Get the questions for the specified group (or all groups)  
                    if (!string.IsNullOrEmpty(questionGroupName) && questionGroupName != "*")
                    {
                        query = query.Where(x => x.QuestionGroupHeading == questionGroupName);
                    }
                }
                else
                {
                    query = from q in db.NEW_QUESTION
                            join qs in db.NEW_QUESTION_SETS on q.Question_Id equals qs.Question_Id
                            join nql in db.NEW_QUESTION_LEVELS on qs.New_Question_Set_Id equals nql.New_Question_Set_Id
                            join usch in db.UNIVERSAL_SUB_CATEGORY_HEADINGS on q.Heading_Pair_Id equals usch.Heading_Pair_Id
                            join stand in db.AVAILABLE_STANDARDS on qs.Set_Name equals stand.Set_Name
                            join qgh in db.QUESTION_GROUP_HEADING on usch.Question_Group_Heading_Id equals qgh.Question_Group_Heading_Id
                            join usc in db.UNIVERSAL_SUB_CATEGORIES on usch.Universal_Sub_Category_Id equals usc.Universal_Sub_Category_Id
                            where stand.Selected == true && stand.Assessment_Id == _assessmentId
                            select new QuestionPlusHeaders()
                            {
                                QuestionId = q.Question_Id,
                                SimpleQuestion = q.Simple_Question,
                                QuestionGroupHeadingId = qgh.Question_Group_Heading_Id,
                                QuestionGroupHeading = qgh.Question_Group_Heading1,
                                UniversalSubCategoryId = usc.Universal_Sub_Category_Id,
                                UniversalSubCategory = usc.Universal_Sub_Category,
                                SubHeadingQuestionText = usch.Sub_Heading_Question_Description,
                                PairingId = usch.Heading_Pair_Id
                            };
                }

                // Get all answers for the assessment
                var answers = from a in db.ANSWER.Where(x => x.Assessment_Id == _assessmentId && !x.Is_Requirement)
                              from b in db.VIEW_QUESTIONS_STATUS.Where(x => x.Answer_Id == a.Answer_Id).DefaultIfEmpty()
                              from c in db.FINDING.Where(x => x.Answer_Id == a.Answer_Id).DefaultIfEmpty()
                              select new FullAnswer() { a = a, b = b, FindingsExist = c != null };

                this.questions = query.Distinct().ToList();
                this.Answers = answers.ToList();

                // Merge the questions and answers into a hierarchy
                return BuildResponse();
            }
        }

      


        /// <summary>
        /// Returns a list of answer IDs that are currently 'active' on the
        /// Assessment, given its SAL level and selected Standards.
        /// 
        /// This piggy-backs on GetQuestionList() so that we don't need to support
        /// multiple copies of the question and answer queries.
        /// </summary>
        /// <returns></returns>
        public List<int> GetActiveAnswerIds()
        {
            QuestionResponse resp = this.GetQuestionList(null);

            List<int> relevantAnswerIds = this.Answers.Where(ans =>
                this.questions.Select(q => q.QuestionId).Contains(ans.a.Question_Or_Requirement_Id))
                .Select(x => x.a.Answer_Id)
                .ToList<int>();

            return relevantAnswerIds;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="assessmentid"></param>
        /// <returns></returns>
        public QuestionDetailsContentViewModel GetDetails(int questionId, int assessmentid, bool IsComponent)
        {
            using (CSET_Context datacontext = new CSET_Context()) {
                QuestionDetailsContentViewModel qvm = new QuestionDetailsContentViewModel(
                    new StandardSpecficLevelRepository(datacontext),
                    new InformationTabBuilder(datacontext),
                    datacontext
                );
                qvm.getQuestionDetails(questionId, assessmentid, IsComponent);
                return qvm;
            }
        }


        /// <summary>
        /// Constructs a response containing the applicable questions with their answers.
        /// </summary>
        /// <returns></returns>
        private QuestionResponse BuildResponse()
        {
            List<QuestionGroup> groupList = new List<QuestionGroup>();
            QuestionGroup qg = new QuestionGroup();
            QuestionSubCategory sc = new QuestionSubCategory();
            QuestionAnswer qa = new QuestionAnswer();

            int curGroupId = 0;
            int curPairingId = 0;


            int displayNumber = 0;

            foreach (var dbQ in this.questions.OrderBy(x => x.QuestionGroupHeading)
                .ThenBy(x => x.UniversalSubCategory)
                .ThenBy(x => x.QuestionId))
            {
                if (dbQ.QuestionGroupHeadingId != curGroupId)
                {
                    qg = new QuestionGroup()
                    {
                        GroupHeadingId = dbQ.QuestionGroupHeadingId,
                        GroupHeadingText = dbQ.QuestionGroupHeading,
                        StandardShortName = "Standard Questions"
                    };

                    groupList.Add(qg);

                    curGroupId = qg.GroupHeadingId;
                    curPairingId = 0;

                    // start numbering again in new group
                    displayNumber = 0;
                }
                



                // new subcategory -- break on pairing ID to separate 'base' and 'custom' pairings
                if (dbQ.PairingId != curPairingId)
                {
                    sc = new QuestionSubCategory()
                    {
                        GroupHeadingId = qg.GroupHeadingId,
                        SubCategoryId = dbQ.UniversalSubCategoryId,
                        SubCategoryHeadingText = dbQ.UniversalSubCategory,
                        HeaderQuestionText = dbQ.SubHeadingQuestionText,
                        SubCategoryAnswer = this.subCatAnswers.Where(sca => sca.GroupHeadingId == qg.GroupHeadingId
                                                && sca.SubCategoryId == dbQ.UniversalSubCategoryId)
                                                .FirstOrDefault()?.AnswerText
                    };

                    qg.SubCategories.Add(sc);

                    curPairingId = dbQ.PairingId;
                }

                FullAnswer answer = this.Answers.Where(x => x.a.Question_Or_Requirement_Id == dbQ.QuestionId).FirstOrDefault();

                qa = new QuestionAnswer()
                {
                    DisplayNumber = (++displayNumber).ToString(),
                    QuestionId = dbQ.QuestionId,
                    QuestionText = FormatLineBreaks(dbQ.SimpleQuestion),
                    Answer = answer?.a?.Answer_Text,
                    Answer_Id = answer?.a?.Answer_Id,
                    AltAnswerText = answer?.a?.Alternate_Justification,
                    Comment = answer?.a?.Comment,
                    Feedback = answer?.a?.Feedback,
                    MarkForReview = answer?.a.Mark_For_Review ?? false,
                    Reviewed = answer?.a.Reviewed ?? false,
                    Is_Component = answer?.a.Is_Component ?? false,
                    ComponentGuid = answer?.a.Component_Guid ?? Guid.Empty,
                    Is_Requirement = answer?.a.Is_Requirement ?? false
                };
                if (answer != null)
                {
                    TinyMapper.Map<VIEW_QUESTIONS_STATUS, QuestionAnswer>(answer.b, qa);
                }

                sc.Questions.Add(qa);
            }

            QuestionResponse resp = new QuestionResponse
            {
                QuestionGroups = groupList,
                ApplicationMode = this.applicationMode
            };

            resp.QuestionCount = this.NumberOfQuestions();
            resp.RequirementCount = new RequirementsManager(this._assessmentId).NumberOfRequirements();

            BuildComponentsResponse(resp);
            return resp;
        }

        public QuestionResponse GetOverrideListOnly()
        {
            QuestionResponse resp = new QuestionResponse
            {
                QuestionGroups = new List<QuestionGroup>(),
                ApplicationMode = this.applicationMode
            };

            resp.QuestionCount = 0;
            resp.RequirementCount = 0;

            BuildOverridesOnly(resp, new CSET_Context());
            return resp;
        }


        public List<Answer_Components_Exploded_ForJSON> GetOverrideQuestions(int assessmentId, int question_id, int Component_Symbol_Id)
        {
            List<Answer_Components_Exploded_ForJSON> rlist = new List<Answer_Components_Exploded_ForJSON>();
            using (CSET_Context context = new CSET_Context())
            {
                List<usp_getExplodedComponent> questionlist = null;

                context.LoadStoredProc("[dbo].[usp_getExplodedComponent]")
                  .WithSqlParam("assessment_id", _assessmentId)
                  .ExecuteStoredProc((handler) =>
                  {
                      questionlist = handler.ReadToList<usp_getExplodedComponent>().Where(c => c.Question_Id == question_id
                                    && c.Component_Symbol_Id == Component_Symbol_Id).ToList();
                  });

                IQueryable<Answer_Components> answeredQuestionList = context.Answer_Components.Where(a =>
                    a.Assessment_Id == assessmentId && a.Question_Or_Requirement_Id == question_id);
                    

                foreach(var question in questionlist.ToList())
                {
                    Answer_Components_Exploded_ForJSON tmp = null;
                    tmp = TinyMapper.Map<Answer_Components_Exploded_ForJSON>(question);
                    tmp.Component_GUID = question.Component_GUID.ToString();
                    rlist.Add(tmp);
                }
                return rlist;
            }
        }

        /// <summary>
        /// get the exploded view where assessment
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="shouldSave"></param>
        public void HandleGuid(Guid guid, bool shouldSave)
        {
            using (CSET_Context context = new CSET_Context())
            {
                if (shouldSave)
                {   
                    var componentName = context.ASSESSMENT_DIAGRAM_COMPONENTS.Where(x => x.Component_Guid == guid).FirstOrDefault();
                    if (componentName != null)
                    {
                        var creates = from a in context.COMPONENT_QUESTIONS
                                      where a.Component_Symbol_Id == componentName.Component_Symbol_Id
                                      select a;
                        var alreadyThere = (from a in context.ANSWER
                                            where a.Assessment_Id == _assessmentId
                                            && a.Component_Guid == guid
                                            select a).ToDictionary(x => x.Question_Or_Requirement_Id, x=> x);
                        foreach (var c in creates.ToList())
                        {
                            if (!alreadyThere.ContainsKey(c.Question_Id))
                            {
                                context.ANSWER.Add(new ANSWER()
                                {
                                    Answer_Text = Constants.UNANSWERED,
                                    Assessment_Id = this._assessmentId,
                                    Component_Guid = guid,
                                    Is_Component = true,
                                    Is_Requirement = false,
                                    Question_Or_Requirement_Id = c.Question_Id
                                });
                            }
                        }
                        context.SaveChanges();
                    }
                    else
                    {
                        throw new ApplicationException("could not find component for guid:" + guid);
                    }
                }
                else
                {
                    foreach (var a in context.ANSWER.Where(x => x.Component_Guid == guid).ToList())
                    {
                        context.ANSWER.Remove(a);
                    }
                    context.SaveChanges();
                }
            }
        }



      

     


        /// <summary>
        /// Returns the number of questions that are relevant for the selected standards 
        /// when in QUESTIONS mode.
        /// 
        /// The query differs whether a single or multiple standards are selected.
        /// 
        /// TODO:  These queries are copies of the ones above.  Find a way to have a single instance of each query
        /// that can be used for both full data queries and counts in an efficient way.
        /// </summary>
        /// <returns></returns>
        public int NumberOfQuestions()
        {
            using (var db = new CSET_Context())
            {
                if (_setNames.Count == 1)
                {
                    // If a single standard is selected, do it this way
                    string selectedSalLevel = db.STANDARD_SELECTION.Where(ss => ss.Assessment_Id == _assessmentId).Select(c => c.Selected_Sal_Level).FirstOrDefault();

                    var query1 = from q in db.NEW_QUESTION
                                 from qs in db.NEW_QUESTION_SETS.Where(x => x.Question_Id == q.Question_Id)
                                 from l in db.NEW_QUESTION_LEVELS.Where(x => qs.New_Question_Set_Id == x.New_Question_Set_Id)
                                 from s in db.SETS.Where(x => x.Set_Name == qs.Set_Name)
                                 from usl in db.UNIVERSAL_SAL_LEVEL.Where(x => x.Full_Name_Sal == selectedSalLevel)
                                 where _setNames.Contains(s.Set_Name)
                                    && l.Universal_Sal_Level == usl.Universal_Sal_Level1
                                 select q.Question_Id;

                    return query1.Distinct().Count();
                }
                else
                {
                    var query2 = from q in db.NEW_QUESTION
                                 join qs in db.NEW_QUESTION_SETS on q.Question_Id equals qs.Question_Id
                                 join nql in db.NEW_QUESTION_LEVELS on qs.New_Question_Set_Id equals nql.New_Question_Set_Id
                                 join usch in db.UNIVERSAL_SUB_CATEGORY_HEADINGS on q.Heading_Pair_Id equals usch.Heading_Pair_Id
                                 join stand in db.AVAILABLE_STANDARDS on qs.Set_Name equals stand.Set_Name
                                 join qgh in db.QUESTION_GROUP_HEADING on usch.Question_Group_Heading_Id equals qgh.Question_Group_Heading_Id
                                 join usc in db.UNIVERSAL_SUB_CATEGORIES on usch.Universal_Sub_Category_Id equals usc.Universal_Sub_Category_Id
                                 where stand.Selected == true && stand.Assessment_Id == _assessmentId
                                 select q.Question_Id;

                    return query2.Distinct().Count();
                }
            }
        }


        /// <summary>
        /// Persists a single answer to the SUB_CATEGORY_ANSWERS table for the 'block answer',
        /// and flips all of the constituent questions' answers.
        /// </summary>
        public void StoreSubcategoryAnswers(SubCategoryAnswers subCatAnswerBlock)
        {
            if (subCatAnswerBlock == null)
            {
                return;
            }

            // SUB_CATEGORY_ANSWERS
            var db = new CSET_Context();


            // Get the USCH so that we will know the Heading_Pair_Id
            var usch = db.UNIVERSAL_SUB_CATEGORY_HEADINGS.Where(u => u.Question_Group_Heading_Id == subCatAnswerBlock.GroupHeadingId
            && u.Universal_Sub_Category_Id == subCatAnswerBlock.SubCategoryId).FirstOrDefault();


            var subCatAnswer = db.SUB_CATEGORY_ANSWERS.Where(sca => sca.Assessement_Id == _assessmentId
                            && sca.Heading_Pair_Id == usch.Heading_Pair_Id).FirstOrDefault();


            if (subCatAnswer == null)
            {
                subCatAnswer = new SUB_CATEGORY_ANSWERS(); 
            }
            subCatAnswer.Assessement_Id = _assessmentId;
            subCatAnswer.Heading_Pair_Id = usch.Heading_Pair_Id;
            subCatAnswer.Answer_Text = subCatAnswerBlock.SubCategoryAnswer;
            db.SUB_CATEGORY_ANSWERS.AddOrUpdate(subCatAnswer, x=>x.Assessement_Id, x=>x.Heading_Pair_Id);

            db.SaveChanges();

            AssessmentUtil.TouchAssessment(_assessmentId);

            // loop and store all of the subcategory's answers
            foreach (Answer ans in subCatAnswerBlock.Answers)
            {
                StoreAnswer(ans);
            }
        }
    }


    /// <summary>
    /// A convenience class so that we can pass around a collection of questions plus heading/category text.
    /// </summary>
    public class QuestionPlusHeaders
    {
        public int QuestionId;
        public string SimpleQuestion;

        public int QuestionGroupHeadingId;
        public string QuestionGroupHeading;

        public int UniversalSubCategoryId;
        public string UniversalSubCategory;
        public string SubHeadingQuestionText;
        public int PairingId;
    }


    /// <summary>
    /// A convenience class that includes UNIVERSAL_SUB_CATEGORY_HEADINGS 
    /// with the SUB_CATEGORY_ANSWERS so that we can get the keys from the bridge
    /// table.
    /// </summary>
    public class SubCategoryAnswersPlus
    {
        public int AssessmentId;
        public int HeadingId;
        public string AnswerText;
        public int GroupHeadingId;
        public int SubCategoryId;
    }
}


