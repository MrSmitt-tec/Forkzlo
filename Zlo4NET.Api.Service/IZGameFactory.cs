using System.Threading.Tasks;
using Zlo4NET.Api.Models.Shared;

namespace Zlo4NET.Api.Service;

public interface IZGameFactory
{
	Task<IZGameProcess> CreateSingleAsync(ZSingleParams args);

	Task<IZGameProcess> CreateMultiAsync(ZMultiParams args);

	Task<IZGameProcess> CreateTestRangeAsync(ZTestRangeParams args);

	Task<IZGameProcess> CreateCoOpAsync(ZCoopParams args);
}
