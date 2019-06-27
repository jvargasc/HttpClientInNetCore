using Microsoft.Extensions.Configuration;
using Movies.Client.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Movies.Client.Services
{

	public class CRUDService : IIntegrationService
    {
		private IConfiguration Configuration { get; }
		private static HttpClient _httpClient = new HttpClient();

		public CRUDService(IConfiguration configuration)
		{
			Configuration = configuration;
			_httpClient.BaseAddress = new Uri(Configuration["UrlList:Url02"]);
			_httpClient.Timeout = new TimeSpan(0, 0, 30);
			_httpClient.DefaultRequestHeaders.Clear();
			//_httpClient.DefaultRequestHeaders.Accept.Add(
			//	new MediaTypeWithQualityHeaderValue("application/json", 0.4));
			//_httpClient.DefaultRequestHeaders.Accept.Add(
			//	new MediaTypeWithQualityHeaderValue("application/xml", 0.1));
		}
		
		public async Task Run()
        {
			// await GetResource();
			//await GetResourceThroughHttpRequestMessage();
			//await CreateResource();
			//await UpdateResource();
			await DeleteResource();
		}

		public async Task GetResource()
		{
			var response = await _httpClient.GetAsync("api/movies");
			response.EnsureSuccessStatusCode();
			var content = response.Content.ReadAsStringAsync();
			//var movies = JsonConvert.DeserializeObject<IEnumerable<Movie>>(content.Result);
			var movies = new List<Movie>();
			if (response.Content.Headers.ContentType.MediaType == "application/json")
			{
				movies = JsonConvert.DeserializeObject<List<Movie>>(content.Result);
			}
			else if (response.Content.Headers.ContentType.MediaType == "application/xml")
			{
				var serializer = new XmlSerializer(typeof(List<Movie>));
				movies = (List<Movie>)serializer.Deserialize(new StringReader(content.Result));
			}

		}

		public async Task GetResourceThroughHttpRequestMessage()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "api/movies");
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			var response = await _httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			var content = await response.Content.ReadAsStringAsync();
			var movies = JsonConvert.DeserializeObject<List<Movie>>(content);


		}

		public async Task CreateResource()
		{
			var movieToCreate = new MovieForCreation()
			{
				Title = "Reservoir Dogs",
				Description = "After a simple jewelry heist goes terribly wrong, the " +
				 "surviving criminals begin to suspect that one of them is a police informant.",
				DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
				ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
				Genre = "Crime, Drama"
			};
			var serializedMovieToCreate = JsonConvert.SerializeObject(movieToCreate);

			var request = new HttpRequestMessage(HttpMethod.Post, "api/movies");
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			request.Content = new StringContent(serializedMovieToCreate);
			request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			var response = await _httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			var content = await response.Content.ReadAsStringAsync();
			var createdMovie = JsonConvert.DeserializeObject<Movie>(content);
		
		}

		private async Task UpdateResource()
		{
			var movieToUpdate = new MovieForUpdate()
			{
				Title = "Pulp Fiction",
				Description = "The movie with zed.",
				DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
				ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
				Genre = "Crime, Drama"
			};
			var serializedMovieToUpdate = JsonConvert.SerializeObject(movieToUpdate);

			var request = new HttpRequestMessage(HttpMethod.Put,
				"api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");

			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			request.Content = new StringContent(serializedMovieToUpdate);
			request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

			var response = await _httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			var content = await response.Content.ReadAsStringAsync();
			var updatedMovie = JsonConvert.DeserializeObject<Movie>(content);

		}

		private async Task DeleteResource()
		{
			var request = new HttpRequestMessage(HttpMethod.Delete,
				"api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			var response = await _httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			var content = await response.Content.ReadAsStringAsync();

		}

		private async Task PostResourceShortCut()
		{
			var movieToCreate = new MovieForCreation()
			{
				Title = "Reservoir Dogs",
				Description = "After a simple jewelry heist goes terribly wrong, the " +
				"surviving criminals begin to suspect that one of them is a police informant.",
				DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
				ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
				Genre = "Crime, Drama"
			};

			var response = await _httpClient.PostAsync(
				"api/movies", 
				new StringContent(
					JsonConvert.SerializeObject(movieToCreate),
					Encoding.UTF8,
					"application/json"));

			response.EnsureSuccessStatusCode();

			var content = await response.Content.ReadAsStringAsync();
			var createdMovie = JsonConvert.DeserializeObject<Movie>(content);
		}

    }
}