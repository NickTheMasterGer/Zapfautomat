using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace BlazorApp.Data
{
   public class HMIService
   {
	  private static Timer aTimer;
	  public bool logo { get ; set ; }
	  public event EventHandler PropertyChangedEvent;
	   public async void PropertyChanged()
	   {
		 PropertyChangedEvent?.Invoke(this, EventArgs.Empty);
	   }

   public HMIService()
	  {
		 logo = true;
		  // Create a timer and set a two second interval.
		 aTimer = new System.Timers.Timer();
		 aTimer.Interval = 10000;

		 // Hook up the Elapsed event for the timer. 
		 aTimer.Elapsed += OnTimedEvent;

		 // Have the timer fire repeated events (true is the default)
		 aTimer.AutoReset = true;

		 // Start the timer
		 aTimer.Enabled = true;
	  }
	  private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
	  {
		 if (logo == true)
		 {
			logo = false;
			aTimer.Interval = 2000;
		 }
		 else
		 {
			logo = true;
			aTimer.Interval = 5000;
		 }
		 PropertyChanged();
	  }



	  /*public Task<WeatherForecast[]> GetForecastAsync(DateTime startDate)
	  {
		 var rng = new Random();
		 return Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
		 {
			Date = startDate.AddDays(index),
			TemperatureC = rng.Next(-20, 55),
			Summary = Summaries[rng.Next(Summaries.Length)]
		 }).ToArray());
	  }
	  */
   }
}
