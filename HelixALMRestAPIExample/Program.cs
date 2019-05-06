/*
 Copyright 2019 Perforce Software Inc.

 Permission is hereby granted, free of charge, to any person obtaining a copy of this 
 software and associated documentation files (the "Software"), to deal in the Software 
 without restriction, including without limitation the rights to use, copy, modify, 
 merge, publish, distribute, sublicense, and/or sell copies of the Software, and to 
 permit persons to whom the Software is furnished to do so, subject to the following 
 conditions:

 The above copyright notice and this permission notice shall be included in all copies 
 or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
 PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
 FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
 OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

/// <summary>
/// This provides some simple examples for using the Helix ALM REST API.
/// These examples use the .NET DataContractSerializer as a mechanism
/// to serialize the JSON data in and out of C# classes for ease of use. This is 
/// not a requirement to use the REST API and there are many alternatives to
/// the DataContractSerializer.
/// </summary>
namespace HelixALMRestAPIExample
{
    /// <summary>
    /// Contains information about a Helix ALM project
    /// </summary>
    public class Project
    {
        public string id { get; set; }
        public string name { get; set; }
        public string uuid { get; set; }
    }

    /// <summary>
    /// Contains a list of projects from the Helix ALM Server
    /// </summary>
    public class ProjectList
    {
        public Project[] projects { get; set; }
        public int projectsLoading { get; set; }
    }

    /// <summary>
    /// Contains the access token required for authorization in requests
    /// </summary>
    public class AccessToken
    {
        /// <summary>
        /// Token type. Value is always "Bearer".
        /// </summary>
        public string tokenType { get; set; }

        /// <summary>
        /// UTC date/time the token expires
        /// </summary>
        public string expiresOn { get; set; }

        /// <summary>
        /// Access token
        /// </summary>
        public string accessToken { get; set; }
    }

    /// <summary>
    ///  Contains paging information
    /// </summary>
    public class PagingLink
    {
        public string @ref { get; set; } // ref is reserved word in C#
        public string href { get; set; }
        public string method { get; set; }
    }

    /// <summary>
    /// Contains information for paged item list results
    /// </summary>
    public class PagingWithLinks
    {
        public int page { get; set; }
        public int totalPages { get; set; }
        public int pageLimit { get; set; }
        public int totalCount { get; set; }
        public PagingLink[] links { get; set; }
    }

    /// <summary>
    /// Contains information about an image embedded in a formatted text field in an item
    /// </summary>
    public class InlineImage
    {
        public string content { get; set; }
        public string encodedFileID { get; set; }
        public string source { get; set; }
    }

    /// <summary>
    /// Represents a field value as text. If the field is a formatted text field, it may 
    /// contain HTML or plain text. If formatted, the field includes HTML with a link to inline images.
    /// </summary>
    [Serializable, DataContract]
    public class TextField
    {
        [DataMember(EmitDefaultValue = false)]
        public string text { get; set; }
        [DataMember(EmitDefaultValue = true, IsRequired = true)]
        public bool isFormatted { get; set; } = false;
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public InlineImage[] inlineImages { get; set; }
    }

    /// <summary>
    /// Contains a value from a dropdown field in an item
    /// </summary>
    [Serializable, DataContract]
    public class MenuItem
    {
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public Int64 id { get; set; } = 0;
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public string label { get; set; } = "";
    }

    /// <summary>
    /// Contains a value from a dropdown field in an item. This
    /// class should be used in cases where the default value
    /// for the id should not be used.
    /// </summary>
    [Serializable, DataContract]
    public class NoDefaultIDMenuItem
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Int64 id { get; set; } = 0;
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public string label { get; set; } = "";
    }

    /// <summary>
    /// Contains a user's name, separated in parts
    /// </summary>
    [Serializable, DataContract]
    public class User
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string lastName { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string firstName { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string mi { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Int64 id { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string username { get; set; }
    }

    /// <summary>
    /// Contains field values for a field. When setting a value, the field ID or label 
    /// is required to identify the field. Since the value passed are based on the "type"
    /// we need to implement the DataContract so that all members that are optional
    /// are exported as null so the server does not reject a custom field if its
    /// value was never filled in. An alternative approach could be to implement 
    /// custom getters and setters.
    /// </summary>
    [Serializable, DataContract]
    public class Field
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Int64 id { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string label { get; set; } = null;
        [DataMember(EmitDefaultValue = false, IsRequired = true)]
        public string type { get; set; }
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public string @string { get; set; } = null;
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public TextField formattedString { get; set; } = null;
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public MenuItem menuItem { get; set; } = null;
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public MenuItem[] menuItemArray { get; set; } = null;
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public string editableVersion { get; set; } = null;
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public bool? boolean { get; set; } = null;
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public Int64? integer { get; set; } = null;
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public decimal? @decimal { get; set; } = null;
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public string date { get; set; } = null;
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public string dateTime { get; set; } = null;
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public User user { get; set; } = null;
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public User[] userArray { get; set; } = null;
    }

    /// <summary>
    /// Contains information about a file attachment
    /// </summary>
    [Serializable, DataContract]
    public class Attachment
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string content { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string encodedFileID { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string self { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Int64 id { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string filename { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string created { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string modified { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Int64 size { get; set; }
    }

    /// <summary>
    /// Contains the list of files attached to an item
    /// </summary>
    [Serializable, DataContract]
    public class AttachmentContainer
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string self { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Attachment[] attachmentsData { get; set; }
    }

    /// <summary>
    /// Contains information about an issue Found by record
    /// </summary>
    [Serializable, DataContract]
    public class FoundByRecord
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Int64 id { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public User foundBy { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string dateFound { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string versionFound { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public TextField description { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public AttachmentContainer attachments { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public MenuItem reproduced { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public TextField steps { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public MenuItem testConfig { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public TextField otherConfig { get; set; }
    }

    /// <summary>
    /// Contains a list of Found by records from issues
    /// </summary>
    [Serializable, DataContract]
    public class FoundByContainer
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string self { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public FoundByRecord[] foundByRecordsData { get; set; }
    }

    /// <summary>
    /// Contains information about a workflow event
    /// </summary>
    [Serializable, DataContract]
    public class Event
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string self { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Int64 id { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string name { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public AttachmentContainer attachments { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Field[] fields { get; set; }
    }

    /// <summary>
    /// Contains a list of workflow events for an item
    /// </summary>
    [Serializable, DataContract]
    public class EventContainer
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string self { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Event[] eventsData { get; set; }
    }

    /// <summary>
    /// Contains basic information about a link definition
    /// </summary>
    [Serializable, DataContract]
    public class LinkDefinitionStub
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Int64 id { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string name { get; set; }
    }

    /// <summary>
    /// Contains a linked item that references an existing Helix ALM item
    /// </summary>
    [Serializable, DataContract]
    public class LinkedItem
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Int64 itemID { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string itemType { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool isSuspect { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string link { get; set; }
    }

    /// <summary>
    /// Contains information about a parent/child link
    /// </summary>
    [Serializable, DataContract]
    public class ParentChildLinks
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public LinkedItem parent { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public LinkedItem[] children { get; set; }
    }

    /// <summary>
    /// Contains information about a link
    /// </summary>
    [Serializable, DataContract]
    public class Link
    {
        [DataMember(EmitDefaultValue = false)]
        public Int64 id { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string comment { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public LinkDefinitionStub linkDefinition { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string type { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public LinkedItem[] peers { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public ParentChildLinks parentChildren { get; set; }
    }

    /// <summary>
    /// Contains information about a link
    /// </summary>
    [Serializable, DataContract]
    public class LinksContainer
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string self { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Link[] linksData { get; set; }
    }

    /// <summary>
    /// Contains base information that all items contain
    /// </summary>
    [Serializable, DataContract]
    public class BaseItem
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Int64 id { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public int number { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string tag { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string self { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string ttsudioURL { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string httpURL { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Field[] fields { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public AttachmentContainer attachments { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public EventContainer events { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public LinksContainer links { get; set; }
    }

    /// <summary>
    /// Contains information about an issue
    /// </summary>
    [Serializable, DataContract]
    public class Issue : BaseItem
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public FoundByContainer foundByRecords { get; set; }
    }

    /// <summary>
    /// Contains base information about an test run used to generate test runs
    /// </summary>
    [Serializable, DataContract]
    public class TestRun : BaseItem
    {

    }

    /// <summary>
    /// Contains information about a folder
    /// </summary>
    [Serializable, DataContract]
    public class Folder
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string path { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Int64 id { get; set; }
    }

    /// <summary>
    /// Contains information about a test run set
    /// </summary>
    [Serializable, DataContract]
    public class TestRunSet
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Int64 id { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string label { get; set; }
    }

    /// <summary>
    /// Contains information about a test variant
    /// </summary>
    [Serializable, DataContract]
    public class Variant
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string label { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public NoDefaultIDMenuItem[] menuItemArray { get; set; }
    }

    /// <summary>
    /// Contains the parameters needed to generate test runs
    /// </summary>
    [Serializable, DataContract]
    public class GenerateTestRunParams
    {
        [DataMember(EmitDefaultValue = false, IsRequired = true)]
        public Int64[] testCaseIDs { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Variant[] variants { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Folder folder { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public TestRunSet testRunSet { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Event[] eventsData { get; set; }
    }

    /// <summary>
    /// Contains an array of issues
    /// </summary>
    [Serializable, DataContract]
    public class IssuesList
    {
        [DataMember(EmitDefaultValue = false)]
        public Issue[] issues { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public PagingWithLinks paging { get; set; }
    }

    /// <summary>
    /// Contains information about errors that occurred when processing a request
    /// </summary>
    [Serializable, DataContract]
    class ErrorResponse
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string message { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public int statusCode { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string code { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string errorElementPath { get; set; }
    }

    /// <summary>
    /// Contains information about errors that occurred when processing a
    /// put or post and the request was partially successful. The server 
    /// encountered issues saving one or more items.
    /// </summary>
    [Serializable, DataContract]
    class UpdateResponse
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public ErrorResponse[] errors { get; set; }
    }

    /// <summary>
    /// Response object when doing a put or post on the issues endpoint
    /// </summary>
    [Serializable, DataContract]
    class UpdateIssuesResponse : UpdateResponse
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public IssuesList issues { get; set; }
    }

    /// <summary>
    /// Response object when doing a put or post on the events endpoint
    /// </summary>
    [Serializable, DataContract]
    class UpdateEventsResponse : UpdateResponse
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Event[] eventsData { get; set; }
    }

    /// <summary>
    /// Response object when doing a put or post on the issues endpoint
    /// </summary>
    [Serializable, DataContract]
    class UpdateTestRunsResponse : UpdateResponse
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public TestRun[] testRuns { get; set; }
    }

    /// <summary>
    /// This program implements a command line application that demonstrates some 
    /// simple examples of using the Helix ALM REST API. These examples are using
    /// the Traditional Template sample project.
    /// </summary>
    class Program
    {
        // The Helix ALM REST API URL. This example uses a HTTPS protocol.
        // If used in a production environment, we strongly recommend using a valid 
        // certificate from a certificate authority.
        static string HelixALMRESTAPIUrl = "https://localhost:8443/helix-alm/api/v0/";

        // Static HttpClient for the program
        //static HttpClient client = new HttpClient();
        static HttpClient client = null;

        // Contains the Helix ALM login username 
        static string userName = "administrator";

        // Contains the Helix ALM login password
        static string password = "";

        // Contains the project name to use
        static string projectName = "Traditional Template";

        /// <summary>
        /// Initializes the HttpClient with the URL for the HelixALM REST API.
        /// </summary>
        static void InitClient()
        {
            // Sets up a custom HttpClientHandler that allows self signed certificates
            AppContext.SetSwitch("System.Net.Http.useSocketsHttpHandler", false);
            HttpClientHandler handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, certChain, policyErrors) =>
            {
                // If no errors, or specifically we are communicating with the configured server and the error is a certificate chain error, allow the connection
                if (policyErrors == System.Net.Security.SslPolicyErrors.None ||
                    (httpRequestMessage.RequestUri.AbsoluteUri.StartsWith(HelixALMRESTAPIUrl) && policyErrors == System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors))
                {
                    return true;
                }
                // Otherwise, reject the connection
                return false;
            };

            // Create the HttpClient passing in the handler
            client = new HttpClient(handler);

            // Update the port number in the following line
            client.BaseAddress = new Uri(HelixALMRESTAPIUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Write all messages to the console
            System.Diagnostics.TraceListener listener = new System.Diagnostics.ConsoleTraceListener();
            System.Diagnostics.Debug.Listeners.Add(listener);
        }

        /// <summary>
        /// Adds the basic authorization header to the request
        /// </summary>
        /// <param name="request">
        /// Contains the reference to the request to add the authorization header to
        /// </param>
        static void SetRequestAuthorization(ref HttpRequestMessage request)
        {
            string authorization = "basic " + Base64EncodeString(userName + ":" + password);
            request.Headers.Add("Authorization", authorization);
        }

        /// <summary>
        /// All requests your application sends to the Helix ALM REST API must include a header that 
        /// contains an access token, except for when getting a list of projects or an access token. Access 
        /// tokens are generated for a project based on a specified Helix ALM username and password and
        /// then used for subsequent requests. To generate a token, use the GET /{projectID}/token resource.
        /// The token must be included in the request header preceded by Bearer.
        /// </summary>
        /// <param name="request">
        /// Contains the reference to the request to add the authorization header to
        /// </param>
        /// <param name="accessToken">
        /// Contains the access token to use for the header
        /// </param>
        static void SetRequestAccessToken(ref HttpRequestMessage request, AccessToken accessToken)
        {
            if (accessToken != null)
            {
                string authorization = "Bearer " + accessToken.accessToken;
                request.Headers.Add("Authorization", authorization);
            }
        }

        /// <summary>
        /// Helper function to Base64 encode the inString
        /// </summary>
        /// <param name="inString">
        /// String to Base64 encode
        /// </param>
        /// <returns>
        /// Returns the Base64-encoded string
        /// </returns>
        static string Base64EncodeString(string inString)
        {
            byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(inString);
            return System.Convert.ToBase64String(data);
        }

        /// <summary>
        /// Helper function to decode the Base64-encoded inString
        /// </summary>
        /// <param name="inString">
        /// Contains the string to decode
        /// </param>
        /// <returns>
        /// Returns the decoded string
        /// </returns>
        static string Base64DecodeString(string inString)
        {
            byte[] data = System.Convert.FromBase64String(inString);
            return System.Text.ASCIIEncoding.ASCII.GetString(data);
        }

        /// <summary>
        /// Helper function to convert a JSON object to StringContent that can be 
        /// added to the request
        /// </summary>
        /// <typeparam name="T">
        /// Type of JSON object to convert
        /// </typeparam>
        /// <param name="inJsonObject">
        /// JSON object to convert
        /// </param>
        /// <returns>
        /// JSON object converted to StringContent
        /// </returns>
        static StringContent ConvertJsonObjectToStringContent<T>(T inJsonObject)
        {
            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(T));
            json.WriteObject(stream1, inJsonObject);
            byte[] jsonByteArry = stream1.ToArray();
            stream1.Close();
            string jsonString = Encoding.UTF8.GetString(jsonByteArry, 0, jsonByteArry.Length);
            StringContent stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            return stringContent;
        }

        /// <summary>
        /// Helper function to send the inRequest and convert the response string to a JSON
        /// object, which is returned
        /// </summary>
        /// <typeparam name="T">
        /// Type of object expected return from the response
        /// </typeparam>
        /// <param name="inRequest">
        /// HttpRequestMessage to send
        /// </param>
        /// <returns>
        /// Returns the JSON object from the response
        /// </returns>
        static async Task<T> SendRequest<T>(HttpRequestMessage inRequest)
        {
            T objectInResponse = default(T);

            try
            {
                // Sends the request
                HttpResponseMessage response = await client.SendAsync(inRequest);

                // Reads the response
                DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(T));
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    if (responseString.Length > 0)
                    {
                        MemoryStream responseStream = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(responseString));
                        responseStream.Position = 0;
                        objectInResponse = (T)json.ReadObject(responseStream);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(responseString);
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message;

                if (e.InnerException != null)
                    errorMessage += " " + e.InnerException.Message;
                        
                System.Diagnostics.Debug.WriteLine(errorMessage);
            }

            return objectInResponse;
        }

        /// <summary>
        /// Gets a list of all projects from the Helix ALM Server. Only includes projects the logged in user can access.
        /// </summary>
        /// <returns>
        /// Returns the project list JSON object
        /// </returns>
        static async Task<ProjectList> GetProjectList()
        {
            ProjectList responseBody = null;

            try
            {
                // Builds the request
                var request = new HttpRequestMessage(HttpMethod.Get, "projects");
                SetRequestAuthorization(ref request);
                responseBody = await SendRequest<ProjectList>(request);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            if (responseBody != null)
            {
                for (int index = 0; index < responseBody.projects.Length; index++)
                    System.Diagnostics.Debug.WriteLine(responseBody.projects[index].name);
            }
            return responseBody;
        }

        /// <summary>
        /// Shows an Authorization header using Basic authentication 
        /// with the username Administrator and blank password
        /// </summary>
        /// <returns>
        /// Returns the AccessToken JSON object
        /// </returns>
        static async Task<AccessToken> GetAuthorizationToken()
        {
            AccessToken responseBody = null;

            try
            {
                // Builds the request
                var url = projectName + "/token";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                SetRequestAuthorization(ref request);
                responseBody = await SendRequest<AccessToken>(request);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return responseBody;
        }

        /// <summary>
        /// Gets a list of issues from a project. To narrow the list of 
        /// returned issues, use a simple query string in the request URL.
        /// </summary>
        /// <param name="accessToken">
        /// accessToken to use for the request
        /// </param>
        /// <returns>
        /// IssueList JSON object
        /// </returns>
        static async Task<IssuesList> GetIssuesList(AccessToken accessToken)
        {
            IssuesList responseBody = null;

            try
            {
                // Builds the request
                var url = projectName + "/issues";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                SetRequestAccessToken(ref request, accessToken);
                responseBody = await SendRequest<IssuesList>(request);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            if (responseBody != null)
            {
                for (int index = 0; index < responseBody.issues.Length; index++)
                    System.Diagnostics.Debug.WriteLine(responseBody.issues[index].tag);
            }

            return responseBody;
        }

        /// <summary>
        /// Saves an issue
        /// </summary>
        /// <param name="accessToken">
        /// accessToken to use for the request
        /// </param>
        /// <param name="issue">
        /// Issue JSON object to save
        /// </param>
        /// <returns>
        /// UpdateIssuesResponse JSON object
        /// </returns>
        static async Task<UpdateIssuesResponse> SaveIssue(AccessToken accessToken, Issue issue)
        {
            UpdateIssuesResponse responseBody = null;

            try
            {
                // Builds the request
                var url = projectName + "/issues/" + issue.id;
                var request = new HttpRequestMessage(HttpMethod.Put, url);
                SetRequestAccessToken(ref request, accessToken);
                StringContent stringContent = ConvertJsonObjectToStringContent<Issue>(issue);
                request.Content = stringContent;

                // Sends the request
                responseBody = await SendRequest<UpdateIssuesResponse>(request);

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return responseBody;
        }

        /// <summary>
        /// Retrieves an issue, updates the Priority, and
        /// submits it back
        /// </summary>
        /// <param name="accessToken">
        /// accessToken to use for the request
        /// </param>
        static async Task UpdateIssuePriorityExample(AccessToken accessToken)
        {
            try
            {
                // Gets the issue
                int issueID = 1;
                var url = projectName + "/issues/" + issueID + "?fields=Priority";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                SetRequestAccessToken(ref request, accessToken);
                Issue issue = await SendRequest<Issue>(request);

                // Updates the Priority field and saves the issue
                if (issue != null)
                {
                    // Updates the Priority field
                    foreach (Field field in issue.fields)
                    {
                        if (field.label == "Priority")
                        {
                            field.menuItem.label = "Before Beta";
                            break;
                        }
                    }

                    // Saves the issue
                    UpdateIssuesResponse response = await SaveIssue(accessToken, issue);

                    System.Diagnostics.Debug.WriteLine("Issue Updated");
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Adds a new Found by record by getting an issue, adding
        /// the record, and submitting it back to the server.
        /// </summary>
        /// <param name="accessToken">
        /// The accessToken to use for the request
        /// </param>
        static async Task AddReportedByRecordExample(AccessToken accessToken)
        {
            try
            {
                // Gets the issue
                int issueID = 1; // IS-11
                var url = projectName + "/issues/" + issueID + "?expand=foundByRecords";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                SetRequestAccessToken(ref request, accessToken);
                Issue issue1 = await SendRequest<Issue>(request);

                if (issue1 != null)
                {
                    // Creates a new Found by record
                    FoundByRecord newRecord = new FoundByRecord();
                    newRecord.foundBy = new User();
                    newRecord.foundBy.username = "administrator";
                    newRecord.dateFound = DateTime.UtcNow.ToString("yyyy-MM-dd");
                    newRecord.description = new TextField();
                    newRecord.description.text = "This is a new record added by AddReportedByRecordExample";
                    newRecord.versionFound = "1.0";

                    // Adds the record to the Found by records array
                    FoundByRecord[] foundbyRecords = issue1.foundByRecords.foundByRecordsData;
                    Array.Resize(ref foundbyRecords, foundbyRecords.Length + 1);
                    foundbyRecords[foundbyRecords.Length - 1] = newRecord;
                    issue1.foundByRecords.foundByRecordsData = foundbyRecords;
                    UpdateIssuesResponse response = await SaveIssue(accessToken, issue1);

                    System.Diagnostics.Debug.WriteLine("Added found by record");
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Adds a new workflow event to an issue
        /// </summary>
        /// <param name="accessToken">
        /// accessToken to use for the request
        /// </param>
        static async Task AddWorkflowEventExample(AccessToken accessToken)
        {
            try
            {
                EventContainer eventContainer = new EventContainer();

                // Creates the event
                Event comment = new Event();
                comment.name = "Comment";
                comment.fields = new Field[1];

                // Adds the Notes fields
                Field notes = new Field();
                notes.label = "Notes";
                notes.type = "string";
                notes.@string = "Comment added by REST API";
                comment.fields[0] = notes;

                // Adds the event to the container
                eventContainer.eventsData = new Event[1];
                eventContainer.eventsData[0] = comment;

                // Posts the event
                int issueID = 1;
                var url = projectName + "/issues/" + issueID + "/events";
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                SetRequestAccessToken(ref request, accessToken);

                StringContent stringContent = ConvertJsonObjectToStringContent<EventContainer>(eventContainer);
                request.Content = stringContent;
                UpdateEventsResponse responseBody = await SendRequest<UpdateEventsResponse>(request);

                System.Diagnostics.Debug.WriteLine("Added workflow event");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Generates a new test run and enters a Pass event
        /// to pass the test run
        /// </summary>
        /// <param name="accessToken">
        /// accessToken to use for the request
        /// </param>
        static async Task GenerateAndPassTestRun(AccessToken accessToken)
        {
            try
            {
                // Generates the test run and passes it
                Int64 testCaseID = 1;
                GenerateTestRunParams generateParams = new GenerateTestRunParams();

                // Sets the test case
                generateParams.testCaseIDs = new Int64[1];
                generateParams.testCaseIDs[0] = testCaseID;

                // Sets the test run set
                generateParams.testRunSet = new TestRunSet();
                generateParams.testRunSet.label = "Alpha 1 Tests";

                // Sets the test variants
                generateParams.variants = new Variant[3];
                generateParams.variants[0] = new Variant();
                generateParams.variants[0].label = "Operating System";
                generateParams.variants[0].menuItemArray = new NoDefaultIDMenuItem[1];
                generateParams.variants[0].menuItemArray[0] = new NoDefaultIDMenuItem();
                generateParams.variants[0].menuItemArray[0].label = "Windows";

                generateParams.variants[1] = new Variant();
                generateParams.variants[1].label = "Database";
                generateParams.variants[1].menuItemArray = new NoDefaultIDMenuItem[1];
                generateParams.variants[1].menuItemArray[0] = new NoDefaultIDMenuItem();
                generateParams.variants[1].menuItemArray[0].label = "Native";

                generateParams.variants[2] = new Variant();
                generateParams.variants[2].label = "Client Type";
                generateParams.variants[2].menuItemArray = new NoDefaultIDMenuItem[1];
                generateParams.variants[2].menuItemArray[0] = new NoDefaultIDMenuItem();
                generateParams.variants[2].menuItemArray[0].label = "Web";

                // Creates the Pass event
                Event passEvent = new Event();
                passEvent.name = "Pass";
                passEvent.fields = new Field[1];

                // Adds the Notes fields
                Field notes = new Field();
                notes.label = "Notes";
                notes.type = "string";
                notes.@string = "Passed by REST API";
                passEvent.fields[0] = notes;

                // Adds the Pass event
                generateParams.eventsData = new Event[1];
                generateParams.eventsData[0] = passEvent;

                // Sends the generate request
                var url = projectName + "/testruns/generate";
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                SetRequestAccessToken(ref request, accessToken);

                StringContent stringContent = ConvertJsonObjectToStringContent<GenerateTestRunParams>(generateParams);
                request.Content = stringContent;
                UpdateTestRunsResponse responseBody = await SendRequest<UpdateTestRunsResponse>(request);

                // Gets the id and tag of generated test run
                if (responseBody != null && responseBody.testRuns.Length == 1)
                {
                    string tag = responseBody.testRuns[0].tag;
                    Int64 id = responseBody.testRuns[0].id;
                    System.Diagnostics.Debug.WriteLine("Generated Test Run " + tag);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Failed to Generate Test Run");
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Runs the program and blocks until it completes. Most HttpClient methods are async
        /// because they perform network I/O. All of the async tasks are done inside RunAsync.
        /// Normally, an app does note block the main thread, but this app does not allow any interaction.
        /// </summary>
        static async Task RunAsync()
        {
            // S etup the client
            InitClient();

            // Gets the list of projects
            ProjectList projectList;
            projectList = await GetProjectList();

            // Gets an authorization token
            AccessToken accessToken;
            accessToken = await GetAuthorizationToken();

            // Gets a list of issues
            IssuesList issuesList = await GetIssuesList(accessToken);

            // Gets issue 1 and changes a value
            await UpdateIssuePriorityExample(accessToken);

            // Adds a new Reported by record
            await AddReportedByRecordExample(accessToken);

            // Adds a workflow event
            await AddWorkflowEventExample(accessToken);

            // Generates and passes a test run
            await GenerateAndPassTestRun(accessToken);
        }

        /// <summary>
        /// Program entry point
        /// </summary>
        static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
        }
    }
}
