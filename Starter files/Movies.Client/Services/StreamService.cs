using Microsoft.Extensions.Configuration;
using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
    public class StreamService : IIntegrationService
    {
		private static HttpClient _httpClient = new HttpClient(
			new HttpClientHandler()
			{
				AutomaticDecompression = System.Net.DecompressionMethods.GZip });

		private IConfiguration Configuration { get; }
		public StreamService(IConfiguration configuration)
		{
			Configuration = configuration;

			_httpClient.BaseAddress = new Uri(Configuration["UrlList:Url02"]);
			_httpClient.Timeout = new TimeSpan(0, 0, 30);
			_httpClient.DefaultRequestHeaders.Clear();
		}

		public async Task Run()
        {

			await TestGetPosterWithoutStream();
			await TestGetPosterWithStream();
			await TestGetPosterWithStreamAndCompletionMode();
			await TestGetPosterWithGZCompression();

			//await PostPosterWithStream();
			//await PostPosterWithStream();
			//await PostAndReadPosterWithStream();

			Console.WriteLine();
			Console.WriteLine();
			await TestPostPosterWithoutStream();
			await TestPostPosterWithStream();
			await TestPostAndReadPosterWithStream();
		}          

		private async Task GetPosterWithStream()
		{
			var request = new HttpRequestMessage(
				HttpMethod.Get,
				$"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");

			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			using (var response = await _httpClient.SendAsync(request))
			{
				response.EnsureSuccessStatusCode();

				var stream = await response.Content.ReadAsStreamAsync();
				var poster = stream.ReadAndDeserializeFromJson<Poster>();

				//using (var streamReader = new StreamReader(stream))
				//{
				//	using (var jsonTextReader = new JsonTextReader(streamReader))
				//	{
				//		var jsonSerializer = new JsonSerializer();
				//		var poster = jsonSerializer.Deserialize<Poster>(jsonTextReader);
				//	}
				//}
			}
		}

		private async Task GetPosterWithStreamAndCompletionMode()
		{
			var request = new HttpRequestMessage(
				HttpMethod.Get,
				$"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");

			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			using (var response = await _httpClient.SendAsync(request, 
												HttpCompletionOption.ResponseHeadersRead))
			{
				response.EnsureSuccessStatusCode();

				var stream = await response.Content.ReadAsStreamAsync();
				var poster = stream.ReadAndDeserializeFromJson<Poster>();

				//using (var streamReader = new StreamReader(stream))
				//{
				//	using (var jsonTextReader = new JsonTextReader(streamReader))
				//	{
				//		var jsonSerializer = new JsonSerializer();
				//		var poster = jsonSerializer.Deserialize<Poster>(jsonTextReader);
				//	}
				//}
			}
		}

		private async Task GetPosterWithoutStream()
		{
			var request = new HttpRequestMessage(
				HttpMethod.Get,
				$"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			var response = await _httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			var content = await response.Content.ReadAsStringAsync();
			var posters = JsonConvert.DeserializeObject<Poster>(content);
		}

		private async Task GetPosterWithGZCompression()
		{
			var request = new HttpRequestMessage(
				HttpMethod.Get,
				$"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");

			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

			using (var response = await _httpClient.SendAsync(request))
			{
				response.EnsureSuccessStatusCode();

				var stream = await response.Content.ReadAsStreamAsync();
				var poster = stream.ReadAndDeserializeFromJson<Poster>();

			}
		}

		private async Task PostPosterWithStream()
		{
			var random = new Random();
			var generatedBytes = new byte[524288];
			random.NextBytes(generatedBytes);

			var posterForCreation = new PosterForCreation()
			{
				Name = "A new poster for The Big Lebowski",
				Bytes = generatedBytes
			};

			var memoryContentStream = new MemoryStream();
			memoryContentStream.SerializeToJsonAndWrite(posterForCreation);
			memoryContentStream.Seek(0, SeekOrigin.Begin);

			using (var request = new HttpRequestMessage(
				HttpMethod.Post,
				$"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/"))
			{
				request.Headers.Accept.Add(
				   new MediaTypeWithQualityHeaderValue("application/json"));

				using (var streamContent = new StreamContent(memoryContentStream))
				{
					request.Content = streamContent;
					request.Content.Headers.ContentType =
							new MediaTypeHeaderValue("application/json");

					var response = await _httpClient.SendAsync(request);
					response.EnsureSuccessStatusCode();

					var createdContent = await response.Content.ReadAsStringAsync();
					var createdPoster = JsonConvert.DeserializeObject<Poster>(createdContent);

				}
			}
		}

		private async Task PostAndReadPosterWithStream()
		{
			var random = new Random();
			var generatedBytes = new byte[524288];
			random.NextBytes(generatedBytes);

			var posterForCreation = new PosterForCreation()
			{
				Name = "A new poster for The Big Lebowski",
				Bytes = generatedBytes
			};

			var memoryContentStream = new MemoryStream();
			memoryContentStream.SerializeToJsonAndWrite(posterForCreation);
			memoryContentStream.Seek(0, SeekOrigin.Begin);

			using (var request = new HttpRequestMessage(
				HttpMethod.Post,
				$"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/"))
			{
				request.Headers.Accept.Add(
				   new MediaTypeWithQualityHeaderValue("application/json"));

				using (var streamContent = new StreamContent(memoryContentStream))
				{
					request.Content = streamContent;
					request.Content.Headers.ContentType =
							new MediaTypeHeaderValue("application/json");

					using (var response = await _httpClient
						.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
					{
						response.EnsureSuccessStatusCode();
						var stream = await response.Content.ReadAsStreamAsync();
						var poster = stream.ReadAndDeserializeFromJson<Poster>();
					}

				}
			}
		}

		private async Task PostPosterWithoutStream()
		{
			var random = new Random();
			var generatedBytes = new byte[524288];
			random.NextBytes(generatedBytes);

			var posterForCreation = new PosterForCreation()
			{
				Name = "A new poster for The Big Lebowski",
				Bytes = generatedBytes
			};

			var serializedPosterForCreation = JsonConvert.SerializeObject(posterForCreation);

			var request = new HttpRequestMessage(HttpMethod.Post,
				$"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters");
			
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			request.Content = new StringContent(serializedPosterForCreation);
			request.Content.Headers.ContentType =
					new MediaTypeHeaderValue("application/json");

			var response = await _httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			var createdContent = await response.Content.ReadAsStringAsync();
			var createdPoster = JsonConvert.DeserializeObject<Poster>(createdContent);
			
		}

		private async Task TestGetPosterWithoutStream()
		{
			await GetPosterWithoutStream();

			var stopWatch = Stopwatch.StartNew();

			for(int i = 0; i < 200; i++)
			{
				await GetPosterWithoutStream();
			}

			stopWatch.Stop();
			Console.WriteLine($"Elapsed milliseconds for get poster without stream " +
				$"{stopWatch.ElapsedMilliseconds}, " + 
				$"averaging {stopWatch.ElapsedMilliseconds / 200 } milliseconds/request"
				);

		}

		private async Task TestGetPosterWithStream()
		{
			await GetPosterWithStream();

			var stopWatch = Stopwatch.StartNew();

			for (int i = 0; i < 200; i++)
			{
				await GetPosterWithStream();
			}

			stopWatch.Stop();
			Console.WriteLine($"Elapsed milliseconds for get poster with stream " +
				$"{stopWatch.ElapsedMilliseconds}, " +
				$"averaging {stopWatch.ElapsedMilliseconds / 200 } milliseconds/request"
				);

		}

		private async Task TestGetPosterWithStreamAndCompletionMode()
		{			
			await GetPosterWithStreamAndCompletionMode();

			var stopWatch = Stopwatch.StartNew();

			for (int i = 0; i < 200; i++)
			{
				await GetPosterWithoutStream();
			}

			stopWatch.Stop();
			Console.WriteLine($"Elapsed milliseconds for get poster with stream and completion mode " +
				$"{stopWatch.ElapsedMilliseconds}, " +
				$"averaging {stopWatch.ElapsedMilliseconds / 200 } milliseconds/request"
				);

		}

		private async Task TestGetPosterWithGZCompression()
		{
			await GetPosterWithStreamAndCompletionMode();

			var stopWatch = Stopwatch.StartNew();

			for (int i = 0; i < 200; i++)
			{
				await GetPosterWithoutStream();
			}

			stopWatch.Stop();
			Console.WriteLine($"Elapsed milliseconds for get poster with GZCompression mode " +
				$"{stopWatch.ElapsedMilliseconds}, " +
				$"averaging {stopWatch.ElapsedMilliseconds / 200 } milliseconds/request"
				);

		}

		private async Task TestPostPosterWithoutStream()
		{
			await GetPosterWithoutStream();

			var stopWatch = Stopwatch.StartNew();

			for (int i = 0; i < 200; i++)
			{
				await PostPosterWithoutStream();
			}

			stopWatch.Stop();
			Console.WriteLine($"Elapsed milliseconds for post poster without stream " +
				$"{stopWatch.ElapsedMilliseconds}, " +
				$"averaging {stopWatch.ElapsedMilliseconds / 200 } milliseconds/request"
				);

		}

		private async Task TestPostPosterWithStream()
		{
			await GetPosterWithoutStream();

			var stopWatch = Stopwatch.StartNew();

			for (int i = 0; i < 200; i++)
			{
				await PostPosterWithStream();
			}

			stopWatch.Stop();
			Console.WriteLine($"Elapsed milliseconds for post poster with stream " +
				$"{stopWatch.ElapsedMilliseconds}, " +
				$"averaging {stopWatch.ElapsedMilliseconds / 200 } milliseconds/request"
				);

		}

		private async Task TestPostAndReadPosterWithStream()
		{
			await GetPosterWithoutStream();

			var stopWatch = Stopwatch.StartNew();

			for (int i = 0; i < 200; i++)
			{
				await PostAndReadPosterWithStream();
			}

			stopWatch.Stop();
			Console.WriteLine($"Elapsed milliseconds for post and read poster with stream " +
				$"{stopWatch.ElapsedMilliseconds}, " +
				$"averaging {stopWatch.ElapsedMilliseconds / 200 } milliseconds/request"
				);

		}
	}
}
