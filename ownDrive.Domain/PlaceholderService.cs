using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Vanara.PInvoke.CldApi;

namespace ownDrive.Domain
{
	public class PlaceholderService : IPlaceholderService
	{
		private readonly IPlaceholderRepository _placeholderRepository;

		public PlaceholderService(IPlaceholderRepository placeholderRepository)
		{
			ArgumentNullException.ThrowIfNull(placeholderRepository);
			_placeholderRepository = placeholderRepository;
		}

		public Task DehydratePlaceholderAsync(string syncRootId, string placeholderPath, CancellationToken token = default)
		{
			throw new NotImplementedException();
		}

		public Task HydratePlaceholderAsync(string syncRootId, string placeholderPath, CancellationToken token = default)
		{
			throw new NotImplementedException();
		}
	}
}