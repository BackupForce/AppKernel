using System;

namespace Application.Abstractions.Data;

public interface IUniqueConstraintDetector
{
    bool IsUniqueConstraint(Exception exception, string constraintName);
}
