using CookIt.Core.Dtos.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookIt.Core.Interfaces.Admin
{
    public interface IAdminDishTypeService
    {
        Task<List<DishTypeAdminDto>> GetAllDishTypesAsync(bool includeDeleted = false);
        Task<DishTypeAdminDto> GetDishTypeByIdAsync(int id);
        Task<DishTypeAdminDto> CreateDishTypeAsync(CreateDishTypeDto dto);
        Task<DishTypeAdminDto> UpdateDishTypeAsync(UpdateDishTypeDto dto);
        Task<bool> DeleteDishTypeAsync(int id);
        Task<bool> RestoreDishTypeAsync(int id);
        Task<bool> IsDishTypeUsedAsync(int id);
    }
}
