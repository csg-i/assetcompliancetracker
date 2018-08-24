using System;
using System.Net;

namespace act.core.web.Models.WebHook
{
    public class NotifyComplianceFailureResult
    {
        public HttpStatusCode StatusCode { get; }
        public Exception Exception { get; }

        private NotifyComplianceFailureResult(HttpStatusCode statusCode, Exception exception = null)
        {
            StatusCode = statusCode;
            Exception = exception;
        }

        public static NotifyComplianceFailureResult Success()
        {
            return new NotifyComplianceFailureResult(HttpStatusCode.Created);
        }
        public static NotifyComplianceFailureResult Failure(Exception ex)
        {
            return new NotifyComplianceFailureResult(HttpStatusCode.BadRequest, ex);
        }
    }
}