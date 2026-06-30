using Jarasoft.Sicotyc.Domain.Common;
using Jarasoft.Sicotyc.Domain.ValueObjects;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class QuoteTransportOffer : Entity
    {
        public QuoteTransportOffer(
            Guid quoteTransportOfferId,
            Guid quoteId,
            Guid requestedByUserId,
            Guid clientCompanyId,
            Guid companyTypeId,
            Guid serviceTypeId,
            Guid transportCompanyId,
            decimal proposedFreight,
            int availableUnits,
            bool isSelected,
            string status,
            Tracking? tracking = null)
            : base(quoteTransportOfferId)
        {
            QuoteId = quoteId;
            RequestedByUserId = requestedByUserId;
            ClientCompanyId = clientCompanyId;
            CompanyTypeId = companyTypeId;
            ServiceTypeId = serviceTypeId;
            TransportCompanyId = transportCompanyId;
            ProposedFreight = proposedFreight;
            AvailableUnits = availableUnits;
            IsSelected = isSelected;
            Status = status;
            Tracking = tracking;
        }

        public Guid QuoteId { get; private set; }
        public Guid RequestedByUserId { get; private set; }
        public Guid ClientCompanyId { get; private set; }
        public Guid CompanyTypeId { get; private set; }
        public Guid ServiceTypeId { get; private set; }
        public Guid TransportCompanyId { get; private set; }
        public decimal ProposedFreight { get; private set; }
        public int AvailableUnits { get; private set; }
        public bool IsSelected { get; private set; }
        public string Status { get; private set; }
        public Tracking? Tracking { get; private set; }

        public Quote? Quote { get; private set; }
        public Company? ClientCompany { get; private set; }
        public CompanyType? CompanyType { get; private set; }
        public ServiceType? ServiceType { get; private set; }
        public Company? TransportCompany { get; private set; }

        public void UpdateOffer(decimal proposedFreight, int availableUnits, Guid? updatedBy = null)
        {
            ProposedFreight = proposedFreight;
            AvailableUnits = availableUnits;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetSelection(bool isSelected, Guid? updatedBy = null)
        {
            IsSelected = isSelected;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetStatus(string status, Guid? updatedBy = null)
        {
            Status = status;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetRelations(
            Quote? quote,
            Company? clientCompany,
            CompanyType? companyType,
            ServiceType? serviceType,
            Company? transportCompany,
            Guid? updatedBy = null)
        {
            Quote = quote;
            QuoteId = quote?.Id ?? QuoteId;
            ClientCompany = clientCompany;
            ClientCompanyId = clientCompany?.Id ?? ClientCompanyId;
            CompanyType = companyType;
            CompanyTypeId = companyType?.Id ?? CompanyTypeId;
            ServiceType = serviceType;
            ServiceTypeId = serviceType?.Id ?? ServiceTypeId;
            TransportCompany = transportCompany;
            TransportCompanyId = transportCompany?.Id ?? TransportCompanyId;
            Tracking = Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void Delete(Guid? deletedBy = null)
        {
            Tracking = Helper.SoftDeleted(Tracking, deletedBy);
        }
    }
}