using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Jb.Test.ODataODataInfrastructura
{
	/// <summary>
	/// Класс результата запроса типа "текст."
	/// </summary>
	public class PlainTextResult : IHttpActionResult
	{
		private readonly HttpRequestMessage _request;

		/// <summary>
		/// Возвращает контент результата.
		/// </summary>
		public string Content { get; }


		/// <summary>
		/// Инициализирует новый экземпляр класса.
		/// </summary>
		/// <param name="content">Контент результата.</param>
		/// <param name="request">Запрос, результатом которого является результат.</param>
		public PlainTextResult(string content, HttpRequestMessage request)
		{
			_request = request;
			Content = content;
		}


		/// <summary>
		/// Выполняет построение ответа.
		/// </summary>
		/// <param name="cancellationToken">Токен централизованной отмены.</param>
		/// <returns>Сообщение-ответ.</returns>
		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			var response = new HttpResponseMessage()
			{
				Content = new StringContent(Content, Encoding.UTF8, "text/plain"),
				RequestMessage = _request
			};

			return Task.FromResult(response);
		}
	}
}