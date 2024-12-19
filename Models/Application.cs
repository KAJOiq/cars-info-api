using System.Diagnostics.CodeAnalysis;

namespace ApiAppPay.Models
{
    public class Application
    {
        public string Application_ID { get; set; }
        public string GivenName { get; set; }
        public string FatherName { get; set; }
        public string GrandfatherName { get; set; }
        public string MotherName { get; set; }
        public string LicenseNumber { get; set; }
        public string LicenseNumberLatin { get; set; }
        public string Governorate { get; set; }
        public string Usage { get; set; }
        public string Passengers { get; set; }
        public string VehicleCategory { get; set; }
        public string Cylinders { get; set; }
        public string Axis { get; set; }
        public string CabinType { get; set; }
        public string LoadWeight { get; set; }
    }
}
