namespace Jarasoft.Sicotyc.Domain.Entities
{
    public sealed class Ubigeo
    {
        private Ubigeo()
        {            
        }

        public Ubigeo(
            string idDist,
            string nombreDepartamento,
            string nombreProvincia,
            string nombreDistrito,
            string nombreCapital,
            int codigoRegionNatural,
            string regionNatural)
        {
            IdDist = idDist;
            NombreDepartamento = nombreDepartamento;
            NombreProvincia = nombreProvincia;
            NombreDistrito = nombreDistrito;
            NombreCapital = nombreCapital;
            CodigoRegionNatural = codigoRegionNatural;
            RegionNatural = regionNatural;
        }
        public string IdDist { get; private set; } = null!;

        public string NombreDepartamento { get; private set; } = null!;

        public string NombreProvincia { get; private set; } = null!;

        public string NombreDistrito { get; private set; } = null!;

        public string NombreCapital { get; private set; } = null!;

        public int CodigoRegionNatural { get; private set; }

        public string RegionNatural { get; private set; } = null!;
    }
}
