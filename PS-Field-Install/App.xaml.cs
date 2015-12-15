using System;
using System.Security.Permissions;
using System.Windows;


namespace PS_Field_Install {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {

		[SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlAppDomain)]
		private void Application_Startup(object sender, StartupEventArgs e) {
			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
		}

		async void MyHandler(object sender, UnhandledExceptionEventArgs args) {
			MessageBox.Show("A fatal error has occured.  The error has been logged and sent to the developer.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
			Exception e = (Exception)args.ExceptionObject;
			Scripts.LogHelper.Log.Fatal("A fatal error has occured. The app will now close.  Information about the fatal error follows: " + e.Message);
			Scripts.LogHelper.Log.Trace(e.StackTrace.ToString());
			await Scripts.LogHelper.UploadLog();
			Environment.Exit(69);
		}
	}

}
