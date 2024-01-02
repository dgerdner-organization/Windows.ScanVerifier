using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS2010.Common
{
	public static class ClsUnitConversions
	{
		public static string KilogramsToPounds(string sInput, int iPrecision)
		{
			if (string.IsNullOrWhiteSpace(sInput))
				return "0";
			decimal dInput = ClsConvert.ToDecimal(sInput);
			decimal dOutput = KilogramsToPounds(dInput, iPrecision);
			return dOutput.ToString();
		}

		public static decimal KilogramsToPounds(decimal? dInput, int iPrecision)
		{
			if (!dInput.HasValue)
				return 0;
			double d = Convert.ToDouble(dInput.GetValueOrDefault()) * 2.20462;
			decimal dOutput = Convert.ToDecimal(d);
			dOutput = Math.Round(dOutput, iPrecision);
			return dOutput;
		}
	}
}
