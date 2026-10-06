using Jarasoft.Sicotyc.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Jarasoft.Sicotyc.Infrastructure.Persistence;

public sealed class CompanyAdministrationLock(
    SicotycDbContext context) : ICompanyAdministrationLock
{
    public async Task AcquireAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var transaction = context.Database.CurrentTransaction
            ?? throw new InvalidOperationException(
                "Se requiere una transacción activa.");

        var connection = context.Database.GetDbConnection();

        await using var command = connection.CreateCommand();

        command.Transaction =
            transaction.GetDbTransaction();

        command.CommandType = CommandType.Text;

        command.CommandText = """
            DECLARE @result INT;

            EXEC @result = sp_getapplock
                @Resource = @Resource,
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = 10000;

            SELECT @result;
            """;

        var parameter = command.CreateParameter();

        parameter.ParameterName = "@Resource";
        parameter.Value =
            $"Sicotyc:CompanyAdministrators:{companyId:N}";

        command.Parameters.Add(parameter);

        var result = await command.ExecuteScalarAsync(
            cancellationToken);

        var lockResult = Convert.ToInt32(result);

        if (lockResult < 0)
        {
            throw new InvalidOperationException(
                $"No se pudo obtener el bloqueo. Código: {lockResult}");
        }
    }

    public Task ReleaseAsync()
    {
        // El bloqueo pertenece a la transacción.
        // SQL Server lo libera al finalizarla.
        return Task.CompletedTask;
    }
}