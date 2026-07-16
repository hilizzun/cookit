using CookIt.Core.Dtos.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookIt.Core.Interfaces.Admin
{
    public interface IAdminUnitService
    {
        Task<List<UnitAdminDto>> GetAllUnitsAsync(bool includeDeleted = false);
        Task<UnitAdminDto> GetUnitByIdAsync(int id);
        Task<UnitAdminDto> CreateUnitAsync(CreateUnitDto dto);
        Task<UnitAdminDto> UpdateUnitAsync(UpdateUnitDto dto);
        Task<bool> DeleteUnitAsync(int id);
        Task<bool> RestoreUnitAsync(int id);
        Task<bool> IsUnitUsedAsync(int id);
    }
}
