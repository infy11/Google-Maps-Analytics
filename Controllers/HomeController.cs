using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dot_authentication.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;

namespace dot_authentication.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult Upload ()
        {
            
            return View();
        }
[HttpPost]
[DisableRequestSizeLimit]
[Authorize]
        public async Task<IActionResult> UploadFiles(IFormFile files)
        {
            if (files == null || files.Length == 0)
                return Content("file not selected");

            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot", 
                        files.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await files.CopyToAsync(stream);
            }

            return RedirectToAction("Files","home",new { whatever = path});
        }
        [Authorize]
        public async Task<IActionResult> Files(string whatever)
        {
            String json;
            ViewBag.message = whatever;
           using (StreamReader r = new StreamReader(whatever))
          {
             json =await  r.ReadToEndAsync();
            
           }
            JObject parsed = JObject.Parse(json); 
            //string rssTitle = (string)parsed["locations"][0]["timestampMs"];
            getActivitiesDistance(parsed);
            ViewBag.timestamp="something";


            return View();
        }

        //dangerous section start 
    void  getActivitiesDistance(JObject parsed){
    var distance = 0.0;
    var walked = 0.0;
    var ran = 0.0;
    var drove = 0.0;
    var flew = 0.0;
    var biked = 0.0;
    var exited_vehicle = 0.0;
    DateTimeOffset first_date;
    


    var prevLat = 0.0;
    var prevLong = 0.0;
    



    for (int  i =((JArray)parsed["locations"]).Count-1; i > 0; i--) {
      if(i ==((JArray) parsed["locations"]).Count - 1){
        prevLong = ((double)parsed["locations"][i]["longitudeE7"] )* Math.Pow(10, -7);
        prevLat = ((double)parsed["locations"][i]["latitudeE7"] )* Math.Pow(10, -7);
        
     //changed    var first_date = new Date(parseFloat(parsed.locations[i].timestampMs));  
        first_date =  DateTimeOffset.FromUnixTimeMilliseconds((long)parsed["locations"][i]["timestampMs"]);



      }
      else{
        var longitude =((double)parsed["locations"][i]["longitudeE7"] )* Math.Pow(10, -7);
        var latitude = ((double)parsed["locations"][i]["latitudeE7"] )* Math.Pow(10, -7);
        


        var current_distance = Math.Abs(getDistance(prevLat, latitude, prevLong, longitude));


        //if contains activities section
        if(parsed["locations"][i]["activity"] != null) {
          var transport = (string)parsed["locations"][i]["activity"][0]["activity"][0]["type"];

          switch(transport){
            case "ON_FOOT":
              for (var c = 0; c < ((JArray)parsed["locations"][i]["activity"][0]["activity"]).Count; c++) {
                if((string)parsed["locations"][i]["activity"][0]["activity"][c]["type"] == "RUNNING" && (int)parsed["locations"][i]["activity"][0]["activity"][c]["confidence"] > 15){
                   ran += current_distance;
                   break;
                }
                if((string)parsed["locations"][i]["activity"][0]["activity"][c]["type"] == "WALKING" && (int)parsed["locations"][i]["activity"][0]["activity"][c]["confidence"] > 20){
                  walked += current_distance;
                  break;
                }
              }
              break;
            case "UNKNOWN":
              for (var c = 0; c < ((JArray)parsed["locations"][i]["activity"][0]["activity"]).Count;  c++) {
                if((string)parsed["locations"][i]["activity"][0]["activity"][c]["type"] == "RUNNING" && (int)parsed["locations"][i]["activity"][0]["activity"][c]["confidence"] > 15){
                   ran += current_distance;
                   break;
                }
                if((string)parsed["locations"][i]["activity"][0]["activity"][c]["type"] == "WALKING" && (int)parsed["locations"][i]["activity"][0]["activity"][c]["confidence"] > 20){
                  walked += current_distance;
                  break;
                }
              }
              break;
            case "STILL":
             for (var c = 0; c < ((JArray)parsed["locations"][i]["activity"][0]["activity"]).Count;  c++) {
                if((string)parsed["locations"][i]["activity"][0]["activity"][c]["type"] == "RUNNING" && (int)parsed["locations"][i]["activity"][0]["activity"][c]["confidence"] > 15){
                   ran += current_distance;
                   break;
                }
                if((string)parsed["locations"][i]["activity"][0]["activity"][c]["type"] == "WALKING" && (int)parsed["locations"][i]["activity"][0]["activity"][c]["confidence"] > 20){
                  walked += current_distance;
                  break;
                }
              }
              break;
            case "WALKING":
                 for (var c = 0; c < ((JArray)parsed["locations"][i]["activity"][0]["activity"]).Count;  c++) {
                if((string)parsed["locations"][i]["activity"][0]["activity"][c]["type"] == "RUNNING" && (int)parsed["locations"][i]["activity"][0]["activity"][c]["confidence"] > 15){
                   ran += current_distance;
                   break;
                }
                if((string)parsed["locations"][i]["activity"][0]["activity"][c]["type"] == "WALKING" && (int)parsed["locations"][i]["activity"][0]["activity"][c]["confidence"] > 20){
                  walked += current_distance;
                  break;
                }
              }
              break;
             
            case "RUNNING":
              ran += current_distance;
              break;
            case "IN_VEHICLE":
              for (var c = 0; c <((JArray)parsed["locations"][i]["activity"][0]["activity"]).Count; c++) {
                if((string)parsed["locations"][i]["activity"][0]["activity"][c]["type"] == "ON_BICYCLE" && (int)parsed["locations"][i]["activity"][0]["activity"][c]["confidence"]  > 40){
                  biked += current_distance;
                  break;
                } 
              }
              drove += current_distance;
              break;
            case "ON_BICYCLE":
              biked += current_distance;
              break;
            case "EXITING_VEHICLE":
              exited_vehicle++;
              break;
          }
        }
        if(current_distance>0){
           prevLong = longitude;
           prevLat = latitude;
          
        }

      }
    }

    distance = walked+ran+drove+flew+biked;
    ViewBag.totalDistance=String.Format("{0:n}", (distance*0.621371));
    DateTimeOffset dt2=DateTimeOffset.Now;
    var days=(dt2-first_date).TotalDays;
    int average_miles=(int)((int)distance*0.621371)/(int)(days);

    ViewBag.average_miles=average_miles;
    ViewBag.walked=String.Format("{0:n}", (walked*0.621371));
    ViewBag.first_date=first_date;
    ViewBag.ran=String.Format("{0:n}", (ran*0.621371));
    ViewBag.drove=String.Format("{0:n}", (drove*0.621371));
    ViewBag.biked=String.Format("{0:n}", (biked*0.621371));
    ViewBag.exited_vehicle=String.Format("{0:n}", (exited_vehicle));
    Console.WriteLine(first_date);
    Console.WriteLine(dt2);
    Console.WriteLine(days);
  
    



}
//String.Format("{0:n}", 1234);

//returns distance in km from latitude and longitude
 double getDistance(double lat1,double lat2,double long1,double long2){
//radius of the earth (meters)
  var r = 6371;
  var latitude1 = Math.PI * lat1 / 180.0;
  var latitude2 = Math.PI * lat2 / 180.0;

  var latDistance = Math.PI * (lat1 - lat2) / 180.0;
  var lonDistance = Math.PI * (long2-long1) / 180.0;

  var a = Math.Sin(latDistance/2)*Math.Sin(latDistance/2) + Math.Cos(latitude1)*Math.Cos(latitude2) * Math.Sin(lonDistance/2) * Math.Sin(lonDistance/2);
  var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));

  // //if height is available we will get height as well (pythagoras' theorum)
  var x = Math.Pow(r*c, 2);
  var totalDistance = Math.Sqrt(x);
  return totalDistance;
}




        //dangerous section end 


        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
