﻿#pragma checksum "..\..\..\Views\MicroView.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "A14750E51B982C3EDCF1CEE37284F50936847A138DBAD24723828929C6CAF6B2"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Xaml.Behaviors;
using Microsoft.Xaml.Behaviors.Core;
using Microsoft.Xaml.Behaviors.Input;
using Microsoft.Xaml.Behaviors.Layout;
using Microsoft.Xaml.Behaviors.Media;
using NanoImprinter.ControlExtensions;
using NanoImprinter.ControlViews;
using NanoImprinter.Converters;
using NanoImprinter.Views;
using Prism.DryIoc;
using Prism.Interactivity;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Regions.Behaviors;
using Prism.Services.Dialogs;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace NanoImprinter.Views {
    
    
    /// <summary>
    /// MicroView
    /// </summary>
    public partial class MicroView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 81 "..\..\..\Views\MicroView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbMicroLevelPositionZ;
        
        #line default
        #line hidden
        
        
        #line 86 "..\..\..\Views\MicroView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbMicroLevelPositionRX;
        
        #line default
        #line hidden
        
        
        #line 91 "..\..\..\Views\MicroView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbMicroLevelPositionRY;
        
        #line default
        #line hidden
        
        
        #line 100 "..\..\..\Views\MicroView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbMicroDemoldPositionZ;
        
        #line default
        #line hidden
        
        
        #line 105 "..\..\..\Views\MicroView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbMicroDemoldPositionRX;
        
        #line default
        #line hidden
        
        
        #line 110 "..\..\..\Views\MicroView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbMicroDemoldPositionRY;
        
        #line default
        #line hidden
        
        
        #line 119 "..\..\..\Views\MicroView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbContactPositionZ;
        
        #line default
        #line hidden
        
        
        #line 124 "..\..\..\Views\MicroView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbContactPositionRX;
        
        #line default
        #line hidden
        
        
        #line 130 "..\..\..\Views\MicroView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbContactPositionRY;
        
        #line default
        #line hidden
        
        
        #line 226 "..\..\..\Views\MicroView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cbComIndex;
        
        #line default
        #line hidden
        
        
        #line 266 "..\..\..\Views\MicroView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cbChannelIndex;
        
        #line default
        #line hidden
        
        
        #line 278 "..\..\..\Views\MicroView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbMicroJogDistance;
        
        #line default
        #line hidden
        
        
        #line 289 "..\..\..\Views\MicroView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider sldJogValue;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/NanoImprinter;component/views/microview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Views\MicroView.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler) {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.tbMicroLevelPositionZ = ((System.Windows.Controls.TextBox)(target));
            return;
            case 2:
            this.tbMicroLevelPositionRX = ((System.Windows.Controls.TextBox)(target));
            return;
            case 3:
            this.tbMicroLevelPositionRY = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.tbMicroDemoldPositionZ = ((System.Windows.Controls.TextBox)(target));
            return;
            case 5:
            this.tbMicroDemoldPositionRX = ((System.Windows.Controls.TextBox)(target));
            return;
            case 6:
            this.tbMicroDemoldPositionRY = ((System.Windows.Controls.TextBox)(target));
            return;
            case 7:
            this.tbContactPositionZ = ((System.Windows.Controls.TextBox)(target));
            return;
            case 8:
            this.tbContactPositionRX = ((System.Windows.Controls.TextBox)(target));
            return;
            case 9:
            this.tbContactPositionRY = ((System.Windows.Controls.TextBox)(target));
            return;
            case 10:
            this.cbComIndex = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 11:
            this.cbChannelIndex = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 12:
            this.tbMicroJogDistance = ((System.Windows.Controls.TextBox)(target));
            return;
            case 13:
            this.sldJogValue = ((System.Windows.Controls.Slider)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
