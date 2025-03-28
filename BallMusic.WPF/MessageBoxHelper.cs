using System.Windows;

namespace BallMusic.WPF;

public static class MessageBoxHelper
{
    public static MessageBoxResult ShowError(string error, string caption = "Error", Window? owner = null)
        => Show(error, caption, MessageBoxButton.OK, MessageBoxImage.Error, owner);

    public static MessageBoxResult ShowWaring(string warning, string caption = "Warning", Window? owner = null)
        => Show(warning, caption, MessageBoxButton.OK, MessageBoxImage.Warning, owner);

    public static MessageBoxResult ShowSuccess(string info, string caption = "Success", Window? owner = null)
        => Show(info, caption, MessageBoxButton.OK, MessageBoxImage.Information, owner);

    public static MessageBoxResult Ask(string question, string caption = "?", MessageBoxImage icon = MessageBoxImage.Question, Window? owner = null)
        => Show(question, caption, MessageBoxButton.YesNo, icon, owner);

    public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, Window? owner = null)
        => owner is null
            ? MessageBox.Show(messageBoxText, caption, button, icon)
            : MessageBox.Show(owner, messageBoxText, caption, button, icon);
}