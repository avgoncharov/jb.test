using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI.WebControls;
using Jb.Test.DAL.Interfaces;
using Jb.Test.DAL.Interfaces.Model;
using Jb.Test.NugetDataExtractor;
using Serilog;

namespace Jb.Test.Web.Controllers
{
	[RoutePrefix("api/packages")]
	public class PackagesController : ApiController
	{
		private readonly IPackagesRepository _repository;
		private readonly ILogger _logger = Log.Logger.ForContext<PackagesController>(); 

		public PackagesController(IPackagesRepository repository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		}

		[HttpGet, Route]
		public async Task<IHttpActionResult> Get()
		{
			var result = await _repository.GetAllByFilterAsync(null);
			return Ok(result);
		}

		[HttpGet, Route("{id}/{level}")]
		public async Task<IHttpActionResult> Get(string id, string level)
		{
			if (Enum.TryParse(level, true, out LoadingLevel innerLevel) != true)
			{
				return BadRequest();
			}

			var result = await _repository.FindByIdAsync(id, innerLevel);

			return Ok(result.ToArray());
		}

		[HttpGet, Route("{id}/{version}/raw-metadata")]
		public async Task<IHttpActionResult> GetRawMetadata(string id, string version)
		{
			var result = await _repository.GetDataByIdAndVersionAsync(id, version);

			if (result == null || result.Length == 0)
			{
				return NotFound();
			}

			var extractor = new DataExtractor();
			
			return Ok(new {Data = extractor.ExtractRawMetadata(result)});
		}

		[HttpPost, Route("find-by-filter")]
		public async Task<IHttpActionResult> Post([FromBody]Filter filter)
		{
			var result = await _repository.GetAllByFilterAsync(filter);

			return Ok(result);
		}

	}
}
