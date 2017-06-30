using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ControllerManager {
	public class ProfileController : BigTask {
		public ProfileController(string aTaskRole, string aTaskName, string aProfileName, string aDataName, string aTraceName)
			: base(aTaskRole, aTaskName, aProfileName, aDataName, aTraceName) {
			base.TaskRole = aTaskRole;
			base.TaskName = aTaskName;
			base.ProfileName = aProfileName;
			base.DataName = aDataName;
			base.TraceName = aTraceName;
		}

		public ProfileController() {}

	}
}
