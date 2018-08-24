using act.core.data;

namespace act.core.web.Services
{
    public class DataPortValidation
    {
        public DataPortValidation(bool isValid, Port[] ports = null)
        {
            IsValid = isValid;
            Ports = ports ?? new Port[0];
        }

        public bool IsValid { get; }
        public Port[] Ports { get; }
    }
}