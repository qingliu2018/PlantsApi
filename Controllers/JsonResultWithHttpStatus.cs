using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PlantsApi.Controllers
{
    public class JsonResultWithHttpStatus : JsonResult
    {
        private readonly HttpStatusCode _httpStatus;

        public JsonResultWithHttpStatus(object value) : this(value, HttpStatusCode.OK)
        {
        }

        public JsonResultWithHttpStatus(object value, HttpStatusCode httpStatus) : base(value)
        {
            _httpStatus = httpStatus;
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)_httpStatus;
            return base.ExecuteResultAsync(context);
        }
    }
}