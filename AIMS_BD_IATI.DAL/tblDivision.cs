//------------------------------------------------------------------------------
// 
//------------------------------------------------------------------------------

namespace AIMS_BD_IATI.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblDivision
    {
        public tblDivision()
        {
            this.tblDistricts = new HashSet<tblDistrict>();
            this.tblProjectGeographicAllocations = new HashSet<tblProjectGeographicAllocation>();
            this.tblUpazilas = new HashSet<tblUpazila>();
        }
    
        public int Id { get; set; }
        public string DivisionName { get; set; }
        public string StandardCode { get; set; }
        public Nullable<decimal> GPSLatitude { get; set; }
        public Nullable<decimal> GPSLongitude { get; set; }
        public Nullable<int> SortOrder { get; set; }
        public string IUser { get; set; }
        public string EUser { get; set; }
        public System.DateTime IDate { get; set; }
        public Nullable<System.DateTime> EDate { get; set; }
    
        public virtual ICollection<tblDistrict> tblDistricts { get; set; }
        public virtual ICollection<tblProjectGeographicAllocation> tblProjectGeographicAllocations { get; set; }
        public virtual ICollection<tblUpazila> tblUpazilas { get; set; }
    }
}
