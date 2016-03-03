﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AIMS_BD_IATI.Library.Parser.ParserIATIv2
{

    public class iatiactivityContainer
    {
        public string DP { get; set; }
        public iatiactivityContainer()
        {
            DP = "";
            iatiActivities = new List<iatiactivity>();
            NewProjects = new List<iatiactivity>();
        }
        public List<iatiactivity> iatiActivities { get; set; }
        public List<iatiactivity> RelevantActivities { get { return iatiActivities.n().FindAll(f => f.IsRelevant == true); } }
        public List<iatiactivity> NewProjects { get; set; }

        public bool HasRelatedActivity { get { return iatiActivities.Exists(e => e.relatedactivity.n().Count(r => r != null && r.type == "2") > 0); } }


    }

    public partial class iatiactivity
    {

        public iatiactivity()
        {
            relatedIatiActivities = new List<iatiactivity>();
            MatchedProjects = new List<iatiactivity>();
        }


        [XmlIgnore]
        public List<iatiactivity> relatedIatiActivities { get; set; }
        [XmlIgnore]
        public List<iatiactivity> MatchedProjects { get; set; }
        //[XmlIgnore]
        //public string SelectedHierarchy { get; set; }

        [XmlIgnore]
        public decimal PercentToBD
        {
            get
            {
                var rc = recipientcountry.n().FirstOrDefault(f => f.n().code == "BD");
                return rc == null ? 0 : rc.percentage;
            }
        }

        [XmlIgnore]
        private bool? isRelevant;
        [XmlIgnore]
        public bool? IsRelevant
        {
            //ToDo add AidType criteria
            get
            {
                return isRelevant ?? PercentToBD >= 20 && activitystatus.code == "2";
            }
            set
            {
                isRelevant = value;
            }
        }

        [XmlIgnore]
        private int? ownedBy;
        [XmlIgnore]
        public int? AimsFundSourceId
        {
            get
            {
                return ownedBy == null || ownedBy == 0 ? participatingorg.n().FirstOrDefault(f => f.n().role == "4").n().AimsFundSourceId : ownedBy;
            }
            set
            {
                ownedBy = value;
            }
        }
        #region Wrappers

        [XmlIgnore]
        public string IatiIdentifier
        {
            get
            {
                return iatiidentifier.n().Value;
            }

            set
            {
                iatiidentifier.n().Value = value;
            }
        }
        [XmlIgnore]
        public string Title
        {
            get
            {
                return title.n().narrative.n(0).Value;
            }
            set
            {
                title.n().narrative = Statix.getNarativeArray(value);
            }
        }
        [XmlIgnore]
        public string Description
        {
            get
            {
                return description.n(0).narrative.n(0).Value;
            }
            set
            {
                description.n(0).narrative = Statix.getNarativeArray(value);
            }
        }
        [XmlIgnore]
        public string ReportingOrg
        {
            get
            {
                return reportingorg.n().narrative.n(0).Value;
            }
            set
            {
                reportingorg.n().narrative = Statix.getNarativeArray(value);
            }
        }

        //[XmlIgnore]
        public string AidType
        {
            get
            {
                string code;
                if (defaultaidtype == null) // for initializing
                    code = AidTypeCode;

                return defaultaidtype.n().name;
                //return "Undefined";
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    defaultaidtype = new defaultaidtype { name = value };
            }
        }
        [XmlIgnore]
        public string AidTypeCode
        {
            get
            {
                if (defaultaidtype != null)
                {
                    return defaultaidtype.code;
                }
                else
                {
                    defaultaidtype = new defaultaidtype();
                    if (transaction != null)
                    {
                        var commitmentTrans = transaction.Where(c => c.transactiontype.n().code == "2");

                        var kk = from t in commitmentTrans
                                 group t by new { t.aidtype, t.value } into g
                                 select new
                                 {
                                     g.Key.aidtype,
                                     Sum = g.Sum(s => s.value.n().Value)
                                 };

                        var dominatingAidType = kk.OrderByDescending(k => k.Sum).FirstOrDefault();

                        defaultaidtype.code = dominatingAidType == null ? "" : dominatingAidType.aidtype == null ? "" : dominatingAidType.aidtype.code;

                    }
                    else
                    {
                        var allChildAticitiesTrans = new List<transaction>();
                        foreach (var ra in relatedIatiActivities)
                        {
                            if (ra.transaction != null)
                            {
                                foreach (var item in ra.transaction)
                                {
                                    if (item.aidtype == null)
                                        item.aidtype = new transactionAidtype { code = ra.defaultaidtype.n().code };
                                    allChildAticitiesTrans.Add(item);

                                }
                            }

                        }

                        var commitmentTrans = allChildAticitiesTrans.FindAll(c => c.transactiontype.n().code == "2");

                        var kk = (from t in commitmentTrans
                                  group t by new { t.aidtype, t.value } into g
                                  select new
                                  {
                                      g.Key.aidtype,
                                      Sum = g.Sum(s => s.value.n().Value)
                                  }).ToList();

                        var dominatingAidType = kk.OrderByDescending(k => k.Sum).FirstOrDefault();

                        defaultaidtype.code = dominatingAidType == null ? "" : dominatingAidType.aidtype == null ? "" : dominatingAidType.aidtype.code;


                        //defaultaidtype.code = "A01";

                    }

                    return defaultaidtype.code;
                }
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    defaultaidtype = new Parser.ParserIATIv2.defaultaidtype { code = value };
            }
        }

        [XmlIgnore]
        public string ActivityStatus
        {
            get
            {
                return activitystatus.n().name;
            }
            set
            {
                activitystatus.n().name = value;
            }

        }

        #region activitydate
        [XmlIgnore]
        public DateTime PlannedStartDate
        {
            get
            {
                var sdate = activitydate.n().FirstOrDefault(f => f.n().type == "1");
                return sdate == null ? default(DateTime) : sdate.isodate;
            }
        } //1
        [XmlIgnore]
        public DateTime ActualStartDate
        {
            get
            {
                var sdate = activitydate.n().FirstOrDefault(f => f.n().type == "2");
                return sdate == null ? default(DateTime) : sdate.isodate;
            }
        } //2
        [XmlIgnore]
        public DateTime PlannedEndDate
        {
            get
            {
                var sdate = activitydate.n().FirstOrDefault(f => f.n().type == "3");
                return sdate == null ? default(DateTime) : sdate.isodate;
            }
        } //3
        [XmlIgnore]
        public DateTime ActualEndDate
        {
            get
            {
                var sdate = activitydate.n().FirstOrDefault(f => f.n().type == "4");
                return sdate == null ? default(DateTime) : sdate.isodate;
            }
        } //4 
        #endregion
        #endregion

    }

    public partial class defaultaidtype
    {
        [XmlIgnore]
        public string name
        {
            get
            {
                if (code == "A01") return "General budget support";
                else if (code == "A02") return "Sector budget support";
                else if (code == "B01") return "Core support to NGOs, other private bodies, PPPs and research institutes";
                else if (code == "B02") return "Core contributions to multilateral institutions";
                else if (code == "B03") return "Contributions to specific-purpose programmes and funds managed by international organisations (multilateral, INGO)";
                else if (code == "B04") return "Basket funds/pooled funding";
                else if (code == "C01") return "Project-type interventions";
                else if (code == "D01") return "Donor country personnel";
                else if (code == "D02") return "Other technical assistance";
                else if (code == "E01") return "Scholarships/training in donor country";
                else if (code == "E02") return "Imputed student costs";
                else if (code == "F01") return "Debt relief";
                else if (code == "G01") return "Administrative costs not included elsewhere";
                else if (code == "H01") return "Development awareness";
                else if (code == "H02") return "Refugees in donor countries";
                else
                    return "";
            }
            set
            {
                if (value == "General budget support") code = "A01";
                else if (value == "Sector budget support") code = "A02";
                else if (value == "Core support to NGOs, other private bodies, PPPs and research institutes") code = "B01";
                else if (value == "Core contributions to multilateral institutions") code = "B02";
                else if (value == "Contributions to specific-purpose programmes and funds managed by international organisations (multilateral, INGO)") code = "B03";
                else if (value == "Basket funds/pooled funding") code = "B04";
                else if (value == "Project-type interventions") code = "C01";
                else if (value == "Donor country personnel") code = "D01";
                else if (value == "Other technical assistance") code = "D02";
                else if (value == "Scholarships/training in donor country") code = "E01";
                else if (value == "Imputed student costs") code = "E02";
                else if (value == "Debt relief") code = "F01";
                else if (value == "Administrative costs not included elsewhere") code = "G01";
                else if (value == "Development awareness") code = "H01";
                else if (value == "Refugees in donor countries") code = "H02";
                else
                    code = "";
            }

        }
    }
    public partial class activitystatus
    {
        [XmlIgnore]
        public string name
        {
            get
            {
                if (code == "1") return "Pipeline/identification";
                else if (code == "2") return "Implementation";
                else if (code == "3") return "Completion";
                else if (code == "4") return "Post-completion";
                else if (code == "5") return "Cancelled";
                else if (code == "6") return "Suspended";
                else
                    return "";
            }
            set
            {
                if (value == "Pipeline/identification") code = "1";
                else if (value == "Implementation") code = "2";
                else if (value == "Completion") code = "3";
                else if (value == "Post-completion") code = "4";
                else if (value == "Cancelled") code = "5";
                else if (value == "Suspended") code = "6";
                else
                    code = "";
            }


        }



    }

    public partial class participatingorg
    {
        [XmlIgnore]
        public int AimsFundSourceId
        {
            get;
            set;
        }



    }





}


