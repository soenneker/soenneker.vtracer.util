using Soenneker.Vtracer.Util.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Vtracer.Util.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class VtracerUtilTests : HostedUnitTest
{
    private readonly IVtracerUtil _util;

    public VtracerUtilTests(Host host) : base(host)
    {
        _util = Resolve<IVtracerUtil>(true);
    }

    [Test]
    public void Default()
    {

    }
}
