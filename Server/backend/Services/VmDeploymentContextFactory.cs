using Microsoft.Extensions.DependencyInjection;
using ScalableTeaching.Data;

namespace ScalableTeaching.Services;

public class VmDeploymentContextFactory : IDbContextFactory
{
    private readonly IServiceScopeFactory _factory;
    
    public VmDeploymentContextFactory(IServiceScopeFactory factory)
    {
        _factory = factory;
    }

    public VmDeploymentContext GetContext()
    {
        return _factory.CreateScope().ServiceProvider.GetRequiredService<VmDeploymentContext>();
    }
}