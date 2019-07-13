using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client
{
	public class RetryPolicyDelegatingHandler: DelegatingHandler
	{
		private readonly int _maximunAmountOfRetries = 3;

		public RetryPolicyDelegatingHandler(int maximunAmountOfRetries)
			: base()
		{
			_maximunAmountOfRetries = maximunAmountOfRetries;
		}

		public RetryPolicyDelegatingHandler(HttpMessageHandler innerHandler, int maximunAmountOfRetries)
			:base()
		{
			_maximunAmountOfRetries = maximunAmountOfRetries;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			HttpResponseMessage response = null;
			for (int i = 0; i < _maximunAmountOfRetries; i++)
			{
				response = await base.SendAsync(request, cancellationToken);

				if (response.IsSuccessStatusCode)
				{
					return response;
				}
			}

			return response;
		}


	}
}
