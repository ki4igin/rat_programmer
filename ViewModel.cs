using Microsoft.UI.Dispatching;
using MvvmGen;
using System.Threading.Tasks;


namespace rat_programmer;
[ViewModel]
public partial class ViewModel
{
    private Programmer _programmer;
    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    [Property] private bool _isConnect;
    [Property] private bool _isSingleMode;
    [Property] private int _counterGun;

    partial void OnInitialize()
    {
        _programmer = new Programmer();
        _programmer.CouunterGunChanged += OnCouunterGunChanged;
        CounterGun = 0;
    }

    private void OnCouunterGunChanged(int obj)
    {
        _dispatcherQueue.TryEnqueue(() => CounterGun = obj);
    }

    [Command]
    private void Connect()
    {
        if (IsConnect)
        {
            _programmer.Disconnect();
            IsConnect = false;
        }
        else
        {
            IsConnect = _programmer.Connect();
        }
    }

    [Command]
    private async Task SingleAsync()
    {
        bool accses = await _programmer.CmdSingleRunAsync();
        IsSingleMode = accses;
    }

}
