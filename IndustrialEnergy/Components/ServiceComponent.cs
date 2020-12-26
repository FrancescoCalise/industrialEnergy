using IndustrialEnergy.UtilityClass.Spinner;
using IndustrialEnergy.UtilityClass.Toast;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace IndustrialEnergy.Components
{
    public interface IServiceComponent
    {
        Task<IRestResponse> InvokeMiddlewareAsync(string controller, string action, object requestBody, Dictionary<string, string> requesteHeader, Method method, ToastModalityShow toastShow);
    }

    public class ServiceComponent : IServiceComponent
    {
        private readonly SpinnerService _spinnerService;
        private readonly ToastService _toastService;
        private readonly NavigationManager _navigationManager;
        private string uri = string.Empty;

        public ServiceComponent(SpinnerService spinnerService, ToastService toastService, NavigationManager navigationManager)
        {
            _spinnerService = spinnerService;
            _toastService = toastService;
            _navigationManager = navigationManager;
            uri = _navigationManager.BaseUri + "api";
        }

        public async Task<IRestResponse> InvokeMiddlewareAsync(string controller, string action, object requestBody, Dictionary<string, string> requesteHeader, Method method, ToastModalityShow toastShow)
        {
            _spinnerService.ShowSpinner();
            string resource = uri + controller + action;
            RestClient client = new RestClient();

            var request = new RestRequest(resource, method, DataFormat.Json);

            request.JsonSerializer = new CamelCaseSerializer();
           

            if (requesteHeader != null)
            {
                foreach (var item in requesteHeader)
                {
                    request.AddHeader(item.Key, item.Value);
                }
            }

            if (requestBody != null)
            {
                request.AddJsonBody(requestBody);
            }

            IRestResponse response = await client.ExecuteAsync(request);

            ResponseContent responseContent = JsonConvert.DeserializeObject<ResponseContent>(response.Content);

            _spinnerService.HideSpinner();

            ToastLevel levelError = GetToastLevel(response.StatusCode);

            if (CanShowToast(levelError, toastShow))
                _toastService.ShowToast(response.StatusCode.ToString(), responseContent.Message, levelError);

            return response;
        }

        public class CamelCaseSerializer : ISerializer
        {
            public string ContentType { get; set; }

            public CamelCaseSerializer()
            {
                ContentType = "application/json";
            }

            public string Serialize(object obj)
            {
                var camelCaseSetting = new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                string json = JsonConvert.SerializeObject(obj, camelCaseSetting);
                return json;

            }
        }

        private ToastLevel GetToastLevel(HttpStatusCode statusCode)
        {
            ToastLevel level = ToastLevel.Error;

            switch (statusCode)
            {
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                case HttpStatusCode.Accepted:
                case HttpStatusCode.Continue:
                    level = ToastLevel.Success;
                    break;
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.NotFound:
                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.BadGateway:
                case HttpStatusCode.Conflict:
                    level = ToastLevel.Error;
                    break;
                //case HttpStatusCode.:
                //    level = ToastLevel.Info;
                //    break;
                //case HttpStatusCode.OK:
                //    level = ToastLevel.Warning;
                //    break;
                default:
                    level = ToastLevel.Info;
                    break;
            }
            return level;
        }

        private bool CanShowToast(ToastLevel level, ToastModalityShow modalityShow)
        {
            bool canShow = false;
  
            if (modalityShow == ToastModalityShow.All)
            {
                canShow = true;
            }
            else if (modalityShow == ToastModalityShow.OnlyError && level == ToastLevel.Error)
            {
                canShow = true;
            }
            else if (modalityShow == ToastModalityShow.OnlySuccess && level == ToastLevel.Success)
            {
                canShow = true;
            }

            return canShow;
        }
    }

    public class ResponseContent
    {
        public string Message { get; set; }
        public Dictionary<string, string> Content { get; set; }
    }

}