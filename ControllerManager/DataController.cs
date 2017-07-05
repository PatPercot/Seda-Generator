using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerManager {
	public class DataController : BigTask {
		public DataController(string aTaskRole, string aTaskName, string aProfileName, string aDataName, string aTraceName)
			: base(aTaskRole, aTaskName, aProfileName, aDataName, aTraceName) {
			base.TaskRole = aTaskRole;
			base.TaskName = aTaskName;
			base.ProfileName = aProfileName;
			base.DataName = aDataName;
			base.TraceName = aTraceName;
		}

		public DataController() {}
	}
}
