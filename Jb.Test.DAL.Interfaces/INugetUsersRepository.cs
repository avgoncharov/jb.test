using System.Threading.Tasks;
using Jb.Test.DAL.Interfaces.Model;

namespace Jb.Test.DAL.Interfaces
{
	/// <summary>
	/// Интерфейс репозитория идентифицированных пользователей.
	/// </summary>
	public interface INugetUsersRepository
	{		      
		/// <summary>
		/// Добавляет пользователя в систему.
		/// </summary>
		/// <param name="apiKey">API-ключ пользователя.</param>
		/// <param name="userName">Имя пользоваетля.</param>
		/// <returns></returns>
		Task AddAsync(string apiKey, string userName);


		/// <summary>
		/// Ищет пользователя по API-ключу.
		/// </summary>
		/// <param name="apiKey">API-ключ пользователя.</param>
		/// <returns>Искомый пользоваетля или null</returns>
		Task<NugetUser> FindByApiKeyAsync(string apiKey);
	}
}
