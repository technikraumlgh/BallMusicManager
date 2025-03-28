using System.Windows.Media;
using System.Windows;
using BallMusic.WPF;

namespace BallMusic.WPF;

public static class DependencyObjectExtensions
{
    public static DependencyObject? GetParent(this DependencyObject child) => VisualTreeHelper.GetParent(child);
    public static T? FindParentOrSelf<T>(this DependencyObject obj) where T : DependencyObject => obj is T typed ? typed : obj.FindParent<T>();
    public static T? FindParent<T>(this DependencyObject obj) where T : DependencyObject
        => VisualTreeHelper.GetParent(obj) switch
        {
            null => null,
            T typed => typed,
            var parent => parent.FindParent<T>(),
        };


    public static DependencyObject? GetChild(this DependencyObject child, int index) => VisualTreeHelper.GetChild(child, index);
    public static T? FindChildOrSelf<T>(this DependencyObject obj) where T : DependencyObject => obj is T typed ? typed : obj.FindChild<T>();
    public static T? FindChild<T>(this DependencyObject obj) where T : DependencyObject
    {
        var children = obj.EnumerateChildren();
        return children.OfType<T>().FirstOrDefault() ?? children.Select(FindChild<T>).FirstOrDefault(c => c is not null);
    }
    public static IEnumerable<DependencyObject> EnumerateChildren(this DependencyObject obj)
    {
        foreach (var index in Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(obj)))
        {
            if (obj.GetChild(index) is DependencyObject child)
            {
                yield return child;
            }
        }
    }
}
