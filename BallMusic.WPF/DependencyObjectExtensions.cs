using System.Windows.Media;
using System.Windows;

namespace BallMusic.WPF;

public static class DependencyObjectExtensions
{
    extension(DependencyObject obj)
    {
        public DependencyObject? GetParent() => VisualTreeHelper.GetParent(obj);
        public T? FindParentOrSelf<T>() where T : DependencyObject => obj is T typed ? typed : obj.FindParent<T>();
        public T? FindParent<T>() where T : DependencyObject
            => VisualTreeHelper.GetParent(obj) switch
            {
                null => null,
                T typed => typed,
                var parent => parent.FindParent<T>(),
            };

        public DependencyObject? GetChild(int index) => VisualTreeHelper.GetChild(obj, index);
        public T? FindChildOrSelf<T>() where T : DependencyObject => obj is T typed ? typed : obj.FindChild<T>();
        public T? FindChild<T>() where T : DependencyObject
        {
            var children = obj.EnumerateChildren();
            return children.OfType<T>().FirstOrDefault() ?? children.Select(FindChild<T>).FirstOrDefault(c => c is not null);
        }
        
        public IEnumerable<DependencyObject> EnumerateChildren()
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
}
