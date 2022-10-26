using System;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace Lab_06_1
{   
    
    class Program
    {   
        static Random rng = new Random();
        static string API_KEY = "HEY! USE YOUR OWN DAMN API KEY!";
        public struct Weather{
            public string Country {get; set;}
            public string Name {get; set;}
            public double Temp {get; set;}
            public string Description {get; set;}
        }

        public class API_call{
            public async Task<Weather> GetWeather(double latitude, double longitude){
                
                var url = $"https://api.openweathermap.org/data/2.5/weather";
                var parameters = $"?lat={latitude}&lon={longitude}&appid={API_KEY}";

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(url);
                
                HttpResponseMessage response = await client.GetAsync(parameters).ConfigureAwait(false);
                Weather result = new Weather();
                
                if (response.IsSuccessStatusCode){
                    var jsonString = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine(jsonString);
                    
                    Regex rx = new Regex("(?<=\"country\":\")[^\"]+(?=\")");
                    result.Country = rx.Match(jsonString).ToString();
                    rx = new Regex("(?<=\"name\":\")[^\"]+(?=\")");
                    result.Name = rx.Match(jsonString).ToString();
                    rx = new Regex("(?<=\"temp\":)[^\"]+(?=,)");
                    result.Temp = Math.Round(Convert.ToDouble(rx.Match(jsonString).ToString())-273);
                    rx = new Regex("(?<=\"description\":\")[^\"]+(?=\")");
                    result.Description = rx.Match(jsonString).ToString();
                    //Console.WriteLine($"\n{result.Country}, {result.Name}: {result.Temp}, {result.Description}\n");

                }

                return result;
            }
        }

        static void Main(string[] args)
        {
            API_call test_call = new API_call();
            Weather test = test_call.GetWeather(1,1).GetAwaiter().GetResult();
            //Regex rx = new Regex("(?<=\"country\":\")[^\"]+(?=\")");
            //string js = "\"country\":\"df44G\",\"sunrise\":1665806505,\"sunset\":1665847774";
            //Console.WriteLine(rx.Match(js));

            Weather[] weatherList = new Weather[50];

            for (int i = 0 ; i < weatherList.Length ; i ++){
                Weather temp = new Weather();
                
                do{
                    temp = test_call.GetWeather(
                    rng.Next(-90,90) + rng.NextDouble(),
                    rng.Next(-180,180) + rng.NextDouble()).GetAwaiter().GetResult();
                } while(temp.Country.Length==0 && temp.Name.Length==0);

                Console.WriteLine($"{temp.Country}, {temp.Name}: {temp.Temp} degrees, {temp.Description}");
                weatherList[i] = temp;
            }



            Weather outputData = (from data in weatherList
                                select data).OrderBy(data => data.Temp).First();
            Console.WriteLine($"\nMin temp:\n{outputData.Country}, {outputData.Name}: {outputData.Temp} degrees, {outputData.Description}");

            outputData = (from data in weatherList
                                select data).OrderBy(data => data.Temp).Last();
            Console.WriteLine($"\nMax temp:\n{outputData.Country}, {outputData.Name}: {outputData.Temp} degrees, {outputData.Description}");
            

            double res = weatherList.Average(data => data.Temp);
            Console.WriteLine($"\nAverage world temperature: {res} degrees");
            
            int countryCount = weatherList.Select(data => data.Country).Distinct().Count();
            Console.WriteLine($"\nCountry count: {countryCount}");

            try{
                var firstSuitable = (from data in weatherList
                                    where data.Description == "rain" ||
                                            data.Description == "clear sky" ||
                                            data.Description == "few clouds"
                                    select data).Take(1).First();
                Console.WriteLine($"\n{firstSuitable.Country}, {firstSuitable.Name}: {firstSuitable.Temp} degrees, {firstSuitable.Description}");
            }
            catch {Console.WriteLine("\nNo suitable data found");}


        }   
    }
}
