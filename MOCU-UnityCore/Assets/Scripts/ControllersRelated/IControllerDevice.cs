using System.Collections;
using System.Threading.Tasks;


public interface IControllerDevice
{
    ModuleStatus ConnectionStatus { get; }
    bool IsInUse { get; set; }
    string DisplayName { get; }

    void TryRepair();
    Task Init();
}