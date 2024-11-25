using System.Diagnostics.CodeAnalysis;

namespace ApiAppPay.Models
{
    public class Application
    {
        

        public string Application_ID { get; set; }
        public DateTime Created { get; set; }
        public string UseCase { get; set; }
        public string Brand { get; set; }
        public string LicenseNumber { get; set; }
        public string LicenseNumberLatin { get; set; }
        public string AppChassisNumber { get; set; }
        public string VehicleType { get; set; }
    }
}
