using Jarasoft.Sicotyc.Application.Interfaces.Repositories;
using Jarasoft.Sicotyc.Domain.Entities;
using Jarasoft.Sicotyc.Infraestructure.Persistence;

namespace Jarasoft.Sicotyc.Infraestructure.Repositories;

public sealed class ApplicationRoleRepository(ApplicationDbContext context) : Repository<ApplicationRole>(context), IApplicationRoleRepository;

public sealed class ApplicationUserRepository(ApplicationDbContext context) : Repository<ApplicationUser>(context), IApplicationUserRepository;

public sealed class CompanyRepository(ApplicationDbContext context) : Repository<Company>(context), ICompanyRepository;

public sealed class CompanyTypeRepository(ApplicationDbContext context) : Repository<CompanyType>(context), ICompanyTypeRepository;

public sealed class CompanyZoneRepository(ApplicationDbContext context) : Repository<CompanyZone>(context), ICompanyZoneRepository;

public sealed class DistrictRepository(ApplicationDbContext context) : Repository<District>(context), IDistrictRepository;

public sealed class DriverRepository(ApplicationDbContext context) : Repository<Driver>(context), IDriverRepository;

public sealed class FreightRateRepository(ApplicationDbContext context) : Repository<FreightRate>(context), IFreightRateRepository;

public sealed class LicenseTypeRepository(ApplicationDbContext context) : Repository<LicenseType>(context), ILicenseTypeRepository;

public sealed class MenuOptionRepository(ApplicationDbContext context) : Repository<MenuOption>(context), IMenuOptionRepository;

public sealed class MenuOptionRoleRepository(ApplicationDbContext context) : Repository<MenuOptionRole>(context), IMenuOptionRoleRepository;

public sealed class NegotiatedFreightRateRepository(ApplicationDbContext context) : Repository<NegotiatedFreightRate>(context), INegotiatedFreightRateRepository;

public sealed class QuoteRepository(ApplicationDbContext context) : Repository<Quote>(context), IQuoteRepository;

public sealed class QuoteTransportOfferRepository(ApplicationDbContext context) : Repository<QuoteTransportOffer>(context), IQuoteTransportOfferRepository;

public sealed class ServiceTypeRepository(ApplicationDbContext context) : Repository<ServiceType>(context), IServiceTypeRepository;

public sealed class UserCompanyRepository(ApplicationDbContext context) : Repository<UserCompany>(context), IUserCompanyRepository;

public sealed class UserDetailRepository(ApplicationDbContext context) : Repository<UserDetail>(context), IUserDetailRepository;

public sealed class VehicleRepository(ApplicationDbContext context) : Repository<Vehicle>(context), IVehicleRepository;

public sealed class VehicleTypeRepository(ApplicationDbContext context) : Repository<VehicleType>(context), IVehicleTypeRepository;

public sealed class WarehouseRepository(ApplicationDbContext context) : Repository<Warehouse>(context), IWarehouseRepository;

public sealed class ZoneRepository(ApplicationDbContext context) : Repository<Zone>(context), IZoneRepository;
