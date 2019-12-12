﻿using CSETWeb_Api.BusinessLogic.BusinessManagers;
using CSETWeb_Api.BusinessLogic.BusinessManagers.AdminTab;
using CSETWeb_Api.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CSETWeb_Api.Controllers
{

    [CSETAuthorize]
    public class AdminTabController : ApiController
    {
        [HttpPost,HttpGet]
        [Route("api/admintab/Data")]
        public AdminTabData GetList()
        {
            int assessmentId = Auth.AssessmentForUser();                                    
            AdminTabManager manager = new AdminTabManager();
            return manager.GetTabData(assessmentId);            
        }

        [HttpPost]
        [Route("api/admintab/save")]
        public AdminSaveResponse SaveData([FromBody]AdminSaveData save)
        {
            int assessmentId = Auth.AssessmentForUser();                       
            AdminTabManager manager = new AdminTabManager();
            return manager.SaveData(assessmentId, save);
        }


        [HttpPost]
        [Route("api/admintab/saveattribute")]
        public void SaveDataAttribute([FromBody] AttributePair attribute)
        {
            int assessmentId = Auth.AssessmentForUser();            
            AdminTabManager manager = new AdminTabManager();
            manager.SaveDataAttribute(assessmentId, attribute);

        }
    }

   
}
