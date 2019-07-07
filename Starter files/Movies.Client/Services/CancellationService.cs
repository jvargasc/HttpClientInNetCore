using Microsoft.Extensions.Configuration;
using Movies.Client.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
    public class CancellationService : IIntegrationService
    {
		private static HttpClient _httpClient = new HttpClient(
			new HttpClientHandler()
			{
				AutomaticDecompression = System.Net.DecompressionMethods.GZip
			});

		private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

		private IConfiguration Configuration { get; }

		public CancellationService(IConfiguration configuration)
		{
			Configuration = configuration;

			_httpClient.BaseAddress = new Uri(Configuration["UrlList:Url02"]);
			_httpClient.Timeout = new TimeSpan(0, 0, 1);
			_httpClient.DefaultRequestHeaders.Clear();
		}

		public async Task Run()
        {
			//cancellationTokenSource.CancelAfter(20);
			//await GetTrailerAndCancel(cancellationTokenSource);
			await GetTrailerAndHandleTimeout();
		}

		private async Task GetTrailerAndCancel(CancellationTokenSource cancellationTokenSource)
		{
			var request = new HttpRequestMessage(
				HttpMethod.Get,
				$"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");

			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

			try
			{
				using (var response = await _httpClient.SendAsync(request,
					HttpCompletionOption.ResponseHeadersRead,
					cancellationTokenSource.Token))
				{
					var stream = await response.Content.ReadAsStreamAsync();

					response.EnsureSuccessStatusCode();
					var trailer = stream.ReadAndDeserializeFromJson<Trailer>();
				}
			}
			catch (OperationCanceledException ocException)
			{
				Console.WriteLine($"An operation was cancelled with message {ocException.Message}.");
			}

		}

		private async Task GetTrailerAndHandleTimeout()
		{
			var request = new HttpRequestMessage(
				HttpMethod.Get,
				$"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");

			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

			try
			{
				using (var response = await _httpClient.SendAsync(request,
					HttpCompletionOption.ResponseHeadersRead))
				{
					var stream = await response.Content.ReadAsStreamAsync();

					response.EnsureSuccessStatusCode();
					var trailer = stream.ReadAndDeserializeFromJson<Trailer>();
					throw new TimeoutException();
				}
			}
			catch (TimeoutException toException)
			{
				Console.WriteLine($"An operation was cancelled with message {toException.Message}.");
			}

		}
	}
}