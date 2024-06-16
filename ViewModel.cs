using Microsoft.UI.Dispatching;
using MvvmGen;
using System;
using System.Threading.Tasks;

namespace rat_programmer;
[ViewModel]
public partial class ViewModel
{
    enum Mode
    {
        Single,
        Burst,
        Continue
    };

    private Programmer _programmer;
    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    [Property] private bool _isConnect;
    [Property] private bool _isSingleMode;
    [Property] private bool _isBurstMode;
    [Property] private bool _isContinueMode;
    [Property] private int _counterGun;
    [Property] private int _progTime;
    [Property] private int _stepTime;
    [Property] private int _widthPulse;

    partial void OnInitialize()
    {
        _programmer = new Programmer();
        _programmer.CouunterGunChanged += OnCouunterGunChanged;
        CounterGun = 0;
        ProgTime = 400;
        StepTime = 10;
        WidthPulse = 5;
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

    [Command(CanExecuteMethod = nameof(CanExec))]
    private async Task RestartAsync()
    {
        await _programmer.CmdResetAsync();
    }

    [Command(CanExecuteMethod = nameof(CanExec))]
    private async Task StopAsync()
    {
        await _programmer.CmdStopAsync();
    }

    [Command(CanExecuteMethod = nameof(CanExec))]
    private async Task TestAsync()
    {
        await _programmer.CmdTestAsync();
    }

    [Command(CanExecuteMethod = nameof(CanExec))]
    private async Task SingleAsync()
    {
        bool accses = await _programmer.CmdSingleRunAsync();
        if (accses)
            ChangeMode(Mode.Single);
    }

    [Command(CanExecuteMethod = nameof(CanExec))]
    private async Task BurstAsync()
    {
        bool accses = await _programmer.CmdBurstRunAsync();
        if (accses)
            ChangeMode(Mode.Burst);
    }

    [Command(CanExecuteMethod = nameof(CanExec))]
    private async Task ContinueAsync()
    {
        bool accses = await _programmer.CmdContinueRunAsync();
        if (accses)
            ChangeMode(Mode.Continue);
    }

    [Command(CanExecuteMethod = nameof(CanExec))]
    private async Task SetTimeAsync() =>
        await _programmer.CmdTimeSetAsync(ProgTime);

    [Command(CanExecuteMethod = nameof(CanExec))]
    private async Task SetStepAsync() =>
        await _programmer.CmdStepSetAsync(StepTime);

    [Command(CanExecuteMethod = nameof(CanExec))]
    private async Task SetWidthPulseAsync() =>
        await _programmer.CmdWidthPulseSetAsync(WidthPulse);

    private bool CanExec() => IsConnect;

    private void ChangeMode(Mode mode)
    {
        IsSingleMode = false;
        IsBurstMode = false;
        IsContinueMode = false;

        switch (mode)
        {
            case Mode.Single:
                IsSingleMode = true;
                break;
            case Mode.Burst:
                IsBurstMode = true;
                break;
            case Mode.Continue:
                IsContinueMode = true;
                break;
            default:
                break;
        }
    }

}
