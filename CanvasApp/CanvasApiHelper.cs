using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanvasApp
{
    public class CanvasApiHelper
    {
        /// <summary>
        /// Returns a new http header with access token
        /// </summary>
        /// <returns></returns>
        public Http SetupHttpWithAuth()
        {
            RestSharp.Http http = new RestSharp.Http();
            var header = new HttpHeader();
            header.Name = "Authorization";
            string token = System.Configuration.ConfigurationManager.AppSettings["canvasToken"];
            header.Value = "Bearer " + token;

            http.Headers.Add(header);
            return http;
        }

        /// <summary>
        /// Recursively gets and deserializes content as long as there is a next link in the header response
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="http"></param>
        /// <param name="listOfObjects"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetContent<T>(Http http, List<T> listOfObjects = null)
         where T : class
        {
            if (listOfObjects == null) listOfObjects = new List<T>();

            HttpResponse response = null;

            try
            {
                response = await Task.Run(() => http.Get());
            }
            catch (Exception)
            {
                Console.WriteLine("Could not retrieve content from web service");
                throw;
            }

            try
            {
                IEnumerable<T> result = await Task.Run(() => JsonConvert.DeserializeObject<IEnumerable<T>>(response.Content));
                listOfObjects.AddRange(result);
            }
            catch (Exception)
            {
                Console.WriteLine("Could not deserialize '" + response.Content + "'");
                throw;
            }

            HttpHeader responseHeader = response.Headers.Where(header => header.Name.ToLower() == "link").First();

            HttpHeader nextHeader = null;
            ActionLink nextAction;

            try
            {
                nextAction = NextRequest(responseHeader, ref nextHeader);
            }
            catch (Exception)
            {
                Console.WriteLine("Could not handle paging request");
                throw;
            }

            if (nextAction != null)
            {
                http.Url = new Uri(nextAction.Link);

                if (!http.Headers.Any(h => h.Name == nextAction.ActionType && h.Value == nextAction.Action))
                    http.Headers.Add(nextHeader);

                await GetContent<T>(http, listOfObjects);
            }

            return listOfObjects;
        }

        private class ActionLink
        {
            public string Action { get; set; }
            public string ActionType { get; set; }
            public string Link { get; set; }

        }

        /// <summary>
        /// Creates an action link object and sets the next header
        /// </summary>
        /// <param name="responseHeader"></param>
        /// <param name="nextHeader"></param>
        /// <returns>Null if no next header was found</returns>
        private ActionLink NextRequest(HttpHeader responseHeader, ref HttpHeader nextHeader)
        {
            ActionLink nextAction;

            IEnumerable<ActionLink> linkActions = PagingRequest(responseHeader);

            nextAction = linkActions.Where(la => la.Action.Contains("next")).FirstOrDefault();
            if (nextAction != null)
                nextHeader = new HttpHeader() { Name = nextAction.ActionType, Value = nextAction.Action };
            return nextAction;
        }

        /// <summary>
        /// Splits the paging request link into series of action links
        /// </summary>
        /// <param name="responseHeader"></param>
        /// <returns></returns>
        private IEnumerable<ActionLink> PagingRequest(HttpHeader responseHeader)
        {
            // This is how the links string looks like:

            /* 
            |------------------              Link             ------------------| |-- ActionType / Action --|
            <https://<canvas>/api/v1/courses/:id/discussion_topics.json?opaqueA>; rel="current", // linkline
            <https://<canvas>/api/v1/courses/:id/discussion_topics.json?opaqueB>; rel="next",
            <https://<canvas>/api/v1/courses/:id/discussion_topics.json?opaqueC>; rel="first",
            <https://<canvas>/api/v1/courses/:id/discussion_topics.json?opaqueD>; rel="last"
             */

            string[] charsToRemove = new string[] { "<", ">" };
            foreach (var charToRemove in charsToRemove)
                responseHeader.Value = responseHeader.Value.Replace(charToRemove, string.Empty);

            string[] linkLines = responseHeader.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            IEnumerable<ActionLink> linkActions = linkLines
                .Select(line =>
                {
                    string[] linkAndAction = line.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    string[] ActionTypeAndAction = linkAndAction[1].Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    return new ActionLink
                    {
                        Link = linkAndAction[0], //https://<canvas>/api/v1/courses/:id/discussion_topics.json?opaqueB
                        ActionType = ActionTypeAndAction[0].Trim(), //rel
                        Action = ActionTypeAndAction[1] //next
                    };
                });

            return linkActions;
        }
    }
}
