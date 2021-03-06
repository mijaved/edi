﻿using AIMS_BD_IATI.DAL;
using AIMS_BD_IATI.Library.Parser.ParserIATIv2;
using AIMS_DB_IATI.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MoreLinq;
using AIMS_BD_IATI.Library;
namespace AIMS_DB_IATI.WebAPI.Controllers
{
    //[Authorize] commented allow all users
    public class CFnTFController : ApiController
    {
        AimsDAL aimsDAL = new AimsDAL();
        AimsDbIatiDAL aimsDbIatiDAL = new AimsDbIatiDAL();
        [HttpGet]
        public CFnTFModel GetAssignedActivities(string dp)
        {
            if (string.IsNullOrEmpty(dp)) return null;

            Sessions.CFnTFModel = aimsDbIatiDAL.GetAssignActivities(dp);
            var trustFunds = aimsDAL.GetTrustFunds(dp);
            return new CFnTFModel
            {
                AssignedActivities = Sessions.CFnTFModel.AssignedActivities,
                AimsProjects = Sessions.CFnTFModel.AimsProjects,
                TrustFunds = trustFunds
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assignedActivities">other dp's project (co-finance and trust fund projects)</param>
        /// <returns>CFnTFModel</returns>
        [AcceptVerbs("GET", "POST")]
        public CFnTFModel SubmitAssignedActivities(List<iatiactivity> assignedActivities)
        {
            if (assignedActivities == null) return Sessions.CFnTFModel;
            CFnTFModel CFnTFModel = new CFnTFModel();
            CFnTFModel.AssignedActivities = assignedActivities;
            #region Co-financed
            CFnTFModel.AimsProjects = (from i in assignedActivities
                                       join a in Sessions.CFnTFModel.AimsProjects on i.MappedProjectId equals a.ProjectId
                                       where i.MappedTrustFundId == default(int)
                                       select a).DistinctBy(d => d.ProjectId).ToList();

            foreach (var project in CFnTFModel.AimsProjects)
            {
                var acts = assignedActivities.FindAll(f => f.MappedProjectId == project.ProjectId);
                project.MatchedProjects.AddRange(acts);
            }
            #endregion

            #region TrustFund
            //get all trust fund activities that the user map by selecting dropdown
            var trastFundsActivities = (from i in assignedActivities
                                        where i.MappedProjectId == default(int)
                                          && i.MappedTrustFundId > 0
                                        select i);

            foreach (var activity in trastFundsActivities.DistinctBy(d => d.MappedTrustFundId))
            {

                CFnTFModel.TrustFundDetails.Add(aimsDAL.GetTrustFundDetails(activity.MappedTrustFundId));
            }

            foreach (var TrustFund in CFnTFModel.TrustFundDetails)
            {
                var acts = assignedActivities.FindAll(f => f.MappedTrustFundId == TrustFund.TrustFundId);
                TrustFund.iatiactivities.AddRange(acts);
            }
            #endregion
            Sessions.CFnTFModel = CFnTFModel;
            return CFnTFModel;
        }
        [AcceptVerbs("GET", "POST")]
        public object SavePreferences(CFnTFModel CFnTFModel)
        {
            if (CFnTFModel == null) return null;

            aimsDbIatiDAL.MapActivities(CFnTFModel.AssignedActivities);

            #region Save preferences

            var fieldMappings = new List<FieldMappingPreferenceDelegated>();

            foreach (var project in CFnTFModel.AimsProjects)
            {
                foreach (var activity in project.MatchedProjects)
                {
                    fieldMappings.Add(new FieldMappingPreferenceDelegated
                    {
                        IatiIdentifier = activity.IatiIdentifier,
                        FieldName = IatiFields.Commitment,
                        IsInclude = activity.IsCommitmentIncluded
                    });
                    fieldMappings.Add(new FieldMappingPreferenceDelegated
                    {
                        IatiIdentifier = activity.IatiIdentifier,
                        FieldName = IatiFields.Disbursment,
                        IsInclude = activity.IsDisbursmentIncluded
                    });
                    fieldMappings.Add(new FieldMappingPreferenceDelegated
                    {
                        IatiIdentifier = activity.IatiIdentifier,
                        FieldName = IatiFields.PlannedDisbursment,
                        IsInclude = activity.IsPlannedDisbursmentIncluded
                    });

                }
            }

            foreach (var trustFund in CFnTFModel.TrustFundDetails)
            {
                foreach (var activity in trustFund.iatiactivities)
                {
                    fieldMappings.Add(new FieldMappingPreferenceDelegated
                    {
                        IatiIdentifier = activity.IatiIdentifier,
                        FieldName = IatiFields.Commitment,
                        IsInclude = activity.IsCommitmentIncluded
                    });
                }

            }


            aimsDbIatiDAL.SaveFieldMappingPreferenceDelegated(fieldMappings); 
            #endregion

            #region Import
            aimsDAL.UpdateCofinanceProjects(CFnTFModel.AimsProjects, Sessions.UserId);
            aimsDAL.UpdateTrustFunds(CFnTFModel.TrustFundDetails, Sessions.UserId);
            #endregion Import
            return true;
        }


    }
}
