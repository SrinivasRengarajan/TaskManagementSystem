using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskManagementSystem_Microservice.ExceptionHandling
{
    /// <summary>
    /// Class for showing various custom http error messages generated in API calls
    /// </summary>
    public class ApiResponse
    {
        public int StatusCode { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; }

        public ApiResponse(int statusCode, string message = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultErrorMessage(statusCode);
        }

        private static string GetDefaultErrorMessage(int statusCode)
        {
            switch (statusCode)
            {
            case 204:
                return "Invalid Input Provided";
            case 403:
                return "Invalid Request";
            case 404:
                return "Resource not Found";
            case 500:
                return "Something Wrong!!";
            default:
                return null;
            }
        }
    }
}
