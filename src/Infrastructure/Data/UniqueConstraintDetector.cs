using System;
using Application.Abstractions.Data;
using Npgsql;

namespace Infrastructure.Data;

internal sealed class UniqueConstraintDetector : IUniqueConstraintDetector
{
    public bool IsUniqueConstraint(Exception exception, string constraintName)
    {
        PostgresException? postgresException = ExtractPostgresException(exception);
        if (postgresException == null)
        {
            return false;
        }

        return postgresException.SqlState == PostgresErrorCodes.UniqueViolation
            && string.Equals(postgresException.ConstraintName, constraintName, StringComparison.Ordinal);
    }

    private static PostgresException? ExtractPostgresException(Exception exception)
    {
        if (exception is PostgresException postgresException)
        {
            return postgresException;
        }

        if (exception.InnerException == null)
        {
            return null;
        }

        return ExtractPostgresException(exception.InnerException);
    }
}
