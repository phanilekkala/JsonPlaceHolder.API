
using JsonPlaceHolder.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace JsonPlaceHolder.API.Controllers
{
    public class UsersController : ApiController
    {
        [HttpGet]
        [Route("users")]
        public HttpResponseMessage GetUsers()
        {
            string JsonStr = GetApiData(string.Empty, false);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

            return response;
        }
        [HttpGet]
        [Route("posts")]
        public HttpResponseMessage GetPost(string userId)
        {
            string JsonStr = GetApiData(userId, true);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonStr, Encoding.UTF8, "application/json");
            return response;
        }

        [HttpPost]
        [Route("person")]
        public string AddPerson(Person person)
        {
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            if (person == null)
                return "Failure";
            else if (FindPerson(person.FirstName, person.LastName, true) != null && string.IsNullOrEmpty(person.ID))
            {
                return "3";
            }
            else
            {
                string JsonStr = AddPersonToJson(person);
                response.Content = new StringContent(JsonStr, Encoding.UTF8, "application/json");
                return JsonStr;
            }


        }

        [HttpGet]
        [Route("person")]
        public HttpResponseMessage SearchPerson(string fname, string lname)
        {
            var JsonStr = JsonConvert.SerializeObject(FindPerson(fname, lname));
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonStr, Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        [Route("personlist")]
        public HttpResponseMessage GetAllPersons()
        {
            var JsonStr = JsonConvert.SerializeObject(PersonList());
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonStr, Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        [Route("deleteperson")]
        public HttpResponseMessage DeletePerson(string id)
        {
            var del = (DeletePersonFromJson(id) ? "Success" : "Fail");
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(del), Encoding.UTF8, "application/json");
            return response;
        }
        private string PersonList()
        {
            string response = null;

            var filePath = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/PersonsDetailsNew.json");
            //Check file exist or not
            if (File.Exists(filePath))
            {

                // Read existing json data
                TextReader tr = new StreamReader(filePath);
                var personJsonData = tr.ReadToEnd();
                tr.Close();
                // De-serialize to object or create new list
                var personList = JsonConvert.DeserializeObject<List<Person>>(personJsonData) ?? new List<Person>();

                // personlist as json string
                response = JsonConvert.SerializeObject(personList);

            }

            return response;
        }

        private bool DeletePersonFromJson(string id)
        {
            var found = false;

            var filePath = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/PersonsDetailsNew.json");
            //Create file if not exist
            if (!(File.Exists(filePath)))
            {
                var fs = File.Create(filePath);
                fs.Close();
            }
            // Read existing json data
            TextReader tr = new StreamReader(filePath);
            var personJsonData = tr.ReadToEnd();
            tr.Close();
            // De-serialize to object or create new list
            var personList = JsonConvert.DeserializeObject<List<Person>>(personJsonData) ?? new List<Person>();

            found = personList.Remove(personList.Where(p => p.ID.ToString() == id).FirstOrDefault());
            //this below logic for update person
            if (found)
            {
                // Update json data string
                personJsonData = JsonConvert.SerializeObject(personList);
                //File.WriteAllText(filePath, personJsonData);

                TextWriter tw = new StreamWriter(filePath);
                tw.WriteLine(personJsonData);
                tw.Close();
            }

            return found;
        }
        private string AddPersonToJson(Person person)
        {
            string response = null;

            var filePath = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/PersonsDetailsNew.json");
            //Create file if not exist
            if (!(File.Exists(filePath)))
            {
                var fs = File.Create(filePath);
                fs.Close();
            }
            // Read existing json data
            TextReader tr = new StreamReader(filePath);
            var personJsonData = tr.ReadToEnd();
            tr.Close();
            // De-serialize to object or create new list
            var personList = JsonConvert.DeserializeObject<List<Person>>(personJsonData) ?? new List<Person>();

            //this below logic for update person
            if (!string.IsNullOrEmpty(person.ID))
            {
                foreach (var p in personList)
                {
                    if (p.ID == person.ID)
                    {
                        p.FirstName = person.FirstName;
                        p.LastName = person.LastName;
                        response = "2";
                        break;
                    }
                }
            }
            else
            {
                // Add any new person
                person.ID = Guid.NewGuid().ToString();
                personList.Add(person);
                response = "1";
            }
            // Update json data string
            personJsonData = JsonConvert.SerializeObject(personList);
            //File.WriteAllText(filePath, personJsonData);

            TextWriter tw = new StreamWriter(filePath);
            tw.WriteLine(personJsonData);
            tw.Close();

            return response;
        }

        private Person FindPerson(string firstName, string lastName, bool isExist = false)
        {
            Person response = null;

            var filePath = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/PersonsDetailsNew.json");
            if ((File.Exists(filePath)))
            {
                var personJsonData = File.ReadAllText(filePath);
                // De-serialize to object or create new list
                var personList = JsonConvert.DeserializeObject<List<Person>>(personJsonData) ?? new List<Person>();
                if (isExist)
                {
                    response = personList.Where(p => p.FirstName == firstName && p.LastName == lastName).FirstOrDefault();
                }
                else
                {
                    response = personList.Where(p => p.FirstName == firstName || p.LastName == lastName).FirstOrDefault();
                }
            }
            return response;
        }
        private string GetApiData(string userId, bool isPost, bool isPhots = false)
        {
            var baseUrl = System.Configuration.ConfigurationManager.AppSettings["JsonPlaceHolderApiURL"];

            string url = string.Empty;
            if (isPost)
            {
                url = string.Format("{0}posts", baseUrl, userId);
            }
            else if (isPhots)
            {
                url = string.Format("{0}photos", baseUrl);
            }
            else
            {
                url = string.Format("{0}users", baseUrl);
            }
            string response = null;

            try
            {
                var client = new HttpClient();
                int apiTimeoutValue = 6000;
                client.Timeout = TimeSpan.FromSeconds(apiTimeoutValue);
                var httpResponse = client.GetAsync(url).ContinueWith(task => task.Result).Result;

                if (httpResponse.IsSuccessStatusCode && !string.IsNullOrEmpty(httpResponse.Content.ReadAsStringAsync().Result))
                {
                    var httpResponseResult = httpResponse.Content.ReadAsStringAsync().ContinueWith(task => task.Result).Result;
                    response = httpResponseResult;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return response;
        }
        [HttpGet]
        [Route("Photos")]
        public HttpResponseMessage GetPhotos()
        {
            string JsonStr = GetApiData(string.Empty, false, true);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonStr, Encoding.UTF8, "application/json");
            return response;
        }
    }



}
