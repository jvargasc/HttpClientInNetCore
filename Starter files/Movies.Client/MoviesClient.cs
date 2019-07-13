using Microsoft.Extensions.Configuration;
using Movies.Client.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client
{
	public class MoviesClient
	{
		//public HttpClient Client { get; }
		private HttpClient _client { get; }

		public MoviesClient(HttpClient client, IConfiguration Configuration)
		{
			_client = client;
			_client.BaseAddress = new Uri(Configuration["UrlList:Url02"]);
			_client.Timeout = new TimeSpan(0, 0, 30);
			_client.DefaultRequestHeaders.Clear();
		}

		public async Task<IEnumerable<Movie>> GetMovies(CancellationToken cancellationToken)
		{
			var request = new HttpRequestMessage(
				HttpMethod.Get,
				"api/movies");

			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gazip"));

			using (var response = await _client.SendAsync(request,
				HttpCompletionOption.ResponseHeadersRead,
				cancellationToken))
			{
				var stream = await response.Content.ReadAsStreamAsync();
				response.EnsureSuccessStatusCode();
				return stream.ReadAndDeserializeFromJson<List<Movie>>();
			}
		}
	}
}
