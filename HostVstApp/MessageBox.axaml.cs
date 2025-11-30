using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace MsgBox;

partial class MessageBox : Window
{
    public enum MessageBoxButtons
    {
        Ok,
        OkCancel,
        YesNo,
        YesNoCancel
    }

    public enum MessageBoxResult
    {
        Ok,
        Cancel,
        Yes,
        No
    }

    public MessageBox()
    {
        AvaloniaXamlLoader.Load(this);

        this.KeyDown += MessageBox_KeyDown;
    }

    private void MessageBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Escape)
        {
            e.Handled = true;
            this.Close();
        }
            
    }

    public static Task<MessageBoxResult> Show(string text, string title = "", MessageBoxButtons buttons = MessageBoxButtons.Ok, Window? parent = null)
    {
        var msgbox = new MessageBox()  {  Title = title };
        
        msgbox.FindControl<TextBlock>("Text")!.Text = text;
        var buttonPanel = msgbox.FindControl<StackPanel>("Buttons");

        var res = MessageBoxResult.Ok;

        void AddButton(string caption, MessageBoxResult r, bool def = false)
        {
            var btn = new Button { Content = caption };
            btn.Click += (_, __) => {
                res = r;
                msgbox.Close();
            };
            buttonPanel!.Children.Add(btn);
            if (def)
                res = r;
        }

        if (buttons is MessageBoxButtons.Ok or MessageBoxButtons.OkCancel)
            AddButton("Ok", MessageBoxResult.Ok, true);
        if (buttons is MessageBoxButtons.YesNo or MessageBoxButtons.YesNoCancel)
        {
            AddButton("Yes", MessageBoxResult.Yes);
            AddButton("No", MessageBoxResult.No, true);
        }

        if (buttons is MessageBoxButtons.OkCancel or MessageBoxButtons.YesNoCancel)
            AddButton("Cancel", MessageBoxResult.Cancel, true);


        var tcs = new TaskCompletionSource<MessageBoxResult>();
        msgbox.Closed += delegate { tcs.TrySetResult(res); };
        if (parent != null && parent.IsVisible)
            msgbox.ShowDialog(parent);
        else
        {
            msgbox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            msgbox.Show();
        }


        buttonPanel!.Children[0].Focus();
            
        return tcs.Task;
    }


}