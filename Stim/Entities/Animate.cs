using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stim.Entities
{
	// Physical motion, Body Motion, (Body) Part Motion, (Body) Energy, 
	// Time, Cognative (control), Knowledge, Emotion, Social, Culture, 
	// Moral, Ability, Transformation, Communication, Sensory, 
	// Local Environment, World Environment, Logic

	class Animate : BaseEntity
	{
		public object Propulsion { get; set; }
		public object FuelLevel { get; set; }
        public object EnergyLevel { get; set; }
        public object Replication { get; set; }
        public object Excitation { get; set; }

        public object LocalEnvironment { get; set; }
        public object GlobalEnvironment { get; set; }
    }
}
