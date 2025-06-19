using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ownDrive.Domain
{
	public interface IPlaceholderService
	{
		public Task HydratePlaceholderAsync(string syncRootId, string placeholderPath, CancellationToken token = default);

		public Task DehydratePlaceholderAsync(string syncRootId, string placeholderPath, CancellationToken token = default);
	}
}