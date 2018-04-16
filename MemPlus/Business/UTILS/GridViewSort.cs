using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace MemPlus.Business.UTILS
{
    /// <summary>
    /// Internal logic for sorting a GridView
    /// </summary>
    internal class GridViewSort
    {
        #region Properties
        /// <summary>
        /// Check whether sorting is enabled or not
        /// </summary>
        /// <param name="obj">The DependencyObject that needs to be checked for sorting</param>
        /// <returns>True if sorting is enabled, otherwise false</returns>
        internal static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnabledProperty);
        }

        /// <summary>
        /// Enable or disable sorting
        /// </summary>
        /// <param name="obj">The DependencyObject that needs to have the Enabled property for sorting changed</param>
        /// <param name="value">True if enabled, otherwise false</param>
        internal static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(EnabledProperty, value);
        }

        /// <summary>
        /// The EnabledProperty that can be used by DependencyObjects
        /// </summary>
        internal static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(GridViewSort),
                new UIPropertyMetadata(
                    false,
                    (o, e) =>
                    {
                        // ReSharper disable once UsePatternMatching
                        ListView listView = o as ListView;
                        if (listView == null) return;
                        bool oldValue = (bool)e.OldValue;
                        bool newValue = (bool)e.NewValue;
                        if (oldValue && !newValue)
                        {
                            listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                        }
                        if (!oldValue && newValue)
                        {
                            listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                        }
                    }
                )
            );

        /// <summary>
        /// Get the name of a property
        /// </summary>
        /// <param name="obj">The DependencyObject</param>
        /// <returns>The name of a property</returns>
        internal static string GetPropertyName(DependencyObject obj)
        {
            return (string)obj.GetValue(PropertyNameProperty);
        }

        /// <summary>
        /// Set the name of a property
        /// </summary>
        /// <param name="obj">The DependencyObject</param>
        /// <param name="value">The name of the property</param>
        internal static void SetPropertyName(DependencyObject obj, string value)
        {
            obj.SetValue(PropertyNameProperty, value);
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc...
        /// </summary>
        internal static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.RegisterAttached(
                "PropertyName",
                typeof(string),
                typeof(GridViewSort),
                new UIPropertyMetadata(null)
            );
        #endregion

        #region ColumnHeader
        /// <summary>
        /// Method that is called when a ColumnHeader object is clicked
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private static void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is GridViewColumnHeader headerClicked)) return;
            string propertyName = GetPropertyName(headerClicked.Column);
            if (string.IsNullOrEmpty(propertyName)) return;
            ListView listView = GetAncestor<ListView>(headerClicked);
            if (listView == null) return;
            if (GetEnabled(listView))
            {
                ApplySort(listView.Items, propertyName);
            }
        }
        #endregion

        #region Helpermethods
        /// <summary>
        /// Get the parent object of a DependencyObject
        /// </summary>
        /// <typeparam name="T">DependencyObject</typeparam>
        /// <param name="reference">A DependencyObject</param>
        /// <returns>The parent of a DependencyObject</returns>
        internal static T GetAncestor<T>(DependencyObject reference) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(reference);
            while (!(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent ?? throw new InvalidOperationException());
            }
            return (T)parent;
        }

        /// <summary>
        /// Apply the sorting to the content of a ICollectionView
        /// </summary>
        /// <param name="view">The ICollectionView</param>
        /// <param name="propertyName">The name of the property</param>
        internal static void ApplySort(ICollectionView view, string propertyName)
        {
            ListSortDirection direction = ListSortDirection.Ascending;
            if (view.SortDescriptions.Count > 0)
            {
                SortDescription currentSort = view.SortDescriptions[0];
                if (currentSort.PropertyName == propertyName)
                {
                    direction = currentSort.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                }
                view.SortDescriptions.Clear();
            }
            if (!string.IsNullOrEmpty(propertyName))
            {
                view.SortDescriptions.Add(new SortDescription(propertyName, direction));
            }
        }
        #endregion
    }
}
