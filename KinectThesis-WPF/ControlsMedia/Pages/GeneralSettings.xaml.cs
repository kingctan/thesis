namespace Microsoft.Samples.Kinect.ControlsBasics
{
    using Microsoft.Kinect.Input;
    using Microsoft.Kinect.Toolkit.Input;
    using Microsoft.Kinect.Wpf.Controls;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class GeneralSettings : UserControl
    {
        /// <summary>
        /// Here to be changed by the settings.
        /// </summary>
        private MainWindow parentWindow;

        public GeneralSettings()
        {
            this.InitializeComponent();
            this.Loaded += EngagementSettings_Loaded;
            this.Unloaded += EngagementSettings_Unloaded;
        }

        private void EngagementSettings_Loaded(object sender, RoutedEventArgs e)
        {
            parentWindow = (MainWindow)Window.GetWindow((DependencyObject)sender);
            // change controls to represent current engagement settings
            // during load of page.
            App app = ((App)App.Current);

            IKinectEngagementManager kinectEngagementManager = app.KinectRegion.KinectEngagementManager;
            var handInScreenEngagementModel = kinectEngagementManager as HandInScreenEngagementModel;
            var handOverHeadEngagementModel = kinectEngagementManager as HandOverheadEngagementModel;

            switch (KinectCoreWindow.KinectEngagementMode)
            {
                case KinectEngagementMode.SystemOnePerson:
                    onePerson.IsChecked = true;
                    system.IsChecked = true;
                    break;

                case KinectEngagementMode.SystemTwoPerson:
                    twoPerson.IsChecked = true;
                    system.IsChecked = true;
                    break;

                case KinectEngagementMode.ManualOnePerson:
                    onePerson.IsChecked = true;
                    if (handInScreenEngagementModel != null)
                    {
                        manualOnScreen.IsChecked = true;
                    }
                    else if (handOverHeadEngagementModel != null)
                    {
                        manualOverHead.IsChecked = true;
                    }
                    break;

                case KinectEngagementMode.ManualTwoPerson:
                    twoPerson.IsChecked = true;
                    if (handInScreenEngagementModel != null)
                    {
                        manualOnScreen.IsChecked = true;
                    }
                    else if (handOverHeadEngagementModel != null)
                    {
                        manualOverHead.IsChecked = true;
                    }
                    break;
            }

            // Manage cursor sprite sheets
            if (app.KinectRegion.CursorSpriteSheetDefinition == KinectRegion.DefaultSpriteSheet)
            {
                cursorSpriteSheetDefault.IsChecked = true;
            }
            else
            {
                cursorSpriteSheetColor.IsChecked = true;
            }

            if (parentWindow.GestureLibIsPaused)
            {
                handGesturesOff.IsChecked = true;
            }
            else
            {
                handGesturesOn.IsChecked = true;
            }
        }

        private void EngagementSettings_Unloaded(object sender, RoutedEventArgs e)
        {
            int people = onePerson.IsChecked.HasValue && onePerson.IsChecked.Value ? 1 : 2;
            App app = ((App)App.Current);

            if (system.IsChecked.HasValue && system.IsChecked.Value)
            {
                switch (people)
                {
                    case 1:
                        app.KinectRegion.SetKinectOnePersonSystemEngagement();
                        break;

                    case 2:
                        app.KinectRegion.SetKinectTwoPersonSystemEngagement();
                        break;
                }
            }
            else if (manualOverHead.IsChecked.HasValue && manualOverHead.IsChecked.Value)
            {
                var engagementModel = new HandOverheadEngagementModel(people);
                switch (people)
                {
                    case 1:
                        app.KinectRegion.SetKinectOnePersonManualEngagement(engagementModel);
                        break;

                    case 2:
                        app.KinectRegion.SetKinectTwoPersonManualEngagement(engagementModel);
                        break;
                }
            }
            else if (manualOnScreen.IsChecked.HasValue && manualOnScreen.IsChecked.Value)
            {
                var engagementModel = new HandInScreenEngagementModel(people, app.KinectRegion.InputPointerManager);
                switch (people)
                {
                    case 1:
                        app.KinectRegion.SetKinectOnePersonManualEngagement(engagementModel);
                        break;

                    case 2:
                        app.KinectRegion.SetKinectTwoPersonManualEngagement(engagementModel);
                        break;
                }
            }

            // Manage cursor sprite sheets
            if (cursorSpriteSheetDefault.IsChecked.HasValue && cursorSpriteSheetDefault.IsChecked.Value)
            {
                app.KinectRegion.CursorSpriteSheetDefinition = KinectRegion.DefaultSpriteSheet;
            }
            else if (cursorSpriteSheetColor.IsChecked.HasValue && cursorSpriteSheetColor.IsChecked.Value)
            {
                app.KinectRegion.CursorSpriteSheetDefinition = new CursorSpriteSheetDefinition(new System.Uri("pack://application:,,,/Assets/CursorSpriteSheetPurple.png"), 4, 20, 137, 137);
            }

            if (handGesturesOn.IsChecked.HasValue && handGesturesOn.IsChecked.Value)
            {
                parentWindow.GestureLibIsPaused = false;
            }
            else if (handGesturesOff.IsChecked.HasValue && handGesturesOff.IsChecked.Value)
            {
                parentWindow.GestureLibIsPaused = true;
            }
        }
    }
}