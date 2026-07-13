using Jarasoft.Sicotyc.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class ApplicationUser: IdentityUser<Guid>
    {
        public ApplicationUser()
        {
            Quotes = new List<Quote>();
            QuoteTransportOffers = new List<QuoteTransportOffer>();
            UserCompanies = new List<UserCompany>();
        }

        // Object Values
        [Required(ErrorMessage = "Nombre es requerido")]
        [MaxLength(100)]
        public FirstName? FirstName { get; private set; }

        [MaxLength(100)]
        public MiddleName? MiddleName { get; private set; }

        [Required(ErrorMessage = "Apellido es requerido")]
        [MaxLength(100)]
        public LastName? LastName { get; private set; }

        [MaxLength(100)]
        public MaidenName? MaidenName { get; private set; }        

        [Required(ErrorMessage = "Numero de Identidad es requerido")]
        public DocumentIdentity? DocumentIdentity { get; private set; }

        public string? Img { get; private set; }        

        // Tracking properties
        public Tracking? Tracking { get; private set; } // Este es un record para agrupar las propiedades de seguimiento

        // Relationships
        public Guid? ApplicationRoleId { get; private set; }
        public ApplicationRole? ApplicationRole { get; private set; }

        public Guid? UserDetailId { get; private set; }
        public UserDetail? UserDetail { get; private set; }

        public ICollection<Quote> Quotes { get; private set; }
        public ICollection<QuoteTransportOffer> QuoteTransportOffers { get; private set; }
        public ICollection<UserCompany> UserCompanies { get; private set; }

        public void UpdateIdentity(
            FirstName firstName,
            MiddleName? middleName,
            LastName lastName,
            MaidenName? maidenName,
            DocumentIdentity documentIdentity,
            Guid? updatedBy = null)
        {
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            MaidenName = maidenName;
            DocumentIdentity = documentIdentity;
            Tracking = Common.Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetProfileImage(string? img, Guid? updatedBy = null)
        {
            Img = img;
            Tracking = Common.Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetUserDetail(UserDetail? userDetail, Guid? updatedBy = null)
        {
            UserDetail = userDetail;
            UserDetailId = userDetail?.UserId;
            Tracking = Common.Helper.TouchUpdated(Tracking, updatedBy);
        }

        public void SetApplicationRole(ApplicationRole? applicationRole, Guid? updatedBy = null)
        {
            ApplicationRole = applicationRole;
            ApplicationRoleId = applicationRole?.Id;
            Tracking = Common.Helper.TouchUpdated(Tracking, updatedBy);
        }

    }
}
