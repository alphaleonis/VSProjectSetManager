using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Alphaleonis.VSProjectSetMgr.Controls
{
   public class InclusionExclusionCheckBox : ButtonBase
   {
      static InclusionExclusionCheckBox()
      {
         DefaultStyleKeyProperty.OverrideMetadata(typeof(InclusionExclusionCheckBox), new FrameworkPropertyMetadata(typeof(InclusionExclusionCheckBox)));         
      }

      #region StateChanged event

      public static readonly RoutedEvent StateChangedEvent = EventManager.RegisterRoutedEvent("StateChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InclusionExclusionCheckBox));
      public static readonly RoutedEvent PreviewStateChangedEvent = EventManager.RegisterRoutedEvent("PreviewStateChanged", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(InclusionExclusionCheckBox));

      public event RoutedEventHandler StateChanged
      {
         add { AddHandler(StateChangedEvent, value); }
         remove { RemoveHandler(StateChangedEvent, value); }
      }

      public event RoutedEventHandler PreviewStateChanged
      {
         add { AddHandler(PreviewStateChangedEvent, value); }
         remove { RemoveHandler(PreviewStateChangedEvent, value); }
      }

      protected virtual void OnStateChanged()
      {
         RoutedEventArgs previewEventArgs = new RoutedEventArgs(PreviewStateChangedEvent);
         RaiseEvent(previewEventArgs);
         if (previewEventArgs.Handled)
            return;
         RoutedEventArgs eventArgs = new RoutedEventArgs(StateChangedEvent);
         RaiseEvent(eventArgs);
      }

      #endregion

      #region State Property

      public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(InclusionExclusionCheckBoxState), typeof(InclusionExclusionCheckBox), new FrameworkPropertyMetadata(InclusionExclusionCheckBoxState.Unchecked, new PropertyChangedCallback(OnStateChanged), null));

      private static void OnStateChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
      {
         InclusionExclusionCheckBox fourStateCheckBox = o as InclusionExclusionCheckBox;
         if (fourStateCheckBox != null)
            fourStateCheckBox.OnStateChanged((InclusionExclusionCheckBoxState)e.OldValue, (InclusionExclusionCheckBoxState)e.NewValue);
      }      

      protected virtual void OnStateChanged(InclusionExclusionCheckBoxState oldValue, InclusionExclusionCheckBoxState newValue)
      {
         if (oldValue != newValue)
            OnStateChanged();

         if (newValue == InclusionExclusionCheckBoxState.ImplicitlyIncluded || newValue == InclusionExclusionCheckBoxState.Included)
            IsIncluded = true;
         else
            IsIncluded = false;
      }

      public InclusionExclusionCheckBoxState State
      {
         get
         {
            return (InclusionExclusionCheckBoxState)GetValue(StateProperty);
         }
         set
         {
            SetValue(StateProperty, value);
         }
      }

      #endregion

      #region HasImplicitInclusion property

      public static readonly DependencyProperty HasImplicitInclusionProperty = DependencyProperty.Register("HasImplicitInclusion", typeof(bool), typeof(InclusionExclusionCheckBox), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsImplicitlyIncludedChanged), null));

      private static void OnIsImplicitlyIncludedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
      {
         InclusionExclusionCheckBox inclusionExclusionCheckBox = o as InclusionExclusionCheckBox;
         if (inclusionExclusionCheckBox != null)
            inclusionExclusionCheckBox.OnIsImplicitlyIncludedChanged((bool)e.OldValue, (bool)e.NewValue);
      }

      protected virtual void OnIsImplicitlyIncludedChanged(bool oldValue, bool newValue)
      {
         if ((State == InclusionExclusionCheckBoxState.Unchecked || State == InclusionExclusionCheckBoxState.PartiallyIncluded) && newValue == true)
            State = InclusionExclusionCheckBoxState.ImplicitlyIncluded;
         else if (State == InclusionExclusionCheckBoxState.ImplicitlyIncluded && newValue == false)
            State = HasPartialInclusion ? InclusionExclusionCheckBoxState.PartiallyIncluded : InclusionExclusionCheckBoxState.Unchecked;               
      }

      public bool HasImplicitInclusion
      {
         get
         {
            return (bool)GetValue(HasImplicitInclusionProperty);
         }
         set
         {
            SetValue(HasImplicitInclusionProperty, value);
         }
      }

      #endregion

      #region Depedency Property: AllowExplicitExclusion

      public static readonly DependencyProperty AllowExplicitExclusionProperty = DependencyProperty.Register("AllowExplicitExclusion", typeof(bool), typeof(InclusionExclusionCheckBox), new FrameworkPropertyMetadata(true, OnAllowExplicitExclusionChanged));

      private static void OnAllowExplicitExclusionChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
      {
         InclusionExclusionCheckBox inclusionExclusionCheckBox = o as InclusionExclusionCheckBox;
         if (inclusionExclusionCheckBox != null)
            inclusionExclusionCheckBox.OnAllowExplicitExclusionChanged((bool)e.OldValue, (bool)e.NewValue);
      }

      protected virtual void OnAllowExplicitExclusionChanged(bool oldValue, bool newValue)
      {
         if (oldValue == true && newValue == false && State == InclusionExclusionCheckBoxState.Excluded)
            OnToggle();
      }

      public bool AllowExplicitExclusion
      {
         get
         {
            return (bool)GetValue(AllowExplicitExclusionProperty);
         }
         set
         {
            SetValue(AllowExplicitExclusionProperty, value);
         }
      }

      #endregion

      
      #region Depedency Property: HasPartialInclusion

      public static readonly DependencyProperty HasPartialInclusionProperty = DependencyProperty.Register("HasPartialInclusion", typeof(bool), typeof(InclusionExclusionCheckBox), new FrameworkPropertyMetadata(false, OnHasPartialInclusionChanged));


      private static void OnHasPartialInclusionChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
      {
         InclusionExclusionCheckBox inclusionExclusionCheckBox = o as InclusionExclusionCheckBox;
         if (inclusionExclusionCheckBox != null)
            inclusionExclusionCheckBox.OnHasPartialInclusionChanged((bool)e.OldValue, (bool)e.NewValue);
      }

      protected virtual void OnHasPartialInclusionChanged(bool oldValue, bool newValue)
      {
         if ((State == InclusionExclusionCheckBoxState.Unchecked || State == InclusionExclusionCheckBoxState.ImplicitlyIncluded) && newValue == true)
         {
            State = InclusionExclusionCheckBoxState.PartiallyIncluded;
         }
         else if (State == InclusionExclusionCheckBoxState.PartiallyIncluded && newValue == false)
         {
            if (HasImplicitInclusion)
               State = InclusionExclusionCheckBoxState.ImplicitlyIncluded;
            else
               State = InclusionExclusionCheckBoxState.Unchecked;
         }
      }


      public bool HasPartialInclusion
      {
         get
         {
            return (bool)GetValue(HasPartialInclusionProperty);
         }
         set
         {
            SetValue(HasPartialInclusionProperty, value);
         }
      }

      #endregion
      
      #region IsIncluded

      public static readonly DependencyPropertyKey IsIncludedPropertyKey = DependencyProperty.RegisterReadOnly("IsIncluded", typeof(bool), typeof(InclusionExclusionCheckBox), new FrameworkPropertyMetadata(false));

      public static readonly DependencyProperty IsIncludedProperty = IsIncludedPropertyKey.DependencyProperty;

      public bool IsIncluded
      {
         get
         {
            return (bool)GetValue(IsIncludedProperty);
         }

         private set
         {
            SetValue(IsIncludedPropertyKey, value);
         }
      }

      #endregion

      #region Methods

      protected override void OnClick()
      {
         OnToggle();
         base.OnClick();
      }

      private void OnToggle()
      {         
         switch (State)
         {
            case InclusionExclusionCheckBoxState.Included:
               if (AllowExplicitExclusion)
                  State = InclusionExclusionCheckBoxState.Excluded;
               else
                  State = HasPartialInclusion ? InclusionExclusionCheckBoxState.PartiallyIncluded : (HasImplicitInclusion ? InclusionExclusionCheckBoxState.ImplicitlyIncluded : InclusionExclusionCheckBoxState.Unchecked);
               break;
            case InclusionExclusionCheckBoxState.Excluded:
               State = HasImplicitInclusion ? InclusionExclusionCheckBoxState.ImplicitlyIncluded : (HasPartialInclusion ? InclusionExclusionCheckBoxState.PartiallyIncluded : InclusionExclusionCheckBoxState.Unchecked);
               break;
            case InclusionExclusionCheckBoxState.ImplicitlyIncluded:
            case InclusionExclusionCheckBoxState.PartiallyIncluded:
            case InclusionExclusionCheckBoxState.Unchecked:
               State = InclusionExclusionCheckBoxState.Included;
               break;
         }

      }

      #endregion
   }

   public enum InclusionExclusionCheckBoxState
   {
      Unchecked,
      ImplicitlyIncluded,
      Included,
      Excluded,
      PartiallyIncluded
   }   
}
