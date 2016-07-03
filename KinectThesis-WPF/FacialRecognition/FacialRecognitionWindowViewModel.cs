using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    /// <summary>
    /// View model for the main window
    /// </summary>
    public class FacialRecognitionWindowViewModel : INotifyPropertyChanged
    {
        private ImageSource currentVideoFrame;

        // private ProcessorTypes processorType;
        private string trainName;

        private string enterText;

        private bool trainNameEnabled, readyForTraining, trainingInProcess, goToLibraryClickedEnabled;

        /// <summary>
        /// Initializes a new instance of the MainWindowViewModel class
        /// </summary>
        public FacialRecognitionWindowViewModel()
        {
            this.TargetFaces = new ObservableCollection<FacialRecognitionWindow.BitmapSourceTargetFace>();
            //when there is an event it will be notified
        }

        /// <summary>
        /// Raised when a property is changed on the view model
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the target faces for matching
        /// </summary>
        public ObservableCollection<FacialRecognitionWindow.BitmapSourceTargetFace> TargetFaces { get; private set; }

        /// <summary>
        /// Gets or sets a command that's executed when the train button is clicked
        /// </summary>
        public ICommand TrainButtonClicked { get; set; }

        public ICommand GoToLibraryClicked { get; set; }
        public ICommand GoToLibraryDirectlyClicked { get; set; }

        /// <summary>
        /// Gets or sets the current video frame
        /// </summary>
        public ImageSource CurrentVideoFrame
        {
            get
            {
                return this.currentVideoFrame;
            }

            set
            {
                this.currentVideoFrame = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("CurrentVideoFrame"));
            }
        }

        /// <summary>
        /// Gets or sets the name of the training image
        /// </summary>
        public string TrainName
        {
            get
            {
                return this.trainName;
            }

            set
            {
                this.trainName = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("TrainName"));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the train button should be enabled
        /// </summary>
        public bool TrainButtonEnabled
        {
            get { return this.ReadyForTraining && !this.trainingInProcess; }
        }

        public bool GoToLibraryClickedEnabled
        {
            get
            {
                return this.goToLibraryClickedEnabled;
            }

            set
            {
                this.goToLibraryClickedEnabled = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("GoToLibraryClickedEnabled"));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether we're ready to train the system
        /// </summary>
        public bool ReadyForTraining
        {
            get
            {
                return this.readyForTraining;
            }

            set
            {
                this.readyForTraining = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("TrainButtonEnabled"));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether training is in process
        /// </summary>
        public bool TrainingInProcess
        {
            get
            {
                return this.trainingInProcess;
            }

            set
            {
                this.trainingInProcess = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("TrainButtonEnabled"));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the training name field should be enabled
        /// </summary>
        public bool TrainNameEnabled
        {
            get
            {
                return this.trainNameEnabled;
            }

            set
            {
                this.trainNameEnabled = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("TrainNameEnabled"));
            }
        }

        public string EnterText
        {
            get
            {
                return this.enterText;
            }

            set
            {
                this.enterText = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("EnterText"));
            }
        }
    }
}