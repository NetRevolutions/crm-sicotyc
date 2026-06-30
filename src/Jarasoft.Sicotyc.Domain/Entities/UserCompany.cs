namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class UserCompany
    {
        public UserCompany(Guid userId, Guid companyId)
        {
            UserId = userId;
            CompanyId = companyId;
        }

        public Guid UserId { get; private set; }
        public ApplicationUser? User { get; private set; }

        public Guid CompanyId { get; private set; }
        public Company? Company { get; private set; }

        public void SetRelations(ApplicationUser? user, Company? company)
        {
            User = user;
            UserId = user?.Id ?? UserId;
            Company = company;
            CompanyId = company?.Id ?? CompanyId;
        }
    }
}
