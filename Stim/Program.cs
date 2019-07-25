using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stim
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			//H3.InstanciateNativeLibrary();
			//ulong h3HQ1 = H3.StringToH3("8f2830828052d25");
			//ulong h3HQ2 = H3.StringToH3("8f283082a30e623");
			//GeoCoord[] geoHQ1 = H3.H3ToGeoBoundary(h3HQ1);
			//GeoCoord[] geoHQ2 = H3.H3ToGeoBoundary(h3HQ2);

			////h3ToGeo(h3HQ2, &geoHQ2);
			//Console.WriteLine(String.Format("origin: (%lf, %lf)\ndestination: (%lf, %lf)\ngrid distance:",
			//	H3.RadToDeg(geoHQ1[0].latitude),
			//	H3.RadToDeg(geoHQ1[0].longitude),
			//	H3.RadToDeg(geoHQ2[0].latitude),
			//	H3.RadToDeg(geoHQ2[0].longitude)));


			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new WorldView());

		}
	}
}
