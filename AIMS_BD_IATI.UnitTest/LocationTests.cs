﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AIMS_BD_IATI.DAL;
using AIMS_BD_IATI.Library.Parser.ParserIATIv2;
using System.Linq;

namespace AIMS_BD_IATI.UnitTest
{
    [TestClass]
    public class LocationTests
    {
        [TestMethod]
        public void GetNearestLocationTest()
        {
            var dbContext = new AIMS_DBEntities();
            var divisions = (from d in dbContext.tblDivisions
                             where d.GPSLatitude != null && d.GPSLongitude != null
                             select new GeoLocation
                             {
                                 DivisionId = d.Id,
                                 Name = d.DivisionName,
                                 Latitude = (double)d.GPSLatitude,
                                 Longitude = (double)d.GPSLongitude
                             }).ToList();

            var districts = (from d in dbContext.tblDistricts
                             where d.GPSLatitude != null && d.GPSLongitude != null
                             select new GeoLocation
                             {
                                 DistrictId = d.Id,
                                 Name = d.DistrictName,
                                 Latitude = (double)d.GPSLatitude,
                                 Longitude = (double)d.GPSLongitude
                             }).ToList();

            location location = new location
            {
                point = new locationPoint { pos = "23.72 90.38" }
            };

            var nearestGeoLocation = AimsDAL.GetNearestGeoLocation(districts, location);

            Assert.AreEqual("Dhaka", nearestGeoLocation.Name);
        }
    }
}