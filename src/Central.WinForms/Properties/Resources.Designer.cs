﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ZeroInstall.Central.WinForms.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ZeroInstall.Central.WinForms.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        internal static System.Drawing.Bitmap App {
            get {
                object obj = ResourceManager.GetObject("App", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please configure sync settings first..
        /// </summary>
        internal static string ConfigSyncFirst {
            get {
                return ResourceManager.GetString("ConfigSyncFirst", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please enter the URL of a Zero Install interface here:.
        /// </summary>
        internal static string EnterInterfaceUrl {
            get {
                return ResourceManager.GetString("EnterInterfaceUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to run {0}..
        /// </summary>
        internal static string FailedToRun {
            get {
                return ResourceManager.GetString("FailedToRun", resourceCulture);
            }
        }
        
        internal static System.Drawing.Bitmap Logo {
            get {
                object obj = ResourceManager.GetObject("Logo", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Search.
        /// </summary>
        internal static string Search {
            get {
                return ResourceManager.GetString("Search", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An update to Zero Install v{0} is available. Install now? (recommended).
        /// </summary>
        internal static string SelfUpdateAvailable {
            get {
                return ResourceManager.GetString("SelfUpdateAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No
        ///Ignore update for now.
        /// </summary>
        internal static string SelfUpdateNo {
            get {
                return ResourceManager.GetString("SelfUpdateNo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Yes
        ///Install update.
        /// </summary>
        internal static string SelfUpdateYes {
            get {
                return ResourceManager.GetString("SelfUpdateYes", resourceCulture);
            }
        }
    }
}
