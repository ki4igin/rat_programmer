using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;

namespace rat_programmer;

public class TextBoxTraceListener : TraceListener
{
    private readonly TextBox _textBox;

    public TextBoxTraceListener(TextBox textBox)
    {
        _textBox = textBox;
    }

    public override void Write(string message)
    {
        AppendText(message);
    }

    public override void WriteLine(string message)
    {
        AppendText(message + Environment.NewLine);
    }

    private void AppendText(string message)
    {
        if (_textBox.DispatcherQueue.HasThreadAccess)
        {
            _textBox.Text += message;
            
        }
        else
        {
            _textBox.DispatcherQueue.TryEnqueue(() =>
            {
                _textBox.Text += message;
            });
        }
    }
}
