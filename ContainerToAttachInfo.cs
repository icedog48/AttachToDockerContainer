using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttachToDockerContainer
{
    public class ContainerToAttachInfo : INotifyPropertyChanged
    {
        private string vsdbgPath;

        private string processId;
        private IList<string> processIds;

        private string containerName;
        private IList<string> containerNames;

        public string VSDBGPath
        {
            get
            {
                return vsdbgPath;
            }
            set
            {
                this.vsdbgPath = value;

                NotifyPropertyChanged("VSDBGPath");
            }
        }

        public string ProcessId
        {
            get
            {
                return processId;
            }
            set
            {
                this.processId = value;

                NotifyPropertyChanged("ProcessId");
            }
        }

        public IList<string> ProcessIds
        {
            get
            {
                return processIds;
            }
            set
            {
                this.processIds = value;

                NotifyPropertyChanged("ProcessIds");

                this.ProcessId = default(string);
            }
        }

        public string ContainerName
        {
            get
            {
                return containerName;
            }
            set
            {
                this.containerName = value;

                NotifyPropertyChanged("ContainerName");
            }
        }

        public IList<string> ContainerNames
        {
            get
            {
                return containerNames;
            }
            set
            {
                this.containerNames = value;

                NotifyPropertyChanged("ContainerNames");

                this.ContainerName = default(string);
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged
    }
}
