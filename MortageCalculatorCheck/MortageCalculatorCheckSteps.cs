
using System;
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

        private string alteryxurl;
        private string _sessionid;
        private string _appid;
        private string _userid;
        private string _appName;
        private string jobid;
        private string outputid;

      private Client Obj = new Client("https://devgallery.alteryx.com/api/");
      //  private Client Obj =new Client("https://gallery.alteryx.com/api/");

        private RootObject jsString = new RootObject();


        [Given(@"alteryx running at""(.*)""")]
        public void GivenAlteryxRunningAt(string url)
        {
            alteryxurl = url;
        }

     


        [Given(@"I am logged in using ""(.*)"" and ""(.*)""")]
        public void GivenIAmLoggedInUsingAnd(string user, string password)
        {
            // Authenticate User and Retreive Session ID
            _sessionid = Obj.Authenticate(user, password).sessionId;

        }

        [Given(@"I publish the application ""(.*)""")]
        public void GivenIPublishTheApplication(string p0)
        {
            //Publish the app & get the ID of the app
            string apppath = @"..\..\docs\Mortgage_Calculator.yxzp";
            Action<long> progress = new Action<long>(Console.Write);
            var pubResult = Obj.SendAppAndGetId(apppath, progress);
            _appid = pubResult.id;
            //string validid = pubResult.validation.validationId;
            //var validres = Obj.GetValidation(_appid, validid);

        }

        [Given(@"I run mortgage calculator with principle (.*) interest (.*) payments (.*)")]
        public void GivenIRunMortgageCalculatorWithPrincipleInterestPayments(int principle, Decimal interest,
            int numpayments)
        {
            //url + "/apps/studio/?search=" + appName + "&limit=20&offset=0"
            //Search for App & Get AppId & userId 

            string response = Obj.SearchApps("mortgage");
            var appresponse =
                new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                    response);
            int count = appresponse["recordCount"];
            //for (int i = 0; i <= count - 1; i++)
            //{
          //  _appid = appresponse["records"][0]["id"];
            _userid = appresponse["records"][0]["owner"]["id"];
            _appName = appresponse["records"][0]["primaryApplication"]["fileName"];
            //}       
            jsString.appPackage.id = _appid;
            jsString.userId = _userid;
            jsString.appName = _appName;

            //url +"/apps/" + appPackageId + "/interface/
            //Get the app interface - not required
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
            solve.Add(new Jsonpayload.datac() {key = "Payment", value = "true"});
            var payat = new List<Jsonpayload.datac>();
            payat.Add(new Jsonpayload.datac() {key = "0", value = "true"});
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

            var jobqueue =
                new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                    resjobqueue);
            jobid = jobqueue["id"];

            //Get the job status

            string status = "";
            while (status != "Completed")
            {
                string jobstatusresp = Obj.GetJobStatus(jobid);
                var statusresp =
                    new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                        jobstatusresp);
                status = statusresp["status"];
            }


        }

        [Then(@"I see output (.*)")]
        public void ThenISeeOutput(decimal answer)
        {
            //url + "/apps/jobs/" + jobId + "/output/"
            string getmetadata = Obj.GetOutputMetadata(jobid);
            dynamic metadataresp = JsonConvert.DeserializeObject(getmetadata);

            // outputid = metadataresp[0]["id"];
            int count = metadataresp.Count;
            for (int j = 0; j <= count - 1; j++)
            {
                outputid = metadataresp[j]["id"];
            }

            string getjoboutput = Obj.GetJobOutput(jobid, outputid, "html");
            string htmlresponse = getjoboutput;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlresponse);
            string output = doc.DocumentNode.SelectSingleNode("//span[@class='DefaultNumericText']").InnerHtml;
           // string output = doc.DocumentNode.SelectSingleNode("//*[@id='preview']/table/tbody/tr[1]/td/div").InnerHtml;
            decimal output1= Convert.ToDecimal(output);
            decimal finaloutput= Math.Round(output1, 2);

            #region
            //  string output = doc.DocumentNode.SelectSingleNode("[@id='preview']").InnerHtml;
            //  var output = doc.DocumentNode.SelectSingleNode("//html/body/div[1]/div/div[9]/div[3]/div[2]/div[1]/table/tbody/tr[1]/td/div").InnerHtml;
            //   var output = doc.DocumentNode.SelectSingleNode("html/body/div[1]/div/div[9]/div[3]/div[2]/div[1]/table/tbody/tr[1]/td/div").InnerHtml;
            //string outputFromHtml = "";
            //foreach (var node in doc.DocumentNode.ChildNodes)
            //{
            //    outputFromHtml += node.InnerText;
            //    if (outputFromHtml == "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Final//EN\">")
            //    {
            //        outputFromHtml = "";
            //    }

            //}
            ////string output1 = Regex.Replace(output, @"\t|\n|\r|,", "");



            //string[] splitOutput;

            //splitOutput = outputFromHtml.Split(default(Char[]), StringSplitOptions.RemoveEmptyEntries);

            //string finalOutput = string.Empty;

            //for (int i = 0; i < splitOutput.Length - 1; i++)
            //{
            //    string finalOutputReg = Regex.Replace(splitOutput[i], @"\t|\n|\r|,", "");
            //    if (finalOutputReg == answer.ToString())
            //    {
            //       finalOutput=finalOutputReg;

            //    }
            //    else
            //    {
            //        finalOutput = splitOutput[1];

            //    }
            //}
            ////StringAssert.Contains(answer.ToString(), output3);
            #endregion
            Assert.AreEqual(answer, finaloutput);


        }
    }
}
