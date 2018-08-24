namespace act.core.web.Models.Health
{
    public class ErrorPage
    {
        public string Code { get; }
        public string Description { get; }
        public string LongDescription { get; }
        public string RequestId { get; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public ErrorPage(string code, string requestId)
        {
            RequestId = requestId;
            Code = code;
            switch (code)
            {
                case "400":
                    Description = "Bad Request";
                    LongDescription = "Your request was not understood.  Please go back and try again.";
                    break;

                case "401":
                    Description = "Unauthorized";
                    LongDescription = "You are not authorized to view this page.";
                    break;

                case "403":
                    Description = "Forbidden";
                    LongDescription = "You are not authorized to view this page.";
                    break;

                case "404":
                    Description = "Page Not Found";
                    LongDescription = "The page you are attempting to reach could not be found.";
                    break;
                default:
                    Description = "An Error Occurred";
                    LongDescription = "Please try again.";
                    break;
            }
        }
    }
}