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
		public object Propulsion;
		public object FuelLevel;
		public object EnergyLevel;
		public object Replication;
		public object Excitation;

		public object LocalEnvironment;
		public object GlobalEnvironment;
	}
}
