using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace rat_programmer;

public class Programmer
{
    private readonly SerialPort _serial;
    private bool _reqClosePort = false;
    private bool _askRecive = false;
    private short _counterGun = 0;

    enum Cmd
    {
        RESET = 1,
        TEST = 2,
        SINGLE_RUN,
        BURST_RUN,
        CONTINUE_RUN,
        STOP,
        TIME_SET,
        STEP_SET,
        WIDTH_SET,
        POLARITY_SET,

        ERR = 0x0101,
        CNT_GUN,
    };

    public event Action<int> CouunterGunChanged;

    public Programmer()
    {
        _serial = new()
        {
            BaudRate = 115200,
            Parity = Parity.None,
            StopBits = StopBits.Two,
            ReadTimeout = 100,
        };
    }

    public bool Connect()
    {
        var ports = SerialPort.GetPortNames();
        foreach (var port in ports)
        {
            _serial.PortName = port;
            if (_serial.TryOpen())
            {
                Trace.WriteLine($"{port}");
                Send(Cmd.TEST, 0);
                byte[] rxBuf = new byte[4];
                if (_serial.TryRead(rxBuf, 4))
                {
                    Trace.WriteLine($"Программатор подключен к {port}");
                    Task.Run(ReceiveProcess);
                    return true;
                }
                _serial.Close();
            }

        }

        return false;
    }

    public void Disconnect()
    {
        _reqClosePort = true;        
    }

    public async Task<bool> CmdResetAsync() =>
        await CmdSendAsync(Cmd.RESET, 0);

    public async Task<bool> CmdStopAsync() =>
        await CmdSendAsync(Cmd.STOP, 0);

    public async Task<bool> CmdTestAsync() =>
        await CmdSendAsync(Cmd.TEST, 0);

    public async Task<bool> CmdSingleRunAsync() =>
        await CmdSendAsync(Cmd.SINGLE_RUN, 0);

    public async Task<bool> CmdBurstRunAsync() =>
        await CmdSendAsync(Cmd.BURST_RUN, 0);

    public async Task<bool> CmdContinueRunAsync() =>
        await CmdSendAsync(Cmd.CONTINUE_RUN, 0);

    public async Task<bool> CmdStopRunAsync() =>
        await CmdSendAsync(Cmd.STOP, 0);

    public async Task<bool> CmdTimeSetAsync(int time) =>
        await CmdSendAsync(Cmd.TIME_SET, (short)time);

    public async Task<bool> CmdStepSetAsync(int time) =>
        await CmdSendAsync(Cmd.STEP_SET, (short)time);

    public async Task<bool> CmdWidthPulseSetAsync(int time) =>
        await CmdSendAsync(Cmd.WIDTH_SET, (short)time);

    private async Task<bool> CmdSendAsync(Cmd cmd, short arg)
    {
        _askRecive = false;
        for (int i = 0; i < 3; i++)
        {
            Send(cmd, arg);
            await Task.Delay(100);
            if (_askRecive)
                return true;
        }
        return false;
    }


    private void Send(Cmd cmd, short arg)
    {
        byte[] cmdBytes = BitConverter.GetBytes((short)cmd);
        byte[] argBytes = BitConverter.GetBytes(arg);
        byte[] sendBytes = [.. cmdBytes, .. argBytes];
        _serial.Write(sendBytes, 0, 4);
        Trace.WriteLine($"Send: {ToHex(sendBytes)}");
    }

    private void ReceiveProcess()
    {
        while (true)
        {
            if (_serial.IsOpen)
            {
                Receive();
            }

            if (_reqClosePort)
            {
                _serial.Close();
                _reqClosePort = false;
                return;
            }

            Thread.Sleep(10);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private string ToHex(byte[] bytes) =>
        string.Join(" ", bytes.Select(b => $"0x{b:X2}"));


    private void Receive()
    {
        if (_serial.BytesToRead < 4)
            return;

        byte[] rxBuf = new byte[4];
        _ = _serial.Read(rxBuf, 0, 4);
        Cmd cmd = (Cmd)BitConverter.ToUInt16(rxBuf);
        short arg = BitConverter.ToInt16(rxBuf, 2);
        Trace.WriteLine($"Rev:  {ToHex(rxBuf)}");
        CmdWork(cmd, arg);
    }

    private void CmdWork(Cmd cmd, short arg)
    {
        switch (cmd)
        {
            case Cmd.ERR:
                if (arg == 0)
                    _askRecive = true;
                else if (arg == 1)
                    Trace.WriteLine("Rev:  Команды не существует");
                else if (arg == 2)
                    Trace.WriteLine("Rev:  Аргумент команды неверен");
                else
                    Trace.WriteLine("Rev:  Неизвестная ошибка");
                break;
            case Cmd.CNT_GUN:
                _counterGun = arg;
                CouunterGunChanged?.Invoke(arg);
                break;
            default:
                Trace.WriteLine("Rev:  неизвестный ответ ");
                break;
        }
    }
}
