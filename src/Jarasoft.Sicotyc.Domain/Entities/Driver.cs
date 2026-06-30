using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class Driver : Entity
    {
        public Driver(
            Guid driverId,
            Guid companyId,
            Guid companyTypeId,
            Guid licenseTypeId,
            string firstName,
            string lastName,
            string documentNumber,
            string licenseNumber,
            DateTime licenseExpirationDate,
            Phone phone,
            Email email,
            bool isActive,
            Tracking? tracking = null)
            : base(driverId)
        {
            CompanyId = companyId;
            CompanyTypeId = companyTypeId;
            LicenseTypeId = licenseTypeId;
            FirstName = firstName;
            LastName = lastName;
            DocumentNumber = documentNumber;
            LicenseNumber = licenseNumber;
            LicenseExpirationDate = licenseExpirationDate;
            Phone = phone;
            Email = email;
            IsActive = isActive;
            Tracking = tracking;
        }

        public Guid CompanyId { get; private set; }
        public Guid CompanyTypeId { get; private set; }
        public Guid LicenseTypeId { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string DocumentNumber { get; private set; }
        public string LicenseNumber { get; private set; }
        public DateTime LicenseExpirationDate { get; private set; }
        public Phone Phone { get; private set; }
        public Email Email { get; private set; }
        public bool IsActive { get; private set; }
        public Tracking? Tracking { get; private set; }

        public Company? Company { get; private set; }
        public CompanyType? CompanyType { get; private set; }
        public LicenseType? LicenseType { get; private set; }

        public void UpdateIdentity(string firstName, string lastName, string documentNumber, Guid? updatedBy = null)
        {
            FirstName = firstName;
            LastName = lastName;
            DocumentNumber = documentNumber;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void UpdateLicense(Guid licenseTypeId, string licenseNumber, DateTime licenseExpirationDate, Guid? updatedBy = null)
        {
            LicenseTypeId = licenseTypeId;
            LicenseNumber = licenseNumber;
            LicenseExpirationDate = licenseExpirationDate;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void UpdateContact(Phone phone, Email email, Guid? updatedBy = null)
        {
            Phone = phone;
            Email = email;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetCompany(Guid companyId, Guid companyTypeId, Guid? updatedBy = null)
        {
            CompanyId = companyId;
            CompanyTypeId = companyTypeId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetRelations(Company? company, CompanyType? companyType, LicenseType? licenseType, Guid? updatedBy = null)
        {
            Company = company;
            CompanyId = company?.Id ?? CompanyId;
            CompanyType = companyType;
            CompanyTypeId = companyType?.Id ?? CompanyTypeId;
            LicenseType = licenseType;
            LicenseTypeId = licenseType?.Id ?? LicenseTypeId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void Activate(Guid? updatedBy = null)
        {
            IsActive = true;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void Deactivate(Guid? updatedBy = null)
        {
            IsActive = false;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void Delete(Guid? deletedBy = null)
        {
            Tracking = Helper.SoftDeleted(Tracking, deletedBy);
        }
    }
}