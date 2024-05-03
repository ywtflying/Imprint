using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WestLakeShape.Common;

namespace NanoImprinter.ViewModels
{
    public class SystemOperatorViewModel
    {
        public RelayCommand StartProcessCommand { get; }

        public SystemOperatorViewModel()
        {
            StartProcessCommand = new RelayCommand(o => StartProcess(), o => true);
        }
        private void StartProcess()
        {
        }
    }
}
