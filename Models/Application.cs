using System.Diagnostics.CodeAnalysis;

namespace ApiAppPay.Models
{
    public class Application
    {
        public long? Id { get; set; }
        public string? GivenName { get; set; }
        public string? FatherName { get; set; }
        public string? GrandfatherName { get; set; }
        public string? MotherName { get; set; }
        public string? MotherFatherName { get; set; }
        public string? UseCase { get; set; }
        public string? LicenseNumber { get; set; }
        public string? LicenseNumberLatin { get; set; }
        public string? Governorate { get; set; }
        public string? Usage { get; set; }
        public string? Passengers { get; set; }
        public string? VehicleCategory { get; set; }
        public string? Cylinders { get; set; }
        public string? Axis { get; set; }
        public string? CabinType { get; set; }
        public string? LoadWeight { get; set; }
        public DateTime? DateOfIssue { get; set; }
        public DateTime? DateOfExpiry { get; set; }
        public string? Tpid { get; set; }
        public int? ApplicationType { get; set; }
        public string? VichleId { get; set; }
        public int? IdCurrentState { get; set; }
        public string? DlCategory { get; set; }

        public Application() { }
      public Application(long id, string? givenName, string? fatherName, string? grandfatherName, string? motherName, string? motherFatherName, string? useCase, string? licenseNumber, string? licenseNumberLatin, string? governorate, string? usage, string? passengers, string? vehicleCategory, string? cylinders, string? axis, string? cabinType, string? loadWeight, string? dlCategory, DateTime? dateOfIssue, DateTime? dateOfExpiry, string tpid, int applicationType, string vichleId, int? idCurrentState)
        {
            Id = id;
            GivenName = givenName;
            FatherName = fatherName;
            GrandfatherName = grandfatherName;
            MotherName = motherName;
            MotherFatherName = motherFatherName;
            UseCase = useCase;
            LicenseNumber = licenseNumber;
            LicenseNumberLatin = licenseNumberLatin;
            Governorate = governorate;
            Usage = usage;
            Passengers = passengers;
            VehicleCategory = vehicleCategory;
            Cylinders = cylinders;
            Axis = axis;
            CabinType = cabinType;
            LoadWeight = loadWeight;
            DlCategory = dlCategory;
            DateOfIssue = dateOfIssue;
            DateOfExpiry = dateOfExpiry;
            Tpid = tpid;
            ApplicationType = applicationType;
            VichleId = vichleId;
            IdCurrentState = idCurrentState;
        }
    }
}
