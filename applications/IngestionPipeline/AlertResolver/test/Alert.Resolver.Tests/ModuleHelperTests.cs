using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Willow.Alert.Resolver.Helpers;
using Willow.Alert.Resolver.Transformers;

namespace Willow.Alert.Resolver.Tests;

[TestClass]
public class ModuleHelperTests
{
    private Dictionary<string, string> _appSettingsStub;
    private IConfigurationRoot _configuration;

    public ModuleHelperTests()
    {
        _appSettingsStub = new Dictionary<string, string>
        {
            { "DependsOn:DefaultChipkinBacnetConnector", "casbacnetrpc" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(_appSettingsStub!)
            .Build();
    }

    [TestMethod]
    public void ModuleHelper_Transform_OPCUA_Success()
    {
        const string connectorType = "DefaultOpcuaConnector";
        const string moduleName = "OpcuaConnectorModule";
        const string connectorName = "Test-OPCUA-Connector";
        var moduleTransformer = new ModuleNameTransformer();
        var moduleHelper = new ModuleHelper(new NullLogger<ModuleHelper>(), moduleTransformer, _configuration);

        var result = moduleHelper.GetDependentModules(connectorType, connectorName);

        Assert.IsNotNull(result);
        var resultList = result.ToList();

        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual($"{connectorName}-{moduleName}", resultList[0]);
    }

    [TestMethod]
    public void ModuleNameTransformer_Transform_CBacnet_DependentModule_Success()
    {
        const string connectorType = "DefaultChipkinBacnetConnector";
        const string moduleName = "CBacnetConnectorModule";
        const string connectorName = "Test-CBACnet-Connector";
        var moduleTransformer = new ModuleNameTransformer();
        var moduleHelper = new ModuleHelper(new NullLogger<ModuleHelper>(), moduleTransformer, _configuration);

        var result = moduleHelper.GetDependentModules(connectorType, connectorName);

        Assert.IsNotNull(result);
        var resultList = result.ToList();

        Assert.AreEqual(2, resultList.Count);
        Assert.AreEqual("casbacnetrpc", resultList[0]);
        Assert.AreEqual($"{connectorName}-{moduleName}", resultList[1]);
    }

    [TestMethod]
    public void ModuleNameTransformer_Transform_CBacnet_Multiple_DependentModules_Success()
    {
        _appSettingsStub = new Dictionary<string, string>
        {
            { "DependsOn:DefaultChipkinBacnetConnector", "casbacnetrpc,ModbusConnectorModule" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(_appSettingsStub!)
            .Build();

        const string connectorType = "DefaultChipkinBacnetConnector";
        const string moduleName = "CBacnetConnectorModule";
        const string connectorName = "Test-CBACnet-Connector";
        var moduleTransformer = new ModuleNameTransformer();
        var moduleHelper = new ModuleHelper(new NullLogger<ModuleHelper>(), moduleTransformer, _configuration);

        var result = moduleHelper.GetDependentModules(connectorType, connectorName);

        Assert.IsNotNull(result);
        var resultList = result.ToList();

        Assert.AreEqual(3, resultList.Count);
        Assert.AreEqual("casbacnetrpc", resultList[0]);
        Assert.AreEqual("ModbusConnectorModule", resultList[1]);
        Assert.AreEqual($"{connectorName}-{moduleName}", resultList[2]);
    }

    [TestMethod]
    public void ModuleNameTransformer_Transform_Bacnet_Success()
    {
        const string connectorType = "DefaultBacnetConnector";
        const string moduleName = "BacnetConnectorModule";
        const string connectorName = "Test-BACnet-Connector";
        var moduleTransformer = new ModuleNameTransformer();
        var moduleHelper = new ModuleHelper(new NullLogger<ModuleHelper>(), moduleTransformer, _configuration);

        var result = moduleHelper.GetDependentModules(connectorType, connectorName);

        Assert.IsNotNull(result);
        var resultList = result.ToList();

        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual($"{connectorName}-{moduleName}", resultList[0]);
    }

    [TestMethod]
    public void ModuleNameTransformer_Transform_Modbus_Success()
    {
        const string connectorType = "DefaultModbusConnector";
        const string moduleName = "ModbusConnectorModule";
        const string connectorName = "Test-Modbus-Connector";
        var moduleTransformer = new ModuleNameTransformer();
        var moduleHelper = new ModuleHelper(new NullLogger<ModuleHelper>(), moduleTransformer, _configuration);

        var result = moduleHelper.GetDependentModules(connectorType, connectorName);

        Assert.IsNotNull(result);
        var resultList = result.ToList();

        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual($"{connectorName}-{moduleName}", resultList[0]);
    }
}
