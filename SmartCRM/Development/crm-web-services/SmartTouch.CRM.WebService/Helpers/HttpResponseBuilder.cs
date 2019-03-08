using SmartTouch.CRM.ApplicationServices.Messaging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace SmartTouch.CRM.WebService.Helpers
{
    /// <summary>
    /// for Http Response Builder
    /// </summary>
    public static class HttpResponseBuilder
    {
        /// <summary>
        /// for Build Response
        /// </summary>
        /// <param name="requestMessage">requestMessage</param>
        /// <param name="baseResponse">baseResponse</param>
        /// <returns></returns>
        public static HttpResponseMessage BuildResponse(this HttpRequestMessage requestMessage, ServiceResponseBase baseResponse)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;
            if (baseResponse.Exception != null)
            {
                statusCode = baseResponse.Exception.ConvertToHttpStatusCode();
                HttpResponseMessage message = new HttpResponseMessage(statusCode);
                message.Content = new StringContent(baseResponse.Exception.Message);
               // throw new HttpResponseException(message);
            }
            return requestMessage.CreateResponse<ServiceResponseBase>(statusCode, baseResponse);
        }

        /// <summary>
        /// for Build Form Submit Response
        /// </summary>
        /// <param name="requestMessage">requestMessage</param>
        /// <param name="baseResponse">baseResponse</param>
        /// <param name="redirectUrl">redirectUrl</param>
        /// <returns></returns>
        public static HttpResponseMessage BuildFormSubmitResponse(this HttpRequestMessage requestMessage, ServiceResponseBase baseResponse, 
            string redirectUrl)
        {
            HttpStatusCode statusCode = HttpStatusCode.Moved;
            if (baseResponse.Exception != null)
            {
                statusCode = baseResponse.Exception.ConvertToHttpStatusCode();
                HttpResponseMessage message = new HttpResponseMessage(statusCode);
                message.Content = new StringContent(baseResponse.Exception.Message);
                // throw new HttpResponseException(message);
                return requestMessage.CreateResponse<ServiceResponseBase>(statusCode, baseResponse);
            }
            else
            {
                var response = requestMessage.CreateResponse<ServiceResponseBase>(statusCode, baseResponse);
                response.Headers.Location = new Uri(redirectUrl);
                return response;
            }
        }

        /// <summary>
        /// for Http Response Message
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="requestMessage">requestMessage</param>
        /// <param name="values">values</param>
        /// <returns></returns>
        public static HttpResponseMessage BuildResponseTemp<T>(this HttpRequestMessage requestMessage, IEnumerable<T> values)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;
            
            return requestMessage.CreateResponse<IEnumerable<T>>(statusCode, values);
        }
    }
}