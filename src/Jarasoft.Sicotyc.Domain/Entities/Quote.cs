using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class Quote : Entity
    {
        public Quote(
            Guid quoteId,
            Guid requestedByUserId,
            Guid clientCompanyId,
            Guid companyTypeId,
            Guid serviceTypeId,
            DateTime serviceDate,
            int unitsQuantity,
            decimal estimatedWeightTons,
            string? additionalNotes,
            string status,
            Tracking? tracking = null)
            : base(quoteId)
        {
            QuoteTransportOffers = new List<QuoteTransportOffer>();
            RequestedByUserId = requestedByUserId;
            ClientCompanyId = clientCompanyId;
            CompanyTypeId = companyTypeId;
            ServiceTypeId = serviceTypeId;
            ServiceDate = serviceDate;
            UnitsQuantity = unitsQuantity;
            EstimatedWeightTons = estimatedWeightTons;
            AdditionalNotes = additionalNotes;
            Status = status;
            Tracking = tracking;
        }

        public Guid RequestedByUserId { get; private set; }
        public Guid ClientCompanyId { get; private set; }
        public Guid CompanyTypeId { get; private set; }
        public Guid ServiceTypeId { get; private set; }
        public DateTime ServiceDate { get; private set; }
        public int UnitsQuantity { get; private set; }
        public decimal EstimatedWeightTons { get; private set; }
        public string? AdditionalNotes { get; private set; }
        public string Status { get; private set; }
        public Tracking? Tracking { get; private set; }

        public ApplicationUser? RequestedByUser { get; private set; }
        public Company? ClientCompany { get; private set; }
        public CompanyType? CompanyType { get; private set; }
        public ServiceType? ServiceType { get; private set; }
        public ICollection<QuoteTransportOffer> QuoteTransportOffers { get; private set; } = new List<QuoteTransportOffer>();

        public void UpdateRequest(
            DateTime serviceDate,
            int unitsQuantity,
            decimal estimatedWeightTons,
            string? additionalNotes,
            Guid? updatedBy = null)
        {
            ServiceDate = serviceDate;
            UnitsQuantity = unitsQuantity;
            EstimatedWeightTons = estimatedWeightTons;
            AdditionalNotes = additionalNotes;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetStatus(string status, Guid? updatedBy = null)
        {
            Status = status;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetServiceType(Guid serviceTypeId, Guid? updatedBy = null)
        {
            ServiceTypeId = serviceTypeId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetClient(Guid clientCompanyId, Guid companyTypeId, Guid? updatedBy = null)
        {
            ClientCompanyId = clientCompanyId;
            CompanyTypeId = companyTypeId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetRelations(Company? clientCompany, CompanyType? companyType, ServiceType? serviceType, Guid? updatedBy = null)
        {
            ClientCompany = clientCompany;
            ClientCompanyId = clientCompany?.Id ?? ClientCompanyId;
            CompanyType = companyType;
            CompanyTypeId = companyType?.Id ?? CompanyTypeId;
            ServiceType = serviceType;
            ServiceTypeId = serviceType?.Id ?? ServiceTypeId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetRequestedByUser(ApplicationUser? requestedByUser, Guid? updatedBy = null)
        {
            RequestedByUser = requestedByUser;
            RequestedByUserId = requestedByUser?.Id ?? RequestedByUserId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void Delete(Guid? deletedBy = null)
        {
            Tracking = Helper.SoftDeleted(Tracking, deletedBy);
        }
    }
}