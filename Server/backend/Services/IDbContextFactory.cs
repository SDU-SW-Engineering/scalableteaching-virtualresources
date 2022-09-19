using ScalableTeaching.Data;

namespace ScalableTeaching.Services;

public interface IDbContextFactory
{
    VmDeploymentContext GetContext();
}