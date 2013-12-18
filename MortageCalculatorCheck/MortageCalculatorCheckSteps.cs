using System;
using System.Net;
using AlteryxGalleryAPIWrapper;
using HtmlAgilityPack;
using Newtonsoft.Json;
using NUnit.Framework;
using TechTalk.SpecFlow;
using System.Collections.Generic;
namespace MortageCalculatorCheck
{

    [Binding]
    public class MortageCalculatorCheckSteps
    {

        public string alteryxurl;
        public string _sessionid;
        private string _appid;
        private string _userid;
        private string _appName;
        private string jobid;
        private string outputid;
        private string validationId;

        public delegate void DisposeObject();
        //private Client Obj = new Client("https://devgallery.alteryx.com/api/");


          private Client Obj =new Client("https://gallery.alteryx.com/api/");

        private RootObject jsString = new RootObject();


        [Given(@"alteryx running at""(.*)""")]
        public void GivenAlteryxRunningAt(string url)
        {
            alteryxurl = url;
        }

        /// <summary>
        /// Authenticate User and Retreive Session ID
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        [Given(@"I am logged in using ""(.*)"" and ""(.*)""")]
        public void GivenIAmLoggedInUsingAnd(string user, string password)
        {

            _sessionid = Obj.Authenticate(user, password).sessionId;

        }

        /// <summary>
        /// //Publish the app & get the ID of the app
        /// </summary>
        /// <param name="appName"></param>
        [Given(@"I publish the application ""(.*)""")]
        public void GivenIPublishTheApplication(string appName)
        {
            string apppath = @"..\..\docs\Mortgage_Calculator.yxzp";
            Action<long> progress = new Action<long>(Console.Write);
            var pubResult = Obj.SendAppAndGetId(apppath, progress);
            _appid = pubResult.id;
            validationId = pubResult.validation.validationId;
            ScenarioContext.Current.Set(Obj, System.Guid.NewGuid().ToString());

        }

        /// <summary>
        /// 
        // validate a published app can be run 
        // two step process. First, GetValidationStatus which indicates when validation disposition is available. 
        /// </summary>
        /// <param name="status"></param>
        [Given(@"I check if the application is ""(.*)""")]
        public void GivenICheckIfTheApplicationIs(string status)
        {
            int count = 0;
            String validStatus = "";

            var validationStatus = Obj.GetValidationStatus(validationId);
            validStatus = validationStatus.status;

        CheckValidate:
            System.Threading.Thread.Sleep(100);
            if (validStatus == "Completed" && count < 5)
            {
                string disposition = validationStatus.disposition;
            }
            else if (count < 5)
            {
                count++;
                var reCheck = Obj.GetValidationStatus(validationId);
                validStatus = reCheck.status;
                goto CheckValidate;
            }

            else
            {

                throw new Exception("Complete Status Not found");

            }


            var finalValidation = Obj.GetValidation(_appid, validationId); // url/api/apps/{APPID}/validation/{VALIDATIONID}/
            var finaldispostion = finalValidation.validation.disposition;
            StringAssert.IsMatch(status, finaldispostion.ToString());
        }


        [When(@"I run mortgage calculator with principle (.*) interest (.*) payments (.*)")]
        public void WhenIRunMortgageCalculatorWithPrincipleInterestPayments(int principle, Decimal interest, int numpayments)
        {
            jsString.appPackage.id = _appid;
            jsString.userId = _userid;
            jsString.appName = _appName;
            string appinterface = Obj.GetAppInterface(_appid);
            dynamic interfaceresp = JsonConvert.DeserializeObject(appinterface);

            //Construct the payload to be posted.
            string header = String.Empty;
            string payatbegin = String.Empty;
            List<Jsonpayload.Question> questionAnsls = new List<Jsonpayload.Question>();
            questionAnsls.Add(new Jsonpayload.Question("IntRate", interest.ToString()));
            questionAnsls.Add(new Jsonpayload.Question("NumPayments", numpayments.ToString()));
            questionAnsls.Add(new Jsonpayload.Question("Payment", "1832.14"));
            questionAnsls.Add(new Jsonpayload.Question("FutureValue", "0"));
            questionAnsls.Add(new Jsonpayload.Question("LoanAmount", principle.ToString()));
            var solve = new List<Jsonpayload.datac>();
            solve.Add(new Jsonpayload.datac() { key = "Payment", value = "true" });
            var payat = new List<Jsonpayload.datac>();
            payat.Add(new Jsonpayload.datac() { key = "0", value = "true" });
            string SolveFor = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(solve);
            string PayAtBegin = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(payat);


            for (int i = 0; i < 3; i++)
            {
                if (i == 0)
                {
                    Jsonpayload.Question questionAns = new Jsonpayload.Question();
                    questionAns.name = "SolveFor";
                    questionAns.answer = SolveFor;
                    jsString.questions.Add(questionAns);
                }
                else if (i == 1)
                {
                    Jsonpayload.Question questionAns = new Jsonpayload.Question();
                    questionAns.name = "PayAtBegin";
                    questionAns.answer = PayAtBegin;
                    jsString.questions.Add(questionAns);
                }
                else
                {
                    jsString.questions.AddRange(questionAnsls);
                }
            }
            jsString.jobName = "Job Name";

            // Make Call to run app
            var postData = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(jsString);
            string postdata = postData.ToString();
            string resjobqueue = Obj.QueueJob(postdata);
            var jobqueue = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(resjobqueue);
            jobid = jobqueue["id"];

            int count = 0;
            string status = "";

        CheckValidate:
            System.Threading.Thread.Sleep(100);
            if (status == "Completed" && count < 5)
            {
                //string disposition = validationStatus.disposition;
            }
            else if (count < 5)
            {
                string jobstatusresp = Obj.GetJobStatus(jobid);
                var statusResponse = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(jobstatusresp);
                status = statusResponse["status"];
                goto CheckValidate;
            }

            else
            {
                throw new Exception("Complete Status Not found");

            }

        }

        [Then(@"I see output (.*)")]
        public void ThenISeeOutput(decimal answer)
        {
            string getmetadata = Obj.GetOutputMetadata(jobid);
            dynamic metadataresp = JsonConvert.DeserializeObject(getmetadata);
            int count = metadataresp.Count;
            for (int j = 0; j <= count - 1; j++)
            {
                outputid = metadataresp[j]["id"];
            }
            string getjoboutput = Obj.GetJobOutput(jobid, outputid, "html");
            string htmlresponse = getjoboutput;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlresponse);
            decimal output = Convert.ToDecimal(doc.DocumentNode.SelectSingleNode("//span[@class='DefaultNumericText']").InnerHtml);
            decimal finaloutput = Math.Round(output, 2);
            Assert.AreEqual(answer, finaloutput);
            //Obj.DeleteApp(_appid);

        }

        [AfterScenario()]
        public void AfterScenario()
        {
            try
            {
                if (ScenarioContext.Current.Count > 0)
                {
                    foreach (var item in ScenarioContext.Current)
                    {
                        Obj.Dispose(_appid);
                    }
                }
                else
                {
                    throw new Exception("No Object found");
                }

            }
            catch (Exception e)
            {
                throw e;
            }

        }


    }


}

