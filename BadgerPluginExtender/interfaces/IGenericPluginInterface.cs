using BadgerPluginExtender.dto;

namespace BadgerPluginExtender.interfaces
{
    public interface IGenericPluginInterface
    {

        PluginInfo GetPluginInfo();

        MethodRecord[] GetMethodToRecords();



    }
}
