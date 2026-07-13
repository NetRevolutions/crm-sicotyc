using Jarasoft.Sicotyc.Domain.Entities;

namespace Jarasoft.Sicotyc.Application.Interfaces.Repositories;

public interface IApplicationRoleRepository : IRepository<ApplicationRole>;

public interface IApplicationUserRepository : IRepository<ApplicationUser>;

public interface ICompanyRepository : IRepository<Company>;

public interface ICompanyTypeRepository : IRepository<CompanyType>;

public interface ICompanyZoneRepository : IRepository<CompanyZone>;

public interface IDistrictRepository : IRepository<District>;

public interface IDriverRepository : IRepository<Driver>;

public interface IFreightRateRepository : IRepository<FreightRate>;

public interface ILicenseTypeRepository : IRepository<LicenseType>;

public interface IMenuOptionRepository : IRepository<MenuOption>;

public interface IMenuOptionRoleRepository : IRepository<MenuOptionRole>;

public interface INegotiatedFreightRateRepository : IRepository<NegotiatedFreightRate>;

public interface IQuoteRepository : IRepository<Quote>;

public interface IQuoteTransportOfferRepository : IRepository<QuoteTransportOffer>;

public interface IServiceTypeRepository : IRepository<ServiceType>;

public interface IUserCompanyRepository : IRepository<UserCompany>;

public interface IUserDetailRepository : IRepository<UserDetail>;

public interface IVehicleRepository : IRepository<Vehicle>;

public interface IVehicleTypeRepository : IRepository<VehicleType>;

public interface IWarehouseRepository : IRepository<Warehouse>;

public interface IZoneRepository : IRepository<Zone>;
